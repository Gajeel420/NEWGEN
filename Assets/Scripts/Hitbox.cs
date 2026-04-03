using UnityEngine;

/// <summary>
/// Hitbox represents an attack collision zone.
/// Attach this to child objects (e.g., "Fist", "Foot") that handle attacks.
/// </summary>
public class Hitbox : MonoBehaviour
{
    [SerializeField] private string attackType = "Light"; // "Light", "Medium", "Heavy"
    [SerializeField] private float damage = 10f;
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float hitstunDuration = 0.2f;

    [SerializeField] private bool isActive = false;
    private Collider2D hitboxCollider;
    private Character owner;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        owner = GetComponentInParent<Character>();

        if (hitboxCollider != null)
            hitboxCollider.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive || owner == null) return;

        Hurtbox hurtbox = collision.GetComponent<Hurtbox>();
        if (hurtbox != null)
        {
            // Hit detected - let HitDetection handle damage
            HitDetection.OnHitDetected(this, hurtbox, owner);
        }
    }

    // Activation/deactivation (called by AttackManager)
    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }

    // Getters
    public string GetAttackType() => attackType;
    public float GetDamage() => damage;
    public float GetKnockbackForce() => knockbackForce;
    public float GetHitstunDuration() => hitstunDuration;
    public Character GetOwner() => owner;
    public bool IsActive() => isActive;

    // Setters (for dynamic balancing)
    public void SetDamage(float newDamage) => damage = newDamage;
    public void SetKnockbackForce(float newForce) => knockbackForce = newForce;
    public void SetHitstunDuration(float newDuration) => hitstunDuration = newDuration;
}
