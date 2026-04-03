# Character Setup Checklists

Quick reference checklist for creating new characters using the framework.

---

## Checklist: Creating a New Character

### Prefab Setup
- [ ] Create new GameObject named `[CharacterName]`
- [ ] Add **Sprite Renderer** component
- [ ] Add **Capsule Collider 2D** (position for ground collision, **not trigger**)
- [ ] Add **Rigidbody 2D** (Dynamic, GM=1, Constraints on rotation)

### Required Scripts (Add in order)
- [ ] **CharacterStats** (reference ScriptableObject asset)
- [ ] **Character** (base class)
- [ ] **CharacterController** (movement/jumping)
- [ ] **ExampleCharacter** or custom subclass (optional, for unique behavior)
- [ ] **CommandBuffer** (input history)
- [ ] **ComboDetector** (assign Combo assets)
- [ ] **AttackManager** (attack execution)
- [ ] **InputManager** (if not a scene singleton)

### Attack Setup
- [ ] Create child GameObjects for hitbox positions (e.g., `Fist`, `Leg`)
- [ ] Add **Circle Collider 2D** (Trigger = ON) to each
- [ ] Add **Hitbox.cs** script to each
- [ ] Configure Attack Type, Damage, Knockback, Hitstun in Inspector

### Defense Setup
- [ ] Create child GameObjects for hurtbox zones (e.g., `HurtBox_Body`, `HurtBox_Head`)
- [ ] Add **Circle Collider 2D** (Trigger = ON) to each
- [ ] Add **Hurtbox.cs** script to each
- [ ] Configure Zone and Damage Multiplier in Inspector

### Combo Setup
- [ ] Create Combo assets (3-5 basic combos)
- [ ] Assign combos to **ComboDetector** Available Combos array
- [ ] Test combo sequences

### Polish (Optional)
- [ ] Add **ParticleSystem** for hit effects
- [ ] Assign to AttackManager's hitParticles reference
- [ ] Add sound effects
- [ ] Create animations (for Phase 3)

---

## Checklist: Balance Testing

### Combat Feel
- [ ] Test basic attack responsiveness
- [ ] Verify damage ranges (10-25 for Light/Medium/Heavy)
- [ ] Test knockback distances (characters pushed back ~1-3 units)
- [ ] Verify hitstun duration (0.2-0.5s feels snappy)

### Combo System
- [ ] Test each combo sequence inputs are registered
- [ ] Verify damage scaling is visible (each hit more damage)
- [ ] Confirm combo timeout works (resets after 1s inactivity)
- [ ] Test attack cancellation during recovery phase

### Movement
- [ ] Walk speed feels responsive (5-8 units/sec typical)
- [ ] Jump height appropriate for map layout (3-4 units)
- [ ] Air control feels fair (0.8 multiplier typical)
- [ ] Ground detection works on all platforms

### Character-Specific
- [ ] Unique stats differentiate from other characters
- [ ] Special mechanics don't break other systems
- [ ] No unintended clipping or collision issues

---

## Checklist: One-vs-One Testing

### Setup
- [ ] Create two character instances in scene
- [ ] Assign different InputManagers or use P1/P2 controls
- [ ] Add ground platform between them (~5 units apart)

### Player 1 (Character A)
- [ ] Can move left/right without clipping
- [ ] Attacks connect on opponent
- [ ] Combos extend and do increased damage
- [ ] Takes damage and hitstun correctly
- [ ] Dies when health reaches 0

### Player 2 (Character B)
- [ ] Same checks as Player 1
- [ ] Can block opponent's attacks (if blocking implemented)

### Overall Match
- [ ] Matches last reasonable duration (not too quick)
- [ ] Both characters feel balanced (neither significantly stronger)
- [ ] No game-breaking bugs (freezes, clips, etc.)

---

## Checklist: Deploy as Prefab

Once testing confirms character is balanced:

1. **Save as Prefab**
   - In Hierarchy, drag character GameObject to `Assets/Prefabs/Characters/`
   - Name: `[CharacterName]_Prefab`

2. **Create Documentation**
   - Add to move list (Phase 8 task)
   - Document unique mechanics
   - Record stat values for future reference

3. **Version Control**
   - Commit prefab: `git add Assets/Prefabs/Characters/`
   - Push: `git push origin main`

---

## Common Issues & Fixes

**Character falls through ground:**
- [ ] Ground platform has BoxCollider2D with **Is Trigger = OFF**
- [ ] Character Ground Layer mask set correctly
- [ ] Ground check raycast distance appropriate (~0.1 units)

**Attacks don't connect:**
- [ ] Hitboxes have **Is Trigger = ON**
- [ ] Hurtboxes have **Is Trigger = ON**
- [ ] AttackManager.GetHitboxes() finding correct hitboxes
- [ ] Attack Active Time > 0 (at least 0.1s)

**Combos not chaining:**
- [ ] ComboDetector has Combos assigned
- [ ] InputManager buffering inputs (0.18s default)
- [ ] Timing windows in Combos set to 0.3-0.7s (not too small)
- [ ] AttackManager.comboLinkCancelWindow > 0

**Knockback too weak/strong:**
- [ ] Adjust Hitbox.GetKnockbackForce (3-8 typical)
- [ ] Check Rigidbody2D gravity scale
- [ ] Verify knockback application in Character.TakeDamage()

---

## Next Character Template

When creating your next character, copy this structure:

```
Assets/Prefabs/Characters/
├── CharacterA_Prefab.prefab
├── CharacterB_Prefab.prefab
└── CharacterC_Prefab.prefab

Assets/ScriptableObjects/Stats/
├── CharacterA_Stats.asset
├── CharacterB_Stats.asset
└── CharacterC_Stats.asset

Assets/ScriptableObjects/Combos/
├── CharacterA_BasicCombo.asset
├── CharacterA_FinisherCombo.asset
└── CharacterB_UniqueCombo.asset
```

Keep assets organized by character for easy iteration during balance changes.
