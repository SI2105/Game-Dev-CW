using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace SG
{


    public class PlayerAttributesManager : MonoBehaviour
    {
        #region Inventory
        //public GameObject Inventory;
        [SerializeField] public InventoryManager inventoryManager;
        public InventoryManager InventoryManager {
            get => inventoryManager;
            set => inventoryManager = value;
        }
        private enum VignetteEffectState
        {
            None,
            Damage,
            Recovery
        }

        private VignetteEffectState currentVignetteEffectState = VignetteEffectState.None;

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

        public float intensity = 0f;
        public PostProcessVolume _volume;
        Vignette _vignette;

        [System.Serializable]
        public class AttributesData
        {
            public float MaxHealth;
            public float CurrentHealth;
            public float MaxStamina;
            public float CurrentStamina;
            public int CurrentLevel;
            public float CurrentXP;
            public float Strength;
            public float Agility;
            public float Endurance;
            public float Intelligence;
        }

        public AttributesData GetAttributesData()
        {
            return new AttributesData
            {
                MaxHealth = MaxHealth,
                CurrentHealth = CurrentHealth,
                MaxStamina = MaxStamina,
                CurrentStamina = CurrentStamina,
                CurrentLevel = CurrentLevel,
                CurrentXP = CurrentXP,
                Strength = Strength,
                Agility = Agility,
                Endurance = Endurance,
                Intelligence = Intelligence
            };
        }

        public void SetAttributesData(AttributesData data)
        {
            MaxHealth = data.MaxHealth;
            CurrentHealth = data.CurrentHealth;
            MaxStamina = data.MaxStamina;
            CurrentStamina = data.CurrentStamina;
            CurrentLevel = data.CurrentLevel;
            CurrentXP = data.CurrentXP;
            Strength = data.Strength;
            Agility = data.Agility;
            Endurance = data.Endurance;
            Intelligence = data.Intelligence;

            RecalculateAttributes();
        }
        //void OnEnable()
        //{
        //    Skill.OnSkillUnlocked += HandleSkillUnlocked;
        //}

        //void OnDisable()
        //{
        //    Skill.OnSkillUnlocked -= HandleSkillUnlocked;
        //}

        //private void HandleSkillUnlocked(Skill.SkillName skillName)
        //{
        //    Debug.Log("HandleSkil");
        //    if (skillName == Skill.SkillName.Carry2)
        //    {
        //        InventoryManager.maxNumOfPotion = 2;
               
        //    }
        //    else if (skillName == Skill.SkillName.Carry3)
        //    {
        //        InventoryManager.maxNumOfPotion = 3;
        //    }
        //    else if (skillName == Skill.SkillName.Carry5)
        //    {
        //        InventoryManager.maxNumOfPotion = 5;
        //    }
        //    Debug.Log("MaxPotions: " + InventoryManager.maxNumOfPotion);
        //}
        #endregion

        public float MaxHealth { get; set; } = 100f;
        public float HealthRegenRate { get; set; } = 10f;
        public float MaxStamina { get; set; } = 100f;
        public float StaminaCostPerSecond { get; set; } = 6f;
        public float StaminaRegenRate { get; set; } = 20f;
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

        public float currentHealth;
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

        private Animator animator;
        private void Awake()
        {
            currentStamina = MaxStamina;
            currentHealth = MaxHealth;
            animator = GetComponent<Animator>();
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

        private IEnumerator RecoveryCoroutine(Func<float> getResource, Action<float> setResource, float targetResource, float regenRate, float delay, Action onStartRecovery)
        {
            // Wait for the delay
            yield return new WaitForSeconds(delay);

            onStartRecovery?.Invoke();

            // Gradually recover the resource up to the target
            while (getResource() < targetResource)
            {
                float newValue = Mathf.Min(getResource() + regenRate * Time.deltaTime, targetResource);
                setResource(newValue);

                yield return null;
            }

            // Stop the recovery coroutine
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
                    0.5f,
                    () => {},
                    null  // Don't start a new coroutine in the callback
                );

                return true;
            }

            return false;
        }
        private Coroutine lowHealthPulseCoroutine;
        private PlayerSoundManager soundEffectManager;
        void Start(){
            soundEffectManager = GetComponent<PlayerSoundManager>();
            _volume.profile.TryGetSettings(out _vignette);
            if (_vignette != null){
                _vignette.enabled.Override(false);
                _vignette.intensity.Override(0f);
            }
        }
        private Coroutine damageEffectCoroutine;
        private Coroutine recoveryEffectCoroutine;
        private float lastInjuredSoundTime = 0f;
        private float injuredSoundCooldown = 7.5f; // Adjust this value to control how often the sound can play

        private IEnumerator DamageEffectCoroutine()
        {
            if (_vignette == null) yield break;

            // Set state to Damage
            currentVignetteEffectState = VignetteEffectState.Damage;

            float lowHealthThreshold = MaxHealth * 0.3f; // 30% health threshold
            _vignette.enabled.Override(true);
            _vignette.color.Override(Color.red); // Red tint for damage

            if (currentHealth <= lowHealthThreshold)
            {
                // Low health pulse (intense, repeats)
                float pulseSpeed = 2f;
                float minIntensity = 0.2f;
                float maxIntensity = 0.3f;

                // Play the injured sound if cooldown is met
                if (lastInjuredSoundTime == 0f || Time.time - lastInjuredSoundTime >= injuredSoundCooldown)
                {
                    Debug.Log("Playing injured sound for low health.");
                    soundEffectManager?.PlayInjuredClip();
                    lastInjuredSoundTime = Time.time;
                }
                while (currentHealth <= lowHealthThreshold && currentVignetteEffectState == VignetteEffectState.Damage)
                {
                    // Pulse effect
                    float elapsed = 0f;
                    while (elapsed < 1f / pulseSpeed)
                    {
                        elapsed += Time.deltaTime;
                        _vignette.intensity.Override(Mathf.Lerp(minIntensity, maxIntensity, elapsed * pulseSpeed));
                        yield return null;
                    }

                    elapsed = 0f;
                    while (elapsed < 1f / pulseSpeed)
                    {
                        elapsed += Time.deltaTime;
                        _vignette.intensity.Override(Mathf.Lerp(maxIntensity, minIntensity, elapsed * pulseSpeed));
                        yield return null;
                    }
                }

                // Stop the injured sound if the player exits low health
                soundEffectManager?.StopInjuredClip();
            }
            else
            {
                // Normal damage pulse (less intense, occurs once)
                float pulseSpeed = 2f;
                float minIntensity = 0.1f;
                float maxIntensity = 0.2f;

                // Single pulse effect
                float elapsed = 0f;
                while (elapsed < 1f / pulseSpeed)
                {
                    elapsed += Time.deltaTime;
                    _vignette.intensity.Override(Mathf.Lerp(minIntensity, maxIntensity, elapsed * pulseSpeed));
                    yield return null;
                }

                elapsed = 0f;
                while (elapsed < 1f / pulseSpeed)
                {
                    elapsed += Time.deltaTime;
                    _vignette.intensity.Override(Mathf.Lerp(maxIntensity, minIntensity, elapsed * pulseSpeed));
                    yield return null;
                }
            }

            // Clean up after the effect
            _vignette.intensity.Override(0f);
            _vignette.enabled.Override(false);
            currentVignetteEffectState = VignetteEffectState.None;
        }



        private IEnumerator RecoveryEffectCoroutine()
        {
            if (_vignette == null) yield break;

            // Set state to Recovery
            currentVignetteEffectState = VignetteEffectState.Recovery;

            _vignette.enabled.Override(true);
            _vignette.color.Override(Color.green); // Green tint for recovery

            float duration = 0.5f;
            float holdTime = 0.2f;
            float fadeOutDuration = 0.5f;
            float targetIntensity = 0.3f;

            // Fade In
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (currentVignetteEffectState != VignetteEffectState.Recovery) yield break;

                elapsed += Time.deltaTime;
                _vignette.intensity.Override(Mathf.Lerp(0f, targetIntensity, elapsed / duration));
                yield return null;
            }

            _vignette.intensity.Override(targetIntensity);

            // Hold
            yield return new WaitForSeconds(holdTime);

            // Fade Out
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                if (currentVignetteEffectState != VignetteEffectState.Recovery) yield break;

                elapsed += Time.deltaTime;
                _vignette.intensity.Override(Mathf.Lerp(targetIntensity, 0f, elapsed / fadeOutDuration));
                yield return null;
            }

            // Clean up if the state is still Recovery
            if (currentVignetteEffectState == VignetteEffectState.Recovery)
            {
                _vignette.intensity.Override(0f);
                _vignette.enabled.Override(false);
                currentVignetteEffectState = VignetteEffectState.None;
            }
        }

        public bool TakeDamage(float damage)
        {
            if (currentHealth - damage > 0)
            {
                // Deduct health
                ModifyResource(
                    () => currentHealth,
                    value => CurrentHealth = value,
                    MaxHealth,
                    -damage,
                    0f, // No health regeneration rate here
                    0f, // No delay for health regen
                    null, // No recovery stop callback for health damage
                    null  // No recovery start callback for health damage
                );

                // Trigger Damage Vignette Effect
                ApplyDamageEffect();
                // Return true to indicate damage was successfully taken
                return true;
            } else {
                ModifyResource(
                    () => currentHealth,
                    value => CurrentHealth = value,
                    MaxHealth,
                    -damage,
                    0f, // No health regeneration rate here
                    0f, // No delay for health regen
                    null, // No recovery stop callback for health damage
                    null  // No recovery start callback for health damage
                );
                // Trigger Damage Vignette Effect
                ApplyDamageEffect();
                animator.SetBool("isDead", true);
                
            }

            // Return false if the player is already at 0 health
            return false;
        }

        /// <summary>
        /// Initiates the Damage Vignette Effect.
        /// </summary>
        private void ApplyDamageEffect()
        {
            if (_vignette == null) return;

            // Stop conflicting effects
            if (currentVignetteEffectState != VignetteEffectState.None && damageEffectCoroutine != null)
            {
                StopCoroutine(damageEffectCoroutine);
            }
            if (currentVignetteEffectState == VignetteEffectState.Recovery && recoveryEffectCoroutine != null)
            {
                StopCoroutine(recoveryEffectCoroutine);
            }

            // Start damage effect
            damageEffectCoroutine = StartCoroutine(DamageEffectCoroutine());
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

        public void OnEnemyDefeated(bool isBoss)
        {
            // Determine the XP reward based on the enemy type
            float xpAwarded = isBoss ? CalculateBossEnemyXP() : CalculateSmallEnemyXP();

            // Gain the calculated experience points
            GainExperience(xpAwarded);

            // Log the XP awarded for debugging
            Debug.Log(isBoss
                ? $"Boss defeated! Awarded {xpAwarded} XP."
                : $"Small enemy defeated! Awarded {xpAwarded} XP.");
        }
        public float CalculateSmallEnemyXP()
        {
            // Small enemies grant 8%–12% of the XP required for the next level
            return Mathf.Floor(XPToNextLevel * UnityEngine.Random.Range(0.08f, 0.12f));
        }

        public float CalculateBossEnemyXP()
        {
            // Boss enemies grant 50%–70% of the XP required for the next level
            return Mathf.Floor(XPToNextLevel * UnityEngine.Random.Range(0.5f, 0.7f));
        }

        private void StartStaminaRecovery()
        {
            if (currentStamina < MaxStamina)
            {
                recoveryCoroutine = StartCoroutine(RecoveryCoroutine(() => currentStamina, v => CurrentStamina = v, MaxStamina, StaminaRegenRate, 0f, null));
            }
            Debug.Log("Starting stamina recovery process now.");
        }

        public void StartHealthRecovery(float targetHealth)
        {
            // Ensure target health does not exceed MaxHealth
            targetHealth = Mathf.Clamp(targetHealth, currentHealth, MaxHealth);

            if (currentHealth < targetHealth)
            {
                // Stop any ongoing recovery
                if (recoveryCoroutine != null)
                {
                    StopCoroutine(recoveryCoroutine);
                }

                // Start new recovery coroutine
                recoveryCoroutine = StartCoroutine(RecoveryCoroutine(
                    () => currentHealth,
                    value => CurrentHealth = value,
                    targetHealth, // Use targetHealth as the limit
                    HealthRegenRate,
                    0f, // No delay before starting recovery
                    null
                ));

                // Trigger Recovery Vignette Effect
                ApplyRecoveryEffect();

                Debug.Log($"Starting health recovery up to {targetHealth}.");
            }
            else
            {
                Debug.Log("Health recovery not started because current health exceeds target health.");
            }
        }

        /// <summary>
        /// Initiates the Recovery Vignette Effect.
        /// </summary>
        private void ApplyRecoveryEffect()
        {
            if (_vignette == null) return;

            // Stop conflicting effects
            if (currentVignetteEffectState != VignetteEffectState.None && recoveryEffectCoroutine != null)
            {
                StopCoroutine(recoveryEffectCoroutine);
            }
            if (currentVignetteEffectState == VignetteEffectState.Damage && damageEffectCoroutine != null)
            {
                StopCoroutine(damageEffectCoroutine);
            }

            // Start recovery effect
            recoveryEffectCoroutine = StartCoroutine(RecoveryEffectCoroutine());
        }

        public void GainHealth(float amount)
        {
            // Immediately gain health
            ModifyResource(
                () => currentHealth,
                value => CurrentHealth = value,
                MaxHealth,
                amount,
                HealthRegenRate,
                0f
            );

            // Calculate the target health
            float targetHealth = Mathf.Clamp(currentHealth + amount, currentHealth, MaxHealth);

            // Start recovery if not already at max health
            if (currentHealth < targetHealth)
            {
                StartHealthRecovery(targetHealth);
            }
        }


        /// <summary>
        /// Generic coroutine to handle Vignette intensity transition.
        /// </summary>
        private IEnumerator VignetteEffectCoroutine(Color effectColor, float targetIntensity, float duration)
        {
            if (_vignette == null)
                yield break;

            // Set Vignette color
            _vignette.color.Override(effectColor);

            // Enable Vignette
            _vignette.enabled.Override(true);

            // Set initial intensity
            _vignette.intensity.Override(targetIntensity);

            // Gradually decrease intensity to 0
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _vignette.intensity.Override(Mathf.Lerp(targetIntensity, 0f, elapsed / duration));
                yield return null;
            }

            // Ensure intensity is set to 0 and disable Vignette
            _vignette.intensity.Override(0f);
            _vignette.enabled.Override(false);
        }

        public void GainExperience(float xp)
        {
            if (CurrentLevel >= MaxLevel)
            {
                Debug.Log("Player has reached the maximum level.");
                return;
            }

            CurrentXP += xp;
            Debug.Log($"Gained {xp} XP. Current XP: {CurrentXP}/{XPToNextLevel}");

            while (CurrentXP >= XPToNextLevel && CurrentLevel < MaxLevel)
            {
                LevelUp();
            }
        }
        public event Action<int> OnLevelChanged;

        /// <summary>
        /// Adjusts player attributes based on the difficulty multiplier.
        /// </summary>
        /// <param name="difficulty">The difficulty level ("Normal" or "Hardcore").</param>
        public void SetDifficultyMultiplier(string difficulty)
        {
            float healthMultiplier = 1f;
            float staminaMultiplier = 1f;
            float damageMultiplier = 1f;
            float regenMultiplier = 1f;

            switch (difficulty)
            {
                case "Normal":
                    healthMultiplier = 1.0f;
                    staminaMultiplier = 1.0f;
                    damageMultiplier = 1.0f;
                    regenMultiplier = 1.0f;
                    break;

                case "Hardcore":
                    healthMultiplier = 0.8f;   // Reduce health by 20%
                    staminaMultiplier = 0.8f; // Reduce stamina by 20%
                    damageMultiplier = 0.9f;  // Reduce damage by 10%
                    regenMultiplier = 0.7f;   // Reduce regen rates by 30%
                    break;

                default:
                    Debug.LogWarning($"Unknown difficulty: {difficulty}. Defaulting to Normal.");
                    break;
            }

            // Apply multipliers to attributes
            MaxHealth *= healthMultiplier;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth); // Adjust current health to fit new max
            MaxStamina *= staminaMultiplier;
            CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina); // Adjust current stamina to fit new max
            BaseDamage *= damageMultiplier;
            StaminaRegenRate *= regenMultiplier;
            HealthRegenRate *= regenMultiplier;

            // Log changes
            Debug.Log($"Difficulty set to {difficulty}. Attributes adjusted accordingly:\n" +
                    $"- MaxHealth: {MaxHealth}\n" +
                    $"- MaxStamina: {MaxStamina}\n" +
                    $"- BaseDamage: {BaseDamage}\n" +
                    $"- StaminaRegenRate: {StaminaRegenRate}\n" +
                    $"- HealthRegenRate: {HealthRegenRate}");
        }

        

        public void LevelUp()
        {
            CurrentXP -= XPToNextLevel;
            CurrentLevel++;
            XPToNextLevel *= 1.2f; // Increase XP required for the next level (adjust multiplier as needed)

            // Boost attributes on level-up
            Strength += 2f;
            Agility += 1.5f;
            Endurance += 2f;
            Intelligence += 1f;

            // Regenerate health and stamina on level-up
            CurrentHealth = MaxHealth;
            CurrentStamina = MaxStamina;

            // Recalculate derived attributes
            RecalculateAttributes();
            OnLevelChanged?.Invoke(CurrentLevel);

            SkillTreeManager.instance.AddSkillPoints(1);

            Debug.Log($"Leveled up! Current Level: {CurrentLevel}. XP for next level: {XPToNextLevel}");
        }


        private void RecalculateAttributes()
        {
            float previousMaxStamina = MaxStamina;

            // Recalculate max attributes
            MaxHealth = 100f + Strength * 5f;
            MaxStamina = 100f + Endurance * 10f; // Stamina scales with Endurance
            BaseDamage = 15f + Strength * 2f;

            // Adjust current stamina proportionally
            if (previousMaxStamina > 0)
            {
                CurrentStamina = (CurrentStamina / previousMaxStamina) * MaxStamina;
            }
            else
            {
                CurrentStamina = MaxStamina;
            }

            Debug.Log("Attributes recalculated based on level and stats.");
        }
       #region Events for Attribute Boosts
        // For the "all attributes" 5-point boost over 30s
        public event Action<float> OnAllAttributesBoostCountdownChanged; // Time left
        public event Action OnAllAttributesBoostEnded;                   // Boost ended

        // For the "strength only" 10-point boost over 15s
        public event Action<float> OnStrengthBoostCountdownChanged; 
        public event Action OnStrengthBoostEnded;
        #endregion

        /// <summary>
        /// Temporarily boosts all attributes by 5 points for 30 seconds.
        /// </summary>
        public void BoostAllAttributesTemporarily()
        {
            // Store the original attribute values
            float originalStrength = Strength;
            float originalAgility = Agility;
            float originalEndurance = Endurance;
            float originalIntelligence = Intelligence;

            // Apply the temporary boost
            Strength += 5f;
            Agility += 5f;
            Endurance += 5f;
            Intelligence += 5f;

            // Recalculate attributes and start the timer
            RecalculateAttributes();
            Debug.Log("All attributes boosted by 5 points for 30 seconds.");

            StartCoroutine(BoostAllAttributesCoroutine(
                30f,
                originalStrength,
                originalAgility,
                originalEndurance,
                originalIntelligence
            ));
        }

        public void ResetHealth(){
            CurrentHealth = MaxHealth;
        }

        private IEnumerator BoostAllAttributesCoroutine(
            float duration,
            float originalStrength,
            float originalAgility,
            float originalEndurance,
            float originalIntelligence
        )
        {
            float timeLeft = duration;
            while (timeLeft > 0f)
            {
                // Notify subscribers how many seconds remain
                OnAllAttributesBoostCountdownChanged?.Invoke(timeLeft);

                yield return null; 
                timeLeft -= Time.deltaTime;
            }

            // Boost has ended
            OnAllAttributesBoostEnded?.Invoke();

            // Reset attributes to their original values
            Strength = originalStrength;
            Agility = originalAgility;
            Endurance = originalEndurance;
            Intelligence = originalIntelligence;

            RecalculateAttributes();
            Debug.Log("All attributes reverted to their original values.");
        }

        /// <summary>
        /// Temporarily grants 10 additional points of strength for 15 seconds.
        /// </summary>
        public void GrantTemporaryStrengthBoost()
        {
            // Store the original strength value
            float originalStrength = Strength;

            // Apply the temporary boost
            Strength += 10f;

            // Recalculate attributes and start the timer
            RecalculateAttributes();
            Debug.Log("Strength boosted by 10 points for 15 seconds.");

            StartCoroutine(BoostStrengthCoroutine(15f, originalStrength));
        }

        private IEnumerator BoostStrengthCoroutine(float duration, float originalStrength)
        {
            float timeLeft = duration;
            while (timeLeft > 0f)
            {
                // Notify subscribers how many seconds remain
                OnStrengthBoostCountdownChanged?.Invoke(timeLeft);

                yield return null;
                timeLeft -= Time.deltaTime;
            }

            // Strength boost ended
            OnStrengthBoostEnded?.Invoke();

            // Reset strength to its original value
            Strength = originalStrength;
            RecalculateAttributes();
            Debug.Log("Strength reverted to its original value.");
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