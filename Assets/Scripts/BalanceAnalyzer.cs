using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Analyzes character balance by comparing stats, attributes, and matchup data.
/// Identifies over/underpowered characters and problematic matchups.
/// </summary>
public class BalanceAnalyzer : MonoBehaviour
{
    public static BalanceAnalyzer Instance { get; private set; }

    [System.Serializable]
    public class BalanceReport
    {
        public string characterName;
        public float damageOutput; // Average DPS
        public float survivability; // Average health per round
        public float hitAccuracy;
        public float comboFrequency;
        public float specialMoveFrequency;
        public float blockSuccessRate;
        public float winRate;
        public BalanceStatus status;
        public List<string> recommendations;
    }

    public enum BalanceStatus
    {
        Balanced,
        Slightly_Underpowered,
        Underpowered,
        Severely_Underpowered,
        Slightly_Overpowered,
        Overpowered,
        Severely_Overpowered
    }

    [System.Serializable]
    public class MatchupData
    {
        public string character1;
        public string character2;
        public int character1Wins;
        public int character2Wins;
        public float character1WinRate;
        public float character2WinRate;
    }

    [SerializeField] private float balanceThreshold = 0.1f; // ±10% win rate deviation
    [SerializeField] private float overpowerThreshold = 0.15f; // ±15% = overpowered
    [SerializeField] private float severityThreshold = 0.25f; // ±25% = severely unbalanced

    private Dictionary<string, List<MatchupData>> matchupHistory = new Dictionary<string, List<MatchupData>>();
    private Dictionary<string, BalanceReport> balanceReports = new Dictionary<string, BalanceReport>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Analyze balance of a character based on session stats
    /// </summary>
    public BalanceReport AnalyzeCharacter(string characterName)
    {
        var stats = StatisticsTracker.GetCharacterStats(characterName);
        if (stats == null)
            return null;

        BalanceReport report = new BalanceReport
        {
            characterName = characterName,
            damageOutput = StatisticsTracker.GetDPS(characterName),
            survivability = stats.totalDamageReceived > 0 ? 
                (100f - stats.totalDamageReceived / 100f) : 100f,
            hitAccuracy = StatisticsTracker.GetHitAccuracy(characterName),
            comboFrequency = stats.totalComboCount / Mathf.Max(1, stats.totalGameTime),
            specialMoveFrequency = stats.specialMovesUsed / Mathf.Max(1, stats.totalGameTime),
            blockSuccessRate = (stats.totalBlocksSuccessful + stats.totalBlocksFailed) > 0 ?
                (stats.totalBlocksSuccessful / (float)(stats.totalBlocksSuccessful + stats.totalBlocksFailed)) * 100f
                : 0f,
            winRate = (stats.roundsWon + stats.roundsLost) > 0 ?
                (stats.roundsWon / (float)(stats.roundsWon + stats.roundsLost)) * 100f
                : 50f,
            recommendations = new List<string>()
        };

        // Determine balance status
        float deviation = Mathf.Abs(report.winRate - 50f) / 50f;

        if (deviation <= balanceThreshold)
            report.status = BalanceStatus.Balanced;
        else if (report.winRate > 50f)
        {
            if (deviation >= severityThreshold)
                report.status = BalanceStatus.Severely_Overpowered;
            else if (deviation >= overpowerThreshold)
                report.status = BalanceStatus.Overpowered;
            else
                report.status = BalanceStatus.Slightly_Overpowered;
        }
        else
        {
            if (deviation >= severityThreshold)
                report.status = BalanceStatus.Severely_Underpowered;
            else if (deviation >= overpowerThreshold)
                report.status = BalanceStatus.Underpowered;
            else
                report.status = BalanceStatus.Slightly_Underpowered;
        }

        // Generate recommendations
        GenerateRecommendations(report, stats);

        balanceReports[characterName] = report;
        return report;
    }

    /// <summary>
    /// Analyze all characters and generate balance report
    /// </summary>
    public Dictionary<string, BalanceReport> AnalyzeAllCharacters()
    {
        balanceReports.Clear();
        var allStats = StatisticsTracker.GetAllCharacterStats();

        foreach (var kvp in allStats)
        {
            AnalyzeCharacter(kvp.Key);
        }

        return balanceReports;
    }

