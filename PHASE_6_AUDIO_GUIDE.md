# Phase 6: Audio System

This guide explains the Phase 6 audio system and how to integrate music, sound effects, voice announcements, and settings persistence.

## System Overview

The audio system provides centralized management for all in-game sounds:
- **Music:** State-based track management with smooth transitions
- **Sound Effects:** Pooled audio sources for efficient playback
- **Voice Lines:** Announcer commentary and game announcements
- **Audio Settings:** Player volume preferences with persistence

### Components

**Core Audio Management:**
- `AudioManager.cs` — Singleton for all audio playback (music, SFX, voice)
- `SoundEffectsLibrary.cs` — ScriptableObject organizing all game sounds by category

**Music & Settings:**
- `MusicManager.cs` — State-based music transitions integrated with GameFlowManager
- `AudioSettings.cs` — Volume persistence using PlayerPrefs

**Voice & Events:**
- `VoiceAnnouncerSystem.cs` — Game announcements triggered by GameFlowManager events
- `AudioEventIntegrator.cs` — Event-driven audio system responding to gameplay

---

## Architecture

### Audio Pipeline

```
Game Event (OnHit, OnBlock, etc.)
    ↓
AudioEventIntegrator (Listen & Trigger)
    ↓
AudioManager.PlaySFX() / PlayVoice()
    ↓
SoundEffectsLibrary (Lookup sound clip + settings)
    ↓
AudioSource Pool (Recycle 4 audio sources efficiently)
    ↓
Speaker Output
```

### Music State Transitions

```
Menu State → PlayMenuMusic()
    ↓
OnGameStart → PlayFightMusic()
    ↓
OnRoundEnd (Victory/Defeat) → PlayVictoryMusic() / PlayDefeatMusic()
    ↓
OnMatchEnd → Return to Menu Music
```

### Volume System

Four independent volume channels:
- **Master Volume:** 0-1 (global volume)
- **Music Volume:** 0-1 (background tracks)
- **SFX Volume:** 0-1 (effects and impacts)
- **Voice Volume:** 0-1 (announcements and lines)

Each has a **Mute** toggle stored in PlayerPrefs.

---

## Setup: Configuring Audio System

### Step 1: Create SoundEffectsLibrary Asset

