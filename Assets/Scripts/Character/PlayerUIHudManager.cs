using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace SG
{
    public class PlayerUIHudManager : MonoBehaviour
    {
        [Header("Popup Manager")]
        public PopupMessageManager popupMessageManager;

        [Header("Stamina UI")]
        public Slider staminaBar;
        public Slider healthBar;

        [Header("Player Reference")]
        private PlayerAttributesManager attributesManager;
        private PlayerSelector selectorManager;

        private void Awake()
        {
            attributesManager = GetComponentInParent<PlayerAttributesManager>();
            selectorManager = GetComponentInParent<PlayerSelector>();
            //OnItemPicked("Sword of the monster");
            //OnItemPicked("Heshams toes");
            //OnItemPicked("Sakiballs");
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

        //public void OnItemPicked(string itemName)
       // {
          //  if (popupMessageManager != null)
           // {
               // popupMessageManager.ShowPopup($"{itemName}");
            //}
        //}
    }
}
