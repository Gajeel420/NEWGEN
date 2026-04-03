using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// UI for displaying balance test results, statistics, and diagnostics.
/// Shows character balance status, matchup data, and performance metrics.
/// </summary>
public class BalanceTestUI : MonoBehaviour
{
    [SerializeField] private Text balanceReportText;
    [SerializeField] private Text performanceReportText;
    [SerializeField] private Text matchupReportText;
    [SerializeField] private Text tierListText;
    [SerializeField] private Text statisticsText;

    [SerializeField] private Button refreshButton;
    [SerializeField] private Button exportButton;
    [SerializeField] private Button clearButton;

    [SerializeField] private Toggle showDetailedStatsToggle;
    [SerializeField] private Toggle showProblematicMatchupsToggle;

    private BalanceTestManager balanceTestManager;
    private BalanceAnalyzer balanceAnalyzer;
    private PerformanceProfiler performanceProfiler;

    private bool showDetailedStats = false;
    private bool showProblematicMatchups = true;

    private void OnEnable()
    {
        balanceTestManager = BalanceTestManager.Instance;
        balanceAnalyzer = BalanceAnalyzer.Instance;
        performanceProfiler = PerformanceProfiler.Instance;

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshAllReports);
        if (exportButton != null)
            exportButton.onClick.AddListener(ExportReport);
        if (clearButton != null)
            clearButton.onClick.AddListener(ClearReports);

