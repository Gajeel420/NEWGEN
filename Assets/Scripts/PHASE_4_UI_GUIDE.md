# Phase 4: Game Flow & UI Integration Guide

This guide explains how to set up UI, menus, and game flow management for your fighting game.

---

## What's New in Phase 4

- **GameFlowManager.cs** — Manages game states, round timers, and match logic
- **UIManager.cs** — Central hub for all UI elements and screens
- **HealthBarUI.cs** — Displays character health with smooth animation
- **ComboCounterUI.cs** — Shows combo count and damage multiplier in real-time
- **MainMenuScreen.cs** — Main menu with start/settings/exit options
- **PauseMenuScreen.cs** — Pause menu with resume/menu options
- **GameOverScreen.cs** — Match end screen with winner and replay options

---

## Architecture

### Game Flow Pipeline

```
MainMenu → CharacterSelect → Loading → Fighting ↔ Paused → RoundEnd → MatchEnd → GameOver → Menu
                                           ↑                                              ↓
                                           └──────────────────────────────────────────────┘
```

### UI Hierarchy

```
GameFlowManager (Singleton)
├── OnGameStateChanged events
├── OnTimeUpdated events
└── OnRoundStart/End/MatchEnd events
        ↓
UIManager (Singleton)
├── MainMenuCanvas
│   └── MainMenuScreen
├── PauseMenuCanvas
│   └── PauseMenuScreen
├── GameOverCanvas
│   └── GameOverScreen
└── HUDCanvas
    ├── HealthBar P1 (HealthBarUI)
    ├── HealthBar P2 (HealthBarUI)
    ├── ComboCounter P1 (ComboCounterUI)
    ├── ComboCounter P2 (ComboCounterUI)
    ├── RoundTimer (Text)
    └── RoundCounter (Text)
```

---

## Step 1: Create Game Flow Manager

### 1.1 Setup in Scene
1. Create an empty GameObject: `GameFlowManager`
2. Add the **GameFlowManager.cs** script
3. Configure in Inspector:
   - **Round Duration**: 60 (seconds)
   - **Max Rounds**: 3
   - **Round Win Delay**: 2 (seconds before showing next round)

### 1.2 Key Properties
- `currentGameState` — Current game state (Menu, Fighting, Paused, etc.)
- `roundTimeRemaining` — Seconds left in current round
- `currentRound` — Which round number (1, 2, 3)
- `activeFighters` — List of characters in the match

### 1.3 Events (Subscribe to these)
```csharp
GameFlowManager.Instance.OnGameStateChanged += HandleStateChange;
GameFlowManager.Instance.OnTimeUpdated += HandleTime;
GameFlowManager.Instance.OnRoundStart += HandleRoundStart;
GameFlowManager.Instance.OnRoundEnd += HandleRoundEnd;
GameFlowManager.Instance.OnMatchEnd += HandleMatchEnd;
```

---

## Step 2: Create UI Manager

### 2.1 Setup in Scene
1. Create a Canvas: Right-click in Hierarchy → **UI** → **Canvas**
2. Name it `UIRoot`
3. Create empty GameObject as child: `UIManager`
4. Add **UIManager.cs** script to UIManager object

### 2.2 Configure UIManager Inspector
Drag Canvas references:
- **Main Menu Canvas** → (create as child of UIRoot)
- **Pause Menu Canvas** → (create as child of UIRoot)
- **Game Over Canvas** → (create as child of UIRoot)
- **HUD Canvas** → (create as child of UIRoot)

Also assign Text references:
- **Title Text** → (for "Player wins round!" messages)
- **Round Timer Text** → (displays MM:SS format)
- **Round Counter Text** → (displays "Round 1", "Round 2", etc.)

---

## Step 3: Create Canvas for Each Screen

### 3.1 Create Main Menu Canvas
1. Right-click `UIRoot` → **UI** → **Panel**
2. Name it `MainMenuCanvas`
3. Add **MainMenuScreen.cs** script
4. Create buttons as children:
   - **Start Button** (Button component)
   - **Settings Button** (Button component)
   - **Exit Button** (Button component)
5. Configure MainMenuScreen inspector:
   - Drag canvas reference to **Canvas Group**
   - Drag buttons to their slots

