using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detects combos by matching input sequences against a list of Combo patterns.
/// Attach to the character and reference available combos.
/// </summary>
public class ComboDetector : MonoBehaviour
{
    [SerializeField] private Combo[] availableCombos;
    [SerializeField] private CommandBuffer commandBuffer;

    private List<string> currentComboInputs = new List<string>();
    private Combo currentCombo = null;
    private int comboHitCount = 0;
    private float lastInputTime = 0f;

    public delegate void ComboDetectedDelegate(Combo combo, int hitCount);
    public event ComboDetectedDelegate OnComboDetected;

    public delegate void ComboBrokenDelegate();
    public event ComboBrokenDelegate OnComboBroken;

    void Awake()
    {
        if (commandBuffer == null)
            commandBuffer = GetComponent<CommandBuffer>();
    }

    void Update()
    {
        if (commandBuffer == null) return;

        // Check for new buffered inputs
        CheckForNewInputs();

        // Check for combo timeout (reset if no input for too long)
        if (currentCombo != null && Time.time - lastInputTime > currentCombo.comboBreakTime)
        {
            ResetCombo();
        }
    }

    private void CheckForNewInputs()
    {
        // Poll InputManager buffer for buffered inputs this frame
        string[] inputTypes = { "Light", "Medium", "Heavy", "Jump" };

        foreach (string inputType in inputTypes)
        {
            if (InputManager.Instance && InputManager.Instance.HasBuffered(inputType))
            {
                RecordInput(inputType);
            }
        }
    }

    /// <summary>
    /// Record an input and check for combo matches.
    /// </summary>
    public void RecordInput(string inputName)
    {
        currentComboInputs.Add(inputName);
        lastInputTime = Time.time;

        Debug.Log($"[ComboDetector] Input recorded: {inputName} | Sequence: {GetSequenceString()}");

        // Check all combos for a match
        foreach (Combo combo in availableCombos)
        {
            if (combo.CheckMatch(currentComboInputs))
            {
                DetectCombo(combo);
                return;
            }
        }

        // Keep history trimmed to avoid memory bloat
        if (currentComboInputs.Count > 10)
        {
            currentComboInputs.RemoveAt(0);
        }
    }

    /// <summary>
    /// Called when a combo is detected.
    /// </summary>
    private void DetectCombo(Combo combo)
    {
        currentCombo = combo;
        comboHitCount++;
        lastInputTime = Time.time;

        Debug.Log($"[COMBO DETECTED] {combo.comboName} (Hit #{comboHitCount})!");

        // Invoke event for UI, effects, etc.
        OnComboDetected?.Invoke(combo, comboHitCount);

        // Clear the matched inputs from history
        int matchLength = combo.GetComboLength();
        currentComboInputs.RemoveRange(currentComboInputs.Count - matchLength, matchLength);
    }

    /// <summary>
    /// Reset combo state (called on timeout or when breaking chain).
    /// </summary>
    public void ResetCombo()
    {
        if (currentCombo != null)
        {
            Debug.Log($"[ComboDetector] Combo broken! Final count: {comboHitCount}");
            OnComboBroken?.Invoke();
        }

        currentCombo = null;
        comboHitCount = 0;
        currentComboInputs.Clear();
        lastInputTime = 0f;
    }

    /// <summary>
    /// Get the current combo (if active).
    /// </summary>
    public Combo GetCurrentCombo() => currentCombo;

    /// <summary>
    /// Get current combo hit count.
    /// </summary>
    public int GetComboCount() => comboHitCount;

    /// <summary>
    /// Get damage scaling for current combo hit.
    /// </summary>
    public float GetComboScalar()
    {
        if (currentCombo == null) return 1f;
        return Mathf.Pow(currentCombo.damageScaling, comboHitCount);
    }

    /// <summary>
    /// Get knockback scaling for current combo hit.
    /// </summary>
    public float GetKnockbackScalar()
    {
        if (currentCombo == null) return 1f;
        return currentCombo.knockbackScaling;
    }

    /// <summary>
    /// Debug: get the current input sequence as a string.
    /// </summary>
    public string GetSequenceString()
    {
        string result = "";
        foreach (string input in currentComboInputs)
        {
            result += input + " > ";
        }
        return result.Length > 0 ? result.Substring(0, result.Length - 3) : "(empty)";
    }

    public void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw combo UI info
        Vector3 pos = transform.position + Vector3.up * 2f;
        if (currentCombo != null)
        {
            Debug.DrawLine(pos, pos + Vector3.right * comboHitCount * 0.2f, Color.yellow);
        }
    }
}
