using UnityEngine;
using UnityEngine.UI;

namespace SG
{
    public class PlayerUIHudManager : MonoBehaviour
    {
        [Header("Stamina UI")]
        public Slider staminaBar;
        public Text staminaText;

        private PlayerAttributesManager attributesManager;

        private void Awake()
        {
            attributesManager = GetComponentInParent<PlayerAttributesManager>();
        }

        private void Update()
        {
            UpdateStaminaUI();
        }

        private void UpdateStaminaUI()
        {
            if (attributesManager != null)
            {
                staminaBar.value = attributesManager.CurrentStamina / attributesManager.MaxStamina;
                staminaText.text = $"{Mathf.RoundToInt(attributesManager.CurrentStamina)} / {Mathf.RoundToInt(attributesManager.MaxStamina)}";
            }
        }
    }
}