using UnityEngine;

/// <summary>
/// Scriptable Object container for character stats and attributes.
/// Create instances for each character/prototype and assign to the Character GameObject.
/// </summary>
[CreateAssetMenu(fileName = "CharacterStats", menuName = "Fighting Game/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float airAcceleration = 3f;
    public float airDeceleration = 2f;

    [Header("Jump")]
    public float jumpHeight = 3f;
    public float jumpDuration = 0.4f;
    public float airControl = 0.8f; // 0-1, how much control during air
    public float maxAirHorizontalVelocity = 6f;

    [Header("Attack")]
    public float lightAttackDamage = 10f;
    public float mediumAttackDamage = 15f;
    public float heavyAttackDamage = 25f;

    public float lightAttackKnockback = 3f;
    public float mediumAttackKnockback = 5f;
    public float heavyAttackKnockback = 8f;

    public float lightAttackHitstun = 0.2f;
    public float mediumAttackHitstun = 0.3f;
    public float heavyAttackHitstun = 0.5f;

    [Header("Combo")]
    public float comboInputWindow = 0.5f; // Time to input next combo
    public float comboDamageScale = 1.1f; // Damage multiplier per combo count

    [Header("Physics")]
    public float gravityScale = 1f;
    public float groundDrag = 0.95f;

    /// <summary>
    /// Get damage and knockback for an attack type.
    /// </summary>
    public void GetAttackStats(string attackType, out float damage, out float knockback, out float hitstun)
    {
        damage = lightAttackDamage;
        knockback = lightAttackKnockback;
        hitstun = lightAttackHitstun;

        switch (attackType.ToLower())
        {
            case "light":
                damage = lightAttackDamage;
                knockback = lightAttackKnockback;
                hitstun = lightAttackHitstun;
                break;
            case "medium":
                damage = mediumAttackDamage;
                knockback = mediumAttackKnockback;
                hitstun = mediumAttackHitstun;
                break;
            case "heavy":
                damage = heavyAttackDamage;
                knockback = heavyAttackKnockback;
                hitstun = heavyAttackHitstun;
                break;
        }
    }

    /// <summary>
    /// Calculate jump velocity from jump height and duration.
    /// </summary>
    public float GetJumpVelocity()
    {
        // v = 2 * height / time
        return (2f * jumpHeight) / jumpDuration;
    }

    public override string ToString()
    {
        return $"Stats: HP={maxHealth}, Walk={walkSpeed}, Jump={jumpHeight}, Damage=[L:{lightAttackDamage} M:{mediumAttackDamage} H:{heavyAttackDamage}]";
    }
}
