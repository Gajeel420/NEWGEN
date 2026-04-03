# Phase 3: Animation System Integration Guide

This guide covers integrating Unity's Animator with the fighting game framework for smooth, frame-perfect animations.

---

## What's New in Phase 3

- **AnimationController.cs** — Synchronizes game state with Animator parameters
- **Enhanced Character.cs** — OnStateChanged event for animation hooks
- **Updated AttackManager.cs** — Frame-perfect timing using animation progress
- **Animator Integration** — Full state machine support with sprite animation

---

## Architecture

### Animation Pipeline

```
Character State Change → OnStateChanged Event → AnimationController → Animator → Sprite Animation
                                ↓
                    PlayAttackAnimation() → Animator Trigger
                                ↓
                    GetNormalizedAnimationTime() → Attack Frame Timing
```

### Key Components

| Component | Purpose |
|-----------|---------|
| **AnimationController.cs** | Manages Animator parameters and state sync |
| **Character.cs** | Broadcasts state changes via OnStateChanged event |
| **AttackManager.cs** | Uses animation timing for frame-perfect attacks |
| **Animator** | Unity component that drives sprite transitions |

---

## Step 1: Create Animator Controller

### 1.1 Create Animation Clips
In your project:
1. Create a folder: `Assets/Animations/`
2. Inside, create clip folders:
   - `Character_Idle.anim`
   - `Character_Moving.anim`
   - `Attack_Light.anim`
   - `Attack_Medium.anim`
   - `Attack_Heavy.anim`
   - `Character_Hit.anim`
   - `Character_KnockedDown.anim`
   - `Character_KO.anim`

**Each animation clip should be created by:**
1. Double-click the .anim file to open the Animation window
2. Add keyframes with sprite changes
3. Set frame rate (typically 12-24 FPS for fighting games)
4. Example Idle: 4 sprite keyframes looping
5. Example Attack_Light: 3 frames (startup), 2 frames (active), 1 frame (recovery)

### 1.2 Create Animator Controller
1. Right-click in `Assets/Animations/` → **Create** → **Animator Controller**
2. Name it `CharacterAnimator.controller`
3. Double-click to open the Animator window

### 1.3 Set Up the State Machine

Create these states in the Animator:
- **Idle** (default)
- **Moving**
- **Attack** (parent state for attack types)
  - Light
  - Medium
  - Heavy
- **GettingHit**
- **KnockedDown**
- **KO**

---

## Step 2: Configure Animator Parameters

In the Animator window, add these parameters:

| Parameter Name | Type | Description |
|---|---|---|
| **State** | Int | Character state (0=Idle, 1=Moving, 2=Attacking, etc.) |
| **Speed** | Float | Current movement speed for blending |
| **IsGrounded** | Bool | Whether character is touching ground |
| **AttackType** | Int | Which attack (0=Light, 1=Medium, 2=Heavy) |
| **Attack** | Trigger | Triggers attack animation |
| **KnockedBack** | Bool | Enables knockback reaction animation |
| **HealthPercent** | Float | For damage state blending |

### Example Parameter Setup
```
State: 0 = Idle, 1 = Moving, 2 = Attacking, 3 = GettingHit, 4 = KnockedDown, 5 = KO
```

---

## Step 3: Create State Transitions

### Idle → Moving
- **Condition:** Speed > 0.1
- **Transition Duration:** 0.1s

### Moving → Idle
- **Condition:** Speed < 0.05
- **Transition Duration:** 0.1s

### Any State → Attacking
- **Condition:** State == 2
- **Transition Duration:** 0s (immediate)

### Idle/Moving → GettingHit
- **Condition:** State == 3
- **Transition Duration:** 0s

### GettingHit → Idle
- **Condition:** State == 0
- **Transition Duration:** 0.2s

### Any State → KO
- **Condition:** State == 5
- **Transition Duration:** 0s

---

## Step 4: Configure Attack Animations

### Attack_Light Setup
1. Select **Attack_Light** state
2. Assign **Character_Attack_Light.anim** clip
3. Set **Speed**: 1.2 (faster for snappy feel)
4. Loop: OFF
5. In animation clip:
   - Frame 0-5 (0.0-0.25s): Startup
   - Frame 5-10 (0.25-0.42s): Active
   - Frame 10-15 (0.42-0.75s): Recovery

### Attack_Medium Setup
Similar to Light, but:
- 1.0x speed (standard)
- Frame 0-8: Startup
- Frame 8-15: Active  
- Frame 15-20: Recovery

### Attack_Heavy Setup
Similar, but:
- 0.8x speed (slower, more powerful feel)
- Frame 0-12: Startup
- Frame 12-18: Active
- Frame 18-25: Recovery

---

## Step 5: Assign Scripts to Character

### 5.1 Create Test Character
1. Create a new GameObject: `TestCharacter`
2. Add **SpriteRenderer** component
3. Add **Rigidbody2D** (gravity on, body type = dynamic)
4. Add **CapsuleCollider2D** (not trigger)
5. Add these scripts:
   - **Character.cs**
   - **CharacterController.cs**
   - **AttackManager.cs**
   - **ComboDetector.cs**
   - **AnimationController.cs** ← NEW

### 5.2 Configure AnimationController
In Inspector:
- **Animator**: (assign your CharacterAnimator.controller)
- **Character**: (auto-assigned from GetComponent)
- **CharacterController**: (auto-assigned from GetComponent)
- **AttackManager**: (auto-assigned from GetComponent)
- **Use Animation Timing**: ON

