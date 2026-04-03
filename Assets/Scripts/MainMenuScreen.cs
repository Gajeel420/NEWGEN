using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays main menu with game start options.
/// Player can select characters or adjust settings from here.
/// </summary>
public class MainMenuScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    // Character selection (simplified for now)
    [SerializeField] private Character[] selectableCharacters;

    private void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
    }

    /// <summary>
    /// Show main menu
    /// </summary>
    public void Show()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Hide main menu
    /// </summary>
    public void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Handle start game button
    /// </summary>
    private void OnStartClicked()
    {
        // TODO: Show character select screen
        // For now, start with default characters
        StartGame();
    }

    /// <summary>
    /// Start the game with selected characters
    /// </summary>
    private void StartGame()
    {
        // Create or spawn fighters
        // This is simplified - in a full game you'd have a character select screen
        
        Hide();

        if (GameFlowManager.Instance != null)
        {
            // Create two test fighters for now
            GameObject fighter1Obj = Instantiate(new GameObject("Player1"));
            GameObject fighter2Obj = Instantiate(new GameObject("Player2"));

            Character fighter1 = fighter1Obj.AddComponent<Character>();
            Character fighter2 = fighter2Obj.AddComponent<Character>();

            // Add other required components
            fighter1Obj.AddComponent<CharacterController>();
            fighter1Obj.AddComponent<SpriteRenderer>();
            fighter1Obj.AddComponent<Rigidbody2D>();
            fighter1Obj.AddComponent<CapsuleCollider2D>();
            fighter1Obj.AddComponent<AttackManager>();
            fighter1Obj.AddComponent<ComboDetector>();
            fighter1Obj.AddComponent<AnimationController>();

            fighter2Obj.AddComponent<CharacterController>();
            fighter2Obj.AddComponent<SpriteRenderer>();
            fighter2Obj.AddComponent<Rigidbody2D>();
            fighter2Obj.AddComponent<CapsuleCollider2D>();
            fighter2Obj.AddComponent<AttackManager>();
            fighter2Obj.AddComponent<ComboDetector>();
            fighter2Obj.AddComponent<AnimationController>();

            System.Collections.Generic.List<Character> fighters = new System.Collections.Generic.List<Character> { fighter1, fighter2 };
            GameFlowManager.Instance.StartRound(fighters);
        }
    }

    /// <summary>
    /// Handle settings button
    /// </summary>
    private void OnSettingsClicked()
    {
        // TODO: Show settings panel
        Debug.Log("Settings not yet implemented");
    }

    /// <summary>
    /// Handle exit button
    /// </summary>
    private void OnExitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
