using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays game over screen with winner, round wins, and replay/menu options.
/// Shows after match ends.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text winnerNameText;
    [SerializeField] private Text winnerStatusText;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button menuButton;

    private Character winner;

    private void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (replayButton != null)
            replayButton.onClick.AddListener(OnReplayClicked);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
    }

    /// <summary>
    /// Show winner information
    /// </summary>
    public void ShowWinner(Character winnerCharacter)
    {
        winner = winnerCharacter;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (winnerCharacter != null)
        {
            if (winnerNameText != null)
                winnerNameText.text = winnerCharacter.gameObject.name;

            if (winnerStatusText != null)
                winnerStatusText.text = "WINS THE MATCH!";
        }
        else
        {
            if (winnerNameText != null)
                winnerNameText.text = "DRAW";

            if (winnerStatusText != null)
                winnerStatusText.text = "Time's Up!";
        }
    }

    /// <summary>
    /// Handle replay button
    /// </summary>
    private void OnReplayClicked()
    {
        Time.timeScale = 1f; // Ensure time is running
        GameFlowManager.Instance.StartRound(GameFlowManager.Instance.GetActiveFighters());
    }

    /// <summary>
    /// Handle menu button
    /// </summary>
    private void OnMenuClicked()
    {
        Time.timeScale = 1f; // Ensure time is running
        GameFlowManager.Instance.ReturnToMenu();
    }

    /// <summary>
    /// Hide the screen
    /// </summary>
    public void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
}
