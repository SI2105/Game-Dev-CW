using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SG
{
    public class PlayerUIHudManager : MonoBehaviour
    {
        [Header("Stamina UI")]
        public Slider staminaBar;
        public Slider healthBar;

        public TextMeshProUGUI playerLevelText;

        [Header("Boost UI")]
        public Image icon; // Icon to show for the active boost
        public TextMeshProUGUI allAttributesBoostTimerText;
        public TextMeshProUGUI strengthBoostTimerText;

        private PlayerAttributesManager attributesManager;

        private void Awake()
        {
            attributesManager = GetComponentInParent<PlayerAttributesManager>();
        }

        private void OnEnable()
        {
            if (attributesManager != null)
            {
                // Subscribe to stamina/health events
                attributesManager.OnStaminaChanged += UpdateStaminaUI;
                attributesManager.OnHealthChanged += UpdateHealthUI;

                // Subscribe to level change event
                attributesManager.OnLevelChanged += UpdateLevelUI;

                // Subscribe to boost events
                attributesManager.OnAllAttributesBoostCountdownChanged += HandleAllAttributesBoostCountdown;
                attributesManager.OnAllAttributesBoostEnded += HandleAllAttributesBoostEnded;

                attributesManager.OnStrengthBoostCountdownChanged += HandleStrengthBoostCountdown;
                attributesManager.OnStrengthBoostEnded += HandleStrengthBoostEnded;
            }

            // Initialize UI with current level
            if (playerLevelText != null && attributesManager != null)
            {
                UpdateLevelUI(attributesManager.CurrentLevel);
            }

            // Ensure the boost icon is hidden by default
            if (icon != null)
            {
                icon.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (attributesManager != null)
            {
                // Unsubscribe from stamina/health events
                attributesManager.OnStaminaChanged -= UpdateStaminaUI;
                attributesManager.OnHealthChanged -= UpdateHealthUI;

                // Unsubscribe from level change event
                attributesManager.OnLevelChanged -= UpdateLevelUI;

                // Unsubscribe from boost events
                attributesManager.OnAllAttributesBoostCountdownChanged -= HandleAllAttributesBoostCountdown;
                attributesManager.OnAllAttributesBoostEnded -= HandleAllAttributesBoostEnded;

                attributesManager.OnStrengthBoostCountdownChanged -= HandleStrengthBoostCountdown;
                attributesManager.OnStrengthBoostEnded -= HandleStrengthBoostEnded;
            }
        }

        // ------------------- Stamina / Health UI ------------------- //
        private void UpdateStaminaUI(float currentStamina, float maxStamina)
        {
            if (staminaBar)
            {
                staminaBar.maxValue = maxStamina;
                staminaBar.value = currentStamina;
            }
        }

        private void UpdateHealthUI(float currentHealth, float maxHealth)
        {
            if (healthBar)
            {
                healthBar.maxValue = maxHealth;
                healthBar.value = currentHealth;
            }
        }

        // --------------- All Attributes Boost UI Handlers --------------- //
        private void HandleAllAttributesBoostCountdown(float timeLeft)
        {
            if (allAttributesBoostTimerText)
            {
                allAttributesBoostTimerText.text = $"{timeLeft:F1}s";
            }

            // Show the boost icon if hidden
            if (icon != null && !icon.gameObject.activeSelf)
            {
                icon.gameObject.SetActive(true);
            }
        }

        private void HandleAllAttributesBoostEnded()
        {
            if (allAttributesBoostTimerText)
            {
                allAttributesBoostTimerText.text = "";
            }

            // Hide the boost icon
            if (icon != null && icon.gameObject.activeSelf)
            {
                icon.gameObject.SetActive(false);
            }
        }

        // --------------- Strength Boost UI Handlers --------------- //
        private void HandleStrengthBoostCountdown(float timeLeft)
        {
            if (strengthBoostTimerText)
            {
                strengthBoostTimerText.text = $"Strength Boost: {timeLeft:F1}s";
            }

            // Show the boost icon if hidden
            if (icon != null && !icon.gameObject.activeSelf)
            {
                icon.gameObject.SetActive(true);
            }
        }

        private void HandleStrengthBoostEnded()
        {
            if (strengthBoostTimerText)
            {
                strengthBoostTimerText.text = "";
            }

            // Hide the boost icon
            if (icon != null && icon.gameObject.activeSelf)
            {
                icon.gameObject.SetActive(false);
            }
        }

        private void UpdateLevelUI(int newLevel)
        {
            if (playerLevelText)
            {
                playerLevelText.text = $"{newLevel}";
            }
        }
    }
}
