using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks combat statistics per character: damage dealt/received, hits, combos, blocks, etc.
/// Used for balance analysis and matchup testing.
/// </summary>
public class StatisticsTracker : MonoBehaviour
{
    [System.Serializable]
    public class CharacterStats
    {
        public string characterName;
        public int totalDamageDealt = 0;
        public int totalDamageReceived = 0;
        public int totalHitsLanded = 0;
        public int totalHitsMissed = 0;
        public int totalBlocksSuccessful = 0;
        public int totalBlocksFailed = 0;
        public int totalComboCount = 0;
        public int maxComboCount = 0;
        public int highestCombo = 0;
        public int specialMovesUsed = 0;
        public int knockdownsInflicted = 0;
        public int knockdownsReceived = 0;
        public int timesBlockBroken = 0;
        public float totalGameTime = 0f;
        public int roundsWon = 0;
        public int roundsLost = 0;

        public void Reset()
        {
            totalDamageDealt = 0;
            totalDamageReceived = 0;
            totalHitsLanded = 0;
            totalHitsMissed = 0;
            totalBlocksSuccessful = 0;
            totalBlocksFailed = 0;
            totalComboCount = 0;
            maxComboCount = 0;
            highestCombo = 0;
            specialMovesUsed = 0;
            knockdownsInflicted = 0;
            knockdownsReceived = 0;
            timesBlockBroken = 0;
            totalGameTime = 0f;
            roundsWon = 0;
            roundsLost = 0;
        }
    }

    private static Dictionary<string, CharacterStats> characterStatsMap = new Dictionary<string, CharacterStats>();
    private static Dictionary<string, CharacterStats> roundStatsMap = new Dictionary<string, CharacterStats>();

    private static bool isTrackingRound = false;
    private static float roundStartTime = 0f;

    /// <summary>
    /// Start tracking a new round
    /// </summary>
    public static void StartRoundTracking(params string[] characterNames)
    {
        roundStatsMap.Clear();
        foreach (var name in characterNames)
        {
            roundStatsMap[name] = new CharacterStats { characterName = name };
        }
        isTrackingRound = true;
        roundStartTime = Time.time;
    }

    /// <summary>
    /// End current round tracking and accumulate to session stats
    /// </summary>
    public static void EndRoundTracking()
    {
        if (!isTrackingRound) return;

        float roundDuration = Time.time - roundStartTime;

        foreach (var kvp in roundStatsMap)
        {
            string charName = kvp.Key;
            CharacterStats roundStats = kvp.Value;
            roundStats.totalGameTime = roundDuration;

            if (!characterStatsMap.ContainsKey(charName))
                characterStatsMap[charName] = new CharacterStats { characterName = charName };

            // Accumulate stats
            characterStatsMap[charName].totalDamageDealt += roundStats.totalDamageDealt;
            characterStatsMap[charName].totalDamageReceived += roundStats.totalDamageReceived;
            characterStatsMap[charName].totalHitsLanded += roundStats.totalHitsLanded;
            characterStatsMap[charName].totalHitsMissed += roundStats.totalHitsMissed;
            characterStatsMap[charName].totalBlocksSuccessful += roundStats.totalBlocksSuccessful;
            characterStatsMap[charName].totalBlocksFailed += roundStats.totalBlocksFailed;
            characterStatsMap[charName].totalComboCount += roundStats.totalComboCount;
            characterStatsMap[charName].highestCombo = Mathf.Max(
                characterStatsMap[charName].highestCombo, 
                roundStats.maxComboCount
            );
            characterStatsMap[charName].specialMovesUsed += roundStats.specialMovesUsed;
            characterStatsMap[charName].knockdownsInflicted += roundStats.knockdownsInflicted;
            characterStatsMap[charName].knockdownsReceived += roundStats.knockdownsReceived;
            characterStatsMap[charName].timesBlockBroken += roundStats.timesBlockBroken;
            characterStatsMap[charName].totalGameTime += roundStats.totalGameTime;
        }

        isTrackingRound = false;
    }

    /// <summary>
    /// Record damage dealt by character
    /// </summary>
    public static void RecordDamageDealt(string characterName, int damage)
    {
        if (!isTrackingRound) return;
        if (roundStatsMap.ContainsKey(characterName))
            roundStatsMap[characterName].totalDamageDealt += damage;
    }

