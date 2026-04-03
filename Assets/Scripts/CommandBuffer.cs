using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a history of input commands for combo detection and pattern matching.
/// Commands are timestamped and can be queried as a sequence.
/// </summary>
public class CommandBuffer : MonoBehaviour
{
    [Tooltip("How long (seconds) to keep commands in history")]
    public float historyDuration = 1.0f;

    [Tooltip("Reference to InputManager for receiving inputs")]
    public InputManager inputManager;

    private Queue<Command> history = new Queue<Command>();

    private struct Command
    {
        public string name;
        public float timestamp;
        public Vector2 direction; // For directional inputs
    }

    void Start()
    {
        if (inputManager == null)
            inputManager = InputManager.Instance;
    }

    void Update()
    {
        // Poll recently buffered inputs and add them to history
        // This is a basic example - extend to match your input names
        CleanupOldCommands();
    }

    /// <summary>
    /// Add a command to the history.
    /// </summary>
    public void RecordCommand(string name, Vector2 direction = default)
    {
        history.Enqueue(new Command { name = name, timestamp = Time.time, direction = direction });
    }

    /// <summary>
    /// Remove commands older than historyDuration.
    /// </summary>
    private void CleanupOldCommands()
    {
        float cutoff = Time.time - historyDuration;
        while (history.Count > 0 && history.Peek().timestamp < cutoff)
            history.Dequeue();
    }

    /// <summary>
    /// Check if the last N commands match a sequence (ignoring time).
    /// Example: CheckSequence(new[] { "Light", "Light", "Medium" })
    /// </summary>
    public bool CheckSequence(params string[] sequence)
    {
        if (history.Count < sequence.Length)
            return false;

        var list = new List<Command>(history);
        int start = list.Count - sequence.Length;

        for (int i = 0; i < sequence.Length; i++)
        {
            if (list[start + i].name != sequence[i])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Check if the last N commands match a sequence within a time window.
    /// Example: CheckSequenceWithTiming(0.5f, new[] { "Light", "Medium", "Heavy" })
    /// </summary>
    public bool CheckSequenceWithTiming(float timingWindowSeconds, params string[] sequence)
    {
        if (history.Count < sequence.Length)
            return false;

        var list = new List<Command>(history);
        int start = list.Count - sequence.Length;

        // Check sequence names
        for (int i = 0; i < sequence.Length; i++)
        {
            if (list[start + i].name != sequence[i])
                return false;
        }

        // Check timing - all commands must occur within timingWindowSeconds
        float firstTime = list[start].timestamp;
        float lastTime = list[start + sequence.Length - 1].timestamp;
        return (lastTime - firstTime) <= timingWindowSeconds;
    }

    /// <summary>
    /// Get a string representation of the last N commands (useful for debugging).
    /// </summary>
    public string GetStringSequence(int count = -1)
    {
        if (count < 0 || count > history.Count)
            count = history.Count;

        var list = new List<Command>(history);
        int start = Mathf.Max(0, list.Count - count);

        var result = "";
        for (int i = start; i < list.Count; i++)
        {
            result += list[i].name;
            if (i < list.Count - 1) result += " > ";
        }

        return result;
    }

    /// <summary>
    /// Clear the entire command history.
    /// </summary>
    public void Clear()
    {
        history.Clear();
    }

    public int Count => history.Count;
}
