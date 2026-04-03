using UnityEngine;

/// <summary>
/// Central hit detection and damage resolution system.
/// Handles hitbox/hurtbox collision, damage calculation, and knockback application.
/// </summary>
public static class HitDetection
{
    // Event for hit feedback (screen shake, particles, etc)
    public delegate void HitDelegate(Character attacker, Character defender, float damage);
    public static event HitDelegate OnHit;

    /// <summary>
    /// Called when a hitbox collides with a hurtbox.
    /// Resolves damage, knockback, and hitstun.
    /// </summary>
    public static void OnHitDetected(Hitbox hitbox, Hurtbox hurtbox, Character attacker)
    {
        if (hitbox == null || hurtbox == null || attacker == null)
            return;

        Character defender = hurtbox.GetCharacter();
        if (defender == null || defender == attacker) return; // No self-damage

        // Calculate damage with zone multiplier
        float damage = hitbox.GetDamage() * hurtbox.GetDamageMultiplier();
        float knockbackForce = hitbox.GetKnockbackForce();
        float hitstunDuration = hitbox.GetHitstunDuration();

        // Apply knockback in the direction of the attacker's facing
        Vector2 knockbackDirection = new Vector2(attacker.GetFacingDirection(), 0.5f).normalized;
        Vector2 knockbackVelocity = knockbackDirection * knockbackForce;

        // Apply damage and knockback to defender
        defender.TakeDamage(damage, knockbackVelocity, hitstunDuration);

        // Invoke hit event
        OnHit?.Invoke(attacker, defender, damage);

        Debug.Log($"[HIT] {attacker.name} hit {defender.name} for {damage} damage! " +
                  $"Zone: {hurtbox.GetZone()}, Knockback: {knockbackForce}, Hitstun: {hitstunDuration}");
    }

    /// <summary>
    /// Calculate effective damage based on character stats and combo meter.
    /// </summary>
    public static float CalculateDamage(string attackType, Character attacker, float baseMultiplier = 1f)
    {
        CharacterStats stats = attacker.GetComponent<CharacterStats>();
        if (stats == null) return 0f;

        stats.GetAttackStats(attackType, out float damage, out _, out _);
        return damage * baseMultiplier;
    }

    /// <summary>
    /// Calculate knockback force based on distance and attack type.
    /// </summary>
    public static float CalculateKnockback(string attackType, float distanceMultiplier = 1f)
    {
        float baseKnockback = attackType.ToLower() switch
        {
            "light" => 3f,
            "medium" => 5f,
            "heavy" => 8f,
            _ => 3f
        };

        return baseKnockback * distanceMultiplier;
    }

    /// <summary>
    /// Apply knockback with drag/friction.
    /// </summary>
    public static Vector2 ApplyKnockbackWithDrag(Vector2 knockback, float dragMultiplier = 0.95f)
    {
        knockback *= dragMultiplier;
        return knockback.magnitude > 0.01f ? knockback : Vector2.zero;
    }
}
