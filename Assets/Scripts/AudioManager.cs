using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized audio management system.
/// Handles all audio playback: music, SFX, and voice lines.
/// Singleton that persists across scenes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource ambientAudioSource;
    [SerializeField] private AudioSource[] sfxAudioSources = new AudioSource[4]; // Pool of SFX sources
    [SerializeField] private AudioSource voiceAudioSource;

    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float voiceVolume = 1f;

    [SerializeField] private float musicFadeDuration = 1f;
    [SerializeField] private float musicCrossfadeDuration = 2f;

    private AudioClip currentMusicClip;
    private float musicFadeTimer = 0f;
    private float targetMusicVolume = 0f;
    private bool isFadingMusic = false;

    private Queue<SFXRequest> sfxQueue = new Queue<SFXRequest>();
    private int currentSFXSourceIndex = 0;

    // Events
    public delegate void MusicChangedDelegate(AudioClip newMusic);
    public event MusicChangedDelegate OnMusicChanged;

    public delegate void VolumeChangedDelegate(string volumeType, float newVolume);
    public event VolumeChangedDelegate OnVolumeChanged;

    public delegate void SFXPlayedDelegate(string sfxName);
    public event SFXPlayedDelegate OnSFXPlayed;

    [System.Serializable]
    private struct SFXRequest
    {
        public AudioClip clip;
        public float volume;
        public float pitch;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioSources();
    }

    private void Update()
    {
        UpdateMusicFade();
        ProcessSFXQueue();
    }

    /// <summary>
    /// Initialize audio sources if not assigned in Inspector
    /// </summary>
    private void InitializeAudioSources()
    {
        if (musicAudioSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicAudioSource = musicObj.AddComponent<AudioSource>();
            musicAudioSource.loop = true;
        }

        if (ambientAudioSource == null)
        {
            GameObject ambientObj = new GameObject("AmbientSource");
            ambientObj.transform.SetParent(transform);
            ambientAudioSource = ambientObj.AddComponent<AudioSource>();
            ambientAudioSource.loop = true;
        }

        if (voiceAudioSource == null)
        {
            GameObject voiceObj = new GameObject("VoiceSource");
            voiceObj.transform.SetParent(transform);
            voiceAudioSource = voiceObj.AddComponent<AudioSource>();
        }

        // Create SFX source pool
        if (sfxAudioSources.Length == 0 || sfxAudioSources[0] == null)
        {
            sfxAudioSources = new AudioSource[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject sfxObj = new GameObject($"SFXSource_{i}");
                sfxObj.transform.SetParent(transform);
                sfxAudioSources[i] = sfxObj.AddComponent<AudioSource>();
            }
        }

        UpdateAllVolumes();
    }

    /// <summary>
    /// Play music track
    /// </summary>
    public void PlayMusic(AudioClip musicClip, bool fade = true)
    {
        if (musicClip == null)
        {
            Debug.LogWarning("[AudioManager] Attempted to play null music clip");
            return;
        }

        currentMusicClip = musicClip;

        if (fade)
        {
            FadeOutMusic(() =>
            {
                musicAudioSource.clip = musicClip;
                musicAudioSource.Play();
                FadeInMusic();
            });
        }
        else
        {
            musicAudioSource.clip = musicClip;
            musicAudioSource.Play();
            musicAudioSource.volume = musicVolume * masterVolume;
        }

        OnMusicChanged?.Invoke(musicClip);
        Debug.Log($"[MUSIC] Now playing: {musicClip.name}");
    }

    /// <summary>
    /// Stop music with fade
    /// </summary>
    public void StopMusic(bool fade = true)
    {
        if (fade)
        {
            FadeOutMusic(() => musicAudioSource.Stop());
        }
        else
        {
            musicAudioSource.Stop();
        }
    }

    /// <summary>
    /// Fade in music
    /// </summary>
    private void FadeInMusic()
    {
        targetMusicVolume = musicVolume;
        musicAudioSource.volume = 0f;
        isFadingMusic = true;
        musicFadeTimer = 0f;
    }

    /// <summary>
    /// Fade out music
    /// </summary>
    private void FadeOutMusic(System.Action onComplete = null)
    {
        targetMusicVolume = 0f;
        isFadingMusic = true;
        musicFadeTimer = 0f;
        StartCoroutine(FadeOutCoroutine(onComplete));
    }

    private System.Collections.IEnumerator FadeOutCoroutine(System.Action onComplete)
    {
        float elapsedTime = 0f;
        float startVolume = musicAudioSource.volume;

        while (elapsedTime < musicFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / musicFadeDuration);
            yield return null;
        }

        musicAudioSource.volume = 0f;
        isFadingMusic = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Update music fade
    /// </summary>
    private void UpdateMusicFade()
    {
        if (!isFadingMusic)
            return;

        musicFadeTimer += Time.deltaTime;

        if (musicFadeTimer >= musicFadeDuration)
        {
            musicAudioSource.volume = targetMusicVolume * masterVolume;
            isFadingMusic = false;
        }
        else
        {
            float t = musicFadeTimer / musicFadeDuration;
            musicAudioSource.volume = Mathf.Lerp(musicAudioSource.volume, targetMusicVolume * masterVolume, t);
        }
    }

    /// <summary>
    /// Play sound effect
    /// </summary>
    public void PlaySFX(AudioClip sfxClip, float volumeScale = 1f, float pitch = 1f)
    {
        if (sfxClip == null)
        {
            Debug.LogWarning("[AudioManager] Attempted to play null SFX clip");
            return;
        }

        SFXRequest request = new SFXRequest
        {
            clip = sfxClip,
            volume = volumeScale,
            pitch = pitch
        };

        sfxQueue.Enqueue(request);
    }

    /// <summary>
    /// Process SFX queue
    /// </summary>
    private void ProcessSFXQueue()
    {
        if (sfxQueue.Count == 0)
            return;

        // Find available SFX source
        AudioSource availableSource = null;
        for (int i = 0; i < sfxAudioSources.Length; i++)
        {
            if (!sfxAudioSources[i].isPlaying)
            {
                availableSource = sfxAudioSources[i];
                currentSFXSourceIndex = i;
                break;
            }
        }

        if (availableSource == null)
        {
            // All sources busy, will retry next frame
            return;
        }

        SFXRequest request = sfxQueue.Dequeue();
        availableSource.clip = request.clip;
        availableSource.volume = request.volume * sfxVolume * masterVolume;
        availableSource.pitch = request.pitch;
        availableSource.Play();

        OnSFXPlayed?.Invoke(request.clip.name);
        Debug.Log($"[SFX] Playing: {request.clip.name} (volume: {availableSource.volume:F2})");
    }

    /// <summary>
    /// Play voice/announcer line
    /// </summary>
    public void PlayVoice(AudioClip voiceClip, float volumeScale = 1f)
    {
        if (voiceClip == null)
        {
            Debug.LogWarning("[AudioManager] Attempted to play null voice clip");
            return;
        }

        voiceAudioSource.clip = voiceClip;
        voiceAudioSource.volume = volumeScale * voiceVolume * masterVolume;
        voiceAudioSource.Play();

        Debug.Log($"[VOICE] Playing: {voiceClip.name}");
    }

    /// <summary>
    /// Stop voice playback
    /// </summary>
    public void StopVoice()
    {
        voiceAudioSource.Stop();
    }

    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        OnVolumeChanged?.Invoke("Master", masterVolume);
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        OnVolumeChanged?.Invoke("Music", musicVolume);
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        OnVolumeChanged?.Invoke("SFX", sfxVolume);
    }

    /// <summary>
    /// Set voice volume
    /// </summary>
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        OnVolumeChanged?.Invoke("Voice", voiceVolume);
    }

    /// <summary>
    /// Update all audio source volumes
    /// </summary>
    private void UpdateAllVolumes()
    {
        musicAudioSource.volume = musicVolume * masterVolume;
        ambientAudioSource.volume = musicVolume * masterVolume;
        voiceAudioSource.volume = voiceVolume * masterVolume;

        foreach (var source in sfxAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.volume = sfxVolume * masterVolume;
            }
        }
    }

    // Getters
    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public float GetVoiceVolume() => voiceVolume;
    public AudioClip GetCurrentMusic() => currentMusicClip;
    public bool IsMusicPlaying() => musicAudioSource.isPlaying;
}