### 3.2 Create HUD Canvas
1. Right-click `UIRoot` → **UI** → **Panel**
2. Name it `HUDCanvas`
3. Create child elements:
   - **HealthBar_P1** (Image + HealthBarUI script)
   - **HealthBar_P2** (Image + HealthBarUI script)
   - **ComboCounter_P1** (Text + ComboCounterUI script)
   - **ComboCounter_P2** (Text + ComboCounterUI script)
   - **RoundTimer** (Text)
   - **RoundCounter** (Text)

### 3.3 Create Pause Menu Canvas
1. Right-click `UIRoot` → **UI** → **Panel**
2. Name it `PauseMenuCanvas`
3. Add **PauseMenuScreen.cs** script
4. Create buttons:
   - **Resume Button**
   - **Settings Button**
   - **Menu Button**

### 3.4 Create Game Over Canvas
1. Right-click `UIRoot` → **UI** → **Panel**
2. Name it `GameOverCanvas`
3. Add **GameOverScreen.cs** script
4. Create text and buttons:
   - **Winner Name Text** (Text)
   - **Winner Status Text** (Text)
   - **Replay Button** (Button)
   - **Menu Button** (Button)

---

## Step 4: Configure Health Bars

### 4.1 Setup Health Bar UI
1. Select **HealthBar_P1** in hierarchy
2. Add **HealthBarUI.cs** script
3. Configure inspector:
   - **Character** → (will auto-detect)
   - **Health Fill Image** → Create a child Image for the fill
   - **Damage Delay Image** → (optional, shows damage)
   - **Health Text** → Text element showing "100/100"

### 4.2 Create Health Bar Prefab
1. Structure: Panel
   - Background Image (dark gray)
   - Fill Image (green, nested under panel)
   - Health Text (white text showing health)

### 4.3 Setup Second Health Bar
Duplicate P1 health bar and mirror position for P2

---

## Step 5: Configure Combo Counters

### 5.1 Setup Combo Counter UI
1. Select **ComboCounter_P1** in hierarchy
2. Add **ComboCounterUI.cs** script
3. Configure inspector:
   - **Character** → (will auto-detect)
   - **Attack Manager** → (will auto-detect)
   - **Combo Count Text** → Text element
   - **Combo Multiplier Text** → Text element for damage multiplier display

### 5.2 Positioning
Position combo counters:
- **P1**: Top-left area
- **P2**: Top-right area

---

## Step 6: Connect Everything

### Initialization Order (Auto-happening)
1. **Scene Start**
   - GameFlowManager creates singleton
   - UIManager creates singleton
   - Subscribes to GameFlowManager events

2. **On Game State Change**
   - UIManager shows/hides appropriate canvas
   - HUD elements initialize

3. **On Round Start**
   - GameFlowManager calls OnRoundStart event
   - UIManager associates health bars with fighters
   - Timer starts counting down

---

## Step 7: Test the Flow

### Manual Testing
1. **Start the Scene** (with test match setup)
2. **Verify Menu Shows** → Should see main menu
3. **Click Start** → Should transition to fight screen
4. **Verify HUD Elements**:
   - Health bars visible and updating
   - Combo counter appears on hits
   - Timer counting down
5. **Press ESC** → Should pause and show pause menu
6. **Press Resume** → Should unpause
7. **Win Match** → Should show game over screen

### Expected Console Logs
```
[ROUND START] Round 1 started
[TIMER] Time: 59, 58, 57...
[COMBO DETECTED] Light → Light (x2 multiplier)
[ROUND END] Character wins!
[MATCH END] 2 rounds won
```

---

## Configuration Guide

### Round Duration
```csharp
// In GameFlowManager Inspector
Round Duration: 60 seconds (standard 1-minute round)
// For testing, use 30 seconds
```

### Round Structure
```
Round: 60 seconds
├── Fighting: 0-60s (player actions)
├── Round End Detection:
│   └── Timer runs out: Highest health wins
│   └── KO: Remaining fighter wins
```

### Health Bar Colors
```csharp
// In HealthBarUI Inspector
Healthy Color: Green (>50% health)
Warn Color: Yellow (25-50% health)
Critical Color: Red (<25% health)
```

