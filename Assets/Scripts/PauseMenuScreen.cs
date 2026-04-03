using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays pause menu during gameplay.
/// Allows player to resume, change settings, or return to main menu.
/// </summary>
public class PauseMenuScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button menuButton;

    private void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    /// <summary>
    /// Show pause menu
    /// </summary>
    public void Show()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Hide pause menu
    /// </summary>
    public void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Handle resume button
    /// </summary>
    private void OnResumeClicked()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.ResumeGame();

        Hide();
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
    /// Handle return to menu button
    /// </summary>
    private void OnMenuClicked()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.ReturnToMenu();

        Hide();
    }
}
