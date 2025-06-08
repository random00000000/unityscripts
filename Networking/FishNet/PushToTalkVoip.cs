using System.Collections;
using UnityEngine;
using FishNet.Object;

/// <summary>
/// Very basic push-to-talk voice chat using FishNet.
/// Hold the configured key to record audio and transmit
/// it to all observers. Others will hear the clip played
/// back from this object.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PushToTalkVoip : NetworkBehaviour
{
    [Tooltip("Key used for voice chat")]
    public KeyCode talkKey = KeyCode.V;

    [Tooltip("Recording sample rate")]
    public int sampleRate = 22050;

    [Tooltip("Maximum length of a single transmission in seconds")]
    public float maxRecordTime = 5f;

    private AudioSource audioSource;
    private string micName;
    private AudioClip recording;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
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
        AudioClip clip = AudioClip.Create("remote", data.Length / channels, channels, frequency, false);
        clip.SetData(data, 0);
        audioSource.PlayOneShot(clip);
    }
}
