using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple InputManager with input buffering and basic axis access.
/// Drop this on a persistent GameObject (or let it autoset as a singleton).
/// Extend mappings and events for your project as needed.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Tooltip("How long (seconds) an input stays buffered")]
    public float inputBufferTime = 0.18f;

    private struct BufferedInput { public string name; public float time; }
    private readonly List<BufferedInput> buffer = new List<BufferedInput>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        PollInputs();
        CleanupBuffer();
    }

    // Example polling - adapt button names to your Input settings or the new InputSystem
    private void PollInputs()
    {
        if (Input.GetButtonDown("Fire1")) AddBuffered("Light");
        if (Input.GetButtonDown("Fire2")) AddBuffered("Medium");
        if (Input.GetButtonDown("Fire3")) AddBuffered("Heavy");
        if (Input.GetButtonDown("Jump")) AddBuffered("Jump");

        // Horizontal/Vertical handled via axes
    }

    private void AddBuffered(string name)
    {
        buffer.Add(new BufferedInput { name = name, time = Time.time });
    }

    private void CleanupBuffer()
    {
        float cutoff = Time.time - inputBufferTime;
        buffer.RemoveAll(b => b.time < cutoff);
    }

    /// <summary>
    /// Consume a buffered input if present (returns true and removes it).
    /// Useful for combo detection where you check and consume inputs.
    /// </summary>
    public bool ConsumeBuffered(string name)
    {
        for (int i = 0; i < buffer.Count; i++)
        {
            if (buffer[i].name == name)
            {
                buffer.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Peek whether a buffered input exists without consuming it.
    /// </summary>
    public bool HasBuffered(string name)
    {
        return buffer.Exists(b => b.name == name);
    }

    /// <summary>
    /// Get raw axis value (e.g. "Horizontal", "Vertical").
    /// </summary>
    public float GetAxis(string axisName)
    {
        return Input.GetAxisRaw(axisName);
    }

    /// <summary>
    /// Clear all buffered inputs (useful when entering new states).
    /// </summary>
    public void ClearBuffer()
    {
        buffer.Clear();
    }
}