1. In Project, create folder: **Assets/Resources/Audio/**
2. Right-click → **Create** → **ScriptableObject** → **SoundEffectsLibrary**
3. Name it `DefaultSoundLibrary`
4. Configure sound entries in the Inspector:

**Attack Sounds:**
- Cut/Slash (light attack)
- Heavy Slash (medium attack)
- Overhead Smash (heavy attack)

**Hit/Impact Sounds:**
- Hit (successful landing)
- Block Hit (blocked attack)
- Knockdown Impact (hard hit)

**UI Sounds:**
- Button Click
- Menu Select
- Victory Fanfare

**Special Move Sounds:**
- Fireball Launch
- Spinning Attack Start
- Power Charge

5. For each entry, assign:
   - **Clip:** AudioClip reference
   - **Volume Scale:** 0.5-1.0 (multiplies master SFX volume)
   - **Pitch Range:** 1.0 ± 0.05 (random variation)

### Step 2: Configure Music Tracks

1. Create folder: **Assets/Resources/Music/**
2. Import music clips:
   - **MenuMusic** (looping, calm)
   - **FightMusic** (looping, intense)
   - **VictoryMusic** (looping, uplifting)
   - **DefeatMusic** (looping, somber)

### Step 3: Configure Voice Lines

1. Create folder: **Assets/Resources/Voice/**
2. Import announcer clips:
   - **RoundStart** (e.g., "Round 1!", "Round 2!")
   - **FightStart** ("FIGHT!")
   - **Countdown** ("3!", "2!", "1!")
   - **Victory** ("VICTORY!", "WINNER!")
   - **Defeat** ("DEFEAT...", "K.O.!")

### Step 4: Setup AudioManager in Scene

1. Create empty GameObject: **AudioManager**
2. Add component: **AudioManager.cs** (script)
3. Create 4 child GameObjects named: **SFX_Source_1**, **SFX_Source_2**, **SFX_Source_3**, **SFX_Source_4**
4. Add **AudioSource** component to each (set **Spatial Blend** to 2D)
5. In AudioManager Inspector, assign all 4 SFX sources to the pool array
6. Set **Music Source** to a separate AudioSource on the AudioManager
7. Assign **SoundEffectsLibrary** from Resources/Audio/

### Step 5: Setup Other Audio Systems

1. Create empty GameObject: **MusicManager**
   - Add component: **MusicManager.cs**
   - Assign music clips in Inspector (Menu, Fight, Victory, Defeat)

2. Create empty GameObject: **VoiceAnnouncerSystem**
   - Add component: **VoiceAnnouncerSystem.cs**
   - Assign announcer clips in Inspector arrays

3. Create empty GameObject: **AudioEventIntegrator**
   - Add component: **AudioEventIntegrator.cs**
   - This subscribes to events automatically in OnEnable()

4. In any menu/settings UI, reference **AudioSettings.cs** for volume controls

---

## Usage

### Playing Sound Effects

**By Audio Event (Automatic):**
```csharp
// AudioEventIntegrator listens to BlockingSystem events automatically
BlockingSystem.OnBlockStateChanged += audioIntegrator.PlayBlockSound;
```

**Manual Playback:**
```csharp
// Play attack sound with default settings
AudioManager.Instance.PlaySFX("attack");

// Play block sound with custom pitch
AudioManager.Instance.PlaySFX("block", volumeScale: 0.8f, pitchVariation: 0.1f);
```

### Managing Music

**Automatic with Game State:**
```csharp
// MusicManager automatically responds to GameFlowManager events
GameFlowManager.OnGameStateChanged += (state) =>
{
    if (state == GameState.Menu) MusicManager.PlayMenuMusic();
    if (state == GameState.Playing) MusicManager.PlayFightMusic();
};
```

**Manual Control:**
```csharp
// Play music with fade transition (1.5 second fade-out, 1.0 second fade-in)
MusicManager.PlayFightMusic(fadeOutDuration: 1.5f, fadeInDuration: 1.0f);
```

### Voice Announcements

**Automatic Announcements:**
```csharp
// VoiceAnnouncerSystem queues announcements on game events
GameFlowManager.OnRoundStart += () => VoiceAnnouncerSystem.AnnounceRound(roundNumber);
GameFlowManager.OnMatchEnd += (winner) => VoiceAnnouncerSystem.AnnounceVictory();
```

**Manual Queue:**
```csharp
// Queue announcer line (processes with 0.5s minimum cooldown)
VoiceAnnouncerSystem.QueueAnnouncement(clip);
```

### Volume Control

**Save User Preferences:**
```csharp
// Player changes master volume to 0.8
AudioSettings.SetVolume(VolumeType.Master, 0.8f);
AudioSettings.SaveSettings(); // Persists to PlayerPrefs

// On next session, settings auto-load from PlayerPrefs
AudioSettings.LoadSettings();
```

**Mute Specific Channels:**
```csharp
// Mute music for video recording
AudioSettings.SetMute(VolumeType.Music, true);
```

---

## Integration Points

### With Character Combat

**AttackManager interactions:**
- Light attack lands → `AudioEventIntegrator.PlayAttackSound(attackType)`
- Hit registers → `AudioEventIntegrator.PlayHitSound(damageAmount)`
- Combo counter increases → `AudioEventIntegrator.PlayComboSound(comboCount)`

**BlockingSystem interactions:**
- Block activates → `AudioEventIntegrator.PlayBlockSound()`
- Block breaks → `AudioEventIntegrator.PlayBlockBrokenSound()`

**KnockdownRecoverySystem interactions:**
- Character knocked down → `AudioEventIntegrator.PlayKnockdownSound()`
- Character recovers → `AudioEventIntegrator.PlayRecoverySound()`

### With Game Flow

**GameFlowManager events:**
- `OnRoundStart` → `VoiceAnnouncerSystem.AnnounceRound()`
- `OnMatchStart` → `VoiceAnnouncerSystem.AnnounceFight()`
- `OnMatchEnd` → `VoiceAnnouncerSystem.AnnounceVictory(winner)` or `AnnounceDefeat()`

**MusicManager responds to:**
- `OnGameStateChanged` (Menu/Playing states)
- `OnRoundEnd` (Victory/Defeat music)
- `OnMatchEnd` (return to menu music)

### With UI Systems

**UIManager can reference:**
- `AudioManager.PlaySFX("ui_click")` for button presses
- `AudioSettings` for volume sliders and mute buttons
- Audio event listeners for hit feedback sounds

---

## Best Practices

### Performance Optimization

1. **Object Pool SFX Sources:** Pre-allocated 4 AudioSource components avoid instantiation overhead
   - Recycle sources in round-robin fashion
   - Reuse sources by stopping and reassigning clips

2. **Music Fade System:** Smooth transitions prevent abrupt audio cuts
   - Default: 1.5s fade-out, 1.0s fade-in
   - Reduces jarring audio changes on state transitions

3. **Voice Line Cooldown:** Minimum 0.5s between announcements
   - Prevents overlapping announcements
   - Ensures announcer lines are heard clearly

4. **2D Spatial Blend:** All AudioSources set to 2D (no 3D spatialization)
   - Appropriate for fighting games
   - Lower CPU cost than 3D audio processing

### Audio Quality Settings

| Channel | Recommended Volume | Mute Toggle | Priority |
|---------|-------------------|------------|----------|
| Master  | 0.8-1.0 | ✓ | Global level |
| Music   | 0.6-0.8 | ✓ | Background |
| SFX     | 0.7-0.9 | ✓ | Immediate feedback |
| Voice   | 0.8-1.0 | ✓ | Announcements |

### Event Subscription Order

1. AudioEventIntegrator subscribes in `OnEnable()` to BlockingSystem, KnockdownRecoverySystem
2. AttackManager triggers audio events on hit detection
3. AudioManager plays sound through SFX source pool
4. PlayerPrefs store volume preferences automatically

---

## Troubleshooting

### Audio Not Playing
- **Check:** SoundEffectsLibrary assigned to AudioManager?
- **Check:** AudioClips imported correctly (not in StreamingAssets)?
- **Check:** SFX sources have AudioListener in scene?
- **Check:** Master volume not muted in AudioSettings?

### Music Not Transitioning
- **Check:** MusicManager subscribed to GameFlowManager events?
- **Check:** Music clips assigned to MusicManager in Inspector?
- **Check:** Music AudioSource marked as 2D spatial blend?
- **Check:** Fade durations reasonable (not too long)?

### Announcer Lines Overlapping
- **Check:** Cooldown between announcements set to 0.5s minimum?
- **Check:** Voice clips have reasonable duration?
- **Check:** VoiceAnnouncerSystem subscribed to correct GameFlowManager events?

### Volume Settings Not Persisting
- **Check:** `AudioSettings.SaveSettings()` called after volume change?
- **Check:** PlayerPrefs keys correct: "AudioSettings_MasterVolume", etc.?
- **Check:** Load called on game start: `AudioSettings.LoadSettings()`?

---

## Example: Complete Audio Setup Checklist

- [ ] Create SoundEffectsLibrary asset in Assets/Resources/Audio/
- [ ] Create folder for music clips in Assets/Resources/Music/
- [ ] Create folder for voice clips in Assets/Resources/Voice/
- [ ] Add AudioManager GameObject to scene
- [ ] Create 4 SFX source children on AudioManager
- [ ] Assign all components in Inspector
- [ ] Add MusicManager, VoiceAnnouncerSystem, AudioEventIntegrator to scene
- [ ] Verify AudioSources have AudioListener in scene
- [ ] Test sound playback in Play mode
- [ ] Check volume persistence across sessions
- [ ] Verify music transitions on state changes
- [ ] Test announcer cooldown (no overlapping lines)

---

## Next Steps

After Phase 6 Audio, the framework is feature-complete. Next priorities:

**Phase 7: Character Balance & Testing**
- Test all audio feedback loops
- Balance volume levels across channels
- Verify announcer line timing and clarity
- Test audio on various hardware (different speakers/headphones)

**Phase 8: Release Packaging**
- Optimize audio compression settings
- Create audio import presets
- Package framework for distribution

