using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a combo pattern: a sequence of inputs with timing windows and damage scaling.
/// Create instances using the ScriptableObject menu.
/// </summary>
[CreateAssetMenu(fileName = "Combo", menuName = "Fighting Game/Combo")]
public class Combo : ScriptableObject
{
    [System.Serializable]
    public struct ComboInput
    {
        public string inputName; // "Light", "Medium", "Heavy", "Jump"
        public float timingWindow; // Time in seconds to input next move
    }

    [SerializeField] public string comboName = "Basic Combo";
    [SerializeField] public ComboInput[] inputs = new ComboInput[]
    {
        new ComboInput { inputName = "Light", timingWindow = 0.5f },
        new ComboInput { inputName = "Light", timingWindow = 0.5f },
        new ComboInput { inputName = "Medium", timingWindow = 0.5f }
    };

    [SerializeField] public float damageScaling = 1.0f; // Multiplier on each hit
    [SerializeField] public float comboBreakTime = 1.0f; // Reset combo if no input for this long
    [SerializeField] public bool canCancel = true; // Allow canceling into next move during recovery
    [SerializeField] public float knockbackScaling = 1.0f; // Modify knockback per hit

    /// <summary>
    /// Check if this combo matches the input sequence.
    /// </summary>
    public bool CheckMatch(List<string> inputSequence)
    {
        if (inputSequence.Count < inputs.Length)
            return false;

        int startIndex = inputSequence.Count - inputs.Length;
        for (int i = 0; i < inputs.Length; i++)
        {
            if (inputSequence[startIndex + i] != inputs[i].inputName)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get the number of inputs in this combo.
    /// </summary>
    public int GetComboLength() => inputs.Length;

    /// <summary>
    /// Get timing window for a specific index.
    /// </summary>
    public float GetTimingWindow(int index)
    {
        if (index < 0 || index >= inputs.Length)
            return 0f;
        return inputs[index].timingWindow;
    }

    /// <summary>
    /// Get the input name at a specific index.
    /// </summary>
    public string GetInputName(int index)
    {
        if (index < 0 || index >= inputs.Length)
            return "";
        return inputs[index].inputName;
    }

    public override string ToString()
    {
        var sequence = "";
        foreach (var input in inputs)
        {
            sequence += input.inputName + " > ";
        }
        return $"{comboName}: {sequence.TrimEnd()} (DMG: {damageScaling}x)";
    }
}
