# 2D Fighting Games Framework - Phase 1 & 2 Complete

A modular, production-ready framework for building 2D fighting games in Unity. Implements core character mechanics, combat systems, input handling, and combo detection.

**Repository:** https://github.com/Gajeel420/NEWGEN  
**Status:** Phase 1 & Phase 2 Complete | Ready for Phase 3

---

## Overview

This framework provides everything needed to build a functional 2D fighting game:

- ✅ **Input System:** Buffered input with new Input System support
- ✅ **Character Controller:** Movement, jumping, ground detection, physics
- ✅ **Combat System:** Hitbox/hurtbox collision with zone-based damage
- ✅ **Attack Timing:** Configurable startup/active/recovery frames
- ✅ **Combo System:** Input pattern matching with exponential damage scaling
- ✅ **Hit Detection:** Damage resolution with knockback
- ✅ **14 C# Scripts:** Production-ready, well-documented code
- ✅ **5 Setup Guides:** Comprehensive documentation for integration

---

## Quick Start

### 1. Requirements
- Unity 2022 LTS or newer
- New Input System package (default)

### 2. Open Project
```bash
git clone https://github.com/Gajeel420/NEWGEN.git
cd NEWGEN
# Open in Unity Hub
```

### 3. Create Test Character
Follow **PHASE_2_INTEGRATION.md** to create your first character:
- Add scripts to GameObject: Character, CharacterController, AttackManager, etc.
- Create hitboxes and hurtboxes as children
- Assign CharacterStats ScriptableObject
- Press Play and test

### 4. Test Combat
- **Movement:** WASD keys
- **Jump:** Space bar
- **Attacks:** Configured buttons (default: Space=Light, E=Medium, R=Heavy)
- **Combo:** Light → Light → Medium (within 0.5s each input)

---

## What's Included

### Phase 1: Environment Setup ✓
- Project structure with Git initialized
- Input System integration with buffering
- VS Code debugging configuration
- GitHub Actions CI/CD pipeline
- MCP/Claude integration ready

### Phase 2: Core Framework ✓
14 production-ready scripts implementing:
- Character state machine (Idle → Moving → Attacking → GettingHit → KO)
- Movement controller with gravity and air physics
- Attack frame timing (startup/active/recovery)
- Collision detection with zone-based damage multipliers
- Combo detection and damage scaling
- Example character with hit feedback

---

## Documentation

| Guide | Purpose |
|-------|---------|
| **PHASE_1_SETUP.md** | VS Code, Input System, debugging setup |
| **PHASE_2_INTEGRATION.md** | Character creation walkthrough |
| **COMBO_SYSTEM_GUIDE.md** | Create combos, test sequences, balance |
| **CHARACTER_SETUP_GUIDE.md** | Templates, checklists, deployment |
| **PHASE_2_VERIFICATION.md** | Testing, debugging, validation |

---

## Architecture

### Core Systems

**Input Pipeline:**
```
Input System → InputManager (buffer) → ComboDetector (pattern match) → AttackManager (execute)
```

**Damage Pipeline:**
```
Hitbox collision → HitDetection (resolve) → Character.TakeDamage() → Knockback + Hitstun
```

**Attack Phases:**
```
Startup (0.1s) → Active (0.2s) → Recovery (0.3s)
           [inactive]  [active]  [inactive, can cancel to next combo]
```

### Damage Calculation
```
Final = Base × ZoneMultiplier × ComboScalar
10 (base) × 1.2 (head) × 1.21 (combo 3) = 14.52 damage
```

---

## Scripts

**Input (Phase 1):**
- InputManager.cs — Input buffering
- CommandBuffer.cs — Input history

**Characters (Phase 2):**
- Character.cs — State machine base class
- CharacterController.cs — Movement & physics
- CharacterStats.cs — Balancing attributes
- ExampleCharacter.cs — Subclass template

**Combat (Phase 2):**
- Hitbox.cs — Attack collision
- Hurtbox.cs — Damage zones
- AttackManager.cs — Attack execution
- HitDetection.cs — Damage resolution

**Combos (Phase 2):**
- Combo.cs — Pattern definition
- ComboDetector.cs — Pattern matching

**Testing (Phase 2):**
- Phase2TestController.cs — Verification helper

---

## Key Features

✅ **Modular** — Use individual systems or full framework  
✅ **Extensible** — Easy to add special moves, blocking, etc.  
✅ **Configurable** — All values in ScriptableObjects  
✅ **Event-Driven** — OnHit event for UI/effects  
✅ **Well-Documented** — 5 guides + inline comments  
✅ **Version Controlled** — Clean Git history  
✅ **Team-Ready** — GitHub Actions CI/CD configured  

---

## Testing

### Run Verification
1. Create any test character
2. Press **T** in-game to run diagnostics
3. Check console for system status

### Example Console Output
```
=== PHASE 2 SYSTEM TEST STARTED ===
✓ Player 1: TestCharacter - Health: 100
✓ InputManager singleton found
✓ ComboDetector found - 3 combos loaded
✓ AttackManager ready for attacks
```

### Manual Test
1. Create two characters facing each other
2. Execute combo: Light → Light → Medium
3. Watch damage increase each hit
4. Verify combo timeout after 1+ seconds of no input

---

## Future Phases (Roadmap)

- **Phase 3:** Animation system (Animator integration)
- **Phase 4:** Game flow & UI (menus, HUD)
- **Phase 5:** Advanced mechanics (blocking, specials)
- **Phase 6:** Balance testing
- **Phase 7:** Audio & VFX
- **Phase 8:** Release packaging

---

## Example: Create a Character

```csharp
// Create GameObject with Rigidbody2D
var character = new GameObject("Fighter1");
character.AddComponent<SpriteRenderer>();
character.AddComponent<Rigidbody2D>();
character.AddComponent<CapsuleCollider2D>();

// Add framework scripts
character.AddComponent<Character>();
character.AddComponent<CharacterController>();
character.AddComponent<CharacterStats>();
character.AddComponent<AttackManager>();
character.AddComponent<ComboDetector>();

// Assign stats
var stats = Resources.Load<CharacterStats>("Stats/BasicCharacter");
character.GetComponent<CharacterStats>().CopyFrom(stats);

// Create hitboxes and hurtboxes as children
// Configure in Inspector
```

---

## Troubleshooting

**Character falls through ground:**
- Ensure ground has non-trigger Collider2D
- Check CharacterController Ground Layer is set

**Attacks not connecting:**
- Verify Hitbox has Is Trigger = ON
- Confirm AttackManager has hitboxes in scene
- Check attack active time > 0

**Combos not detecting:**
- Verify ComboDetector has Combo assets assigned
- Check input timing windows (should be 0.3-0.7s)
- Confirm InputManager is buffering inputs

---

## Support & Resources

- **Setup Issue?** → See PHASE_1_SETUP.md
- **Character Problem?** → See CHARACTER_SETUP_GUIDE.md
- **Combat Not Working?** → See PHASE_2_INTEGRATION.md
- **Combo Issues?** → See COMBO_SYSTEM_GUIDE.md
- **Debugging Help?** → See PHASE_2_VERIFICATION.md

---

**Ready to extend?** Start Phase 3 by implementing AnimationController for animation system integration.
