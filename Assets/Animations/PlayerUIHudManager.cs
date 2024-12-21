using UnityEngine;
using UnityEngine.UI;

namespace SG
{
    public class PlayerUIHudManager : MonoBehaviour
    {
        [Header("Stamina UI")]
        public Slider staminaBar;

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
            }
        }

        private void OnDisable()
        {
            if (attributesManager != null)
            {
                attributesManager.OnStaminaChanged -= UpdateStaminaUI;
            }
        }

        private void UpdateStaminaUI(float currentStamina, float maxStamina)
        {
            staminaBar.value = currentStamina;
        }
    }
}
