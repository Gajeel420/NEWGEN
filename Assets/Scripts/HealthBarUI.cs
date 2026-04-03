using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays health bar for a character in the HUD.
/// Visualizes current health and max health with smooth animation.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Character character;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image damageDelayImage; // Shows old health briefly
    [SerializeField] private Text healthText;

    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color warnColor = Color.yellow;
    [SerializeField] private Color criticallColor = Color.red;
    [SerializeField] private float damageIndicatorDelay = 0.3f;

    private float currentHealthPercent = 1f;
    private float targetHealthPercent = 1f;
    private float damageIndicatorTimer = 0f;

    private void OnEnable()
    {
        if (character != null)
        {
            character.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (character != null)
        {
            character.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void Update()
    {
        // Smoothly animate health bar
        if (Mathf.Abs(currentHealthPercent - targetHealthPercent) > 0.01f)
        {
            currentHealthPercent = Mathf.Lerp(currentHealthPercent, targetHealthPercent, Time.deltaTime * 5f);
        }
        else
        {
            currentHealthPercent = targetHealthPercent;
        }

        UpdateHealthBar();

        // Fade out damage indicator
        if (damageIndicatorTimer > 0)
        {
            damageIndicatorTimer -= Time.deltaTime;
            float alpha = damageIndicatorTimer / damageIndicatorDelay;
            if (damageDelayImage != null)
            {
                Color color = damageDelayImage.color;
                color.a = alpha;
                damageDelayImage.color = color;
            }
        }
    }

    /// <summary>
    /// Update the health bar fill
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = currentHealthPercent;

            // Color based on health
            if (currentHealthPercent > 0.5f)
                healthFillImage.color = Color.Lerp(warnColor, healthyColor, (currentHealthPercent - 0.5f) * 2f);
            else if (currentHealthPercent > 0.25f)
                healthFillImage.color = Color.Lerp(criticallColor, warnColor, (currentHealthPercent - 0.25f) * 2f);
            else
                healthFillImage.color = criticallColor;
        }

        if (healthText != null && character != null)
        {
            healthText.text = $"{character.GetHealth():F0} / {character.MaxHealth:F0}";
        }
    }

    /// <summary>
    /// Handle health change
    /// </summary>
    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        targetHealthPercent = currentHealth / maxHealth;

        // Show damage indicator
        if (damageDelayImage != null)
        {
            damageDelayImage.fillAmount = currentHealthPercent; // Show previous health
            damageIndicatorTimer = damageIndicatorDelay;
        }
    }

    /// <summary>
    /// Set the character this health bar displays
    /// </summary>
    public void SetCharacter(Character newCharacter)
    {
        if (character != null)
        {
            character.OnHealthChanged -= HandleHealthChanged;
        }

        character = newCharacter;

        if (character != null)
        {
            character.OnHealthChanged += HandleHealthChanged;
            targetHealthPercent = character.GetHealthPercent();
            currentHealthPercent = character.GetHealthPercent();
        }
    }

    /// <summary>
    /// Update health display
    /// </summary>
    public void UpdateHealth(float health, float maxHealth)
    {
        targetHealthPercent = health / maxHealth;
    }

    public Character GetCharacter() => character;
}
