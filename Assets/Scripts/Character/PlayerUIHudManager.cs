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

        private void Awake()
        {
            attributesManager = GetComponentInParent<PlayerAttributesManager>();
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
            healthBar.value = currentHealth;
        }
    }
}
