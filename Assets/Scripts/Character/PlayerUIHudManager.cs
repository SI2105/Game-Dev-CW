using UnityEngine;
using UnityEngine.UI;

namespace SG
{
    public class PlayerUIHudManager : MonoBehaviour
    {
        [Header("Stamina UI")]
        public Slider staminaBar;
        public Slider healthBar;

        [Header("Player Reference")]
        private PlayerAttributesManager attributesManager;

        private Vector3 initialHealthBarPosition; // Store the initial position

        private void Awake()
        {
            attributesManager = GetComponentInParent<PlayerAttributesManager>();
            
            // Store the initial position of the health bar
            if (healthBar != null)
            {
                initialHealthBarPosition = healthBar.GetComponent<RectTransform>().position;
            }
        }

        private void OnEnable()
        {
            if (attributesManager != null)
            {
                attributesManager.OnStaminaChanged += UpdateStaminaUI;
                attributesManager.OnHealthChanged += UpdateHealthUI;
            }
        }

        private void OnDisable()
        {
            if (attributesManager != null)
            {
                attributesManager.OnStaminaChanged -= UpdateStaminaUI;
                attributesManager.OnHealthChanged -= UpdateHealthUI;
            }
        }

        private void UpdateStaminaUI(float currentStamina, float maxStamina)
        {
            staminaBar.value = currentStamina;
        }

        private void UpdateHealthUI(float currentHealth, float maxHealth)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }
}