    /// <summary>
    /// Record damage received by character
    /// </summary>
    public static void RecordDamageReceived(string characterName, int damage)
    {
        if (!isTrackingRound) return;
        if (roundStatsMap.ContainsKey(characterName))
            roundStatsMap[characterName].totalDamageReceived += damage;
    }

    /// <summary>
    /// Record successful hit
    /// </summary>
    public static void RecordHitLanded(string characterName)
    {
        if (!isTrackingRound) return;
        if (roundStatsMap.ContainsKey(characterName))
            roundStatsMap[characterName].totalHitsLanded++;
    }

    /// <summary>
    /// Record successful block
    /// </summary>
    public static void RecordBlockSuccessful(string characterName)
    {
        if (!isTrackingRound) return;
        if (roundStatsMap.ContainsKey(characterName))
            roundStatsMap[characterName].totalBlocksSuccessful++;
    }

    /// <summary>
    /// Record special move usage
    /// </summary>
    public static void RecordSpecialMoveUsed(string characterName)
    {
        if (!isTrackingRound) return;
        if (roundStatsMap.ContainsKey(characterName))
            roundStatsMap[characterName].specialMovesUsed++;
    }

    /// <summary>
    /// Record knockdown inflicted
    /// </summary>
    public static void RecordKnockdown(string attackerName, string victimName)
    {
        if (!isTrackingRound) return;
        if (roundStatsMap.ContainsKey(attackerName))
            roundStatsMap[attackerName].knockdownsInflicted++;
        if (roundStatsMap.ContainsKey(victimName))
            roundStatsMap[victimName].knockdownsReceived++;
    }

    /// <summary>
    /// Record block break
    /// </summary>
    public static void RecordBlockBroken(string characterName)
    {
        if (!isTrackingRound) return;
        if (roundStatsMap.ContainsKey(characterName))
            roundStatsMap[characterName].timesBlockBroken++;
    }

    /// <summary>
    /// Record round won/lost
    /// </summary>
    public static void RecordRoundEnd(string winnerName, string loserName)
    {
        if (!isTrackingRound) return;
        if (roundStatsMap.ContainsKey(winnerName))
            roundStatsMap[winnerName].roundsWon++;
        if (roundStatsMap.ContainsKey(loserName))
            roundStatsMap[loserName].roundsLost++;
    }

    /// <summary>
    /// Get accumulated stats for character
    /// </summary>
    public static CharacterStats GetCharacterStats(string characterName)
    {
        if (characterStatsMap.ContainsKey(characterName))
            return characterStatsMap[characterName];
        return null;
    }

    /// <summary>
    /// Get current round stats
    /// </summary>
    public static CharacterStats GetRoundStats(string characterName)
    {
        if (roundStatsMap.ContainsKey(characterName))
            return roundStatsMap[characterName];
        return null;
    }

    /// <summary>
    /// Calculate hit accuracy percentage
    /// </summary>
    public static float GetHitAccuracy(string characterName)
    {
        var stats = GetCharacterStats(characterName);
        if (stats == null) return 0f;

        int totalAttempts = stats.totalHitsLanded + stats.totalHitsMissed;
        if (totalAttempts == 0) return 0f;

        return (stats.totalHitsLanded / (float)totalAttempts) * 100f;
    }

    /// <summary>
    /// Get average damage per hit
    /// </summary>
    public static float GetAverageDamagePerHit(string characterName)
    {
        var stats = GetCharacterStats(characterName);
        if (stats == null || stats.totalHitsLanded == 0) return 0f;

        return stats.totalDamageDealt / (float)stats.totalHitsLanded;
    }

    /// <summary>
    /// Calculate damage output per second
    /// </summary>
    public static float GetDPS(string characterName)
    {
        var stats = GetCharacterStats(characterName);
        if (stats == null || stats.totalGameTime == 0) return 0f;

        return stats.totalDamageDealt / stats.totalGameTime;
    }

    /// <summary>
    /// Reset all session stats
    /// </summary>
    public static void ResetSessionStats()
    {
        characterStatsMap.Clear();
        roundStatsMap.Clear();
        isTrackingRound = false;
    }

    /// <summary>
    /// Get all tracked characters
    /// </summary>
    public static Dictionary<string, CharacterStats> GetAllCharacterStats()
    {
        return new Dictionary<string, CharacterStats>(characterStatsMap);
    }
}
