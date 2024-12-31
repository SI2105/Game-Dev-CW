using System.Collections;
using UnityEngine;
using System;

namespace SG
{
    public class PlayerAttributesManager : MonoBehaviour
    {
        #region Inventory
        //public GameObject Inventory;
        [SerializeField] private InventoryManager inventoryManager;
        public InventoryManager InventoryManager {
            get => inventoryManager;
            set => inventoryManager = value;
        }

        #endregion
        #region Objectives
        [SerializeField] private ObjectiveManager objectiveManager;

        public ObjectiveManager ObjectiveManager
        {
            get => objectiveManager;
            set => objectiveManager = value;
        }

        #endregion

        #region SkillTree
        [SerializeField] private SkillTreeManager skillTreeManager;
        public SkillTreeManager SkillTreeManager { get => skillTreeManager; 
            set => skillTreeManager = value; 
        }
        #endregion

        public float MaxHealth { get; set; } = 100f;
        public float HealthRegenRate { get; set; } = 10f;
        public float MaxStamina { get; set; } = 100f;
        public float StaminaCostPerSecond { get; set; } = 25f;
        public float StaminaRegenRate { get; set; } = 15f;
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

        private float currentHealth;
        public float CurrentHealth
        {
            get => currentHealth;
            set
            {
                currentHealth = Mathf.Clamp(value, 0, MaxHealth);
                OnHealthChanged?.Invoke(currentHealth, MaxHealth);
            }
        }

        public delegate void StaminaChangedHandler(float currentStamina, float maxStamina);
        public event StaminaChangedHandler OnStaminaChanged;

        public delegate void HealthChangedHandler(float currentHealth, float maxHealth);
        public event HealthChangedHandler OnHealthChanged;

        private Coroutine recoveryCoroutine;

        private void Awake()
        {
            currentStamina = MaxStamina;
            currentHealth = MaxHealth;
        }

        public void ModifyResource(
            Func<float> getCurrentResource,
            Action<float> setCurrentResource,
            float maxResource,
            float changeAmount,
            float regenRate,
            float delay,
            System.Action onStopRecovery = null,
            System.Action onStartRecovery = null)
        {
            // Adjust resource value
            float newValue = Mathf.Clamp(getCurrentResource() + changeAmount, 0, maxResource);
            setCurrentResource(newValue);

            // Stop any ongoing recovery
            if (recoveryCoroutine != null)
            {
                StopCoroutine(recoveryCoroutine);
                recoveryCoroutine = null;
            }

            onStopRecovery?.Invoke();

            // Start recovery after delay
            if (changeAmount < 0 && regenRate > 0)
            {
                recoveryCoroutine = StartCoroutine(RecoveryCoroutine(getCurrentResource, setCurrentResource, maxResource, regenRate, delay, onStartRecovery));
            }
        }

        private IEnumerator RecoveryCoroutine(System.Func<float> getResource, System.Action<float> setResource, float maxResource, float regenRate, float delay, System.Action onStartRecovery)
        {
            // Wait for the delay
            yield return new WaitForSeconds(delay);

            onStartRecovery?.Invoke();

            // Gradually recover the resource
            while (getResource() < maxResource)
            {
                setResource(Mathf.Min(getResource() + regenRate * Time.deltaTime, maxResource));
                yield return null;
            }

            recoveryCoroutine = null;
        }

        public bool UseStamina(float amount)
        {
            if (CurrentStamina >= amount)
            {
                // Stop any existing recovery
                if (recoveryCoroutine != null)
                {
                    StopCoroutine(recoveryCoroutine);
                    recoveryCoroutine = null;
                }

                // Deduct stamina
                ModifyResource(
                    () => currentStamina,
                    value => CurrentStamina = value,
                    MaxStamina,
                    -amount,
                    StaminaRegenRate,
                    2f,
                    () => {},
                    null  // Don't start a new coroutine in the callback
                );

                return true;
            }

            return false;
        }

        public bool TakeDamage(float damage)
        {
            if (currentHealth > 0)
            {
                // Deduct health
                ModifyResource(
                    () => currentHealth,
                    value => CurrentHealth = value,
                    MaxHealth,
                    -damage,
                    0f, // No health regeneration rate here (set to 0 if you don’t want auto-regen)
                    0f, // No delay for health regen (set a delay if regen is required)
                    null, // No recovery stop callback for health damage
                    null  // No recovery start callback for health damage
                );

                // Return true to indicate damage was successfully taken
                return true;
            }

            // Return false if the player is already at 0 health
            return false;
        }

        private void StopStaminaRecovery()
        {
            if (recoveryCoroutine != null)
            {
                StopCoroutine(recoveryCoroutine);
                recoveryCoroutine = null;
            }
            Debug.Log("Stamina recovery manually interrupted. Ongoing recovery halted.");
        }

        private void StartStaminaRecovery()
        {
            if (currentStamina < MaxStamina)
            {
                recoveryCoroutine = StartCoroutine(RecoveryCoroutine(() => currentStamina, v => CurrentStamina = v, MaxStamina, StaminaRegenRate, 0f, null));
            }
            Debug.Log("Starting stamina recovery process now.");
        }

        private void StopHealthRecovery()
        {
            if (recoveryCoroutine != null)
            {
                StopCoroutine(recoveryCoroutine);
                recoveryCoroutine = null;
            }
            Debug.Log("Health recovery manually interrupted. Ongoing recovery halted.");
        }

        private void StartHealthRecovery()
        {
            if (currentHealth < MaxHealth)
            {
                recoveryCoroutine = StartCoroutine(RecoveryCoroutine(() => currentHealth, v => CurrentHealth = v, MaxHealth, HealthRegenRate, 0f, null));
            }
            Debug.Log("Starting health recovery process now.");
        }
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
