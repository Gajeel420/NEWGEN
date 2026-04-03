using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Central manager for all UI elements.
/// Handles showing/hiding screens, updating HUD, and managing menus.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private CanvasGroup mainMenuCanvas;
    [SerializeField] private CanvasGroup pauseMenuCanvas;
    [SerializeField] private CanvasGroup gameOverCanvas;
    [SerializeField] private CanvasGroup hudCanvas;

    [SerializeField] private Text titleText;
    [SerializeField] private Text roundTimerText;
    [SerializeField] private Text roundCounterText;

    private HealthBarUI[] healthBars;
    private ComboCounterUI[] comboCounters;
    private GameOverScreen gameOverScreen;
    private List<Character> currentFighters = new List<Character>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUIElements();
    }

    private void OnEnable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGameStateChanged += HandleGameStateChange;
            GameFlowManager.Instance.OnTimeUpdated += UpdateTimer;
            GameFlowManager.Instance.OnRoundStart += HandleRoundStart;
            GameFlowManager.Instance.OnRoundEnd += HandleRoundEnd;
            GameFlowManager.Instance.OnMatchEnd += HandleMatchEnd;
        }
    }

    private void OnDisable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnGameStateChanged -= HandleGameStateChange;
            GameFlowManager.Instance.OnTimeUpdated -= UpdateTimer;
            GameFlowManager.Instance.OnRoundStart -= HandleRoundStart;
            GameFlowManager.Instance.OnRoundEnd -= HandleRoundEnd;
            GameFlowManager.Instance.OnMatchEnd -= HandleMatchEnd;
        }
    }

    /// <summary>
    /// Initialize all UI element references
    /// </summary>
    private void InitializeUIElements()
    {
        healthBars = FindObjectsOfType<HealthBarUI>();
        comboCounters = FindObjectsOfType<ComboCounterUI>();
        gameOverScreen = FindObjectOfType<GameOverScreen>();

        // Ensure canvases exist (create if missing)
        if (hudCanvas == null)
            CreateHUDCanvas();
    }

    /// <summary>
    /// Create default HUD canvas if none exists
    /// </summary>
    private void CreateHUDCanvas()
    {
        GameObject canvasObj = new GameObject("HUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        hudCanvas = canvasObj.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// Handle game state changes
    /// </summary>
    private void HandleGameStateChange(GameFlowManager.GameState newState)
    {
        switch (newState)
        {
            case GameFlowManager.GameState.Menu:
                ShowMainMenu();
                break;
            case GameFlowManager.GameState.Fighting:
                ShowHUD();
                HideMenus();
                break;
            case GameFlowManager.GameState.Paused:
                ShowPauseMenu();
                break;
            case GameFlowManager.GameState.GameOver:
                ShowGameOverScreen();
                break;
        }
    }

    /// <summary>
    /// Show main menu
    /// </summary>
    public void ShowMainMenu()
    {
        SetCanvasActive(mainMenuCanvas, true);
        SetCanvasActive(pauseMenuCanvas, false);
        SetCanvasActive(gameOverCanvas, false);
        SetCanvasActive(hudCanvas, false);
    }

    /// <summary>
    /// Show HUD during gameplay
    /// </summary>
    public void ShowHUD()
    {
        SetCanvasActive(hudCanvas, true);
        SetCanvasActive(mainMenuCanvas, false);
        SetCanvasActive(pauseMenuCanvas, false);
    }

    /// <summary>
    /// Show pause menu
    /// </summary>
    public void ShowPauseMenu()
    {
        SetCanvasActive(pauseMenuCanvas, true);
    }

    /// <summary>
    /// Hide all menus
    /// </summary>
    public void HideMenus()
    {
        SetCanvasActive(mainMenuCanvas, false);
        SetCanvasActive(pauseMenuCanvas, false);
    }

    /// <summary>
    /// Show game over screen
    /// </summary>
    public void ShowGameOverScreen()
    {
        SetCanvasActive(gameOverCanvas, true);
        if (gameOverScreen != null)
        {
            Character winner = GameFlowManager.Instance.GetWinner();
            gameOverScreen.ShowWinner(winner);
        }
    }

    /// <summary>
    /// Update round timer display
    /// </summary>
    private void UpdateTimer(float timeRemaining)
    {
        if (roundTimerText != null)
        {
            int minutes = (int)(timeRemaining / 60);
            int seconds = (int)(timeRemaining % 60);
            roundTimerText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    /// <summary>
    /// Handle round start
    /// </summary>
    private void HandleRoundStart(int roundNumber)
    {
        if (roundCounterText != null)
            roundCounterText.text = $"Round {roundNumber}";

        // Associate health bars with fighters
        currentFighters = GameFlowManager.Instance.GetActiveFighters();
        for (int i = 0; i < healthBars.Length && i < currentFighters.Count; i++)
        {
            healthBars[i].SetCharacter(currentFighters[i]);
        }

        // Associate combo counters with fighters
        for (int i = 0; i < comboCounters.Length && i < currentFighters.Count; i++)
        {
            comboCounters[i].SetCharacter(currentFighters[i]);
        }
    }

    /// <summary>
    /// Handle round end
    /// </summary>
    private void HandleRoundEnd(Character roundWinner)
    {
        if (titleText != null && roundWinner != null)
        {
            titleText.text = $"{roundWinner.gameObject.name} wins the round!";
        }
    }

    /// <summary>
    /// Handle match end
    /// </summary>
    private void HandleMatchEnd(Character matchWinner)
    {
        // GameFlowManager will call ShowGameOverScreen
    }

    /// <summary>
    /// Set CanvasGroup active with fade effect
    /// </summary>
    private void SetCanvasActive(CanvasGroup canvas, bool active)
    {
        if (canvas == null)
            return;

        canvas.alpha = active ? 1f : 0f;
        canvas.blocksRaycasts = active;
        canvas.interactable = active;
    }

    /// <summary>
    /// Update a health bar for a fighter
    /// </summary>
    public void UpdateHealthBar(Character character, float health, float maxHealth)
    {
        foreach (var healthBar in healthBars)
        {
            if (healthBar.GetCharacter() == character)
            {
                healthBar.UpdateHealth(health, maxHealth);
            }
        }
    }

    /// <summary>
    /// Update combo counter for a fighter
    /// </summary>
    public void UpdateComboCounter(Character character, int comboCount)
    {
        foreach (var comboCounter in comboCounters)
        {
            if (comboCounter.GetCharacter() == character)
            {
                comboCounter.UpdateCombo(comboCount);
            }
        }
    }
}
