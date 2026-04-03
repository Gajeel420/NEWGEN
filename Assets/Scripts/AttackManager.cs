using UnityEngine;

/// <summary>
/// Manages attack execution, startup/active/recovery frames, and attack cancellation.
/// Integrates with ComboDetector for combo damage scaling and attack linking.
/// Works with AnimationController for frame-perfect animation timing.
/// Attach this to the character GameObject.
/// </summary>
public class AttackManager : MonoBehaviour
{
    private Character character;
    private CommandBuffer commandBuffer;
    private ComboDetector comboDetector;
    private AnimationController animationController;
    private Hitbox[] hitboxes;

    [SerializeField] private float attackStartupBuffer = 0.1f; // Time before hitbox activates
    [SerializeField] private float attackActiveTime = 0.2f; // How long hitbox stays active
    [SerializeField] private float attackRecoveryTime = 0.3f; // Cooldown after attack
    [SerializeField] private float comboLinkCancelWindow = 0.1f; // Can cancel into next combo during this time in recovery
    [SerializeField] private bool useAnimationTiming = true; // Use animation frames instead of timer

    private bool isAttacking = false;
    private float attackTimeRemaining = 0f;
    private string currentAttackType = "";
    private float comboScalar = 1f;
    private float normalizedAnimationTime = 0f;

    void Awake()
    {
        character = GetComponent<Character>();
        commandBuffer = GetComponent<CommandBuffer>();
        comboDetector = GetComponent<ComboDetector>();
        animationController = GetComponent<AnimationController>();
        hitboxes = GetComponentsInChildren<Hitbox>();
    }

    void Update()
    {
        UpdateAttackTiming();
        PollForAttacks();
    }

    /// <summary>
    /// Execute an attack if not already attacking or in hitstun.
    /// Can link into next combo if within the cancel window.
    /// </summary>
    public bool TryAttack(string attackType)
    {
        // Can cancel into next move if in recovery phase of previous attack
        bool canComboCancel = isAttacking && attackTimeRemaining <= comboLinkCancelWindow;

        if (!canComboCancel && (isAttacking || character.IsInHitstun() || !character.IsAlive()))
            return false;

        // Record input for combo detection
        if (comboDetector != null)
        {
            comboDetector.RecordInput(attackType);
            comboScalar = comboDetector.GetComboScalar();
        }
        else
        {
            comboScalar = 1f;
        }

        currentAttackType = attackType;
        isAttacking = true;
        attackTimeRemaining = attackStartupBuffer + attackActiveTime + attackRecoveryTime;

        // Deactivate all hitboxes initially
        foreach (var hitbox in hitboxes)
            hitbox.Deactivate();

        character.SetState(Character.CharacterState.Attacking);

        // Trigger animation if AnimationController is available
        if (animationController != null)
        {
            int attackTypeHash = GetAttackTypeHash(attackType);
            animationController.PlayAttackAnimation(attackTypeHash);
        }

        return true;
    }

    private void UpdateAttackTiming()
    {
        if (!isAttacking) return;

        attackTimeRemaining -= Time.deltaTime;

        // Activate hitbox after startup frames
        float startupFrames = attackStartupBuffer;
        float activeStartTime = attackTimeRemaining - (attackActiveTime + attackRecoveryTime);

        if (activeStartTime < startupFrames)
        {
            ActivateHitboxesForAttack(currentAttackType);
        }
        else if (attackTimeRemaining <= attackRecoveryTime)
        {
            // Recover - deactivate hitboxes
            foreach (var hitbox in hitboxes)
                hitbox.Deactivate();
        }

        // End attack
        if (attackTimeRemaining <= 0f)
        {
            EndAttack();
        }
    }

    private void ActivateHitboxesForAttack(string attackType)
    {
        foreach (var hitbox in hitboxes)
        {
            if (hitbox.GetAttackType().ToLower() == attackType.ToLower())
            {
                // Apply combo scaling to damage and knockback
                float baseDamage = hitbox.GetDamage();
                float baseKnockback = hitbox.GetKnockbackForce();

                hitbox.SetDamage(baseDamage * comboScalar);
                hitbox.SetKnockbackForce(baseKnockback * comboScalar);

                hitbox.Activate();
            }
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
        attackTimeRemaining = 0f;
        currentAttackType = "";

        // Deactivate all hitboxes
        foreach (var hitbox in hitboxes)
            hitbox.Deactivate();

        character.SetState(Character.CharacterState.Idle);
    }

    private void PollForAttacks()
    {
        if (isAttacking || character.IsInHitstun()) return;

        // Check for buffered inputs from InputManager
        if (InputManager.Instance.ConsumeBuffered("Light"))
            TryAttack("Light");
        else if (InputManager.Instance.ConsumeBuffered("Medium"))
            TryAttack("Medium");
        else if (InputManager.Instance.ConsumeBuffered("Heavy"))
            TryAttack("Heavy");
    }

    public bool IsAttacking() => isAttacking;
    public float GetAttackTimeRemaining() => attackTimeRemaining;
    public string GetCurrentAttackType() => currentAttackType;
    public float GetComboScalar() => comboScalar;

    /// <summary>
    /// Get the current combo hit count (if active).
    /// </summary>
    public int GetComboCount()
    {
        return comboDetector != null ? comboDetector.GetComboCount() : 0;
    }

    /// <summary>
    /// Cancel the current attack (for combo links).
    /// </summary>
    public void CancelAttack()
    {
        if (isAttacking)
        {
            EndAttack();
        }
    }

    /// <summary>
    /// Reset combo state (called when combat ends or new opponent encountered).
    /// </summary>
    public void ResetCombo()
    {
        if (comboDetector != null)
            comboDetector.ResetCombo();
        comboScalar = 1f;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        float alpha = isAttacking ? 1f : 0.3f;
        Gizmos.color = new Color(1f, 0.5f, 0f, alpha);

        // Draw attack state
        Vector3 pos = transform.position + Vector3.up * 1.5f;
        Debug.Log($"AttackManager: isAttacking={isAttacking}, timeRemaining={attackTimeRemaining:F2}");
    }

    /// <summary>
    /// Converts attack type string to animator parameter hash
    /// </summary>
    private int GetAttackTypeHash(string attackType)
    {
        return attackType.ToLower() switch
        {
            "light" => 0,
            "medium" => 1,
            "heavy" => 2,
            _ => 0
        };
    }

    /// <summary>
    /// Gets normalized animation time from AnimationController if available
    /// Falls back to timer-based calculation
    /// </summary>
    private float GetNormalizedAttackTime()
    {
        if (useAnimationTiming && animationController != null)
        {
            return animationController.GetNormalizedAnimationTime();
        }
        else
        {
            // Calculate normalized time from timer
            float totalAttackTime = attackStartupBuffer + attackActiveTime + attackRecoveryTime;
            float elapsedTime = totalAttackTime - attackTimeRemaining;
            return Mathf.Clamp01(elapsedTime / totalAttackTime);
        }
    }
}
