using UnityEngine;

/// <summary>
/// Manages music tracks and transitions.
/// Handles fight music, menu music, and music intensity levels.
/// </summary>
public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public struct MusicTrack
    {
        public string name;
        public AudioClip clip;
        public float intensity; // 0-1 for intensity level
    }

    [SerializeField] private MusicTrack[] menuMusicTracks;
    [SerializeField] private MusicTrack[] fightMusicTracks;
    [SerializeField] private MusicTrack[] victoryMusicTrack;
    [SerializeField] private MusicTrack[] defeatMusicTrack;

    [SerializeField] private bool useRandomTrack = true;
    [SerializeField] private float transitionDuration = 2f;

    private MusicTrack currentTrack;
    private MusicTrack previousTrack;

    public delegate void MusicTrackChangedDelegate(string trackName, float intensity);
    public event MusicTrackChangedDelegate OnMusicTrackChanged;

    private void OnEnable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGameStateChanged += HandleGameStateChange;
            GameFlowManager.Instance.OnMatchEnd += HandleMatchEnd;
        }
    }

    private void OnDisable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGameStateChanged -= HandleGameStateChange;
            GameFlowManager.Instance.OnMatchEnd -= HandleMatchEnd;
        }
    }

    /// <summary>
    /// Handle game state changes
    /// </summary>
    private void HandleGameStateChange(GameFlowManager.GameState newState)
    {
        switch (newState)
        {
            case GameFlowManager.GameState.Menu:
                PlayMenuMusic();
                break;

            case GameFlowManager.GameState.Fighting:
                PlayFightMusic();
                break;

            case GameFlowManager.GameState.Paused:
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.musicAudioSource.Pause();
                }
                break;

            case GameFlowManager.GameState.RoundEnd:
                // Continue with current music or transition
                break;
        }
    }

    /// <summary>
    /// Handle match end
    /// </summary>
    private void HandleMatchEnd(Character winner)
    {
        if (winner != null)
        {
            PlayVictoryMusic();
        }
        else
        {
            PlayDefeatMusic();
        }
    }

    /// <summary>
    /// Play menu music
    /// </summary>
    public void PlayMenuMusic()
    {
        if (menuMusicTracks.Length == 0)
        {
            Debug.LogWarning("[MusicManager] No menu music tracks assigned");
            return;
        }

        MusicTrack track = useRandomTrack
            ? menuMusicTracks[Random.Range(0, menuMusicTracks.Length)]
            : menuMusicTracks[0];

        PlayTrack(track);
    }

    /// <summary>
    /// Play fight music
    /// </summary>
    public void PlayFightMusic()
    {
        if (fightMusicTracks.Length == 0)
        {
            Debug.LogWarning("[MusicManager] No fight music tracks assigned");
            return;
        }

        MusicTrack track = useRandomTrack
            ? fightMusicTracks[Random.Range(0, fightMusicTracks.Length)]
            : fightMusicTracks[0];

        PlayTrack(track);
    }

    /// <summary>
    /// Play victory music
    /// </summary>
    public void PlayVictoryMusic()
    {
        if (victoryMusicTrack.Length == 0)
        {
            Debug.LogWarning("[MusicManager] No victory music tracks assigned");
            return;
        }

        MusicTrack track = victoryMusicTrack[0];
        PlayTrack(track);
    }

    /// <summary>
    /// Play defeat music
    /// </summary>
    public void PlayDefeatMusic()
    {
        if (defeatMusicTrack.Length == 0)
        {
            Debug.LogWarning("[MusicManager] No defeat music tracks assigned");
            return;
        }

        MusicTrack track = defeatMusicTrack[0];
        PlayTrack(track);
    }

    /// <summary>
    /// Play a specific music track
    /// </summary>
    private void PlayTrack(MusicTrack track)
    {
        if (track.clip == null)
        {
            Debug.LogWarning("[MusicManager] Attempted to play null music clip");
            return;
        }

        previousTrack = currentTrack;
        currentTrack = track;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(track.clip, fade: true);
        }

        OnMusicTrackChanged?.Invoke(track.name, track.intensity);
        Debug.Log($"[MUSIC] Playing track: {track.name} (Intensity: {track.intensity})");
    }

    /// <summary>
    /// Stop music
    /// </summary>
    public void StopMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(fade: true);
        }
    }

    public MusicTrack GetCurrentTrack() => currentTrack;
    public string GetCurrentTrackName() => currentTrack.name;
}