### Combo Counter Colors
```csharp
// In ComboCounterUI Inspector
Active Combo Color: Cyan (when combo active)
Inactive Combo Color: Gray (no combo)
```

---

## Complete Character Integration Checklist

- [ ] GameFlowManager created and configured
- [ ] UIManager created with canvas references assigned
- [ ] Main menu canvas with buttons functional
- [ ] HUD canvas with health bars positioned
- [ ] Health bar shows fighter health correctly
- [ ] Combo counter shows hits and multiplier
- [ ] Round timer displays MM:SS format
- [ ] Pause menu works (ESC key)
- [ ] Game over screen shows winner
- [ ] Replay button restarts match
- [ ] Menu button returns to main menu

---

## Advanced: Custom Events

### Subscribe to Game Events in Your Scripts
```csharp
void Start()
{
    GameFlowManager.Instance.OnGameStateChanged += MyCustomHandler;
}

void MyCustomHandler(GameFlowManager.GameState newState)
{
    if (newState == GameFlowManager.GameState.Fighting)
    {
        Debug.Log("Combat started!");
        // Play music, show effects, etc.
    }
}
```

---

## State Flow Diagram

```
┌─────────────────────────────────────────────┐
│           MAIN MENU STATE                   │
│  (Player sees main menu, can start)        │
└──────────────┬──────────────────────────────┘
               │ Start Button Clicked
               ▼
┌─────────────────────────────────────────────┐
│       CHARACTER SELECT STATE                │
│  (Player selects fighters - Phase 5)       │
└──────────────┬──────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────┐
│         LOADING STATE                       │
│  (Load assets, prepare fighters)           │
└──────────────┬──────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────┐
│         FIGHTING STATE                      │
│  (Active gameplay, UI shows HUD)           │
│  ┌──────────────────────────────────────┐  │
│  │  ESC → PAUSED STATE                  │  │
│  │  ┌────────────────────────────────┐  │  │
│  │  │ (Pause menu, can resume/quit) │  │  │
│  │  │ Resume → back to FIGHTING      │  │  │
│  │  │ Menu → back to MAIN MENU       │  │  │
│  │  └────────────────────────────────┘  │  │
│  │  KO/Time Up → ROUND END               │  │
│  └──────────────────────────────────────┘  │
└──────────────┬──────────────────────────────┘
               │ If more rounds: Go to next round
               │ If final round: Go to MATCH END
               ▼
┌─────────────────────────────────────────────┐
│       ROUND END STATE                       │
│  (Show round winner, brief delay)          │
└──────────────┬──────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────┐
│       MATCH END STATE                       │
│  (Determine match winner)                  │
└──────────────┬──────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────┐
│      GAMEOVER STATE                         │
│  (Show winner, replay/menu buttons)        │
│  Replay → back to CHARACTER SELECT         │
│  Menu → back to MAIN MENU                  │
└─────────────────────────────────────────────┘
```

---

## UI Text Format Reference

### Timer Display
```
Format: MM:SS
Examples:
01:23 (1 minute, 23 seconds)
00:45 (45 seconds)
00:05 (5 seconds, critical)
```

### Combo Display
```
Format: COMBO x{count}
Examples:
COMBO x2 (double hit)
COMBO x5 (5-hit combo)
```

### Damage Multiplier
```
Format: {scalar}x DAMAGE
Examples:
1.10x DAMAGE (first combo hit)
1.61x DAMAGE (5-hit combo)
```

---

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Health bar not updating | Verify Character.OnHealthChanged event is firing |
| UI elements not visible | Check CanvasGroup alpha values and RectTransforms |
| Timer not counting | Verify GameFlowManager is in scene and enabled |
| Pause not working | Ensure Time.timeScale is being set correctly |
| Combo counter stuck | Check AttackManager.GetComboCount() returns 0 when combo resets |
| Game over screen doesn't show | Verify GameOverScreen script attached and buttons configured |

---

## Next Steps

1. **Create aesthetic designs** for menus and HUD elements
2. **Add audio** when buttons are clicked
3. **Implement settings menu** (volume, controls rebinding)
4. **Add character select screen** (Phase 5)
5. **Improve visuals** with animations and particle effects

---

**Phase 4 Complete!** Your game now has full UI and game flow management. Ready for Phase 5: Enhanced Features & Polish.
