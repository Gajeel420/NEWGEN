using UnityEngine;

/// <summary>
/// Base Character class with state machine, health, and physics.
/// All playable and AI characters inherit from this.
/// </summary>
public class Character : MonoBehaviour
{
    // State machine
    public enum CharacterState { Idle, Moving, Attacking, GettingHit, KnockedDown, KO }
    [SerializeField] private CharacterState currentState = CharacterState.Idle;

    // Components
    protected CharacterStats stats;
    protected CharacterController characterController;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;

    // Health
    [SerializeField] protected float currentHealth;
    protected bool isAlive = true;

    // Physics
    [SerializeField] protected float gravityScale = 1f;
    [SerializeField] protected float groundDragMultiplier = 0.95f;

    // Facing direction
    protected int facingDirection = 1; // 1 = right, -1 = left

    // Hitstun/Knockback
    protected float hitstunTimeRemaining = 0f;
    protected Vector2 knockbackVelocity = Vector2.zero;

    // Events
    public delegate void HealthChangeDelegate(float currentHealth, float maxHealth);
    public event HealthChangeDelegate OnHealthChanged;
    public delegate void DeathDelegate();
    public event DeathDelegate OnDeath;

    protected virtual void Awake()
    {
        stats = GetComponent<CharacterStats>();
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (stats != null)
            currentHealth = stats.maxHealth;
    }

    protected virtual void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, stats != null ? stats.maxHealth : 100f);
    }

    protected virtual void Update()
    {
        UpdateState();
        UpdateHitstun();
    }

    protected virtual void FixedUpdate()
    {
        ApplyGravity();
        ApplyKnockback();
    }

    /// <summary>
    /// Update character state based on current conditions.
    /// Override in derived classes for custom state logic.
    /// </summary>
    protected virtual void UpdateState()
    {
        // If in hitstun, don't update state (hitstun blocks actions)
        if (hitstunTimeRemaining > 0f)
            return;

        // Default state transitions
        if (!isAlive)
        {
            SetState(CharacterState.KO);
        }
        else if (currentState == CharacterState.Idle || currentState == CharacterState.Moving)
        {
            // Can transition to attacking or moving (handled by controller)
        }
    }

    protected virtual void UpdateHitstun()
    {
        if (hitstunTimeRemaining > 0f)
        {
            hitstunTimeRemaining -= Time.deltaTime;
            if (hitstunTimeRemaining <= 0f)
            {
                hitstunTimeRemaining = 0f;
                // Return to idle or previous state
                if (currentState == CharacterState.GettingHit)
                    SetState(CharacterState.Idle);
            }
        }
    }

    protected virtual void ApplyGravity()
    {
        if (rb != null && IsGrounded())
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        else if (rb != null)
            rb.gravityScale = gravityScale;
    }

    protected virtual void ApplyKnockback()
    {
        if (knockbackVelocity.magnitude > 0.01f)
        {
            rb.velocity = new Vector2(knockbackVelocity.x, rb.velocity.y);
            knockbackVelocity.x *= groundDragMultiplier;
        }
    }

    /// <summary>
    /// Set the character's current state.
    /// </summary>
    public virtual void SetState(CharacterState newState)
    {
        if (currentState != newState)
        {
            OnStateExit(currentState);
            currentState = newState;
            OnStateEnter(currentState);
        }
    }

    /// <summary>
    /// Called when entering a new state. Override for state-specific setup.
    /// </summary>
    protected virtual void OnStateEnter(CharacterState state)
    {
        // Debug.Log($"{gameObject.name} entered state: {state}");
    }

    /// <summary>
    /// Called when exiting a state. Override for cleanup.
    /// </summary>
    protected virtual void OnStateExit(CharacterState state)
    {
        // Cleanup
    }

    /// <summary>
    /// Apply damage and knockback to this character.
    /// </summary>
    public virtual void TakeDamage(float damage, Vector2 knockbackForce, float hitstunDuration)
    {
        if (!isAlive) return;

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, stats != null ? stats.maxHealth : 100f);

        // Apply knockback
        knockbackVelocity = knockbackForce;

        // Apply hitstun
        hitstunTimeRemaining = hitstunDuration;
        SetState(CharacterState.GettingHit);

        // Play hit feedback (override in derived classes)
        OnHitFeedback();

        // Check if dead
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal the character.
    /// </summary>
    public virtual void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, stats != null ? stats.maxHealth : 100f);
        OnHealthChanged?.Invoke(currentHealth, stats != null ? stats.maxHealth : 100f);
    }

    /// <summary>
    /// Kill this character.
    /// </summary>
    public virtual void Die()
    {
        if (!isAlive) return;
        isAlive = false;
        currentHealth = 0f;
        SetState(CharacterState.KO);
        OnDeath?.Invoke();
    }

    /// <summary>
    /// Called when hit (play effects, sounds, etc).
    /// </summary>
    protected virtual void OnHitFeedback()
    {
        // Override in derived classes for visual/audio feedback
        // e.g., screen shake, color flash, impact sound
    }

    // Getters
    public CharacterState GetState() => currentState;
    public float GetHealth() => currentHealth;
    public float GetHealthPercent() => stats != null ? currentHealth / stats.maxHealth : 0f;
    public bool IsAlive() => isAlive;
    public bool IsGrounded() => characterController != null && characterController.IsGrounded();
    public int GetFacingDirection() => facingDirection;
    public float GetHitstunTimeRemaining() => hitstunTimeRemaining;
    public bool IsInHitstun() => hitstunTimeRemaining > 0f;

    // Setters
    public void SetFacingDirection(int direction) => facingDirection = Mathf.Sign(direction) != 0 ? (int)Mathf.Sign(direction) : 1;
    public void FlipDirection() => facingDirection *= -1;

    public virtual void OnDrawGizmos()
    {
        // Draw simple debug capsule
        Gizmos.color = isAlive ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 1f, 0.1f));
    }
}
