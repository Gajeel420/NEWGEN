# Phase 2: Core Framework Architecture

This guide explains the Phase 2 character and combat systems and how to integrate them.

## System Overview

### Components

**Character System:**
- `Character.cs` — Base class with state machine, health, and hitstun
- `CharacterStats.cs` — ScriptableObject with stat attributes (damage, speed, health)
- `CharacterController.cs` — Handles movement, jumping, and physics

**Combat System:**
- `InputManager.cs` — Already from Phase 1; buffers and polls inputs
- `CommandBuffer.cs` — From Phase 1; stores input history for combos
- `Hitbox.cs` — Active attack collision zone
- `Hurtbox.cs` — Character vulnerability zone with damage multipliers
- `AttackManager.cs` — Manages attack execution (startup/active/recovery frames)
- `HitDetection.cs` — Static collision resolver; applies damage and knockback

---

## Setup: Creating a Test Character

Follow these steps to create a playable character in your scene.

### Step 1: Create Character GameObject

1. In the Hierarchy, create an empty GameObject: **GameObject** → **Create Empty**
2. Name it `TestCharacter`
3. Add a Sprite Renderer component (set any sprite or leave empty for now)
4. Add a Collider2D:
   - Choose **Capsule Collider 2D** (best for humanoid characters)
   - Set **Is Trigger** to **OFF** (this is for ground collision)
   - Adjust size/offset to match your sprite

### Step 2: Add Rigidbody2D

1. Add component: **Rigidbody 2D**
2. Set **Body Type** to **Dynamic**
3. Set **Gravity Scale** to **1**
4. Set **Constraints**:
   - Freeze Rotation Z: ✓
   - Freeze Rotation X: ✓
   - Freeze Rotation Y: ✓

### Step 3: Add Character Scripts

Add these components to the TestCharacter GameObject:

1. **Character.cs** (script)
2. **CharacterStats.cs** (reference to a CharacterStats asset)
3. **CharacterController.cs** (script)
4. **AttackManager.cs** (script)
5. **CommandBuffer.cs** (script)
6. **InputManager.cs** (if not a singleton; or assign via Inspector)

### Step 4: Create CharacterStats Asset

