using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages voice announcements and commentary.
/// Plays announcer lines for round starts, victories, and other events.
/// </summary>
public class VoiceAnnouncerSystem : MonoBehaviour
{
    [System.Serializable]
    public struct AnnouncerClip
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [SerializeField] private AnnouncerClip[] roundStartClips;
    [SerializeField] private AnnouncerClip[] fightStartClip;
    [SerializeField] private AnnouncerClip[] countdownClips;
    [SerializeField] private AnnouncerClip[] victoryClips;
    [SerializeField] private AnnouncerClip[] defeatClips;
    [SerializeField] private AnnouncerClip[] knockdownClips;
    [SerializeField] private AnnouncerClip[] koClips;

    [SerializeField] private float delayBetweenAnnouncements = 0.5f;

    private float announcementCooldown = 0f;
    private Queue<AnnouncerClip> announcementQueue = new Queue<AnnouncerClip>();

    public delegate void AnnouncementPlayedDelegate(string announcementName);
    public event AnnouncementPlayedDelegate OnAnnouncementPlayed;

    private void OnEnable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnRoundStart += HandleRoundStart;
            GameFlowManager.Instance.OnRoundEnd += HandleRoundEnd;
            GameFlowManager.Instance.OnMatchEnd += HandleMatchEnd;
        }
    }

    private void OnDisable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnRoundStart -= HandleRoundStart;
            GameFlowManager.Instance.OnRoundEnd -= HandleRoundEnd;
            GameFlowManager.Instance.OnMatchEnd -= HandleMatchEnd;
        }
    }

    private void Update()
    {
        UpdateAnnouncementCooldown();
        ProcessAnnouncementQueue();
    }

    /// <summary>
    /// Update announcement cooldown
    /// </summary>
    private void UpdateAnnouncementCooldown()
    {
        if (announcementCooldown > 0)
        {
            announcementCooldown -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Process announcement queue
    /// </summary>
    private void ProcessAnnouncementQueue()
    {
        if (announcementQueue.Count == 0 || announcementCooldown > 0)
            return;

        AnnouncerClip clip = announcementQueue.Dequeue();
        PlayAnnouncement(clip);
    }

    /// <summary>
    /// Handle round start
    /// </summary>
    private void HandleRoundStart(int roundNumber)
    {
        // Queue round announcement
        if (roundNumber == 1)
        {
            QueueAnnouncement("get_ready");
        }

        QueueAnnouncement(GetRoundAnnouncementName(roundNumber));
        QueueAnnouncement("fight");
    }

    /// <summary>
    /// Handle round end
    /// </summary>
    private void HandleRoundEnd(Character roundWinner)
    {
        if (roundWinner != null)
        {
            QueueAnnouncement("round_win");
        }
    }

    /// <summary>
    /// Handle match end
    /// </summary>
    private void HandleMatchEnd(Character matchWinner)
    {
        if (matchWinner != null)
        {
            QueueAnnouncement("victory");
        }
        else
        {
            QueueAnnouncement("draw");
        }
    }

    /// <summary>
    /// Get round announcement name
    /// </summary>
    private string GetRoundAnnouncementName(int roundNumber)
    {
        return roundNumber switch
        {
            1 => "round_1",
            2 => "round_2",
            3 => "round_3",
            4 => "round_4",
            5 => "round_5",
            _ => "round_start"
        };
    }

    /// <summary>
    /// Queue an announcement
    /// </summary>
    private void QueueAnnouncement(string announcementName)
    {
        AnnouncerClip clip = FindClipByName(announcementName);
        if (clip.clip != null)
        {
            announcementQueue.Enqueue(clip);
        }
    }

    /// <summary>
    /// Play an announcement immediately
    /// </summary>
    private void PlayAnnouncement(AnnouncerClip clip)
    {
        if (clip.clip == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlayVoice(clip.clip, clip.volume);
        announcementCooldown = delayBetweenAnnouncements;

        OnAnnouncementPlayed?.Invoke(clip.name);
        Debug.Log($"[ANNOUNCER] {clip.name}");
    }

    /// <summary>
    /// Find announcer clip by name
    /// </summary>
    private AnnouncerClip FindClipByName(string name)
    {
        AnnouncerClip[] allClips = System.Array.CreateInstance(typeof(AnnouncerClip), 0) as AnnouncerClip[];

        // Search in all arrays
        AnnouncerClip clip = SearchArray(roundStartClips, name);
        if (clip.clip != null) return clip;

        clip = SearchArray(fightStartClip, name);
        if (clip.clip != null) return clip;

        clip = SearchArray(countdownClips, name);
        if (clip.clip != null) return clip;

        clip = SearchArray(victoryClips, name);
        if (clip.clip != null) return clip;

        clip = SearchArray(defeatClips, name);
        if (clip.clip != null) return clip;

        clip = SearchArray(knockdownClips, name);
        if (clip.clip != null) return clip;

        clip = SearchArray(koClips, name);
        if (clip.clip != null) return clip;

        Debug.LogWarning($"[VoiceAnnouncerSystem] Announcement '{name}' not found");
        return new AnnouncerClip();
    }

    /// <summary>
    /// Search array for clip by name
    /// </summary>
    private AnnouncerClip SearchArray(AnnouncerClip[] array, string name)
    {
        if (array == null)
            return new AnnouncerClip();

        foreach (var clip in array)
        {
            if (clip.name == name)
                return clip;
        }

        return new AnnouncerClip();
    }

    /// <summary>
    /// Play countdown
    /// </summary>
    public void PlayCountdown(int count)
    {
        QueueAnnouncement($"countdown_{count}");
    }

    /// <summary>
    /// Play knockdown announcement
    /// </summary>
    public void PlayKnockdown()
    {
        QueueAnnouncement("knockdown");
    }

    /// <summary>
    /// Play KO announcement
    /// </summary>
    public void PlayKO()
    {
        QueueAnnouncement("ko");
    }

    /// <summary>
    /// Clear announcement queue
    /// </summary>
    public void ClearQueue()
    {
        announcementQueue.Clear();
    }
}
