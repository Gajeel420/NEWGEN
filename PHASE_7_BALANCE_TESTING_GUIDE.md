# Phase 7: Character Balance & Testing

This guide explains the Phase 7 balance testing and profiling system for analyzing character statistics, identifying balance issues, and optimizing performance.

## System Overview

The balance testing framework provides tools for:
- **Performance Profiling:** Frame rate, memory usage, garbage collection tracking
- **Combat Statistics:** Damage, hits, combos, blocks, special moves tracking
- **Balance Analysis:** Win rates, character tiers, matchup analysis
- **Test Management:** Automated test sessions with detailed reporting

### Components

**Core Testing Systems:**
- `PerformanceProfiler.cs` — Real-time performance monitoring with frame/memory metrics
- `StatisticsTracker.cs` — Combat statistics collection per character per round
- `BalanceAnalyzer.cs` — Balance analysis with tier ranking and matchup tracking
- `BalanceTestManager.cs` — Test session management and aggregate reporting
- `CharacterBalanceConfig.cs` — ScriptableObject for balance presets and test configurations
- `BalanceTestUI.cs` — UI display for test results and diagnostics

---

## Architecture

### Performance Profiling Pipeline

```
Update Loop
    ↓
PerformanceProfiler (samples every 100ms)
    ↓
Record Frame Metrics (FPS, frame time, memory, GC)
    ↓
Detect Spikes (>16.7ms at 60fps)
    ↓
Fire Events (OnFrameSampled, OnFrameTimeSpike)
    ↓
Store History (queue of 300 samples = ~5 seconds)
```

### Statistics Collection Pipeline

```
Combat Event (Hit, Block, Damage, etc.)
    ↓
StatisticsTracker.Record* (during round)
    ↓
Store in Round Stats
    ↓
OnRoundEnd → Accumulate to Session Stats
    ↓
Generate Analysis Reports
```

### Balance Analysis Pipeline

```
Session Complete
    ↓
BalanceAnalyzer.AnalyzeAllCharacters()
    ↓
Calculate Metrics (Win Rate, DPS, Accuracy, etc.)
    ↓
Determine Status (Balanced/Over/Underpowered)
    ↓
Generate Recommendations
    ↓
Tier Ranking (S/A/B/C/D/F)
```

---

## Setup: Configuring Balance Testing

### Step 1: Setup PerformanceProfiler

1. In scene, create empty GameObject: **PerformanceProfiler**
2. Add component: **PerformanceProfiler.cs**
3. Configure Inspector:
   - **History Size:** 300 (samples for ~5 seconds at 60fps)
   - **Track Memory:** ✓ (enable memory profiling)
   - **Track GC:** ✓ (detect garbage collection)
   - **Profile Interval:** 0.1 (sample every 100ms)

### Step 2: Setup StatisticsTracker

1. Create empty GameObject: **StatisticsTracker**
2. Add component: **StatisticsTracker.cs** (static class, no UI needed)

This component is automatically used by the framework to track:
- Damage dealt/received
- Hits landed/accuracy
- Blocks and block breaks
- Special moves used
- Knockdowns
- Rounds won/lost

### Step 3: Setup BalanceAnalyzer

1. Create empty GameObject: **BalanceAnalyzer**
2. Add component: **BalanceAnalyzer.cs**
3. Configure Inspector:
   - **Balance Threshold:** 0.1 (±10% = balanced)
   - **Overpower Threshold:** 0.15 (±15% = overpowered)
   - **Severity Threshold:** 0.25 (±25% = severely imbalanced)

### Step 4: Create Balance Configuration Asset

