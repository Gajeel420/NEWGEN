using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages balance test sessions: tracks multiple rounds, collects data, generates reports.
/// Integrates PerformanceProfiler, StatisticsTracker, and BalanceAnalyzer.
/// </summary>
public class BalanceTestManager : MonoBehaviour
{
    public static BalanceTestManager Instance { get; private set; }

    [System.Serializable]
    public class TestSession
    {
        public string sessionName;
        public System.DateTime startTime;
        public System.DateTime endTime;
        public List<string> participantCharacters;
        public int roundsPlayed;
        public Dictionary<string, int> characterWins;
        public float averageFPS;
        public float memoryUsageMB;
    }

    [SerializeField] private PerformanceProfiler performanceProfiler;
    [SerializeField] private BalanceAnalyzer balanceAnalyzer;

    private List<TestSession> testSessions = new List<TestSession>();
    private TestSession currentSession;
    private int roundCounter = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (performanceProfiler == null)
            performanceProfiler = GetComponent<PerformanceProfiler>();
        if (balanceAnalyzer == null)
            balanceAnalyzer = GetComponent<BalanceAnalyzer>();
    }

    /// <summary>
    /// Start a new balance test session
    /// </summary>
    public void StartTestSession(string sessionName, params string[] characterNames)
    {
        currentSession = new TestSession
        {
            sessionName = sessionName,
            startTime = System.DateTime.Now,
            participantCharacters = new List<string>(characterNames),
            roundsPlayed = 0,
            characterWins = new Dictionary<string, int>()
        };

        foreach (var name in characterNames)
            currentSession.characterWins[name] = 0;

        roundCounter = 0;
        StatisticsTracker.ResetSessionStats();
        if (performanceProfiler != null)
            performanceProfiler.Reset();

        Debug.Log($"[BalanceTest] Started session: {sessionName} with {characterNames.Length} characters");
    }

    /// <summary>
    /// Record start of a test round
    /// </summary>
    public void StartTestRound(string character1, string character2)
    {
        roundCounter++;
        StatisticsTracker.StartRoundTracking(character1, character2);
        Debug.Log($"[BalanceTest] Round {roundCounter}: {character1} vs {character2}");
    }

    /// <summary>
    /// Record end of a test round with winner
    /// </summary>
    public void EndTestRound(string winnerName, string loserName)
    {
        StatisticsTracker.EndRoundTracking();
        StatisticsTracker.RecordRoundEnd(winnerName, loserName);

        if (currentSession != null)
        {
            currentSession.roundsPlayed++;
            if (currentSession.characterWins.ContainsKey(winnerName))
                currentSession.characterWins[winnerName]++;

            // Record matchup
            if (balanceAnalyzer != null)
                balanceAnalyzer.RecordMatchup(winnerName, loserName, winnerName);
        }

        Debug.Log($"[BalanceTest] Round {roundCounter} complete. Winner: {winnerName}");
    }

    /// <summary>
    /// End the current test session and generate report
    /// </summary>
    public TestSession EndTestSession()
    {
        if (currentSession == null)
            return null;

        currentSession.endTime = System.DateTime.Now;

        if (performanceProfiler != null)
        {
            currentSession.averageFPS = performanceProfiler.GetAverageFPS();
            currentSession.memoryUsageMB = performanceProfiler.GetCurrentMemoryMB();
        }

        testSessions.Add(currentSession);

        Debug.Log($"[BalanceTest] Ended session: {currentSession.sessionName} ({currentSession.roundsPlayed} rounds)");
        Debug.Log(GenerateSessionReport(currentSession));

        TestSession completedSession = currentSession;
        currentSession = null;
        return completedSession;
    }

    /// <summary>
    /// Get balance report for current session
    /// </summary>
    public Dictionary<string, BalanceAnalyzer.BalanceReport> GetBalanceReport()
    {
        if (balanceAnalyzer == null)
            return null;

        return balanceAnalyzer.AnalyzeAllCharacters();
    }

    /// <summary>
    /// Get performance report for current session
    /// </summary>
    public (float avgFPS, float avgFrameTime, float memoryMB) GetPerformanceReport()
    {
        if (performanceProfiler == null)
            return (0f, 0f, 0f);

        return (
            performanceProfiler.GetAverageFPS(),
            performanceProfiler.GetAverageFrameTime(),
            performanceProfiler.GetCurrentMemoryMB()
        );
    }

    /// <summary>
    /// Get problematic matchups in current session
    /// </summary>
    public List<(string char1, string char2, float deviation)> GetProblematicMatchups()
    {
        if (balanceAnalyzer == null)
            return new List<(string, string, float)>();

        return balanceAnalyzer.GetProblematicMatchups();
    }

    /// <summary>
    /// Export session data to readable format
    /// </summary>
    public string ExportSessionReport(TestSession session)
    {
        return GenerateSessionReport(session);
    }

    /// <summary>
    /// Get character tier list based on current session
    /// </summary>
    public Dictionary<char, List<string>> GetTierList()
    {
        var tiers = new Dictionary<char, List<string>>();
        var tierChars = new[] { 'S', 'A', 'B', 'C', 'D', 'F' };

        foreach (var tier in tierChars)
            tiers[tier] = new List<string>();

        if (currentSession != null)
        {
            foreach (var charName in currentSession.participantCharacters)
            {
                char tier = balanceAnalyzer.GetCharacterTier(charName);
                if (tiers.ContainsKey(tier))
                    tiers[tier].Add(charName);
            }
        }

        return tiers;
    }

    /// <summary>
    /// Get all test sessions
    /// </summary>
    public List<TestSession> GetTestSessions()
    {
        return new List<TestSession>(testSessions);
    }

    // Private methods

    private string GenerateSessionReport(TestSession session)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("========== BALANCE TEST REPORT ==========");
        sb.AppendLine($"Session: {session.sessionName}");
        sb.AppendLine($"Date: {session.startTime:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Duration: {(session.endTime - session.startTime).TotalSeconds:F1}s");
        sb.AppendLine($"Rounds: {session.roundsPlayed}");
        sb.AppendLine();

        sb.AppendLine("--- CHARACTER WINS ---");
        foreach (var kvp in session.characterWins)
        {
            float winRate = session.roundsPlayed > 0 ? (kvp.Value / (float)session.roundsPlayed) * 100f : 0f;
            sb.AppendLine($"{kvp.Key}: {kvp.Value} wins ({winRate:F1}%)");
        }
        sb.AppendLine();

        sb.AppendLine("--- PERFORMANCE ---");
        sb.AppendLine($"Avg FPS: {session.averageFPS:F1}");
        sb.AppendLine($"Memory: {session.memoryUsageMB:F1} MB");
        sb.AppendLine();

        // Balance analysis
        var reports = GetBalanceReport();
        if (reports != null && reports.Count > 0)
        {
            sb.AppendLine("--- BALANCE ANALYSIS ---");
            foreach (var kvp in reports)
            {
                var report = kvp.Value;
                sb.AppendLine($"{report.characterName} (Status: {report.status})");
                sb.AppendLine($"  Win Rate: {report.winRate:F1}%");
                sb.AppendLine($"  Hit Accuracy: {report.hitAccuracy:F1}%");
                sb.AppendLine($"  DPS: {report.damageOutput:F2}");
                sb.AppendLine($"  Block Success Rate: {report.blockSuccessRate:F1}%");

                if (report.recommendations.Count > 0)
                {
                    sb.AppendLine("  Recommendations:");
                    foreach (var rec in report.recommendations)
                        sb.AppendLine($"    - {rec}");
                }
            }
        }

        sb.AppendLine("==========================================");
        return sb.ToString();
    }
}
