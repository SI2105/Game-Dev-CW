using System.Collections;
using UnityEngine;

namespace SG
{
    public class PlayerAttributesManager : MonoBehaviour
    {
        #region Core Attributes
        public float MaxHealth { get; set; } = 100f;
        public float CurrentHealth { get; set; } = 100f;
        public float HealthRegenRate { get; set; } = 10f;
        public float MaxStamina { get; set; } = 100f;
        public float StaminaCostPerSecond { get; set; } = 25f;
        private float currentStamina;
        public float CurrentStamina
        {
            get => currentStamina;
            set
            {
                currentStamina = Mathf.Clamp(value, 0, MaxStamina);
                OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
            }
        }

        public float StaminaRegenRate { get; set; } = 15f;
        #endregion

        #region Methods for Stamina
        public delegate void StaminaChangedHandler(float currentStamina, float maxStamina);
        public event StaminaChangedHandler OnStaminaChanged;

        private bool isRecoveringStamina = true;
        private Coroutine staminaRecoveryCoroutine;
        public bool UseStamina(float amount)
        {
            if (CurrentStamina >= amount)
            {
                // Deduct stamina
                CurrentStamina -= amount;
                // Stop recovery temporarily
                StopStaminaRecovery();
                // Delay recovery by 2 seconds
                StartStaminaRecoveryWithDelay(2f);

                return true;
            }

            return false;
        }

        private void Awake()
        {
            currentStamina = MaxStamina;  // Make sure it starts at max
        }
        private void StartStaminaRecoveryWithDelay(float delay)
        {
            if (staminaRecoveryCoroutine != null)
            {
                StopCoroutine(staminaRecoveryCoroutine);
            }
            staminaRecoveryCoroutine = StartCoroutine(StaminaRecoveryCoroutine(delay));
        }

        public void StartStaminaRecovery()
        {
            StartStaminaRecoveryWithDelay(0f);
        }


       private IEnumerator StaminaRecoveryCoroutine(float delay)
       {
            // Wait for the delay before starting recovery
            yield return new WaitForSeconds(delay);

            // Recover stamina over time
            while (CurrentStamina < MaxStamina)
            {
                CurrentStamina += StaminaRegenRate * Time.deltaTime;

                // Debug log for tracking recovery

                yield return null; // Wait for the next frame
            }

            staminaRecoveryCoroutine = null; // Recovery is complete
        }

        private void StopStaminaRecovery()
        {
            if (staminaRecoveryCoroutine != null)
            {
                StopCoroutine(staminaRecoveryCoroutine);
                staminaRecoveryCoroutine = null;
            }
        }

        public void SetMaxStaminaBasedOnEndurance()
        {
            MaxStamina = CalculateStaminaBasedOnEnduranceLevel(Endurance);
            CurrentStamina = Mathf.Min(CurrentStamina, MaxStamina);
        }

        public float CalculateStaminaBasedOnEnduranceLevel(float endurance)
        {
            return endurance * 10f; // Example: Each point of Endurance gives 10 stamina
        }
        #endregion

        #region Exploration and Survival Attributes
        public float TorchFuel { get; set; } = 100f;
        public float MaxTorchFuel { get; set; } = 100f;
        public float TorchDrainRate { get; set; } = 1f;
        public int MaxInventorySlots { get; set; } = 20;
        public float TrapDetectionLevel { get; set; } = 1f;
        public float PoisonResistance { get; set; } = 0.2f;
        public float BleedResistance { get; set; } = 0.1f;
        #endregion

        #region Combat Attributes
        public float BaseDamage { get; set; } = 15f;
        public float CriticalHitChance { get; set; } = 0.1f;
        public float CriticalHitMultiplier { get; set; } = 2f;
        public float AttackSpeed { get; set; } = 1.2f;
        public float Armor { get; set; } = 10f;
        public float BlockChance { get; set; } = 0.2f;
        public float DodgeChance { get; set; } = 0.15f;
        #endregion

        #region Progression Attributes
        public int CurrentLevel { get; set; } = 1;
        public int MaxLevel { get; set; } = 50;
        public float CurrentXP { get; set; } = 0f;
        public float XPToNextLevel { get; set; } = 100f;
        public float Strength { get; set; } = 10f;
        public float Agility { get; set; } = 8f;
        public float Endurance { get; set; } = 12f;
        public float Intelligence { get; set; } = 5f;
        public float Luck { get; set; } = 3f;
        #endregion

        #region Dungeon-Crawling Specific Attributes
        public float VisionRange { get; set; } = 10f;
        public float NoiseLevel { get; set; } = 1f;
        public int LockpickingSkill { get; set; } = 1;
        public int TrapsDisarmSkill { get; set; } = 1;
        public float FearResistance { get; set; } = 0.5f;
        public float Hunger { get; set; } = 100f;
        public float MaxHunger { get; set; } = 100f;
        public float HungerDrainRate { get; set; } = 1f;
        #endregion
    }
}
