using UnityEngine;

/// <summary>
/// Manages character animations and synchronizes them with game state.
/// Works with Unity's Animator component to drive animation transitions.
/// </summary>
public class AnimationController : MonoBehaviour
{
    /// <summary>
    /// Animation state parameters - used to set Animator parameters
    /// </summary>
    public static class AnimationParameters
    {
        public const string STATE = "State";
        public const string SPEED = "Speed";
        public const string IS_GROUNDED = "IsGrounded";
        public const string ATTACK_TYPE = "AttackType";
        public const string KNOCKED_BACK = "KnockedBack";
        public const string HEALTH_PERCENT = "HealthPercent";
    }

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Character character;

    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    private AttackManager attackManager;

    private float normalizedAnimationTime = 0f;

    /// <summary>
    /// Cached animator parameter hashes for performance
    /// </summary>
    private int stateHash;
    private int speedHash;
    private int isGroundedHash;
    private int attackTypeHash;
    private int knockedBackHash;
    private int healthPercentHash;

    private void OnEnable()
    {
        if (character != null)
        {
            character.OnStateChanged += HandleCharacterStateChanged;
        }
    }

    private void OnDisable()
    {
        if (character != null)
        {
            character.OnStateChanged -= HandleCharacterStateChanged;
        }
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (character == null)
            character = GetComponent<Character>();

        if (characterController == null)
            character = GetComponent<CharacterController>();

        if (attackManager == null)
            attackManager = GetComponent<AttackManager>();

        // Cache parameter hashes for performance
        stateHash = Animator.StringToHash(AnimationParameters.STATE);
        speedHash = Animator.StringToHash(AnimationParameters.SPEED);
        isGroundedHash = Animator.StringToHash(AnimationParameters.IS_GROUNDED);
        attackTypeHash = Animator.StringToHash(AnimationParameters.ATTACK_TYPE);
        knockedBackHash = Animator.StringToHash(AnimationParameters.KNOCKED_BACK);
        healthPercentHash = Animator.StringToHash(AnimationParameters.HEALTH_PERCENT);
    }

    private void Update()
    {
        if (animator == null || character == null)
            return;

        UpdateAnimationParameters();
        UpdateAttackFrameTiming();
    }

    /// <summary>
    /// Updates all Animator parameters based on current game state
    /// </summary>
    private void UpdateAnimationParameters()
    {
        // State parameter (for state machine transitions)
        animator.SetInteger(stateHash, (int)character.CurrentState);

        // Movement speed
        if (characterController != null)
        {
            animator.SetFloat(speedHash, Mathf.Abs(characterController.CurrentVelocity.x));
        }

        // Ground check
        if (characterController != null)
        {
            animator.SetBool(isGroundedHash, characterController.IsGrounded);
        }

        // Health percentage for visual feedback
        animator.SetFloat(healthPercentHash, (float)character.Health / character.MaxHealth);
    }

    /// <summary>
    /// Updates attack frame timing based on animation progress
    /// Crucial for frame-perfect attacks
    /// </summary>
    private void UpdateAttackFrameTiming()
    {
        if (character.CurrentState != Character.CharacterState.Attacking || attackManager == null)
            return;

        // Get current animation progress (0-1)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        normalizedAnimationTime = stateInfo.normalizedTime;

        // Convert animation time to frame count (assuming 60 FPS game)
        // This allows frame-perfect attack window management
        int currentFrame = Mathf.FloorToInt(normalizedAnimationTime * 60f);
        
        // Update attack manager with current frame
        // (ensure AttackManager.cs has a SetCurrentAnimationFrame method)
    }

    /// <summary>
    /// Handles state transitions from the Character state machine
    /// </summary>
    private void HandleCharacterStateChanged(Character.CharacterState newState)
    {
        animator.SetInteger(stateHash, (int)newState);

        // Play entry animation for special states
        switch (newState)
        {
            case Character.CharacterState.KnockedDown:
                animator.SetBool(knockedBackHash, true);
                break;
            case Character.CharacterState.Idle:
                animator.SetBool(knockedBackHash, false);
                break;
        }
    }

    /// <summary>
    /// Returns the current normalized animation time (0-1)
    /// Used by AttackManager for precise frame timing
    /// </summary>
    public float GetNormalizedAnimationTime()
    {
        if (animator == null)
            return 0f;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime % 1f; // Modulo 1 to keep in 0-1 range
    }

    /// <summary>
    /// Triggers an attack animation based on attack type
    /// </summary>
    public void PlayAttackAnimation(int attackType)
    {
        if (animator == null)
            return;

        animator.SetInteger(attackTypeHash, attackType);
        animator.SetTrigger("Attack");
    }

    /// <summary>
    /// Gets current animation state info for external systems
    /// </summary>
    public AnimatorStateInfo GetCurrentStateInfo()
    {
        if (animator == null)
            return default;

        return animator.GetCurrentAnimatorStateInfo(0);
    }

    /// <summary>
    /// Checks if current animation is in a specific state
    /// Useful for frame-perfect attack detection
    /// </summary>
    public bool IsInAnimationState(string stateName)
    {
        if (animator == null)
            return false;

        int stateHash = Animator.StringToHash(stateName);
        return animator.GetCurrentAnimatorStateInfo(0).fullPathHash == stateHash;
    }
}
