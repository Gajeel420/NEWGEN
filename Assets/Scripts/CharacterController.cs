using UnityEngine;

/// <summary>
/// Handles character movement, jumping, and physics.
/// Works with Character.cs for a complete character system.
/// </summary>
public class CharacterController : MonoBehaviour
{
    private Character character;
    private CharacterStats stats;
    private Rigidbody2D rb;
    private CapsuleCollider2D groundCollider;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;

    // Movement state
    private bool isMovingRight = false;
    private bool isMovingLeft = false;
    private bool isJumping = false;
    private bool isGrounded = false;
    private float jumpTimeRemaining = 0f;

    // Air state
    private float horizontalAirVelocity = 0f;

    void Awake()
    {
        character = GetComponent<Character>();
        stats = GetComponent<CharacterStats>();
        rb = GetComponent<Rigidbody2D>();
        groundCollider = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        // Get input from InputManager
        float move = InputManager.Instance != null ? InputManager.Instance.GetAxis("Horizontal") : 0f;
        HandleMovementInput(move);
        HandleJumpInput();
    }

    void FixedUpdate()
    {
        UpdateGroundCheck();
        UpdateMovement();
        UpdateJump();
    }

    /// <summary>
    /// Check if character is touching the ground.
    /// </summary>
    private void UpdateGroundCheck()
    {
        if (rb == null) return;

        // Raycast down from character position
        Vector2 rayOrigin = rb.position + Vector2.down * (groundCollider.size.y / 2f);
        Debug.DrawRay(rayOrigin, Vector2.down * groundCheckDistance, Color.green);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    private void HandleMovementInput(float horizontalInput)
    {
        isMovingRight = horizontalInput > 0.1f;
        isMovingLeft = horizontalInput < -0.1f;

        if (isMovingRight)
            character.SetFacingDirection(1);
        else if (isMovingLeft)
            character.SetFacingDirection(-1);
    }

    private void HandleJumpInput()
    {
        if (InputManager.Instance == null) return;

        if (InputManager.Instance.ConsumeBuffered("Jump"))
        {
            if (isGrounded && !isJumping)
            {
                StartJump();
            }
        }
    }

    private void UpdateMovement()
    {
        if (rb == null || stats == null) return;

        float moveInput = InputManager.Instance != null ? InputManager.Instance.GetAxis("Horizontal") : 0f;
        float moveSpeed = stats.walkSpeed;

        if (isGrounded)
        {
            // Ground movement
            float targetVelocity = moveInput * moveSpeed;
            rb.velocity = new Vector2(targetVelocity, rb.velocity.y);
            horizontalAirVelocity = 0f;
        }
        else
        {
            // Air movement - more limited control
            float moveDirection = Mathf.Sign(moveInput) != 0 ? Mathf.Sign(moveInput) : 0f;
            horizontalAirVelocity = Mathf.Lerp(horizontalAirVelocity, moveDirection * stats.maxAirHorizontalVelocity, stats.airAcceleration * Time.fixedDeltaTime);

            // Clamp to max velocity
            horizontalAirVelocity = Mathf.Clamp(horizontalAirVelocity, -stats.maxAirHorizontalVelocity, stats.maxAirHorizontalVelocity);

            rb.velocity = new Vector2(horizontalAirVelocity, rb.velocity.y);
        }
    }

    private void StartJump()
    {
        if (stats == null) return;

        isJumping = true;
        jumpTimeRemaining = stats.jumpDuration;
        float jumpVelocity = stats.GetJumpVelocity();
        rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
    }

    private void UpdateJump()
    {
        if (!isJumping) return;

        jumpTimeRemaining -= Time.fixedDeltaTime;

        if (jumpTimeRemaining <= 0f || (isGrounded && rb.velocity.y <= 0f))
        {
            EndJump();
        }
    }

    private void EndJump()
    {
        isJumping = false;
        jumpTimeRemaining = 0f;
    }

    // Public accessors
    public bool IsGrounded() => isGrounded;
    public bool IsJumping() => isJumping;
    public Vector2 GetVelocity() => rb != null ? rb.velocity : Vector2.zero;
    public void SetVelocity(Vector2 velocity)
    {
        if (rb != null)
            rb.velocity = velocity;
    }

    public void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw ground check area
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        if (col != null)
        {
            Vector2 rayOrigin = transform.position + Vector3.down * (col.size.y / 2f);
            Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
        }
    }
}
