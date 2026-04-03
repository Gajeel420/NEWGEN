using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages special move detection and execution.
/// Detects input sequences and triggers corresponding special moves.
/// </summary>
public class SpecialMovesManager : MonoBehaviour
{
    private Character character;
    private AttackManager attackManager;
    private CommandBuffer commandBuffer;

    [SerializeField] private SpecialMove[] availableSpecialMoves;
    [SerializeField] private float inputHistoryTimeout = 2f; // How long to remember inputs

    private List<string> recentInputs = new List<string>();
    private float inputTimeoutTimer = 0f;
    private SpecialMove lastExecutedSpecial = null;
    private float specialMoveRecastTime = 0f;

    // Events
    public delegate void SpecialMoveExecutedDelegate(SpecialMove specialMove);
    public event SpecialMoveExecutedDelegate OnSpecialMoveExecuted;

    public delegate void SpecialMoveFaiiedDelegate(string reason);
    public event SpecialMoveFaiiedDelegate OnSpecialMoveFailed;

    private void Awake()
    {
        character = GetComponent<Character>();
        attackManager = GetComponent<AttackManager>();
        commandBuffer = GetComponent<CommandBuffer>();
    }

    private void Update()
    {
        UpdateInputHistory();
        CheckForSpecialMoves();
    }

    /// <summary>
    /// Update input history (record new inputs and timeout old ones)
    /// </summary>
    private void UpdateInputHistory()
    {
        inputTimeoutTimer -= Time.deltaTime;
        if (inputTimeoutTimer <= 0f)
        {
            recentInputs.Clear();
            inputTimeoutTimer = 0f;
        }
    }

    /// <summary>
    /// Record a new input to the history
    /// </summary>
    public void RecordInput(string inputName)
    {
        recentInputs.Add(inputName);
        inputTimeoutTimer = inputHistoryTimeout;

        // Keep only last 10 inputs to avoid too much history
        if (recentInputs.Count > 10)
        {
            recentInputs.RemoveAt(0);
        }

        Debug.Log($"[INPUT] {inputName} - Sequence: {string.Join(" → ", recentInputs)}");
    }

    /// <summary>
    /// Check if any special move input sequences match recent inputs
    /// </summary>
    private void CheckForSpecialMoves()
    {
        if (specialMoveRecastTime > 0)
        {
            specialMoveRecastTime -= Time.deltaTime;
            return;
        }

        foreach (var specialMove in availableSpecialMoves)
        {
            if (specialMove == null || !specialMove.IsValid())
                continue;

            if (CheckSpecialMoveInput(specialMove))
            {
                ExecuteSpecialMove(specialMove);
                break; // Only execute one special per frame
            }
        }
    }

    /// <summary>
    /// Check if a special move input sequence is present in recent inputs
    /// </summary>
    private bool CheckSpecialMoveInput(SpecialMove specialMove)
    {
        int sequenceLength = specialMove.inputSequence.Length;
        if (recentInputs.Count < sequenceLength)
            return false;

        // Check if last N inputs match the sequence
        int offset = recentInputs.Count - sequenceLength;
        for (int i = 0; i < sequenceLength; i++)
        {
            string recentInput = recentInputs[offset + i].ToLower();
            string requiredInput = specialMove.inputSequence[i].inputName.ToLower();

            if (recentInput != requiredInput)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Execute a special move
    /// </summary>
    private bool ExecuteSpecialMove(SpecialMove specialMove)
    {
        // Check preconditions
        if (!CanExecuteSpecialMove(specialMove))
        {
            OnSpecialMoveFailed?.Invoke("Cannot execute special move now");
            return false;
        }

        lastExecutedSpecial = specialMove;
        specialMoveRecastTime = specialMove.animationDuration;

        // Clear recent inputs (consumed for the special)
        recentInputs.Clear();

        // Play special move
        character.SetState(Character.CharacterState.Attacking);

        // Create a temporary attack with special move properties
        Debug.Log($"[SPECIAL MOVE] {specialMove.displayName} executed!");

        // Trigger animation
        if (character.GetComponent<AnimationController>() != null)
        {
            character.GetComponent<AnimationController>().PlayAttackAnimation(2); // Hash for special
        }

        // Spawn particle effect at character position
        if (!string.IsNullOrEmpty(specialMove.particleEffectPrefab))
        {
            SpawnParticleEffect(specialMove);
        }

        // Play sound
        if (specialMove.specialMoveSound != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.PlayOneShot(specialMove.specialMoveSound);
        }

        OnSpecialMoveExecuted?.Invoke(specialMove);
        return true;
    }

    /// <summary>
    /// Spawn particle effect for special move
    /// </summary>
    private void SpawnParticleEffect(SpecialMove specialMove)
    {
        // Try to load particle from resources
        GameObject particlePrefab = Resources.Load<GameObject>(specialMove.particleEffectPrefab);
        
        if (particlePrefab != null)
        {
            GameObject instance = Instantiate(particlePrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = instance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(instance, ps.main.duration + ps.main.startLifetime.constantMax);
            }
        }
        else if (specialMove.muzzleParticle != null)
        {
            specialMove.muzzleParticle.Play();
        }
    }

    /// <summary>
    /// Check if special move can be executed
    /// </summary>
    private bool CanExecuteSpecialMove(SpecialMove specialMove)
    {
        // Check state
        if (!character.IsAlive())
            return false;

        if (character.IsInHitstun())
            return false;

        // Check movement requirements
        if (specialMove.requiresGround && !character.IsGrounded())
            return false;

        if (specialMove.requiresAir && character.IsGrounded())
            return false;

        // Check if already attacking
        if (attackManager != null && attackManager.IsAttacking())
            return false;

        return true;
    }

    /// <summary>
    /// Get recently executed special move
    /// </summary>
    public SpecialMove GetLastExecutedSpecial() => lastExecutedSpecial;

    /// <summary>
    /// Get remaining time before special move can be recast
    /// </summary>
    public float GetSpecialMoveRecastTime() => Mathf.Max(0, specialMoveRecastTime);

    /// <summary>
    /// Reset special move state (for new round)
    /// </summary>
    public void ResetSpecialMoves()
    {
        recentInputs.Clear();
        lastExecutedSpecial = null;
        specialMoveRecastTime = 0f;
    }

    /// <summary>
    /// Get recent inputs as a string (for debugging/display)
    /// </summary>
    public string GetRecentInputsString()
    {
        if (recentInputs.Count == 0)
            return "";
        return string.Join(" → ", recentInputs);
    }
}
