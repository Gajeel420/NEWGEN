using UnityEngine;

/// <summary>
/// Integrates audio with game events.
/// Listens to hit, block, combo, and other events and triggers appropriate sounds.
/// </summary>
public class AudioEventIntegrator : MonoBehaviour
{
    [SerializeField] private SoundEffectsLibrary soundLibrary;
    [SerializeField] private bool enableAttackSounds = true;
    [SerializeField] private bool enableHitSounds = true;
    [SerializeField] private bool enableBlockSounds = true;
    [SerializeField] private bool enableComboSounds = true;
    [SerializeField] private bool enableKnockdownSounds = true;

    private Character character;
    private BlockingSystem blockingSystem;
    private AttackManager attackManager;
    private KnockdownRecoverySystem knockdownSystem;

    private void Start()
    {
        character = GetComponent<Character>();
        blockingSystem = GetComponent<BlockingSystem>();
        attackManager = GetComponent<AttackManager>();
        knockdownSystem = GetComponent<KnockdownRecoverySystem>();

        if (soundLibrary == null)
        {
            soundLibrary = Resources.Load<SoundEffectsLibrary>("SoundEffectsLibrary");
        }

        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// Subscribe to relevant events
    /// </summary>
    private void SubscribeToEvents()
    {
        if (blockingSystem != null)
        {
            blockingSystem.OnBlockStateChanged += HandleBlockStateChange;
            blockingSystem.OnBlockBroken += HandleBlockBroken;
        }

        if (knockdownSystem != null)
        {
            knockdownSystem.OnKnockedDown += HandleKnockedDown;
            knockdownSystem.OnRecovered += HandleRecovered;
        }
    }

    /// <summary>
    /// Unsubscribe from events
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (blockingSystem != null)
        {
            blockingSystem.OnBlockStateChanged -= HandleBlockStateChange;
            blockingSystem.OnBlockBroken -= HandleBlockBroken;
        }

        if (knockdownSystem != null)
        {
            knockdownSystem.OnKnockedDown -= HandleKnockedDown;
            knockdownSystem.OnRecovered -= HandleRecovered;
        }
    }

    /// <summary>
    /// Play attack sound when attack is executed
    /// </summary>
    public void PlayAttackSound(string attackType)
    {
        if (!enableAttackSounds || soundLibrary == null || AudioManager.Instance == null)
            return;

        AudioClip clip = soundLibrary.GetAttackSound(attackType);
        if (clip != null)
        {
            AudioManager.Instance.PlaySFX(clip, volumeScale: 0.8f);
        }
    }

    /// <summary>
    /// Play hit sound when attack connects
    /// </summary>
    public void PlayHitSound(int hitZone = 0, float volumeScale = 1f)
    {
        if (!enableHitSounds || soundLibrary == null || AudioManager.Instance == null)
            return;

        AudioClip clip = soundLibrary.GetHitSound(hitZone);
        if (clip != null)
        {
            float pitchVariation = Random.Range(0.95f, 1.05f);
            AudioManager.Instance.PlaySFX(clip, volumeScale, pitchVariation);
        }
    }

    /// <summary>
    /// Play block sound
    /// </summary>
    private void HandleBlockStateChange(bool isBlocking)
    {
        if (!enableBlockSounds || soundLibrary == null || AudioManager.Instance == null)
            return;

        if (isBlocking)
        {
            AudioClip clip = soundLibrary.GetBlockSound();
            if (clip != null)
            {
                AudioManager.Instance.PlaySFX(clip, volumeScale: 0.9f);
            }
        }
    }

    /// <summary>
    /// Play block break sound
    /// </summary>
    private void HandleBlockBroken()
    {
        if (!enableBlockSounds || soundLibrary == null || AudioManager.Instance == null)
            return;

        AudioClip clip = soundLibrary.GetImpactSound();
        if (clip != null)
        {
            AudioManager.Instance.PlaySFX(clip, volumeScale: 1.2f, pitch: 0.8f); // Lower pitch for impact
        }
    }

    /// <summary>
    /// Play knockdown sound
    /// </summary>
    private void HandleKnockedDown()
    {
        if (!enableKnockdownSounds || soundLibrary == null || AudioManager.Instance == null)
            return;

        AudioClip clip = soundLibrary.GetKnockdownSound();
        if (clip != null)
        {
            AudioManager.Instance.PlaySFX(clip, volumeScale: 1f);
        }
    }

    /// <summary>
    /// Play recovery sound (optional)
    /// </summary>
    private void HandleRecovered()
    {
        // Could play a recovery/getup sound here
    }

    /// <summary>
    /// Play special move sound
    /// </summary>
    public void PlaySpecialMoveSound(string moveName)
    {
        if (soundLibrary == null || AudioManager.Instance == null)
            return;

        AudioClip clip = soundLibrary.GetSpecialMoveSound(moveName);
        if (clip != null)
        {
            AudioManager.Instance.PlaySFX(clip, volumeScale: 1.1f);
        }
    }

    /// <summary>
    /// Play combo milestone sound
    /// </summary>
    public void PlayComboSound(int comboCount)
    {
        if (!enableComboSounds || soundLibrary == null || AudioManager.Instance == null)
            return;

        // Play different sounds for different combo milestones
        if (comboCount == 3 || comboCount == 5 || comboCount == 10)
        {
            AudioClip clip = soundLibrary.GetUISound("combo_hit");
            if (clip != null)
            {
                float pitch = 1f + (comboCount * 0.05f); // Higher pitch for more combos
                AudioManager.Instance.PlaySFX(clip, volumeScale: 0.9f, pitch: pitch);
            }
        }
    }

    /// <summary>
    /// Globally play UI sound
    /// </summary>
    public static void PlayUISound(string actionName)
    {
        if (AudioManager.Instance == null)
            return;

        // Try to get from SoundEffectsLibrary
        SoundEffectsLibrary library = Resources.Load<SoundEffectsLibrary>("SoundEffectsLibrary");
        if (library != null)
        {
            AudioClip clip = library.GetUISound(actionName);
            if (clip != null)
            {
                AudioManager.Instance.PlaySFX(clip, volumeScale: 0.8f);
            }
        }
    }
}