    /// <summary>
    /// Track matchup result
    /// </summary>
    public void RecordMatchup(string character1, string character2, string winner)
    {
        string key = GetMatchupKey(character1, character2);

        if (!matchupHistory.ContainsKey(key))
            matchupHistory[key] = new List<MatchupData>();

        MatchupData matchup = new MatchupData
        {
            character1 = character1,
            character2 = character2,
            character1Wins = winner == character1 ? 1 : 0,
            character2Wins = winner == character2 ? 1 : 0
        };

        matchupHistory[key].Add(matchup);
        UpdateMatchupWinRates(key);
    }

    /// <summary>
    /// Get matchup win rates
    /// </summary>
    public MatchupData GetMatchupData(string character1, string character2)
    {
        string key = GetMatchupKey(character1, character2);
        if (matchupHistory.ContainsKey(key) && matchupHistory[key].Count > 0)
        {
            var list = matchupHistory[key];
            return list[list.Count - 1];
        }
        return null;
    }

    /// <summary>
    /// Identify problematic matchups (>30% deviation)
    /// </summary>
    public List<(string char1, string char2, float deviation)> GetProblematicMatchups()
    {
        var problematic = new List<(string, string, float)>();

        foreach (var kvp in matchupHistory)
        {
            var list = kvp.Value;
            if (list.Count < 3) continue; // Need at least 3 matches to be meaningful

            var latest = list[list.Count - 1];
            float deviation = Mathf.Abs(latest.character1WinRate - 50f) / 50f;

            if (deviation > 0.3f)
            {
                string favored = latest.character1WinRate > 50f ? latest.character1 : latest.character2;
                string unfavored = latest.character1WinRate > 50f ? latest.character2 : latest.character1;
                problematic.Add((favored, unfavored, deviation));
            }
        }

        return problematic;
    }

    /// <summary>
    /// Get character tier based on balance report
    /// </summary>
    public char GetCharacterTier(string characterName)
    {
        if (!balanceReports.ContainsKey(characterName))
            AnalyzeCharacter(characterName);

        if (!balanceReports.ContainsKey(characterName))
            return 'D';

        var report = balanceReports[characterName];
        return report.status switch
        {
            BalanceStatus.Severely_Overpowered => 'S',
            BalanceStatus.Overpowered => 'A',
            BalanceStatus.Slightly_Overpowered => 'B',
            BalanceStatus.Balanced => 'C',
            BalanceStatus.Slightly_Underpowered => 'B',
            BalanceStatus.Underpowered => 'D',
            BalanceStatus.Severely_Underpowered => 'F',
            _ => 'C'
        };
    }

    // Private methods

    private void GenerateRecommendations(BalanceReport report, StatisticsTracker.CharacterStats stats)
    {
        if (report.winRate > 55f)
            report.recommendations.Add("Consider reducing damage output");
        if (report.winRate > 60f)
            report.recommendations.Add("Increase cooldowns on special moves");
        if (report.hitAccuracy > 70f && stats.totalHitsLanded > 20)
            report.recommendations.Add("Increase attack startup frames or reduce range");

        if (report.winRate < 45f)
            report.recommendations.Add("Consider increasing damage output");
        if (report.winRate < 40f)
            report.recommendations.Add("Reduce cooldowns on special moves");
        if (report.hitAccuracy < 40f)
            report.recommendations.Add("Increase attack range or reduce startup frames");

        if (report.blockSuccessRate > 80f)
            report.recommendations.Add("Block may be too strong - review block stamina");
        if (stats.timesBlockBroken > stats.totalBlocksSuccessful * 0.5f)
            report.recommendations.Add("Block break is too frequent - review scaling");

        if (report.comboFrequency > 1f)
            report.recommendations.Add("Combo output may be excessive");
        if (report.comboFrequency < 0.2f && stats.totalGameTime > 10f)
            report.recommendations.Add("Combos underutilized - check scaling or difficulty");
    }

    private string GetMatchupKey(string char1, string char2)
    {
        // Ensure consistent ordering
        if (string.Compare(char1, char2) > 0)
            return $"{char2}_vs_{char1}";
        return $"{char1}_vs_{char2}";
    }

    private void UpdateMatchupWinRates(string key)
    {
        var list = matchupHistory[key];
        int wins1 = 0, wins2 = 0;

        foreach (var matchup in list)
        {
            wins1 += matchup.character1Wins;
            wins2 += matchup.character2Wins;
        }

        var latest = list[list.Count - 1];
        int totalMatches = wins1 + wins2;

        latest.character1WinRate = totalMatches > 0 ? (wins1 / (float)totalMatches) * 100f : 50f;
        latest.character2WinRate = totalMatches > 0 ? (wins2 / (float)totalMatches) * 100f : 50f;
    }
}
