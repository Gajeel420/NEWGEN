using UnityEngine;

/// <summary>
/// Hurtbox represents a character's vulnerability zone.
/// Attach multiple to different body parts (Head, Body, Legs) for zone-specific damage modifiers.
/// </summary>
public class Hurtbox : MonoBehaviour
{
    public enum HurtboxZone { Head, Body, Legs }

    [SerializeField] private HurtboxZone zone = HurtboxZone.Body;
    [SerializeField] private float damageMultiplier = 1f; // Head = 1.2, Legs = 0.8, etc.

    private Character character;
    private Collider2D hurtboxCollider;

    void Awake()
    {
        character = GetComponentInParent<Character>();
        hurtboxCollider = GetComponent<Collider2D>();

        if (hurtboxCollider != null)
            hurtboxCollider.isTrigger = true;
    }

    public HurtboxZone GetZone() => zone;
    public float GetDamageMultiplier() => damageMultiplier;
    public Character GetCharacter() => character;
    public Collider2D GetCollider() => hurtboxCollider;

    public void SetDamageMultiplier(float multiplier) => damageMultiplier = multiplier;

    void OnDrawGizmos()
    {
        // Draw hurtbox color based on zone
        Color zoneColor = zone switch
        {
            HurtboxZone.Head => Color.red,
            HurtboxZone.Body => Color.yellow,
            HurtboxZone.Legs => Color.cyan,
            _ => Color.gray
        };

        Gizmos.color = zoneColor;
        Collider2D col = GetComponent<Collider2D>();
        if (col is CircleCollider2D circle)
        {
            Gizmos.DrawWireSphere(transform.position, circle.radius);
        }
        else if (col is BoxCollider2D box)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
        }
    }
}
