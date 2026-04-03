using UnityEngine;

/// <summary>
/// Manages audio settings and persistence.
/// Saves and loads volume preferences from PlayerPrefs.
/// </summary>
public class AudioSettings : MonoBehaviour
{
    private const string MASTER_VOLUME_KEY = "AudioSettings_MasterVolume";
    private const string MUSIC_VOLUME_KEY = "AudioSettings_MusicVolume";
    private const string SFX_VOLUME_KEY = "AudioSettings_SFXVolume";
    private const string VOICE_VOLUME_KEY = "AudioSettings_VoiceVolume";
    private const string MUTED_KEY = "AudioSettings_Muted";

    [SerializeField] private float defaultMasterVolume = 1f;
    [SerializeField] private float defaultMusicVolume = 0.7f;
    [SerializeField] private float defaultSFXVolume = 1f;
    [SerializeField] private float defaultVoiceVolume = 1f;

    private bool isMuted = false;

    // Events
    public delegate void SettingsChangedDelegate();
    public event SettingsChangedDelegate OnSettingsChanged;

    public delegate void MutedChangedDelegate(bool isMuted);
    public event MutedChangedDelegate OnMutedChanged;

    public static AudioSettings Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    /// <summary>
    /// Load settings from PlayerPrefs
    /// </summary>
    public void LoadSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, defaultMasterVolume);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, defaultMusicVolume);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
        float voiceVolume = PlayerPrefs.GetFloat(VOICE_VOLUME_KEY, defaultVoiceVolume);
        isMuted = PlayerPrefs.GetInt(MUTED_KEY, 0) == 1;

        ApplySettings(masterVolume, musicVolume, sfxVolume, voiceVolume);
        Debug.Log("[AudioSettings] Settings loaded from PlayerPrefs");
    }

    /// <summary>
    /// Save settings to PlayerPrefs
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, AudioManager.Instance.GetMasterVolume());
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, AudioManager.Instance.GetMusicVolume());
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, AudioManager.Instance.GetSFXVolume());
        PlayerPrefs.SetFloat(VOICE_VOLUME_KEY, AudioManager.Instance.GetVoiceVolume());
        PlayerPrefs.SetInt(MUTED_KEY, isMuted ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("[AudioSettings] Settings saved to PlayerPrefs");
    }

    /// <summary>
    /// Apply volume settings
    /// </summary>
    private void ApplySettings(float masterVolume, float musicVolume, float sfxVolume, float voiceVolume)
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.SetMasterVolume(masterVolume);
        AudioManager.Instance.SetMusicVolume(musicVolume);
        AudioManager.Instance.SetSFXVolume(sfxVolume);
        AudioManager.Instance.SetVoiceVolume(voiceVolume);

        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(volume);
            OnSettingsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
            OnSettingsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
            OnSettingsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Set voice volume
    /// </summary>
    public void SetVoiceVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVoiceVolume(volume);
            OnSettingsChanged?.Invoke();
        }
    }

    /// <summary>
    /// Toggle mute
    /// </summary>
    public void ToggleMute()
    {
        SetMute(!isMuted);
    }

    /// <summary>
    /// Set mute state
    /// </summary>
    public void SetMute(bool mute)
    {
        isMuted = mute;

        if (AudioManager.Instance != null)
        {
            if (isMuted)
            {
                AudioManager.Instance.SetMasterVolume(0f);
            }
            else
            {
                AudioManager.Instance.SetMasterVolume(PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, defaultMasterVolume));
            }
        }

        OnMutedChanged?.Invoke(isMuted);
        Debug.Log($"[AudioSettings] Master mute: {isMuted}");
    }

    /// <summary>
    /// Reset to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(MASTER_VOLUME_KEY);
        PlayerPrefs.DeleteKey(MUSIC_VOLUME_KEY);
        PlayerPrefs.DeleteKey(SFX_VOLUME_KEY);
        PlayerPrefs.DeleteKey(VOICE_VOLUME_KEY);
        PlayerPrefs.DeleteKey(MUTED_KEY);
        PlayerPrefs.Save();

        LoadSettings();
        Debug.Log("[AudioSettings] Reset to default values");
    }

    public bool IsMuted() => isMuted;
    public float GetMasterVolume() => AudioManager.Instance.GetMasterVolume();
    public float GetMusicVolume() => AudioManager.Instance.GetMusicVolume();
    public float GetSFXVolume() => AudioManager.Instance.GetSFXVolume();
    public float GetVoiceVolume() => AudioManager.Instance.GetVoiceVolume();
}
