using UnityEngine;

/// <summary>
/// Defines a special move with input sequence requirements and effects.
/// Created as a ScriptableObject asset for easy configuration and reuse.
/// </summary>
[CreateAssetMenu(fileName = "SpecialMove_", menuName = "Fighting Game/Special Move")]
public class SpecialMove : ScriptableObject
{
    [System.Serializable]
    public struct SpecialMoveInput
    {
        public string inputName; // e.g., "Light", "Medium", "Heavy", "Kick"
        public float timingWindow; // Time allowed after previous input
    }

    [SerializeField] public string moveName = "Special Move";
    [SerializeField] public string displayName = "Special Attack";
    [SerializeField] public string description = "A powerful special attack";

    [SerializeField] public SpecialMoveInput[] inputSequence = new SpecialMoveInput[3];
    [SerializeField] public float totalInputWindow = 1.5f; // Total time to complete the sequence

    // Effects
    [SerializeField] public float baseDamage = 30f;
    [SerializeField] public float knockbackForce = 10f;
    [SerializeField] public float hitstunDuration = 0.8f;
    [SerializeField] public int comboScaling = 50; // Hit count for first combo hit (higher = less scaling)

    // Animation
    [SerializeField] public string attackAnimationName = "SpecialAttack";
    [SerializeField] public float animationDuration = 1.5f;
    [SerializeField] public float startupFrames = 0.3f; // Before hitbox activates
    [SerializeField] public float activeFrames = 0.6f; // While hitbox is active
    [SerializeField] public float recoveryFrames = 0.6f; // After hitbox deactivates

    // Cost/Requirement
    [SerializeField] public float energyCost = 50f; // If using an energy system
    [SerializeField] public bool requiresGround = false; // Can only use on ground
    [SerializeField] public bool requiresAir = false; // Can only use in air

    // Particle effect
    [SerializeField] public string particleEffectPrefab = "Particles/SpecialAttackImpact";
    [SerializeField] public ParticleSystem muzzleParticle; // Particle at attack source

    // Audio
    [SerializeField] public AudioClip specialMoveSound;

    /// <summary>
    /// Get the total input sequence as a string for display
    /// </summary>
    public string GetInputSequenceString()
    {
        string sequence = "";
        foreach (var input in inputSequence)
        {
            if (!string.IsNullOrEmpty(input.inputName))
            {
                if (sequence.length > 0)
                    sequence += " → ";
                sequence += input.inputName;
            }
        }
        return sequence;
    }

    /// <summary>
    /// Check if an input sequence matches this special move
    /// </summary>
    public bool CheckInputSequence(string[] inputHistory)
    {
        if (inputHistory.Length < inputSequence.Length)
            return false;

        // Check last N inputs match the sequence
        int offset = inputHistory.Length - inputSequence.Length;
        for (int i = 0; i < inputSequence.Length; i++)
        {
            if (inputHistory[offset + i].ToLower() != inputSequence[i].inputName.ToLower())
                return false;
        }

        return true;
    }

    /// <summary>
    /// Validate the special move configuration
    /// </summary>
    public bool IsValid()
    {
        if (inputSequence.Length == 0)
            return false;

        foreach (var input in inputSequence)
        {
            if (string.IsNullOrEmpty(input.inputName))
                return false;
        }

        return baseDamage > 0 && animationDuration > 0;
    }
}
