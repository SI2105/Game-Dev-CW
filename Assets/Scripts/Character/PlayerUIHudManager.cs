using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace SG
{
    public class PlayerUIHudManager : MonoBehaviour
    {
        [Header("Stamina UI")]
        public Slider staminaBar;
        public Slider healthBar;

        public TextMeshProUGUI playerLevelText;

        [Header("Hit Indicator UI")]
        public Image selectorDot; // Default active image
        public Image onHit; // Inactive by default, shown when a hit occurs

        public TextMeshProUGUI LevelUpText;

        [Header("Boost UI")]
        public Image icon; // Icon to show for the active boost
        public TextMeshProUGUI allAttributesBoostTimerText;
        public TextMeshProUGUI strengthBoostTimerText;

        private PlayerAttributesManager attributesManager;
        private WeaponCollisionHandler weaponCollisionHandler;

        private void Awake()
            {
                attributesManager = GetComponentInParent<PlayerAttributesManager>();
                
                // Search in entire hierarchy
                weaponCollisionHandler = GetComponentInParent<WeaponCollisionHandler>();
                if (weaponCollisionHandler == null)
                {
                    weaponCollisionHandler = FindObjectOfType<WeaponCollisionHandler>();
                }
                
                if (weaponCollisionHandler != null)
                {
                    Debug.Log($"Found WeaponCollisionHandler: {weaponCollisionHandler.gameObject.name}");
                    // Subscribe immediately if found
                    weaponCollisionHandler.OnHit += HandleWeaponHit;
                }
                else
                {
                    Debug.LogError("WeaponCollisionHandler not found in scene.");
                }
            }
        
        private T GetChildComponent<T>() where T : Component
        {
            foreach (Transform child in transform)
            {
                var component = child.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
            return null;
        }

        private void OnDestroy()
        {
            // Ensure cleanup
            if (weaponCollisionHandler != null)
            {
                weaponCollisionHandler.OnHit -= HandleWeaponHit;
            }
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

            // Ensure the boost icon is hidden by default
            if (icon != null)
            {
                icon.gameObject.SetActive(false);
            }

            if (playerLevelText != null && attributesManager != null)
            {
                UpdateLevelUI(attributesManager.CurrentLevel);
            }

            // Initialize the hit indicator
            if (selectorDot != null)
            {
                selectorDot.gameObject.SetActive(true);
            }

            if (onHit != null)
            {
                onHit.gameObject.SetActive(false);
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

            // Unsubscribe from the OnHit event
            if (weaponCollisionHandler != null)
            {
                print("here");
                weaponCollisionHandler.OnHit -= HandleWeaponHit;
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
                // Update the level text
                playerLevelText.text = $"{newLevel}";

                // Show the level-up popup message
                // if (LevelUpText)
                // {
                    // LevelUpText.text = $"New Level Reached! {newLevel}";
                //     StartCoroutine(ShowLevelUpMessage());
                // }
            }
        }

        private void Start(){
            RefreshUI();
        }

        private IEnumerator ShowLevelUpMessage()
        {
            // Ensure the text is visible
            LevelUpText.gameObject.SetActive(true);

            // Wait for 1 second
            yield return new WaitForSeconds(1f);

            // Hide the text after the delay
            LevelUpText.gameObject.SetActive(false);
        }


        // ------------------- Hit Indicator Handler ------------------- //

        /// <summary>
        /// Handles the hit event from the WeaponCollisionHandler.
        /// Toggles between the selectorDot and onHit images.
        /// </summary>
        private void HandleWeaponHit()
        {
            print("Hit");
            if (selectorDot != null && onHit != null)
            {
                // Stop any existing coroutine to prevent multiple coroutines running simultaneously
                StopAllCoroutines();

                // Show the onHit image and hide the selectorDot
                selectorDot.gameObject.SetActive(false);
                onHit.gameObject.SetActive(true);

                // Start coroutine to reset the images after 1 second
                StartCoroutine(ResetHitIndicator());
            }
        }

        /// <summary>
        /// Coroutine to reset the hit indicator to the selectorDot after a delay.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ResetHitIndicator()
        {
            yield return new WaitForSeconds(1f); // 1 second delay

            if (selectorDot != null && onHit != null)
            {
                // Show the selectorDot and hide the onHit image
                selectorDot.gameObject.SetActive(true);
                onHit.gameObject.SetActive(false);
            }
        }
        public void RefreshUI()
        {
            if (attributesManager != null)
            {
                // Refresh Stamina and Health
                UpdateStaminaUI(attributesManager.CurrentStamina, attributesManager.MaxStamina);
                UpdateHealthUI(attributesManager.CurrentHealth, attributesManager.MaxHealth);

                // Refresh Level UI
                UpdateLevelUI(attributesManager.CurrentLevel);
            }
        }

    }
}