1. In Project: **Assets/Resources/** (create if needed)
2. Right-click → **Create** → **Fighting Game** → **Character Stats**
3. Name it `BasicCharacterStats`
4. Configure values:
   - **Max Health:** 100
   - **Walk Speed:** 5
   - **Jump Height:** 3
   - **Light Attack Damage:** 10
   - **Medium Attack Damage:** 15
   - **Heavy Attack Damage:** 25

5. Drag this asset to the TestCharacter's **CharacterStats** component

### Step 5: Create Hitboxes (Attack Collision)

1. Under TestCharacter, create child GameObjects for each attack zone:
   - `Fist` (for Light/Medium attacks)
   - `Leg` (for Heavy attacks)

2. For each child, add:
   - **Circle Collider 2D** (set **Is Trigger** to ON)
   - **Hitbox.cs** script
   - Configure in Inspector:
     - **Attack Type:** "Light" or "Heavy"
     - **Damage:** 10-25 depending on type
     - **Knockback Force:** 3-8
     - **Hitstun Duration:** 0.2-0.5 seconds

### Step 6: Create Hurtboxes (Vulnerability Zones)

1. Under TestCharacter, create child GameObjects for vulnerability zones:
   - `HurtBox_Body`
   - `HurtBox_Head` (optional, with 1.2x damage multiplier)
   - `HurtBox_Legs` (optional, with 0.8x damage multiplier)

2. For each, add:
   - **Circle Collider 2D** (set **Is Trigger** to ON)
   - **Hurtbox.cs** script
   - Configure **Zone** and **Damage Multiplier** in Inspector

### Step 7: Set Layer Masks

1. Create a new Layer: **Window** → **Tags and Layers** → **Edit Layers** → **User Layer 6** → name: `Ground`
2. Create another layer: `Character`
3. Assign TestCharacter to **Character** layer
4. Assign ground platforms to **Ground** layer
5. On TestCharacter's **CharacterController** component, set **Ground Layer** to `Ground`

### Step 8: Set Up Input Actions (Phase 1)

1. Verify `FightingGameInputs.inputactions` exists (from Phase 1 INPUT_SYSTEM_SETUP.md)
2. On TestCharacter, find **InputManager** component (if separate object, add it)
3. Assign InputActionReferences for Light, Medium, Heavy, Jump, Move

### Step 9: Create a Ground Plane

1. Create a new GameObject: **GameObject** → **2D Object** → **Quad** (or use a sprite)
2. Scale it horizontally (e.g., 10 units wide, 0.5 units tall)
3. Position it below the character (Y = -2)
4. Add **Box Collider 2D** (is Trigger = OFF)
5. Assign it to the **Ground** layer

### Step 10: Test in Editor

1. Press **Play**
2. Use **WASD** (or configured keys) to move
3. Press **Space** to jump (if configured)
4. Press input buttons (defined in InputActionAsset) to attack:
   - Light attack should do 10 damage
   - Heavy attack should do 25 damage, more knockback

---

## System Architecture

### Character State Machine

States: `Idle` → `Moving` → `Attacking` → `GettingHit` → `KnockedDown` → `KO`

- **Idle/Moving:** Normal state; accept input for attacks/jumps
- **Attacking:** Character frozen; hitboxes active per attack timing
- **GettingHit:** Character in hitstun; cannot perform new actions
- **KnockedDown:** Character knocked back; takes time to recover
- **KO:** Character dead; no more input processing

### Attack Timing Phases

Each attack has three frames:

1. **Startup** (0.1s default): Hitbox inactive; animation plays
2. **Active** (0.2s default): Hitbox active; can hit opponents
3. **Recovery** (0.3s default): Hitbox inactive; character vulnerable but can move after

Total attack duration = Startup + Active + Recovery = 0.6s

### Damage Calculation

```
Final Damage = Base Damage × Zone Multiplier × Combo Multiplier

Example:
- Heavy attack: 25 damage
- Head zone multiplier: 1.2x
- Final: 25 × 1.2 = 30 damage
```

### Knockback Direction

Knockback always pushes in the direction of the attacker's facing (+ 45° upward for aerial angle).

```csharp
Vector2 knockbackDir = new Vector2(attacker.GetFacingDirection(), 0.5f).normalized;
```

---

## Debugging & Testing

### Enable Debug Visualization

In the scene, you should see:
- **Green rays** from character feet (ground check)
- **Colored wireframes** for hurtboxes (red = head, yellow = body, cyan = legs)
- **Orange gizmos** for attack state

### Console Logs

When a hit occurs, you'll see:
```
[HIT] TestCharacter hit Enemy for 20 damage! Zone: Body, Knockback: 5, Hitstun: 0.3
```

### Adjust Parameters

Tweak these values to test balance:
- `CharacterStats.lightAttackDamage` → Increase for easier combo testing
- `CharacterStats.jumpHeight` → Decrease for tighter platforming
- `CharacterController.walkSpeed` → Increase for faster movement
- `AttackManager.attackActiveTime` → Increase to make hits easier to land

---

## Next Steps

1. **Combo Detection:**
   - Extend `CommandBuffer` to feed into `AttackManager`
   - Implement combo chaining/cancellation logic

2. **Animation Integration:**
   - Create `AnimationController.cs` to sync animations with attack timing
   - Trigger hitbox activation on animation events

3. **Test Characters:**
   - Create multiple character prefabs with unique stats
   - Balance damage/knockback ratios

4. **Visual Polish:**
   - Add screen shake on hit
   - Add particle effects
   - Add sound effects

---

## Troubleshooting

**Character not moving:**
- Check InputManager is assigned and InputActionReferences are set
- Verify Ground Layer is set correctly in CharacterController

**Attacks not connecting:**
- Ensure Hitboxes have **Is Trigger = ON**
- Verify Hurtboxes have **Is Trigger = ON**
- Check AttackManager is added to character
- Confirm AttackManager's hitboxes have Attack Type set ("Light", "Medium", "Heavy")

**Character falls through ground:**
- Verify ground platform has a **non-trigger** Collider2D
- Check layer masks: Character should detect Ground layer

**No hitstun/knockback:**
- Check HitDetection.OnHit event is being called
- Verify Character.TakeDamage logic and knockback application
