using UnityEngine;

/// <summary>
/// Manages blocking mechanics including block damage reduction, stamina/durability, and block break.
/// Attached to character GameObject.
/// </summary>
public class BlockingSystem : MonoBehaviour
{
    private Character character;
    private CharacterStats stats;

    [SerializeField] private float blockDamageReduction = 0.35f; // 35% of damage passes through
    [SerializeField] private float blockStunReduction = 0.5f; // Hitstun reduced by 50% when blocking
    [SerializeField] private float baseBlockStamina = 100f; // Max block stamina/durability
    [SerializeField] private float blockStaminaRecoveryRate = 20f; // Stamina recovered per second when not blocking
    [SerializeField] private float blockStaminaRecoveryDelay = 1f; // Delay before stamina recovery starts

    private bool isBlocking = false;
    private float currentBlockStamina;
    private float blockStaminaRecoveryTimer = 0f;
    private bool isBlockBroken = false;
    private float blockBrokenDuration = 0.5f;
    private float blockBrokenTimer = 0f;

    // Knockback direction while blocking (usually less than normal)
    private float blockKnockbackForce = 0.3f;

    // Events
    public delegate void BlockStateChangeDelegate(bool isBlocking);
    public event BlockStateChangeDelegate OnBlockStateChanged;

    public delegate void BlockStaminaChangeDelegate(float stamina, float maxStamina);
    public event BlockStaminaChangeDelegate OnBlockStaminaChanged;

    public delegate void BlockBrokenDelegate();
    public event BlockBrokenDelegate OnBlockBroken;

    private void Awake()
    {
        character = GetComponent<Character>();
        stats = GetComponent<CharacterStats>();
        currentBlockStamina = baseBlockStamina;
    }

    private void Update()
    {
        UpdateBlockStamina();
        UpdateBlockBroken();
        HandleBlockInput();
    }

    /// <summary>
    /// Update block stamina recovery when not blocking
    /// </summary>
    private void UpdateBlockStamina()
    {
        if (!isBlocking && !isBlockBroken)
        {
            blockStaminaRecoveryTimer -= Time.deltaTime;

            if (blockStaminaRecoveryTimer <= 0f)
            {
                currentBlockStamina = Mathf.Min(currentBlockStamina + blockStaminaRecoveryRate * Time.deltaTime, baseBlockStamina);
                OnBlockStaminaChanged?.Invoke(currentBlockStamina, baseBlockStamina);
            }
        }
    }

    /// <summary>
    /// Update block broken state and recovery
    /// </summary>
    private void UpdateBlockBroken()
    {
        if (isBlockBroken)
        {
            blockBrokenTimer += Time.deltaTime;
            if (blockBrokenTimer >= blockBrokenDuration)
            {
                isBlockBroken = false;
                blockBrokenTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Handle block input
    /// </summary>
    private void HandleBlockInput()
    {
        // TODO: Wire to InputManager when "Block" action is added
        // For now, check if player holds a button
        bool wantToBlock = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (wantToBlock && !isBlockBroken && currentBlockStamina > 0)
        {
            SetBlocking(true);
        }
        else
        {
            SetBlocking(false);
        }
    }

    /// <summary>
    /// Set blocking state
    /// </summary>
    public void SetBlocking(bool newBlockingState)
    {
        if (isBlocking == newBlockingState)
            return;

        isBlocking = newBlockingState;

        if (isBlocking)
        {
            // Transition to idle state while blocking (prevents attacking)
            character.SetState(Character.CharacterState.Idle);
            blockStaminaRecoveryTimer = blockStaminaRecoveryDelay;
        }

        OnBlockStateChanged?.Invoke(isBlocking);
    }

    /// <summary>
    /// Apply blocked damage (reduced) to character
    /// </summary>
    public void ApplyBlockedDamage(float damage, Vector2 knockbackForce, float hitstun)
    {
        if (!isBlocking || isBlockBroken)
            return;

        // Reduce stamina based on damage (50% of damage goes to stamina drain)
        float staminaDrain = damage * 0.5f;
        currentBlockStamina -= staminaDrain;
        blockStaminaRecoveryTimer = blockStaminaRecoveryDelay;
        OnBlockStaminaChanged?.Invoke(currentBlockStamina, baseBlockStamina);

        // Check if block is broken
        if (currentBlockStamina <= 0)
        {
            BreakBlock(damage, knockbackForce, hitstun);
            return;
        }

        // Apply reduced damage
        float reducedDamage = damage * blockDamageReduction;
        float reducedKnockback = knockbackForce.magnitude * blockKnockbackForce;
        float reducedHitstun = hitstun * blockStunReduction;

        character.TakeDamage(
            reducedDamage,
            new Vector2(Mathf.Sign(knockbackForce.x) * reducedKnockback, knockbackForce.y * 0.5f),
            reducedHitstun
        );
    }

    /// <summary>
    /// Break the block (too much damage, stamina depleted)
    /// </summary>
    private void BreakBlock(float damage, Vector2 knockbackForce, float hitstun)
    {
        isBlockBroken = true;
        blockBrokenTimer = 0f;
        SetBlocking(false);

        // Full damage applied when block breaks
        character.TakeDamage(damage, knockbackForce, hitstun * 1.5f); // Extra hitstun for break

        OnBlockBroken?.Invoke();
    }

    /// <summary>
    /// Check if character is currently blocking
    /// </summary>
    public bool IsBlocking() => isBlocking && !isBlockBroken;

    /// <summary>
    /// Check if block is broken
    /// </summary>
    public bool IsBlockBroken() => isBlockBroken;

    /// <summary>
    /// Get remaining block stamina
    /// </summary>
    public float GetBlockStamina() => currentBlockStamina;

    /// <summary>
    /// Get block stamina percentage
    /// </summary>
    public float GetBlockStaminaPercent() => currentBlockStamina / baseBlockStamina;

    /// <summary>
    /// Reset block stamina (for new round)
    /// </summary>
    public void ResetBlockStamina()
    {
        currentBlockStamina = baseBlockStamina;
        isBlockBroken = false;
        blockBrokenTimer = 0f;
        SetBlocking(false);
    }
}
