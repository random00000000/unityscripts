using System.Collections;
using UnityEngine;
using FishNet.Object;
using FishNet.Transporting;

/// <summary>
/// Proximity-based push-to-talk voice chat using FishNet.
/// Hold the configured key to record audio and transmit
/// it to all observers. Others will hear the clip played
/// back from this object, with volume based on distance.
/// </summary>
public class PushToTalkVoip : NetworkBehaviour
{
    [Header("Voice Settings")]
    [Tooltip("Key used for voice chat")]
    public KeyCode talkKey = KeyCode.V;

    [Tooltip("Recording sample rate")]
    public int sampleRate = 22050;

    [Tooltip("Maximum length of a single transmission in seconds")]
    public float maxRecordTime = 5f;

    [Header("Proximity Settings")]
    [Tooltip("Maximum distance at which voice can be heard")]
    public float maxHearDistance = 20f;

    [Tooltip("Minimum volume at max distance (0-1)")]
    [Range(0f, 1f)]
    public float minVolume = 0.1f;

    [Tooltip("AudioSource component that will play received voice data")]
    [SerializeField] private AudioSource audioSource;

    private string micName;
    private AudioClip recording;
    private Transform localPlayerTransform;

    private void Awake()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not assigned to PushToTalkVoip!");
            enabled = false;
            return;
        }
        
        audioSource.loop = false;
        audioSource.spatialBlend = 1f; // Enable 3D spatial audio
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = maxHearDistance;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        localPlayerTransform = transform;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(talkKey))
            StartRecording();

        if (Input.GetKeyUp(talkKey))
            StopRecording();
    }

    private void StartRecording()
    {
        if (Microphone.devices.Length == 0)
            return;

        micName = Microphone.devices[0];
        recording = Microphone.Start(micName, false, (int)maxRecordTime, sampleRate);
    }

    private void StopRecording()
    {
        if (recording == null)
            return;

        int samples = Microphone.GetPosition(micName);
        Microphone.End(micName);

        if (samples <= 0)
        {
            recording = null;
            return;
        }

        float[] data = new float[samples * recording.channels];
        recording.GetData(data, 0);
        SendVoiceServerRpc(data, recording.channels, sampleRate);
        recording = null;
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
