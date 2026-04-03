# InputActionAsset Setup Guide

This guide explains how to create and configure an InputActionAsset for the `InputManager` to use.

## Steps to Create an InputActionAsset

### 1. Create the InputActionAsset
- In the Project panel, right-click in `Assets/` → **Create** → **Input Actions**
- Name it `FightingGameInputs.inputactions`
- Move it to `Assets/Resources/` for easy access

### 2. Define Action Maps
The default map will be called "Player". Configure it with these actions:

| Action Name | Action Type | Binding Examples |
|-----------|-----------|-----------|
| **Light** | Button | Spacebar, Gamepad Button South (X), Mouse Left Click |
| **Medium** | Button | E, Gamepad Button East (Circle), Mouse Right Click |
| **Heavy** | Button | R, Gamepad Button West (Square), Mouse Middle Click |
| **Jump** | Button | W, Gamepad Button North (Triangle) |
| **Move** | Value (Vector 2) | WASD, Gamepad Left Stick |

### 3. Configure Each Action

#### Light/Medium/Heavy/Jump (Button Actions)
1. Select the action (e.g., "Light")
2. Click **+** under "Bindings" to add bindings
3. For keyboard: select **Keyboard** → choose key (Space, E, R, etc.)
4. For gamepad: select **Gamepad** → choose button
5. Click "Save Asset"

#### Move (Vector2 Action)
1. Select "Move"
2. Select Composite type → **2D Vector**
3. Add bindings:
   - **Up**: W key (or Gamepad Left Stick Up)
   - **Down**: S key (or Gamepad Left Stick Down)
   - **Left**: A key (or Gamepad Left Stick Left)
   - **Right**: D key (or Gamepad Left Stick Right)
4. Click "Save Asset"

### 4. Enable "Generate C# Class"
1. Open `FightingGameInputs.inputactions` in the Inspector
2. Enable **Generate C# Class**
3. Click **Regenerate C# class**

This creates a C# wrapper class (e.g., `FightingGameInputs`) for type-safe access.

### 5. Link to InputManager
1. Create an empty GameObject in your scene (or a persistent Manager object)
2. Add the `InputManager` component
3. In the Inspector, assign the InputActionReferences:
   - **Light Action**: `FightingGameInputs/Player/Light`
   - **Medium Action**: `FightingGameInputs/Player/Medium`
   - **Heavy Action**: `FightingGameInputs/Player/Heavy`
   - **Jump Action**: `FightingGameInputs/Player/Jump`
   - **Move Action**: `FightingGameInputs/Player/Move`

## Alternative: UI Controls
If playing in the Editor with multiple controls, enable **UI Module** in Window → TextMesh Pro → Settings to avoid conflicts, or use **Save/Load Actions** to persist your configuration.

## Example Usage (In Your Combat Script)
```csharp
// Check if Light was pressed this frame
if (InputManager.Instance.HasBuffered("Light"))
{
    Debug.Log("Light attack buffered!");
}

// Get movement input
float horizontal = InputManager.Instance.GetAxis("Horizontal");
```

## Testing
1. Click the **Listen** button in the Input Manager window to test bindings
2. Press your configured keys to verify they register
3. Adjust sensitivity or deadzones as needed (in InputActionAsset settings)

---

**Next Steps:**
- Once configured, create a **combo detection system** using `CommandBuffer.cs`
- Link `CommandBuffer` to your character controller for combo execution
- Add animation and visual feedback for detected combos
