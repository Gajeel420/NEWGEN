using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays combo counter for a character in the HUD.
/// Shows combo count and damage multiplier in real-time during combat.
/// </summary>
public class ComboCounterUI : MonoBehaviour
{
    [SerializeField] private Character character;
    [SerializeField] private AttackManager attackManager;
    [SerializeField] private Text comboCountText;
    [SerializeField] private Text comboMultiplierText;
    [SerializeField] private Image comboIndicatorImage;

    [SerializeField] private Color activeComboColor = Color.cyan;
    [SerializeField] private Color inactiveComboColor = Color.gray;

    private int currentComboCount = 0;
    private float comboFadeTimer = 0f;
    private const float comboFadeDuration = 0.5f;

    private void Update()
    {
        if (attackManager == null)
            return;

        // Update combo display
        int newComboCount = attackManager.GetComboCount();
        if (newComboCount != currentComboCount)
        {
            currentComboCount = newComboCount;
            comboFadeTimer = comboFadeDuration;
            PlayComboAnimation();
        }

        // Fade out combo display when inactive
        if (comboFadeTimer > 0)
        {
            comboFadeTimer -= Time.deltaTime;

            if (comboCountText != null)
            {
                Color color = comboCountText.color;
                color.a = Mathf.Max(0, comboFadeTimer / comboFadeDuration);
                comboCountText.color = color;
            }

            if (comboMultiplierText != null)
            {
                Color color = comboMultiplierText.color;
                color.a = Mathf.Max(0, comboFadeTimer / comboFadeDuration);
                comboMultiplierText.color = color;
            }
        }

        UpdateComboDisplay();
    }

    /// <summary>
    /// Update combo counter display
    /// </summary>
    private void UpdateComboDisplay()
    {
        if (attackManager == null)
            return;

        float comboScalar = attackManager.GetComboScalar();

        if (comboCountText != null)
        {
            comboCountText.text = currentComboCount > 0 ? $"COMBO x{currentComboCount}" : "";
        }

        if (comboMultiplierText != null)
        {
            comboMultiplierText.text = comboScalar > 1.0f ? $"{comboScalar:F2}x DAMAGE" : "";
        }

        // Update indicator color
        if (comboIndicatorImage != null)
        {
            comboIndicatorImage.color = currentComboCount > 0 ? activeComboColor : inactiveComboColor;
        }
    }

    /// <summary>
    /// Play animation when combo hits
    /// </summary>
    private void PlayComboAnimation()
    {
        // Could add scale pulse, particle effects, etc.
        if (comboCountText != null)
        {
            // Flash the combo text
            comboCountText.color = activeComboColor;
        }
    }

    /// <summary>
    /// Update combo counter
    /// </summary>
    public void UpdateCombo(int comboCount)
    {
        currentComboCount = comboCount;
        comboFadeTimer = comboFadeDuration;
        UpdateComboDisplay();
    }

    /// <summary>
    /// Set the character this combo counter displays
    /// </summary>
    public void SetCharacter(Character newCharacter)
    {
        character = newCharacter;
        if (character != null)
        {
            attackManager = character.GetComponent<AttackManager>();
        }
    }

    public Character GetCharacter() => character;
}
