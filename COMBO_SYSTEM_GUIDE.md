# Combo System Setup Guide

This guide explains how to set up and use the basic combo detection system for attack linking.

---

## System Overview

### Components

**Combo Scripts:**
- `Combo.cs` — ScriptableObject defining combo patterns (input sequence, timing, damage scaling)
- `ComboDetector.cs` — Detects combos by matching input history against patterns
- Updates to `AttackManager.cs` — Applies combo scaling to attacks and allows combo cancellation

### How It Works

1. **ComboDetector** listens to inputs via `InputManager.HasBuffered()`
2. Stores recent inputs (Light, Medium, Heavy, Jump)
3. Matches input sequence against registered combos
4. When a combo is detected, sets damage/knockback scaling
5. **AttackManager** applies scaling when hitboxes activate
6. Subsequent hits in the combo use increased damage (scaled exponentially)

---

## Step 1: Create Combo Assets

### 1.1 Create a Basic Combo

1. In **Assets/** create a folder: `Combos/` (optional, for organization)
2. Right-click → **Create** → **Fighting Game** → **Combo**
3. Name it `BasicCombo`
4. In the Inspector, configure:

**Fields:**
- **Combo Name:** "Light x3 Medium" (descriptive)
- **Inputs:** Define the sequence
  - Input 0: Light (Timing Window: 0.5s)
  - Input 1: Light (Timing Window: 0.5s)
  - Input 2: Medium (Timing Window: 0.5s)
- **Damage Scaling:** 1.1 (each hit does 10% more damage)
- **Combo Break Time:** 1.0 (reset if no input for 1 second)
- **Can Cancel:** ON (allow combo linking)
- **Knockback Scaling:** 1.0 (knockback scales with damage)

### 1.2 Create More Combos

Create several more combos with different patterns:

**Combo 2: Medium > Heavy (Ender)**
- Input 0: Medium (Timing Window: 0.5s)
- Input 1: Heavy (Timing Window: 0.5s)
- Damage Scaling: 1.15
- Knockback Scaling: 1.2 (final hit has more knockback)

**Combo 3: Light > Medium > Heavy (BnB - Bread & Butter)**
- Input 0: Light
- Input 1: Medium
- Input 2: Heavy
- Damage Scaling: 1.2
- Knockback Scaling: 1.3

---

## Step 2: Integrate with Character

### 2.1 Add ComboDetector to Character

1. Select your test character in the scene (e.g., `TestCharacter`)
2. Add component: **ComboDetector** (script)
3. In the Inspector:
   - **Available Combos:** 3 (or how many you created)
   - Slot 0: BasicCombo
   - Slot 1: Medium > Heavy combo
   - Slot 2: Light > Medium > Heavy combo
   - Leave **Command Buffer** empty (auto-detected)

### 2.2 Verify AttackManager Integration

The **AttackManager** on the same GameObject should now:
- Reference **ComboDetector** (auto-detected)
- Apply combo scaling to attacks
- Allow attack cancellation within the combo link window

---

## Step 3: Test Combo System

### 3.1 Play a Test Combo

1. Press **Play** in the Editor
2. Perform the first attack: Press **Light** input (spacebar or configured key)
3. Quickly press **Light** again within 0.5 seconds
4. Then press **Medium** quickly after
5. **Expected result:** Combo detected, each hit does slightly more damage

**Console output:**
```
[ComboDetector] Input recorded: Light | Sequence: Light
[ComboDetector] Input recorded: Light | Sequence: Light > Light
[ComboDetector] Input recorded: Medium | Sequence: Light > Light > Medium
[COMBO DETECTED] Light x3 Medium (Hit #1)!
[HIT] TestCharacter hit Enemy for 11 damage! (1.1x scaling)
```

### 3.2 Monitor Combo Counter

Look at the debug logs and gizmos:
- Combo count appears in console as `(Hit #N)`
- Damage scales: `10 * 1.1 = 11` (first hit), `10 * 1.1^2 = 12.1` (second hit), etc.

### 3.3 Break a Combo

1. Start a combo but wait more than 1 second
2. Console should log: `[ComboDetector] Combo broken! Final count: X`
3. Next attack resets to base damage (1.0x scaling)

---

## Step 4: UI Integration (Optional)

To display combo counter on screen, listen to ComboDetector events:

```csharp
public class ComboUI : MonoBehaviour
{
    private ComboDetector comboDetector;
    private TextMeshProUGUI comboCountText;

    void Start()
    {
        comboDetector = GetComponentInParent<Character>().GetComponent<ComboDetector>();
        comboDetector.OnComboDetected += ShowCombo;
        comboDetector.OnComboBroken += HideCombo;
    }

    private void ShowCombo(Combo combo, int hitCount)
    {
        comboCountText.text = $"COMBO x{hitCount}!";
        comboCountText.color = Color.yellow;
    }

    private void HideCombo()
    {
        comboCountText.text = "";
    }
}
```

---

## Step 5: Balance & Tuning

### Attack Timing for Combos

Adjust these values in `AttackManager` if combos feel too easy/hard to execute:

```csharp
[SerializeField] private float comboLinkCancelWindow = 0.1f; // How long during recovery you can cancel into next move
```

- **Smaller window (0.05s):** Harder to link combos, requires precise timing
- **Larger window (0.2s):** Easier to link, more forgiving

### Damage Scaling

Combos scale exponentially. For `damageScaling = 1.1`:
- Hit 1: 10 damage × 1.1^0 = 10 damage
- Hit 2: 10 damage × 1.1^1 = 11 damage
- Hit 3: 10 damage × 1.1^2 = 12.1 damage
- Hit 4: 10 damage × 1.1^3 = 13.31 damage
- Hit 5: 10 damage × 1.1^4 = 14.64 damage

Adjust `damageScaling` in Combo assets:
- **1.05:** Gentle scaling (long combos don't get too powerful)
- **1.1:** Default (moderate scaling)
- **1.2:** Aggressive scaling (combos get very strong quickly)

### Combo Break Time

Controls how long the combo window stays open:

```csharp
[SerializeField] public float comboBreakTime = 1.0f;
```

- **0.5s:** Tight window, requires fast input
- **1.0s:** Standard window
- **2.0s:** Generous window, easy to link

---

## Advanced: Custom Combo Logic

To add additional combo mechanics (directional inputs, special moves), extend `ComboDetector`:

```csharp
public class CustomComboDetector : ComboDetector
{
    protected override void CheckForNewInputs()
    {
        base.CheckForNewInputs();
        
        // Add custom input types here
        if (InputManager.Instance.HasBuffered("SpecialA"))
        {
            RecordInput("SpecialA");
        }
    }
}
```

Then create Combos with "SpecialA" in the input sequence.

---

## Debugging

### ComboDetector Console Output

```
[ComboDetector] Input recorded: Light | Sequence: Light > Medium > Heavy
[COMBO DETECTED] Light > Medium > Heavy (Hit #3)!
[ComboDetector] Combo broken! Final count: 3
```

### Verify Damage Scaling

In the console when hits connect:
```
[HIT] Character hit Enemy for 15 damage! Zone: Body, Knockback: 5.5, Hitstun: 0.3
```

The damage should increase with each combo hit.

### Check Combo Status

Add this to any script to debug:

```csharp
ComboDetector cd = character.GetComponent<ComboDetector>();
Debug.Log($"Current combo: {cd.GetCurrentCombo().comboName}");
Debug.Log($"Hit count: {cd.GetComboCount()}");
Debug.Log($"Sequence: {cd.GetSequenceString()}");
```

---

## Common Issues

**Combos not detecting:**
- Verify input timing windows are not too small (should be 0.3-0.7s)
- Check InputManager is buffering inputs correctly
- Confirm ComboDetector is on the same GameObject as character

**Damage not scaling:**
- Ensure `damageScaling > 1.0` in the Combo asset
- Check AttackManager is applying the combo scalar to hitbox damage
- Verify hitbox damage values are non-zero

**Combo breaking unexpectedly:**
- Increase `comboBreakTime` in Combo asset if window is too tight
- Check input buffer time in InputManager (should be ~0.18s)

---

## Next Steps

1. **Create character movesets** using multiple combo chains
2. **Balance across characters** — ensure no character has overpowered combos
3. **Add visual feedback** — screen shake, particle effects on combo hits
4. **Implement special moves** — extend combo patterns with button combinations
5. **Add UI polish** — combo counter, damage values, visual indicators