        if (showDetailedStatsToggle != null)
            showDetailedStatsToggle.onValueChanged.AddListener((value) => { showDetailedStats = value; RefreshAllReports(); });
        if (showProblematicMatchupsToggle != null)
            showProblematicMatchupsToggle.onValueChanged.AddListener((value) => { showProblematicMatchups = value; RefreshAllReports(); });
    }

    private void OnDisable()
    {
        if (refreshButton != null)
            refreshButton.onClick.RemoveListener(RefreshAllReports);
        if (exportButton != null)
            exportButton.onClick.RemoveListener(ExportReport);
        if (clearButton != null)
            clearButton.onClick.RemoveListener(ClearReports);
    }

    public void RefreshAllReports()
    {
        RefreshBalanceReport();
        RefreshPerformanceReport();
        RefreshMatchupReport();
        RefreshTierList();
        RefreshStatistics();
    }

    private void RefreshBalanceReport()
    {
        if (balanceTestManager == null || balanceReportText == null)
            return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>BALANCE REPORT</b>");
        sb.AppendLine();

        var reports = balanceTestManager.GetBalanceReport();
        if (reports == null || reports.Count == 0)
        {
            sb.AppendLine("No balance data available.");
        }
        else
        {
            foreach (var kvp in reports)
            {
                var report = kvp.Value;
                
                // Color code by status
                string statusColor = report.status switch
                {
                    BalanceAnalyzer.BalanceStatus.Severely_Overpowered => "red",
                    BalanceAnalyzer.BalanceStatus.Overpowered => "yellow",
                    BalanceAnalyzer.BalanceStatus.Slightly_Overpowered => "orange",
                    BalanceAnalyzer.BalanceStatus.Balanced => "green",
                    BalanceAnalyzer.BalanceStatus.Slightly_Underpowered => "orange",
                    BalanceAnalyzer.BalanceStatus.Underpowered => "yellow",
                    BalanceAnalyzer.BalanceStatus.Severely_Underpowered => "red",
                    _ => "white"
                };

                sb.AppendLine($"<b><color={statusColor}>{report.characterName}</color></b>");
                sb.AppendLine($"  Status: <color={statusColor}>{report.status}</color>");
                sb.AppendLine($"  Win Rate: {report.winRate:F1}%");
                sb.AppendLine($"  Hit Accuracy: {report.hitAccuracy:F1}%");
                sb.AppendLine($"  DPS: {report.damageOutput:F2}");
                sb.AppendLine($"  Block Success: {report.blockSuccessRate:F1}%");

                if (showDetailedStats && report.recommendations.Count > 0)
                {
                    sb.AppendLine("  Recommendations:");
                    foreach (var rec in report.recommendations)
                        sb.AppendLine($"    • {rec}");
                }
                sb.AppendLine();
            }
        }

        balanceReportText.text = sb.ToString();
    }

    private void RefreshPerformanceReport()
    {
        if (performanceProfiler == null || performanceReportText == null)
            return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>PERFORMANCE REPORT</b>");
        sb.AppendLine();

        var (avgFPS, avgFrameTime, memoryMB) = balanceTestManager != null ? 
            balanceTestManager.GetPerformanceReport() : (0f, 0f, 0f);

        var (frameAvg, frameMin, frameMax) = performanceProfiler.GetFrameTimeStats();
        float spikePercentage = performanceProfiler.GetFrameTimeSpikesPercentage();

        sb.AppendLine($"Average FPS: <b>{avgFPS:F1}</b>");
        sb.AppendLine($"Frame Time: {frameAvg:F2}ms (min: {frameMin:F2}ms, max: {frameMax:F2}ms)");
        sb.AppendLine($"Frame Spikes (>16.7ms): <b>{spikePercentage:F1}%</b>");
        sb.AppendLine($"Memory Usage: {memoryMB:F1} MB");

        if (spikePercentage > 10f)
            sb.AppendLine("<color=orange>⚠ High frame time variance detected</color>");

        performanceReportText.text = sb.ToString();
    }

    private void RefreshMatchupReport()
    {
        if (balanceAnalyzer == null || matchupReportText == null)
            return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>MATCHUP ANALYSIS</b>");
        sb.AppendLine();

        if (!showProblematicMatchups)
        {
            sb.AppendLine("Matchup analysis disabled");
            matchupReportText.text = sb.ToString();
            return;
        }

        var problematic = balanceTestManager != null ? 
            balanceTestManager.GetProblematicMatchups() : 
            new List<(string, string, float)>();

        if (problematic.Count == 0)
        {
            sb.AppendLine("All matchups are balanced (≤30% deviation)");
        }
        else
        {
            sb.AppendLine($"Found {problematic.Count} problematic matchups:");
            sb.AppendLine();
            foreach (var (char1, char2, deviation) in problematic)
            {
                sb.AppendLine($"<color=red>{char1} vs {char2}</color>");
                sb.AppendLine($"  Win rate deviation: {deviation * 100f:F1}%");
            }
        }

        matchupReportText.text = sb.ToString();
    }

    private void RefreshTierList()
    {
        if (balanceTestManager == null || tierListText == null)
            return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>TIER LIST</b>");
        sb.AppendLine();

        var tierList = balanceTestManager.GetTierList();
        var tierOrder = new[] { 'S', 'A', 'B', 'C', 'D', 'F' };

        foreach (var tier in tierOrder)
        {
            if (tierList.ContainsKey(tier) && tierList[tier].Count > 0)
            {
                string tierColor = tier switch
                {
                    'S' => "red",
                    'A' => "orange",
                    'B' => "yellow",
                    'C' => "green",
                    'D' => "cyan",
                    'F' => "blue",
                    _ => "white"
                };

                sb.AppendLine($"<color={tierColor}><b>TIER {tier}</b></color>");
                foreach (var character in tierList[tier])
                    sb.AppendLine($"  • {character}");
            }
        }

        tierListText.text = sb.ToString();
    }

    private void RefreshStatistics()
    {
        if (statisticsText == null)
            return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>SESSION STATISTICS</b>");
        sb.AppendLine();

        var allStats = StatisticsTracker.GetAllCharacterStats();
        if (allStats == null || allStats.Count == 0)
        {
            sb.AppendLine("No statistics available.");
            statisticsText.text = sb.ToString();
            return;
        }

        foreach (var kvp in allStats)
        {
            var stats = kvp.Value;
            float dps = StatisticsTracker.GetDPS(kvp.Key);
            float accuracy = StatisticsTracker.GetHitAccuracy(kvp.Key);

            if (showDetailedStats)
            {
                sb.AppendLine($"<b>{kvp.Key}</b>");
                sb.AppendLine($"  Damage Dealt: {stats.totalDamageDealt}");
                sb.AppendLine($"  Damage Received: {stats.totalDamageReceived}");
                sb.AppendLine($"  Hits Landed: {stats.totalHitsLanded}");
                sb.AppendLine($"  Hit Accuracy: {accuracy:F1}%");
                sb.AppendLine($"  DPS: {dps:F2}");
                sb.AppendLine($"  Combos Used: {stats.totalComboCount} (highest: {stats.highestCombo})");
                sb.AppendLine($"  Special Moves: {stats.specialMovesUsed}");
                sb.AppendLine($"  Blocks: {stats.totalBlocksSuccessful} successful, {stats.totalBlocksFailed} failed");
                sb.AppendLine($"  Knockdowns: {stats.knockdownsInflicted} inflicted, {stats.knockdownsReceived} received");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"{kvp.Key}: {stats.totalHitsLanded} hits ({accuracy:F1}%) | {stats.totalDamageDealt} dmg | {dps:F2} DPS");
            }
        }

        statisticsText.text = sb.ToString();
    }

    private void ExportReport()
    {
        Debug.Log("Exporting balance test report...");
        // TODO: Export to file (JSON, CSV, or text)
    }

    private void ClearReports()
    {
        if (balanceReportText) balanceReportText.text = "Cleared";
        if (performanceReportText) performanceReportText.text = "Cleared";
        if (matchupReportText) matchupReportText.text = "Cleared";
        if (tierListText) tierListText.text = "Cleared";
        if (statisticsText) statisticsText.text = "Cleared";

        Debug.Log("Balance test reports cleared");
    }
}
