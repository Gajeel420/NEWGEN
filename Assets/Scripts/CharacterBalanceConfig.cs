using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject for storing character balance presets and test configurations.
/// Allows easy comparison between balance states without code changes.
/// </summary>
[CreateAssetMenu(fileName = "CharacterBalanceConfig", menuName = "Fighting Game/Balance Config", order = 115)]
public class CharacterBalanceConfig : ScriptableObject
{
    [System.Serializable]
    public class BalancePreset
    {
        public string presetName;
        public string description;
        [Tooltip("Light attack damage multiplier")]
        public float lightAttackMultiplier = 1f;
        [Tooltip("Medium attack damage multiplier")]
        public float mediumAttackMultiplier = 1f;
        [Tooltip("Heavy attack damage multiplier")]
        public float heavyAttackMultiplier = 1f;
        [Tooltip("Special move damage multiplier")]
        public float specialMoveDamageMultiplier = 1f;
        [Tooltip("Attack startup frames multiplier")]
        public float startupFrameMultiplier = 1f;
        [Tooltip("Recovery frames multiplier")]
        public float recoveryFrameMultiplier = 1f;
        [Tooltip("Block stamina multiplier")]
        public float blockStaminaMultiplier = 1f;
        [Tooltip("Combo damage scaling multiplier")]
        public float comboDamageScalingMultiplier = 1f;
        [Tooltip("Knockback force multiplier")]
        public float knockbackMultiplier = 1f;
        [Tooltip("Passive health regeneration per second")]
        public float healthRegenPerSecond = 0f;
    }

    [System.Serializable]
    public class TestCharacterConfig
    {
        public string characterName;
        [Tooltip("Applied balance preset")]
        public BalancePreset appliedPreset;
        [Tooltip("Max health")]
        public float maxHealth = 100f;
        [Tooltip("Walk speed")]
        public float walkSpeed = 5f;
        [Tooltip("Jump height")]
        public float jumpHeight = 3f;
        [Tooltip("AI difficulty for testing")]
        public int aiDifficulty = 5; // 1-10 scale
    }

    [SerializeField] private List<BalancePreset> balancePresets = new List<BalancePreset>();
    [SerializeField] private List<TestCharacterConfig> testCharacters = new List<TestCharacterConfig>();

    [SerializeField] private bool autoApplyPresets = true;
    [SerializeField] private bool enableDetailedLogging = false;

    // Test parameters
    [SerializeField] private int testRoundsPerMatchup = 5;
    [SerializeField] private float targetTestDuration = 60f; // seconds per round
    [SerializeField] private bool recordDetailedStats = true;
    [SerializeField] private bool enablePerformanceProfiling = true;

    /// <summary>
    /// Get preset by name
    /// </summary>
    public BalancePreset GetPreset(string presetName)
    {
        foreach (var preset in balancePresets)
        {
            if (preset.presetName == presetName)
                return preset;
        }
        return null;
    }

    /// <summary>
    /// Get test character config
    /// </summary>
    public TestCharacterConfig GetTestCharacter(string characterName)
    {
        foreach (var testChar in testCharacters)
        {
            if (testChar.characterName == characterName)
                return testChar;
        }
        return null;
    }

    /// <summary>
    /// Apply preset to character stats
    /// </summary>
    public void ApplyPreset(CharacterStats characterStats, BalancePreset preset)
    {
        if (characterStats == null || preset == null)
            return;

        // Apply multipliers to character stats
        characterStats.lightAttackDamage = Mathf.RoundToInt(characterStats.lightAttackDamage * preset.lightAttackMultiplier);
        characterStats.mediumAttackDamage = Mathf.RoundToInt(characterStats.mediumAttackDamage * preset.mediumAttackMultiplier);
        characterStats.heavyAttackDamage = Mathf.RoundToInt(characterStats.heavyAttackDamage * preset.heavyAttackMultiplier);

        if (enableDetailedLogging)
            Debug.Log($"[BalanceConfig] Applied preset '{preset.presetName}' to {characterStats.name}");
    }

    /// <summary>
    /// Get all presets
    /// </summary>
    public List<BalancePreset> GetAllPresets()
    {
        return new List<BalancePreset>(balancePresets);
    }

