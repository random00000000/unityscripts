using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Tooltip("Audio mixer controlling master volume")]
    public AudioMixer masterMixer;

    [Range(0f, 1f)]
    [Tooltip("Initial music volume")] 
    public float musicVolume = 1f;

    private void Start()
    {
        SetMusicVolume(musicVolume);
    }

    /// <summary>
    /// Set music volume (0..1). Maps to -80..0 dB on the mixer.
    /// </summary>
    /// <param name="volume">Normalized volume.</param>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        float dB = Mathf.Lerp(-80f, 0f, musicVolume);
        masterMixer.SetFloat("MusicVolume", dB);
    }
}
