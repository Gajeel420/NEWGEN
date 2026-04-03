using UnityEngine;

/// <summary>
/// Manages knockdown state and recovery mechanics.
/// Handles when a character is knocked down and can recover.
/// </summary>
public class KnockdownRecoverySystem : MonoBehaviour
{
    private Character character;

    [SerializeField] private float knockdownDuration = 1.5f; // How long to stay knocked down
    [SerializeField] private float recoveryStartupTime = 0.3f; // Time before can stand up
    [SerializeField] private float recoveryAnimationDuration = 0.8f; // How long recovery animation plays

    [SerializeField] private bool allowWakeupInvulnerability = true; // Invincible for a short time after waking up
    [SerializeField] private float wakeupInvulnerabilityDuration = 0.5f;

    private bool isKnockedDown = false;
    private float knockdownTimer = 0f;
    private float wakeupInvulnerabilityTimer = 0f;

    // Events
    public delegate void KnockdownDelegate();
    public event KnockdownDelegate OnKnockedDown;

    public delegate void RecoveringDelegate();
    public event RecoveringDelegate OnRecovering;

    public delegate void RecoveredDelegate();
    public event RecoveredDelegate OnRecovered;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Update()
    {
        UpdateKnockdown();
        UpdateWakeupInvulnerability();
    }

    /// <summary>
    /// Update knockdown state
    /// </summary>
    private void UpdateKnockdown()
    {
        if (!isKnockedDown)
            return;

        knockdownTimer -= Time.deltaTime;

        if (knockdownTimer <= 0f)
        {
            RecoverFromKnockdown();
        }
    }

    /// <summary>
    /// Update wakeup invulnerability
    /// </summary>
    private void UpdateWakeupInvulnerability()
    {
        if (wakeupInvulnerabilityTimer > 0)
        {
            wakeupInvulnerabilityTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Apply knockdown to character
    /// </summary>
    public void KnockDown()
    {
        if (isKnockedDown)
            return;

        isKnockedDown = true;
        knockdownTimer = knockdownDuration;
        wakeupInvulnerabilityTimer = wakeupInvulnerabilityDuration;

        character.SetState(Character.CharacterState.KnockedDown);

        OnKnockedDown?.Invoke();

        Debug.Log($"{gameObject.name} knocked down!");
    }

    /// <summary>
    /// Recover from knockdown
    /// </summary>
    private void RecoverFromKnockdown()
    {
        if (!isKnockedDown)
            return;

        isKnockedDown = false;
        knockdownTimer = 0f;

        character.SetState(Character.CharacterState.Idle);

        OnRecovered?.Invoke();

        Debug.Log($"{gameObject.name} recovered from knockdown!");
    }

    /// <summary>
    /// Check if character is knocked down
    /// </summary>
    public bool IsKnockedDown() => isKnockedDown;

    /// <summary>
    /// Get remaining knockdown time
    /// </summary>
    public float GetKnockdownTimeRemaining() => Mathf.Max(0, knockdownTimer);

    /// <summary>
    /// Check if character has wakeup invulnerability active
    /// </summary>
    public bool HasWakeupInvulnerability() => wakeupInvulnerabilityTimer > 0;

    /// <summary>
    /// Get wakeup invulnerability remaining time
    /// </summary>
    public float GetWakeupInvulnerabilityRemaining() => Mathf.Max(0, wakeupInvulnerabilityTimer);

    /// <summary>
    /// Force recovery (for debugging or special situations)
    /// </summary>
    public void ForceRecovery()
    {
        knockdownTimer = 0f;
        RecoverFromKnockdown();
    }

    /// <summary>
    /// Reset knockdown system (for new round)
    /// </summary>
    public void ResetKnockdown()
    {
        isKnockedDown = false;
        knockdownTimer = 0f;
        wakeupInvulnerabilityTimer = 0f;
    }
}