1. In Project, create folder: **Assets/Resources/Balance/**
2. Right-click → **Create** → **Fighting Game** → **Balance Config**
3. Name it `DefaultBalanceConfig`
4. Configure test parameters:
   - **Test Rounds Per Matchup:** 5
   - **Target Test Duration:** 60 (seconds)
   - **Record Detailed Stats:** ✓
   - **Enable Performance Profiling:** ✓

**Create Balance Presets:**
1. Click **+** under Balance Presets
2. Add preset and configure multipliers:
   - **AGGRESSIVE:** Damage ×1.25, Fast startup (×0.9), Weak blocks (×0.8)
   - **DEFENSIVE:** Damage ×0.75, Slow startup (×1.15), Strong blocks (×1.4)
   - **SPEED:** Startup ×0.75, Recovery ×0.7, Balanced damage
   - **DEFAULT:** All ×1.0 (baseline)

### Step 5: Setup BalanceTestManager

1. Create empty GameObject: **BalanceTestManager**
2. Add component: **BalanceTestManager.cs**
3. Assign in Inspector:
   - **Performance Profiler:** Link to PerformanceProfiler GameObject
   - **Balance Analyzer:** Link to BalanceAnalyzer GameObject

### Step 6: Setup BalanceTestUI (Optional)

1. In Canvas, create Panel: **BalanceTestPanel**
2. Add component: **BalanceTestUI.cs**
3. Create Text elements for each report:
   - **Balance Report Text**
   - **Performance Report Text**
   - **Matchup Report Text**
   - **Tier List Text**
   - **Statistics Text**
4. Create Buttons:
   - **Refresh Button** (refresh all reports)
   - **Export Button** (export data)
   - **Clear Button** (clear displays)
5. Create Toggles:
   - **Show Detailed Stats Toggle**
   - **Show Problematic Matchups Toggle**
6. Assign all UI elements in Inspector

---

## Usage

### Running Balance Tests

**In Code (BalanceTestManager):**

```csharp
// Start a test session
BalanceTestManager.Instance.StartTestSession("vs_RyuVsKen", "Ryu", "Ken");

// Run multiple rounds
for (int i = 0; i < 5; i++)
{
    BalanceTestManager.Instance.StartTestRound("Ryu", "Ken");
    
    // Battle logic here...
    // Events trigger StatisticsTracker automatically
    // PerformanceProfiler samples in background
    
    string winner = DetermineBattleWinner();
    BalanceTestManager.Instance.EndTestRound(winner, winner == "Ryu" ? "Ken" : "Ryu");
}

// End session and get report
var session = BalanceTestManager.Instance.EndTestSession();
```

### Tracking Combat Statistics

**Automatic event-driven tracking:**

```csharp
// In Character, when hit lands:
StatisticsTracker.RecordDamageDealt(characterName, damageAmount);
StatisticsTracker.RecordHitLanded(opponentName);

// In BlockingSystem, when block activates:
StatisticsTracker.RecordBlockSuccessful(characterName);

// In KnockdownRecoverySystem, when knocked down:
StatisticsTracker.RecordKnockdown(attackerName, victimName);
```

### Analyzing Balance

**Get character balance report:**

```csharp
// Single character analysis
var report = BalanceAnalyzer.Instance.AnalyzeCharacter("Ryu");
Debug.Log($"Win Rate: {report.winRate}%");
Debug.Log($"Status: {report.status}");

// All characters
var allReports = BalanceTestManager.Instance.GetBalanceReport();
foreach (var kvp in allReports)
{
    Debug.Log($"{kvp.Key}: {kvp.Value.status}");
}
```

**Get tier list:**

```csharp
var tierList = BalanceTestManager.Instance.GetTierList();
foreach (var tier in tierList)
{
    Debug.Log($"Tier {tier.Key}: {string.Join(", ", tier.Value)}");
}
```

### Performance Monitoring

**Check performance metrics:**

```csharp
// Average FPS and frame time
float avgFPS = PerformanceProfiler.Instance.GetAverageFPS();
float avgFrameTime = PerformanceProfiler.Instance.GetAverageFrameTime();

// Min/max frame times
var (avg, min, max) = PerformanceProfiler.Instance.GetFrameTimeStats();

// Spike detection (% of frames exceeding 16.7ms)
float spikePercent = PerformanceProfiler.Instance.GetFrameTimeSpikesPercentage();

// Memory usage
float memoryMB = PerformanceProfiler.Instance.GetCurrentMemoryMB();
```

### Applying Balance Presets

**Apply preset to character:**

```csharp
CharacterBalanceConfig config = Resources.Load<CharacterBalanceConfig>("Balance/DefaultBalanceConfig");

// Get preset
var aggressivePreset = config.GetPreset("AGGRESSIVE");

// Apply to character stats
var charStats = GetComponent<CharacterStats>();
config.ApplyPreset(charStats, aggressivePreset);
```

---

## Balance Analysis Metrics

### Character Metrics

| Metric | Formula | What it Means |
|--------|---------|---------------|
| **Win Rate** | (Wins / Total Rounds) × 100 | Overall character strength |
| **Hit Accuracy** | (Hits Landed / Total Attacks) × 100 | Attack effectiveness |
| **DPS** | Total Damage / Game Time (seconds) | Damage output/second |
| **Block Success Rate** | (Successful Blocks / Total Blocks) × 100 | Defensive strength |
| **Combo Frequency** | Combos Per Second | How often combos connect |
| **Special Move Frequency** | Special Moves Per Second | Special move accessibility |

### Balance Status Definitions

| Status | Win Rate Deviation | Meaning |
|--------|-------------------|---------|
| **Severely Overpowered** | >±25% | Dominates all matchups - urgent nerf |
| **Overpowered** | ±15% to ±25% | Wins too often - reduce strengths |
| **Slightly Overpowered** | ±10% to ±15% | Minor advantage - monitor closely |
| **Balanced** | ±0% to ±10% | Ideal state - no changes needed |
| **Slightly Underpowered** | ±10% to ±15% | Minor disadvantage - monitor |
| **Underpowered** | ±15% to ±25% | Loses too often - increase strengths |
| **Severely Underpowered** | >±25% | Cannot compete - urgent buff |

### Tier List (S to F)

```
Tier S: Severely Overpowered (Win Rate >65%)
Tier A: Overpowered (Win Rate 55-65%)
Tier B: Slightly Over/Under (Win Rate 45-55%, minor deviations)
Tier C: Balanced (Win Rate exactly 50%)
Tier D: Underpowered (Win Rate 35-45%)
Tier F: Severely Underpowered (Win Rate <35%)
```

---

## Balance Testing Workflow

### 1. Baseline Testing (Phase 1)

```
Run 5 rounds each character vs every other character
Record all statistics
Generate balance report
Identify problem characters
```

### 2. Apply Adjustments (Phase 2)

```
For each problem character:
  - Count recommendations from analysis
  - Apply changes (e.g., +5% damage, faster startup)
  - Note change in config
```

### 3. Regression Testing (Phase 3)

```
Re-run same matchups with new settings
Compare win rates to baseline
Check if the character moved toward 50% win rate
Ensure no new problems created
```

### 4. Matchup Testing (Phase 4)

```
Identify problematic matchups (>30% win rate deviation)
Run extended testing (10+ rounds) on problem matchups
May need character-specific adjustments
```

### 5. Final Validation (Phase 5)

```
Run full tournament simulation
All characters, all matchups, 3 rounds each
Generate final report
Should have all characters within ±10% of 50% win rate
```

---

## Troubleshooting

### "No balance data available"
- **Check:** Did you call `StartTestSession()` and run rounds?
- **Check:** Did you call `EndTestRound()` to record results?
- **Check:** Is `StatisticsTracker` properly initialized?

### Frame spikes detected (>20% frames exceeding 16.7ms)
- **Check:** Are you pooling audio sources in AudioManager?
- **Check:** Check particle effect count during combat
- **Check:** Review combo frequency - too many simultaneous events?
- **Solution:** Reduce particle effect count or pool size

### Tier list shows all characters as Tier A/F
- **Check:** Did you run enough rounds (minimum 5-10 per matchup)?
- **Check:** Are win rate calculations recording correctly?
- **Check:** Test with different thresholds in BalanceAnalyzer

### Memory usage constantly increasing
- **Check:** Are StatisticsTracker dictionaries growing unbounded?
- **Solution:** Call `ResetSessionStats()` between test sessions
- **Check:** Are UI Text updates causing string allocation?
- **Solution:** Use StringBuilder and update text less frequently

### Performance metrics show 0 FPS
- **Check:** Is PerformanceProfiler added to scene?
- **Check:** Is game running? (Time.deltaTime = 0 during pause)
- **Check:** Frame history empty - run profiler for ~5 seconds first

---

## Example: Complete Balance Test Scenario

```csharp
// Setup
var config = Resources.Load<CharacterBalanceConfig>("Balance/DefaultBalanceConfig");
BalanceTestManager.Instance.StartTestSession("Phase7_Baseline", "Ryu", "Ken", "Chun-Li");

// Test all matchups
string[] characters = { "Ryu", "Ken", "Chun-Li" };
for (int i = 0; i < characters.Length; i++)
{
    for (int j = i + 1; j < characters.Length; j++)
    {
        string char1 = characters[i];
        string char2 = characters[j];
        
        // Run 5 rounds per matchup
        for (int round = 0; round < 5; round++)
        {
            BalanceTestManager.Instance.StartTestRound(char1, char2);
            
            // Battle simulation...
            int damage1 = UnityEngine.Random.Range(10, 30);
            int damage2 = UnityEngine.Random.Range(10, 30);
            
            StatisticsTracker.RecordDamageDealt(char1, damage1);
            StatisticsTracker.RecordDamageReceived(char2, damage1);
            StatisticsTracker.RecordDamageDealt(char2, damage2);
            StatisticsTracker.RecordDamageReceived(char1, damage2);
            
            string winner = damage1 > damage2 ? char1 : char2;
            BalanceTestManager.Instance.EndTestRound(winner, winner == char1 ? char2 : char1);
        }
    }
}

// Generate reports
var session = BalanceTestManager.Instance.EndTestSession();
var reports = BalanceTestManager.Instance.GetBalanceReport();
var tierList = BalanceTestManager.Instance.GetTierList();

Debug.Log(session.sessionName + " complete!");
Debug.Log("Tier list generated");
```

---

## Next Steps

After Phase 7 Balance & Testing is complete:

**Phase 8: Release Packaging**
- Build standalone executable
- Generate balance documentation
- Create video tutorials
- Package for distribution

**Ongoing Activities:**
- Monitor player feedback post-release
- Collect real match data for future patches
- Plan character DLC or balance updates
- Maintain balance through patch cycles

