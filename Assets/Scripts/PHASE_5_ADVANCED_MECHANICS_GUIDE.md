# Phase 5: Advanced Mechanics & Polish Guide

This guide covers implementing blocking, special moves, particle effects, and knockdown recovery for your fighting game framework.

---

## What's New in Phase 5

- **BlockingSystem.cs** — Complete blocking mechanics with stamina/durability
- **SpecialMove.cs** — ScriptableObject for defining special moves
- **SpecialMovesManager.cs** — Detects input sequences and executes special moves
- **ParticleEffectManager.cs** — Handles visual feedback with object pooling
- **KnockdownRecoverySystem.cs** — Manages knockdown and wakeup mechanics
- **Updated AttackManager.cs** — Integrates with all new Phase 5 systems

---

## Architecture

### Blocking System Flow

```
Block Input (Shift) → BlockingSystem.SetBlocking(true) 
                        ↓
                    Character.SetState(Idle)
                        ↓
                    Incoming Attack Hit
                        ↓
                    Hitbox calls ApplyBlockedDamage()
                        ↓
                    Reduced Damage + Stamina Drain
                        ↓
                    Check: Stamina > 0?
                    ├─ YES: Continue blocking
                    └─ NO: BreakBlock()
                            ├─ Full damage applied
                            └─ Extra hitstun
```

### Special Move Detection Flow

```
Player Input (Light/Medium/Heavy) → AttackManager.PollForAttacks()
                                         ↓
                                    SpecialMovesManager.RecordInput()
                                         ↓
                                    SpecialMovesManager.CheckForSpecialMoves()
                                         ↓
                                   Match Against Patterns?
                    ┌──────────────────┼──────────────────┐
                    YES                NO                (other specials?)
                     ↓                  ↓                        ↓
            ExecuteSpecialMove()   Continue Normal        Continue Checking
                     ↓             Attack Execution
            Animation + Particles
            + Sound + Script
```

### Particle Effect System

```
Hit/Block/Special Event → ParticleEffectManager.Spawn*Effect()
                              ↓
                         ObjectPool.Get() or Instantiate()
                              ↓
                         Particle.Play()
                              ↓
                         Lifetime Complete
                              ↓
                         ObjectPool.Return() or Destroy()
```

---

## Step 1: Implement Blocking System

### 1.1 Add BlockingSystem to Character

1. In your character GameObject in the scene:
   - Click **Add Component**
   - Search for **BlockingSystem**
   - Add it

2. Configure in Inspector:
   - **Block Damage Reduction**: 0.35 (35% damage passes through)
   - **Block Stun Reduction**: 0.5 (50% of hitstun)
   - **Base Block Stamina**: 100 (full durability)
   - **Block Stamina Recovery Rate**: 20 (stamina/sec when not blocking)
   - **Block Stamina Recovery Delay**: 1 (sec before recovery starts)
   - **Block Knockback Force**: 0.3 (reduced knockback while blocking)

### 1.2 Integrate with HitDetection

When a hit happens, check if target is blocking:

```csharp
// In HitDetection.cs OnHitDetected (conceptual)
public static void OnHitDetected(Hitbox hitbox, Hurtbox hurtbox)
{
    Character target = hurtbox.GetComponent<Character>();
    BlockingSystem blockingSystem = target.GetComponent<BlockingSystem>();
    
    if (blockingSystem.IsBlocking())
    {
        blockingSystem.ApplyBlockedDamage(damage, knockback, hitstun);
        // Spawn block particles
    }
    else
    {
        // Normal damage
        target.TakeDamage(damage, knockback, hitstun);
    }
}
```

### 1.3 Create Block Stamina UI

Create a new Canvas element for block meter:

1. **BlockStaminaBar** (UI Image)
   - Add **HealthBarUI** script (rename to BlockStaminaUI)
   - Bind to `blockingSystem.OnBlockStaminaChanged` event

```csharp
// Example BlockStaminaUI pseudo-code
public class BlockStaminaUI : MonoBehaviour
{
    private BlockingSystem blockingSystem;
    private Image staminaFill;
    
    void Start()
    {
        blockingSystem = FindObjectOfType<Character>().GetComponent<BlockingSystem>();
        blockingSystem.OnBlockStaminaChanged += UpdateStamina;
    }
    
    void UpdateStamina(float stamina, float maxStamina)
    {
        staminaFill.fillAmount = stamina / maxStamina;
        // Red when low, green when full
    }
}
```

### 1.4 Test Blocking

1. Hold **Shift** while opponent attacks
2. Verify damage is reduced (35% gets through)
3. Watch block stamina decrease
4. After stamina depletes: **Block Break** = full damage + extra stun
5. Release Shift, wait 1 second → Stamina recovers

---

