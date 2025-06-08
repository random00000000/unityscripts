using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Transporting;

/// <summary>
/// Push-to-talk voice chat that streams audio data while the key
/// is held down. Captured samples are sent every few frames so
/// remote listeners hear you almost instantly, like water flowing
/// from a hose.
/// </summary>
public class StreamingVoip : NetworkBehaviour
{
    [Header("Voice Settings")]
    [Tooltip("Key used for voice chat")]
    public KeyCode talkKey = KeyCode.V;

    [Tooltip("Recording sample rate")]
    public int sampleRate = 22050;

    [Tooltip("How often to send captured audio samples (seconds)")]
    public float sendInterval = 0.1f;

    [Header("Proximity Settings")]
    [Tooltip("Maximum distance at which voice can be heard")]
    public float maxHearDistance = 20f;

    [Tooltip("Minimum volume at max distance (0-1)")]
    [Range(0f, 1f)]
    public float minVolume = 0.1f;

    [Tooltip("AudioSource component that will play received voice data")]
    [SerializeField] private AudioSource audioSource;

    private string micName;
    private AudioClip micClip;
    private int lastSample;
    private Coroutine streamRoutine;

    private void Awake()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not assigned to StreamingVoip!");
            enabled = false;
            return;
        }

        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = maxHearDistance;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(talkKey))
            StartStreaming();

        if (Input.GetKeyUp(talkKey))
            StopStreaming();
    }

    private void StartStreaming()
    {
        if (Microphone.devices.Length == 0)
            return;

        micName = Microphone.devices[0];
        micClip = Microphone.Start(micName, true, 1, sampleRate);
        lastSample = 0;
        streamRoutine = StartCoroutine(StreamAudio());
    }

    private void StopStreaming()
    {
        if (micClip == null)
            return;

        Microphone.End(micName);
        if (streamRoutine != null)
            StopCoroutine(streamRoutine);
        micClip = null;
    }

    private IEnumerator StreamAudio()
    {
        while (Microphone.IsRecording(micName))
        {
            int pos = Microphone.GetPosition(micName);
            if (pos < lastSample)
                pos += micClip.samples;

            int samples = pos - lastSample;
            if (samples > 0)
            {
                float[] data = new float[samples * micClip.channels];
                micClip.GetData(data, lastSample % micClip.samples);
                SendVoiceServerRpc(data, micClip.channels, sampleRate);
                lastSample = pos % micClip.samples;
            }

            yield return new WaitForSeconds(sendInterval);
        }
    }

    [ServerRpc]
    private void SendVoiceServerRpc(float[] data, int channels, int frequency, Channel channel = Channel.Unreliable)
    {
        ReceiveVoiceObserversRpc(data, channels, frequency);
    }

    [ObserversRpc]
    private void ReceiveVoiceObserversRpc(float[] data, int channels, int frequency)
    {
        if (!IsOwner)
        {
            AudioClip clip = AudioClip.Create("remote", data.Length / channels, channels, frequency, false);
            clip.SetData(data, 0);
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnValidate()
    {
        if (audioSource != null)
        {
            audioSource.maxDistance = maxHearDistance;
        }
    }
}
