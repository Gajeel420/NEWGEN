using UnityEngine;

/// <summary>
/// Manages attack execution, startup/active/recovery frames, and attack cancellation.
/// Attach this to the character GameObject.
/// </summary>
public class AttackManager : MonoBehaviour
{
    private Character character;
    private CommandBuffer commandBuffer;
    private Hitbox[] hitboxes;

    [SerializeField] private float attackStartupBuffer = 0.1f; // Time before hitbox activates
    [SerializeField] private float attackActiveTime = 0.2f; // How long hitbox stays active
    [SerializeField] private float attackRecoveryTime = 0.3f; // Cooldown after attack

    private bool isAttacking = false;
    private float attackTimeRemaining = 0f;
    private string currentAttackType = "";

    void Awake()
    {
        character = GetComponent<Character>();
        commandBuffer = GetComponent<CommandBuffer>();
        hitboxes = GetComponentsInChildren<Hitbox>();
    }

    void Update()
    {
        UpdateAttackTiming();
        PollForAttacks();
    }

    /// <summary>
    /// Execute an attack if not already attacking or in hitstun.
    /// </summary>
    public bool TryAttack(string attackType)
    {
        if (isAttacking || character.IsInHitstun() || !character.IsAlive())
            return false;

        currentAttackType = attackType;
        isAttacking = true;
        attackTimeRemaining = attackStartupBuffer + attackActiveTime + attackRecoveryTime;

        // Deactivate all hitboxes initially
        foreach (var hitbox in hitboxes)
            hitbox.Deactivate();

        character.SetState(Character.CharacterState.Attacking);
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

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        float alpha = isAttacking ? 1f : 0.3f;
        Gizmos.color = new Color(1f, 0.5f, 0f, alpha);

        // Draw attack state
        Vector3 pos = transform.position + Vector3.up * 1.5f;
        Debug.Log($"AttackManager: isAttacking={isAttacking}, timeRemaining={attackTimeRemaining:F2}");
    }
}