## Step 2: Create Special Moves

### 2.1 Define a Special Move Asset

1. Right-click in Assets → **Create** → **Fighting Game** → **Special Move**
2. Name it `SpecialMove_DragonPunch`
3. Configure in Inspector:

| Property | Example Value |
|----------|---|
| Move Name | `special_dragon_punch` |
| Display Name | `Dragon Punch` |
| Description | `A powerful uppercut with knockup effect` |
| Input Sequence[0] | `Light` (timing: 0.3s) |
| Input Sequence[1] | `Medium` (timing: 0.4s) |
| Input Sequence[2] | `Heavy` (timing: 0.5s) |
| Base Damage | `40` (higher than normal attack) |
| Knockback Force | `15` (more knockback) |
| Hitstun Duration | `1.2` (longer stun) |
| Startup Frames | `0.4` |
| Active Frames | `0.8` |
| Recovery Frames | `0.6` |
| Requires Ground | `true` |
| Particle Effect Prefab | `Particles/DragonPunchEffect` |

### 2.2 Add SpecialMovesManager to Character

1. Select character GameObject
2. **Add Component** → **SpecialMovesManager**
3. Configure in Inspector:
   - **Available Special Moves**: Drag your special move assets here
   - **Input History Timeout**: 2 (seconds)

### 2.3 Create Special Move Prefabs

Create folders in Assets:
```
Assets/
├── Resources/
│   └── Particles/
│       ├── DragonPunchEffect.prefab
│       ├── HitImpact.prefab
│       ├── BlockEffect.prefab
│       └── SpecialMoveFlash.prefab
```

Each prefab should have **ParticleSystem** component configured.

### 2.4 Test Special Move Detection

1. Your character must perform: **Light → Medium → Heavy** within 1.5 seconds
2. On console, you should see: `[SPECIAL MOVE] Dragon Punch executed!`
3. Animation triggers, particles spawn
4. Damage is applied (40 base damage)

---

## Step 3: Implement Particle Effects

### 3.1 Add ParticleEffectManager to Character

1. Select character GameObject
2. **Add Component** → **ParticleEffectManager**
3. Configure in Inspector:
   - **Hit Impact Prefab**: Drag your hit particle prefab
   - **Block Prefab**: Drag your block particle prefab
   - **Special Move Prefab**: Drag your special particle prefab
   - **Screen Shake Particles**: (optional)

### 3.2 Trigger Particles on Events

When a hit happens, call from HitDetection:

```csharp
// Spawn hit effect at impact point
ParticleEffectManager pem = hitbox.GetComponent<ParticleEffectManager>();
if (pem != null)
{
    Vector3 hitPosition = hurtbox.transform.position;
    Vector3 hitNormal = (hitbox.transform.position - hitPosition).normalized;
    pem.SpawnHitEffect(hitPosition, hitNormal);
}
```

When blocking:
```csharp
// Spawn block effect
BlockingSystem bs = target.GetComponent<BlockingSystem>();
bs.OnBlockStateChanged += (isBlocking) =>
{
    if (!isBlocking && wasBlocking)
    {
        pem.SpawnBlockEffect(target.transform.position);
    }
};
```

### 3.3 Screen Shake on Hit

For powerful attacks, add screen shake:

```csharp
// In AttackManager when hitbox connects
pem.ScreenShake(duration: 0.1f, intensity: 0.5f);
```

---

## Step 4: Knockdown & Recovery

### 4.1 Add KnockdownRecoverySystem to Character

1. Select character GameObject
2. **Add Component** → **KnockdownRecoverySystem**
3. Configure in Inspector:
   - **Knockdown Duration**: 1.5 (seconds on ground)
   - **Recovery Startup Time**: 0.3 (before standing)
   - **Recovery Animation Duration**: 0.8
   - **Allow Wakeup Invulnerability**: true
   - **Wakeup Invulnerability Duration**: 0.5 (sec immune after waking)

### 4.2 Trigger Knockdown

Modify HitDetection to detect heavy hits:

```csharp
// If damage is high or it's a special attack
if (damage > 30 || isSpecialMove)
{
    KnockdownRecoverySystem krs = target.GetComponent<KnockdownRecoverySystem>();
    if (krs != null)
    {
        krs.KnockDown();
        // Spawn knockdown effect
        pem.SpawnSpecialMoveEffect(target.transform.position, 1);
    }
}
```

### 4.3 Wakeup Invulnerability

While recovering, character is invulnerable:

```csharp
// In Character.TakeDamage()
KnockdownRecoverySystem krs = GetComponent<KnockdownRecoverySystem>();
if (krs != null && krs.HasWakeupInvulnerability())
{
    return; // Can't take damage while waking up
}
```

---

## Complete Character Setup Checklist