### 5.3 Assign the Animator
1. Select your character GameObject
2. In Inspector, find **Animator** component
3. Drag `CharacterAnimator.controller` to the **Controller** field

---

## Step 6: Test Animation Sync

### Test in Play Mode
1. Press Play
2. Use WASD to move → Watch "Moving" animation play
3. Stand still → Watch "Idle" animation play
4. Press Space for Light attack → Watch attack animation play with correct timing
5. Get hit (if in multiplayer test) → Watch "GettingHit" animation play

### Troubleshooting

**Animation not playing:**
- Verify CharacterAnimator.controller is assigned to Animator component
- Check animation clips are in the correct states
- Ensure AnimationController.cs is enabled

**State transitions not working:**
- Verify parameter names match exactly (case-sensitive)
- Check transition conditions in Animator window
- Increase transition detection in Animator (if using Mecanim)

**Attacks not frame-perfect:**
- Ensure **Use Animation Timing** is ON in AttackManager
- Check animation clip's FPS matches expected frames
- Verify hitbox activates during correct animation frame range

---

## Step 7: Advanced - Animation Events

### Add Animation Event for Hit Effects
In animation clip editor:
1. Open **Attack_Light.anim**
2. Find the frame where attack should "connect" (usually middle of active phase)
3. Right-click → Select **Add Event**
4. Create empty function in your script:
```csharp
public void OnAttackHit()
{
    Debug.Log("Hit confirmed at frame!");
    // Play sound, spawn effects, etc.
}
```

### Add Animation Event for Attack End
On the last frame of recovery:
```csharp
public void OnAttackComplete()
{
    // Called when animation finishes
}
```

---

## Step 8: Blending and Transitions

### Smooth Movement Blend
The **Speed** parameter allows smooth transition between idle and moving:

1. Create **Idle** state with walking animation clip
2. Create **Moving** state with run animation clip  
3. Add transition with:
   - Condition: `Speed > 0.1`
   - Has Exit Time: OFF
   - Transition Duration: 0.2s

This creates smooth animation blending as character speeds up/down.

### Hand-to-Hand Combat Stance
For realistic fighting stance:
1. Create states for Stance_Idle, Stance_Moving
2. Use different sprite assets
3. Transition based on proximity to opponent

---

## Step 9: Multiple Characters

### Create Character Variants
To support multiple playable characters with different animations:

1. Duplicate `CharacterAnimator.controller` → `RyuAnimator.controller`
2. Replace animation clips with Ryu-specific animations
3. On your Ryu character GameObject:
   - Drag `RyuAnimator.controller` to Animator's **Controller** field
4. AnimationController.cs adapts automatically

---

## Comparison: Timer vs Animation Timing

### Timer-Based (Phase 2)
```csharp
// Manual frame counting
float timeRemaining = 0.3f;
if (timeRemaining > 0.2f) ActivateHitbox(); // Between 0.1-0.2s
```

**Pros:** Simple, predictable  
**Cons:** Doesn't match animation visuals if animation speed changes

### Animation-Based (Phase 3)
```csharp
// Uses animator's actual frame position
float normalized = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
if (normalized > 0.3f && normalized < 0.7f) ActivateHitbox(); // 30%-70% through animation
```

**Pros:** Always matches visuals, handles animation speed changes  
**Cons:** Slightly more complex

---

## Complete Character Setup Checklist

- [ ] Animator Controller created with all states
- [ ] Animation clips imported and assigned to states  
- [ ] Parameters added (State, Speed, IsGrounded, AttackType, Attack, KnockedBack, HealthPercent)
- [ ] State transitions configured with conditions
- [ ] Attack animations set to non-looping
- [ ] AnimationController.cs added to character GameObject
- [ ] AnimationController references assigned
- [ ] Use Animation Timing enabled on AttackManager
- [ ] Test in Play mode - movement animations work
- [ ] Test in Play mode - attacks sync with visuals
- [ ] Test in Play mode - hit feedback animations play

---

## Quick Reference: Animation Timing Values

For a 60 FPS game with 12 FPS animations:

| Attack Type | Startup | Active | Recovery | Total |
|---|---|---|---|---|
| Light | 0.10s (5 frames) | 0.17s (10 frames) | 0.17s (10 frames) | 0.44s |
| Medium | 0.13s (8 frames) | 0.25s (15 frames) | 0.21s (12 frames) | 0.59s |
| Heavy | 0.20s (12 frames) | 0.30s (18 frames) | 0.25s (15 frames) | 0.75s |

Adjust based on your game's desired speed and feel.

---

## Next Steps

1. **Create your first animated character** using this guide
2. **Test combat** between two animated characters
3. **Balance animation speeds** for gameplay feel
4. **Advance to Phase 4** for UI and game flow integration

---

## Common Issues & Solutions

| Issue | Solution |
|---|---|
| Animations choppy | Increase animation clip FPS to 24 or higher |
| Attack doesn't hit | Verify hitbox activates during active phase; check `normalizedTime` bounds |
| State changes delayed | Reduce transition duration in Animator to 0s for immediate transitions |
| Multiple attacks stacking | Ensure `isAttacking` flag prevents overlapping attacks |
| Animation doesn't loop | Make sure loop is enabled in Idle/Moving clips only |

---

**Phase 3 Complete!** Your framework now has full animation support. Ready for Phase 4: Game Flow & UI.
