using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages overall game flow, scene transitions, and match state.
/// Singleton that persists across scenes.
/// Coordinates between fighters, UI, and game logic.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    /// <summary>
    /// Game states during a match
    /// </summary>
    public enum GameState
    {
        Menu,
        CharacterSelect,
        Loading,
        Fighting,
        Paused,
        RoundEnd,
        MatchEnd,
        GameOver
    }

    public static GameFlowManager Instance { get; private set; }

    [SerializeField] private GameState currentGameState = GameState.Menu;

    // Match settings
    [SerializeField] private float roundDuration = 60f; // 60 second rounds
    [SerializeField] private int maxRounds = 3;
    [SerializeField] private float roundWinDelay = 2f; // Delay before showing round win

    // Match state
    private int currentRound = 1;
    private float roundTimeRemaining;
    private List<Character> activeFighters = new List<Character>();
    private Character winner;

    // Events
    public delegate void GameStateChangeDelegate(GameState newState);
    public event GameStateChangeDelegate OnGameStateChanged;

    public delegate void RoundStartDelegate(int roundNumber);
    public event RoundStartDelegate OnRoundStart;

    public delegate void RoundEndDelegate(Character roundWinner);
    public event RoundEndDelegate OnRoundEnd;

    public delegate void MatchEndDelegate(Character matchWinner);
    public event MatchEndDelegate OnMatchEnd;

    public delegate void TimeUpdateDelegate(float timeRemaining);
    public event TimeUpdateDelegate OnTimeUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (currentGameState == GameState.Fighting)
            UpdateRoundTimer();

        HandleInput();
    }

    /// <summary>
    /// Update the round timer
    /// </summary>
    private void UpdateRoundTimer()
    {
        roundTimeRemaining -= Time.deltaTime;
        OnTimeUpdated?.Invoke(Mathf.Max(0, roundTimeRemaining));

        if (roundTimeRemaining <= 0)
        {
            EndRound();
        }

        // Check for KO (all fighters except one are KO'd)
        CheckForKO();
    }

    /// <summary>
    /// Check if any fighter has won by KO
    /// </summary>
    private void CheckForKO()
    {
        List<Character> aliveCharacters = new List<Character>();
        foreach (var fighter in activeFighters)
        {
            if (fighter.IsAlive())
                aliveCharacters.Add(fighter);
        }

        if (aliveCharacters.Count == 1)
        {
            winner = aliveCharacters[0];
            EndRound();
        }
    }

    /// <summary>
    /// Start a new round with fresh fighters
    /// </summary>
    public void StartRound(List<Character> fighters)
    {
        activeFighters = new List<Character>(fighters);
        roundTimeRemaining = roundDuration;
        currentRound = 1;
        winner = null;

        SetGameState(GameState.Fighting);
        OnRoundStart?.Invoke(currentRound);
    }

    /// <summary>
    /// End current round and check for match winner
    /// </summary>
    private void EndRound()
    {
        if (currentGameState != GameState.Fighting)
            return;

        SetGameState(GameState.RoundEnd);
        OnRoundEnd?.Invoke(winner);

        // Determine round winner (by health if time ran out)
        if (winner == null)
        {
            float maxHealth = -1;
            foreach (var fighter in activeFighters)
            {
                if (fighter.GetHealth() > maxHealth)
                {
                    maxHealth = fighter.GetHealth();
                    winner = fighter;
                }
            }
        }

        // Check for match winner
        if (currentRound >= maxRounds || winner != null)
        {
            Invoke(nameof(EndMatch), roundWinDelay);
        }
        else
        {
            currentRound++;
            Invoke(nameof(ResetForNextRound), roundWinDelay + 1f);
        }
    }

    /// <summary>
    /// Reset fighters for next round
    /// </summary>
    private void ResetForNextRound()
    {
        foreach (var fighter in activeFighters)
        {
            fighter.Heal(fighter.MaxHealth); // Full heal between rounds
            fighter.transform.position = GetSpawnPosition(activeFighters.IndexOf(fighter));
            fighter.SetState(Character.CharacterState.Idle);
        }

        StartRound(activeFighters);
    }

    /// <summary>
    /// End the match and show game over screen
    /// </summary>
    private void EndMatch()
    {
        SetGameState(GameState.MatchEnd);
        OnMatchEnd?.Invoke(winner);

        Invoke(nameof(ShowGameOver), 2f);
    }

    /// <summary>
    /// Show game over screen
    /// </summary>
    private void ShowGameOver()
    {
        SetGameState(GameState.GameOver);
        // UIManager will show game over screen based on this state change
    }

    /// <summary>
    /// Get spawn position for a fighter
    /// </summary>
    private Vector3 GetSpawnPosition(int fighterIndex)
    {
        float spacing = 5f;
        if (fighterIndex == 0)
            return new Vector3(-spacing, 0, 0);
        else
            return new Vector3(spacing, 0, 0);
    }

    /// <summary>
    /// Handle player input (pause, quit, etc)
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentGameState == GameState.Fighting)
                PauseGame();
            else if (currentGameState == GameState.Paused)
                ResumeGame();
        }
    }

    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        if (currentGameState != GameState.Fighting)
            return;

        SetGameState(GameState.Paused);
        Time.timeScale = 0f; // Freeze game
    }

    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        if (currentGameState != GameState.Paused)
            return;

        SetGameState(GameState.Fighting);
        Time.timeScale = 1f; // Resume time
    }

    /// <summary>
    /// Set the current game state
    /// </summary>
    public void SetGameState(GameState newState)
    {
        if (currentGameState != newState)
        {
            currentGameState = newState;
            OnGameStateChanged?.Invoke(currentGameState);
        }
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMenu()
    {
        Time.timeScale = 1f; // Unfreeze if paused
        SetGameState(GameState.Menu);
        // UIManager will load menu scene
    }

    // Getters
    public GameState GetGameState() => currentGameState;
    public float GetRoundTime() => Mathf.Max(0, roundTimeRemaining);
    public int GetCurrentRound() => currentRound;
    public int GetMaxRounds() => maxRounds;
    public Character GetWinner() => winner;
    public List<Character> GetActiveFighters() => new List<Character>(activeFighters);
}