### Components Added
- [ ] BlockingSystem added and configured
- [ ] SpecialMovesManager added with special move assets assigned
- [ ] ParticleEffectManager added with particle prefabs
- [ ] KnockdownRecoverySystem added and configured

### Systems Integrated
- [ ] HitDetection checks for blocking before applying damage
- [ ] ParticleEffectManager called on hits
- [ ] Special moves record inputs from attacks
- [ ] Knockdown triggers on heavy attacks or specials
- [ ] Wakeup invulnerability prevents damage during recovery

### UI Elements Created
- [ ] Block stamina bar added to HUD
- [ ] Block break indicator (red flash?)
- [ ] Special move input display (for player learning)
- [ ] Knockdown timer display (optional)

### Testing Checklist
- [ ] Hold Shift to block, verify reduced damage
- [ ] Attack blocked stamina drains, recovers after 1 sec
- [ ] Deplete block stamina, verify block breaks
- [ ] Perform special move combo (Light→Medium→Heavy)
- [ ] Verify special move damage is higher
- [ ] See particles on hit, block, and special move
- [ ] Get knocked down on heavy/special hit
- [ ] Wake up with invulnerability (can't take damage)

---

## Advanced: Custom Special Moves

### Create DragonKick Special

```
Input Sequence: Heavy → Heavy → Light
Base Damage: 50
Knockback: 20
Effect: Knockdown + Screen Shake
```

Configuration:
1. **Create** → **Special Move** → Name: `SpecialMove_DragonKick`
2. Set Input Sequence:
   - [0]: Heavy
   - [1]: Heavy  
   - [2]: Light
3. Set Damage: 50
4. Set Knockback: 20
5. Assign to character's SpecialMovesManager

### Create Hadoken (Projectable Special)

For a fireball projectile:

```
Input Sequence: Medium → Medium → Heavy
Effect: Spawn projectile, travel forward, apply damage on hit
```

This requires additional projectile system (Phase 6 enhancement).

---

## Event System Reference

### BlockingSystem Events

```csharp
blockingSystem.OnBlockStateChanged += (isBlocking) => { };
blockingSystem.OnBlockStaminaChanged += (stamina, maxStamina) => { };
blockingSystem.OnBlockBroken += () => { };
```

### SpecialMovesManager Events

```csharp
specialMovesManager.OnSpecialMoveExecuted += (move) => { };
specialMovesManager.OnSpecialMoveFailed += (reason) => { };
```

### KnockdownRecoverySystem Events

```csharp
knockdownRecoverySystem.OnKnockedDown += () => { };
knockdownRecoverySystem.OnRecovering += () => { };
knockdownRecoverySystem.OnRecovered += () => { };
```

---

## Performance Optimization

### Object Pooling

ParticleEffectManager uses object pooling for particles:

```csharp
// Automatically pools hit effects
// Reuses instances instead of creating/destroying
// Configurable pool size (default: 5 hit effects, 3 block effects)
```

### Best Practices

1. **Limit simultaneous particles** — Max 50 particles at once
2. **Use LOD** — Reduce particle count on low-end systems
3. **Preload particles** — Load at scene start, not during combat
4. **Monitor frame rate** — Check that particles don't cause FPS drops

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Block not reducing damage | Verify BlockingSystem attached, check `blockDamageReduction` value |
| Special move not executing | Check input sequence exactly matches (case-sensitive), verify timing |
| Particles not showing | Verify prefabs assigned to ParticleEffectManager, check Resources path |
| Knockdown too long/short | Adjust `knockdownDuration` in KnockdownRecoverySystem |
| Block break feels unfair | Increase `blockStaminaRecoveryRate` or reduce `blockDamageReduction` |

---

## Real-World Balance Examples

### Defensive Balance (Favors Blocking)
- Block Damage Reduction: 0.25 (75% reduction)
- Block Stamina Recovery: 30 (fast recovery)
- Block Stamina: 150 (high durability)

### Offensive Balance (Favors Attacking)
- Block Damage Reduction: 0.45 (55% reduction)
- Block Stamina Recovery: 15 (slow recovery)
- Block Stamina: 80 (low durability)

### Tournament Balance (Neutral)
- Block Damage Reduction: 0.35 (65% reduction)
- Block Stamina Recovery: 20 (moderate)
- Block Stamina: 100 (standard)

---

## Next Steps

1. **Create special moves** for each character
2. **Record special move inputs** for player reference
3. **Balance values** through playtesting
4. **Add sound effects** for blocks and specials
5. **Implement waveshine** or other advanced mechanics
6. **Advance to Phase 6** for audio and VFX polish

---

**Phase 5 Complete!** Your framework now has advanced combat mechanics. Ready for Phase 6: Audio System & Visual Polish.
