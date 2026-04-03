using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple InputManager with input buffering and basic axis access.
/// Uses Unity's new Input System. Assign InputActionReferences in the Inspector.
/// Drop this on a persistent GameObject (or let it autoset as a singleton).
/// Extend mappings and events for your project as needed.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Tooltip("How long (seconds) an input stays buffered")]
    public float inputBufferTime = 0.18f;

    // Assign these in the Inspector to your InputActionAsset actions
    public InputActionReference lightAction;
    public InputActionReference mediumAction;
    public InputActionReference heavyAction;
    public InputActionReference jumpAction;
    public InputActionReference moveAction; // Vector2 for horizontal/vertical

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

        // Enable actions if assigned
        lightAction?.action.Enable();
        mediumAction?.action.Enable();
        heavyAction?.action.Enable();
        jumpAction?.action.Enable();
        moveAction?.action.Enable();
    }

    void Update()
    {
        PollInputs();
        CleanupBuffer();
    }

    // Poll using new Input System actions
    private void PollInputs()
    {
        if (lightAction != null && lightAction.action.WasPressedThisFrame()) AddBuffered("Light");
        if (mediumAction != null && mediumAction.action.WasPressedThisFrame()) AddBuffered("Medium");
        if (heavyAction != null && heavyAction.action.WasPressedThisFrame()) AddBuffered("Heavy");
        if (jumpAction != null && jumpAction.action.WasPressedThisFrame()) AddBuffered("Jump");

        // Movement handled via GetAxis method
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
    /// Get axis value from moveAction (e.g. "Horizontal" -> x, "Vertical" -> y).
    /// </summary>
    public float GetAxis(string axisName)
    {
        if (moveAction == null) return 0f;
        Vector2 move = moveAction.action.ReadValue<Vector2>();
        return axisName == "Horizontal" ? move.x : axisName == "Vertical" ? move.y : 0f;
    }

    /// <summary>
    /// Clear all buffered inputs (useful when entering new states).
    /// </summary>
    public void ClearBuffer()
    {
        buffer.Clear();
    }

    void OnDestroy()
    {
        // Disable actions
        lightAction?.action.Disable();
        mediumAction?.action.Disable();
        heavyAction?.action.Disable();
        jumpAction?.action.Disable();
        moveAction?.action.Disable();
    }
}
