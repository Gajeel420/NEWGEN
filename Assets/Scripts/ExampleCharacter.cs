using UnityEngine;
using System.Collections;

/// <summary>
/// Example character controller - extends base Character class with custom behavior.
/// Demonstrates how to create a playable character with unique mechanics.
/// </summary>
public class ExampleCharacter : Character
{
    [Header("Example Character Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    private float dashTimeRemaining = 0f;
    private bool isDashing = false;

    private AttackManager attackManager;
    private ParticleSystem hitParticles;

    protected override void Awake()
    {
        base.Awake();
        attackManager = GetComponent<AttackManager>();
        hitParticles = GetComponentInChildren<ParticleSystem>();
    }

    protected override void OnStateEnter(CharacterState state)
    {
        base.OnStateEnter(state);

        switch (state)
        {
            case CharacterState.Idle:
                Debug.Log($"{gameObject.name} is now Idle");
                break;
            case CharacterState.Moving:
                Debug.Log($"{gameObject.name} is Moving");
                break;
            case CharacterState.Attacking:
                Debug.Log($"{gameObject.name} is Attacking!");
                break;
            case CharacterState.GettingHit:
                Debug.Log($"{gameObject.name} got hit!");
                break;
            case CharacterState.KO:
                Debug.Log($"{gameObject.name} has been KO'd!");
                break;
        }
    }

    protected override void OnHitFeedback()
    {
        base.OnHitFeedback();

        // Visual feedback on hit
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashOnHit());
        }

        // Play hit particles
        if (hitParticles != null)
        {
            hitParticles.Play();
        }
    }

    private IEnumerator FlashOnHit()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    public override void Die()
    {
        base.Die();
        // Add death animation or effects here
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }
    }
}
