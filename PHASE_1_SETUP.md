# Phase 1 Setup Instructions

This guide walks you through setting up the development environment for the 2D Fighting Games Framework.

## Prerequisites
- **Unity 2022 LTS or newer** (download from [unity.com](https://unity.com/))
- **VS Code** (download from [code.visualstudio.com](https://code.visualstudio.com/))
- **Git** (already initialized in this repo)
- **GitHub account** with access to this repository

---

## Step 1: Clone the Repository

```bash
git clone https://github.com/Gajeel420/NEWGEN.git
cd NEWGEN
```

---

## Step 2: Open the Project in Unity

1. **Open Unity Hub**
2. Click **Add** → select the `NEWGEN` folder (the cloned repo root)
3. Choose **Unity 2022 LTS** (or newer) as the editor version
4. Wait for Unity to import the project

Once imported, you should see the `Assets/` folder structure in the Project panel.

---

## Step 3: Set Up VS Code for Unity Development

### 3.1 Install Required Extensions

Open VS Code and install these extensions (search by ID in the Extensions marketplace):

| Extension | ID | Purpose |
|-----------|----|---------| 
| **C# Dev Kit** | `ms-dotnettools.csharp` | Intellisense, debugging, refactoring |
| **Unity Code Snippets** | `kleber-swf.unity-code-snippets` | Unity-specific code snippets |
| **IntelliCode** | `VisualStudioExptTeam.vsintellisense` | AI-powered code completion |
| **Debugger for Unity** | `Unity.debug` | Attach debugger to Unity Editor |

Quick install commands (in VS Code terminal):
```powershell
code --install-extension ms-dotnettools.csharp
code --install-extension kleber-swf.unity-code-snippets
code --install-extension VisualStudioExptTeam.vsintellisense
code --install-extension Unity.debug
```

### 3.2 Configure Unity Editor for VS Code

1. Open the NEWGEN project in **Unity Editor**
2. Go to **Edit** → **Preferences** (Windows) or **Unity** → **Preferences** (Mac)
3. Select **External Tools**
4. Set **External Script Editor** to your VS Code executable:
   - **Windows**: `C:\Users\[YourUsername]\AppData\Local\Programs\Microsoft VS Code\Code.exe`
   - **Mac**: `/Applications/Visual Studio Code.app/Contents/Frameworks/Code Helper (GPU).app/Contents/MacOS/Code Helper (GPU)`
   - **Linux**: `/usr/bin/code`
5. Click **Regenerate project files** to ensure `.csproj` files are up-to-date

### 3.3 Test VS Code Integration

1. In Unity, double-click any `.cs` script (e.g., `InputManager.cs`)
2. VS Code should open with full Intellisense and debugging support
3. Try typing to verify Intellisense autocomplete works

---

## Step 4: Set Up C# Debugging in VS Code

### 4.1 Install .NET Debugging
The C# Dev Kit includes debugging by default. To verify:

1. In VS Code, open the command palette: **Ctrl+Shift+P** (Windows/Linux) or **Cmd+Shift+P** (Mac)
2. Type `C#: Select Project for IntelliSense`
3. Choose the `Assembly-CSharp.csproj` project

### 4.2 Attach Debugger to Unity Editor

1. Play a scene in Unity
2. In VS Code, go to **Run** → **Start Debugging** (or press **F5**)
3. Choose **Unity Editor** as the target
4. Set breakpoints in your scripts and they will pause execution when hit

---

## Step 5: Set Up Input System

Follow the detailed guide in [INPUT_SYSTEM_SETUP.md](INPUT_SYSTEM_SETUP.md) to:

1. Create `Assets/Resources/FightingGameInputs.inputactions`
2. Define actions for Light, Medium, Heavy, Jump, and Move
3. Assign InputActionReferences to the `InputManager` component

**Quick Test:**
- Create an empty GameObject
- Add `InputManager` component
- Assign the InputAction references in the Inspector
- Press play and check the console for input logs

---

## Step 6: Verify MCP Connection for Claude Integration

### 6.1 Install MCP Extension in VS Code

1. Open VS Code Extensions marketplace
2. Search for `GitHub Copilot Chat`
3. Install `GitHub.copilot-chat` (includes MCP support)

### 6.2 Configure Claude Integration

See [Copilot Chat MCP Documentation](https://docs.github.com/en/copilot/github-copilot-chat/github-copilot-chat-in-vs-code) for detailed setup.

### 6.3 Test Claude Inline Code Generation

1. Open a `.cs` file in VS Code
2. Highlight a comment or selection
3. Open Copilot Chat: **Ctrl+Shift+I** (or **Cmd+Shift+I** on Mac)
4. Ask: *"Generate a simple character movement function"*
5. Claude should generate C# code inline

### 6.4 Test Code Review

1. Ask Copilot: *"Review this code for performance issues"*
2. Copilot should analyze and suggest improvements

---

## Step 7: Project Structure Verification

Verify all expected directories exist:

```
NEWGEN/
├── Assets/
│   ├── Scripts/
│   │   ├── InputManager.cs       ✓
│   │   ├── CommandBuffer.cs      ✓
│   │   └── INPUT_SYSTEM_SETUP.md ✓
│   ├── Prefabs/
│   ├── Animations/
│   ├── Sprites/
│   ├── Scenes/
│   ├── Audio/
│   └── Resources/
├── .github/
│   └── workflows/
│       ├── unity-ci.yml
│       └── unity-ci-full.yml
├── .gitignore
├── README.md
└── ...
```

---

## Step 8: Configure GitHub Actions (Optional)

To enable CI/CD with GitHub Actions:

1. Go to your GitHub repo settings: **Settings** → **Secrets and variables** → **Actions**
2. Add secret `UNITY_LICENSE`:
   - Get your Unity license file from your local machine: `C:\ProgramData\Unity\Unity_lic.ulf` (Windows)
   - Copy the file contents as a secret value
3. The CI workflow will now run on push to `main`

---

## Step 9: First Test Build

1. **In Unity Editor:**
   - Open the default scene
   - Press **Play** to test in Editor
   - Check console for InputManager debug logs

2. **Create a test scene:**
   - File → New Scene
   - Add a Canvas with a debug text display
   - Add an empty GameObject with `InputManager` and `CommandBuffer`
   - Test input buffering by logging `InputManager.Instance.HasBuffered()`

---

## Troubleshooting

### VS Code Intellisense Not Working
- **Solution:** Go to `Edit` → `Preferences` → `External Tools` → click **Regenerate project files**
- Restart VS Code after regenerating

### Debugger Not Attaching
- **Solution:** Ensure the `Debugger for Unity` extension is installed
- Check that the project has `debug.json` in `.vscode/` folder
- Restart Unity and VS Code

### Input System Not Recognizing Bindings
- **Solution:** Restart Unity after creating InputActionAsset
- Ensure `Generate C# Class` is enabled on the asset
- Verify Input Manager has InputActionReferences assigned

### GitHub Actions Build Failing
- **Solution:** Check if `UNITY_LICENSE` secret is properly set
- Verify the license file content (not expired)
- Check workflow logs in GitHub under **Actions**

---

## Next Steps

Phase 1 is complete! You can now:

1. **Move to Phase 2**: Build core framework components
   - Character controller with movement
   - Combat system with hitboxes/hurtboxes
   - Animation state machine

2. **Strengthen Phase 1**: 
   - Add more input mappings (directional, defensive moves)
   - Create a starter character prefab template
   - Write unit tests for InputManager and CommandBuffer

3. **Version Control**:
   - Commit your changes: `git add . && git commit -m "Initialize Phase 1 development environment"`
   - Push: `git push origin main`

---

**Need Help?**
- Check the [Unity Documentation](https://docs.unity3d.com/)
- Review [Input System Manual](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/)
- Ask Claude (Copilot Chat) in VS Code for code examples and debugging help