    /// <summary>
    /// Get all test characters
    /// </summary>
    public List<TestCharacterConfig> GetAllTestCharacters()
    {
        return new List<TestCharacterConfig>(testCharacters);
    }

    /// <summary>
    /// Add new preset
    /// </summary>
    public void AddPreset(BalancePreset preset)
    {
        if (preset != null)
            balancePresets.Add(preset);
    }

    /// <summary>
    /// Remove preset by name
    /// </summary>
    public void RemovePreset(string presetName)
    {
        balancePresets.RemoveAll(p => p.presetName == presetName);
    }

    /// <summary>
    /// Add test character config
    /// </summary>
    public void AddTestCharacter(TestCharacterConfig testChar)
    {
        if (testChar != null)
            testCharacters.Add(testChar);
    }

    /// <summary>
    /// Create default preset (baseline/no changes)
    /// </summary>
    public static BalancePreset CreateDefaultPreset()
    {
        return new BalancePreset
        {
            presetName = "DEFAULT",
            description = "No balance adjustments applied",
            lightAttackMultiplier = 1f,
            mediumAttackMultiplier = 1f,
            heavyAttackMultiplier = 1f,
            specialMoveDamageMultiplier = 1f,
            startupFrameMultiplier = 1f,
            recoveryFrameMultiplier = 1f,
            blockStaminaMultiplier = 1f,
            comboDamageScalingMultiplier = 1f,
            knockbackMultiplier = 1f
        };
    }

    /// <summary>
    /// Create aggressive preset (deal more damage)
    /// </summary>
    public static BalancePreset CreateAggressivePreset()
    {
        return new BalancePreset
        {
            presetName = "AGGRESSIVE",
            description = "Increased damage output",
            lightAttackMultiplier = 1.15f,
            mediumAttackMultiplier = 1.25f,
            heavyAttackMultiplier = 1.35f,
            specialMoveDamageMultiplier = 1.25f,
            startupFrameMultiplier = 0.9f,
            recoveryFrameMultiplier = 0.85f,
            blockStaminaMultiplier = 0.8f,
            comboDamageScalingMultiplier = 1.2f,
            knockbackMultiplier = 1.1f
        };
    }

    /// <summary>
    /// Create defensive preset (take less damage)
    /// </summary>
    public static BalancePreset CreateDefensivePreset()
    {
        return new BalancePreset
        {
            presetName = "DEFENSIVE",
            description = "Reduced damage taken and increased block strength",
            lightAttackMultiplier = 0.8f,
            mediumAttackMultiplier = 0.75f,
            heavyAttackMultiplier = 0.7f,
            specialMoveDamageMultiplier = 0.75f,
            startupFrameMultiplier = 1.15f,
            recoveryFrameMultiplier = 1.2f,
            blockStaminaMultiplier = 1.4f,
            comboDamageScalingMultiplier = 0.8f,
            knockbackMultiplier = 0.7f
        };
    }

    /// <summary>
    /// Create speed preset (faster actions)
    /// </summary>
    public static BalancePreset CreateSpeedPreset()
    {
        return new BalancePreset
        {
            presetName = "SPEED",
            description = "Faster startup and recovery frames",
            lightAttackMultiplier = 1.05f,
            mediumAttackMultiplier = 1.1f,
            heavyAttackMultiplier = 1.15f,
            specialMoveDamageMultiplier = 1.05f,
            startupFrameMultiplier = 0.75f,
            recoveryFrameMultiplier = 0.7f,
            blockStaminaMultiplier = 1f,
            comboDamageScalingMultiplier = 1f,
            knockbackMultiplier = 0.9f
        };
    }

    // Public properties for easy access
    public int TestRoundsPerMatchup => testRoundsPerMatchup;
    public float TargetTestDuration => targetTestDuration;
    public bool RecordDetailedStats => recordDetailedStats;
    public bool EnablePerformanceProfiling => enablePerformanceProfiling;
    public bool AutoApplyPresets => autoApplyPresets;
    public bool EnableDetailedLogging => enableDetailedLogging;
}
