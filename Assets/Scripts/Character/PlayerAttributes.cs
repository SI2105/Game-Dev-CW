using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG{
    public class PlayerAttriutes : MonoBehaviour
    {
        #region Core Attributes
        public float MaxHealth { get; set; } = 100f;
        public float CurrentHealth { get; set; } = 100f;
        public float HealthRegenRate { get; set; } = 0.5f;
        public float MaxStamina { get; set; } = 100f;
        public float CurrentStamina { get; set; } = 100f;
        public float StaminaRegenRate { get; set; } = 5f;
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


        public float CalculateStaminaBasedOnEnduranceLevel(float endurance){
            float stamina = 0;
            stamina = endurance * 10;

            return stamina;
        }



//     // lock on
//     // currently available skills
//     // 
//     public void TakeDamage(float damage)
//     {
//         CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
//         Debug.Log($"Player took {damage} damage. Current Health: {CurrentHealth}");
        
//         if (CurrentHealth <= 0)
//         {
//             Die();
//         }
//     }

//     public void Heal(float amount)
//     {
//         CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
//         Debug.Log($"Player healed by {amount}. Current Health: {CurrentHealth}");
//     }

//     public void RegenerateHealth(float deltaTime)
//     {
//         CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + (HealthRegenRate * deltaTime));
//         Debug.Log($"Regenerating health. Current Health: {CurrentHealth}");
//     }

//     public bool UseStamina(float amount)
//     {
//         if (CurrentStamina >= amount)
//         {
//             CurrentStamina -= amount;
//             Debug.Log($"Used {amount} stamina. Remaining Stamina: {CurrentStamina}");
//             return true; // Successful usage
//         }
//         else
//         {
//             Debug.Log("Not enough stamina!");
//             return false; // Failed usage
//         }
//     }

//     public void RegenerateStamina(float deltaTime)
//     {
//         CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + (StaminaRegenRate * deltaTime));
//         Debug.Log($"Regenerating stamina. Current Stamina: {CurrentStamina}");
//     }

//     public void ResetAttributes()
//     {
//         CurrentHealth = MaxHealth;
//         CurrentStamina = MaxStamina;
//         Debug.Log("Player attributes reset to maximum values.");
//     }

//     private void Die()
//     {
//         Debug.Log("Player has died.");
//         // Add death logic here (e.g., disable movement, play animation, respawn)
//     }

//     public void AdjustInventorySlots(int adjustment)
//     {
//         MaxInventorySlots = Mathf.Max(1, MaxInventorySlots + adjustment);
//         Debug.Log($"Inventory slots adjusted. Max Slots: {MaxInventorySlots}");
//     }

//     public float CalculateDamage(float incomingDamage)
//     {
//         // Reduce damage based on armor
//         float effectiveDamage = Mathf.Max(0, incomingDamage - Armor);

//         // Determine if a critical hit occurred
//         if (UnityEngine.Random.value < CriticalHitChance)
//         {
//             effectiveDamage *= CriticalHitMultiplier;
//             Debug.Log($"Critical Hit! Damage increased to: {effectiveDamage}");
//         }

//         Debug.Log($"Final Damage Taken: {effectiveDamage}");
//         return effectiveDamage;
//     }

//     public bool AttemptBlock()
//     {
//         bool blockSuccessful = UnityEngine.Random.value < BlockChance;

//         if (blockSuccessful)
//         {
//             Debug.Log("Attack Blocked!");
//         }
//         else
//         {
//             Debug.Log("Block Failed!");
//         }

//         return blockSuccessful;
//     }
//     public float PerformAttack()
//     {
//         // Start with base damage
//         float damage = BaseDamage;

//         // Determine if a critical hit occurred
//         if (UnityEngine.Random.value < CriticalHitChance)
//         {
//             damage *= CriticalHitMultiplier;
//             Debug.Log($"Critical Hit! Damage dealt: {damage}");
//         }
//         else
//         {
//             Debug.Log($"Damage dealt: {damage}");
//         }

//         return damage;
//     }

//     public void AdjustArmor(float adjustment)
//     {
//         Armor = Mathf.Max(0, Armor + adjustment);
//         Debug.Log($"Armor adjusted. New Armor Value: {Armor}");
//     }

//     public void ModifyBlockChance(float adjustment)
//     {
//         BlockChance = Mathf.Clamp(BlockChance + adjustment, 0, 1);
//         Debug.Log($"Block Chance adjusted to: {BlockChance * 100}%");
//     }

//     public void ModifyAttackSpeed(float adjustment)
//     {
//         AttackSpeed = Mathf.Max(0.1f, AttackSpeed + adjustment); // Minimum attack speed is 0.1
//         Debug.Log($"Attack Speed adjusted to: {AttackSpeed}");
//     }

//     public void GainExperience(float xpGained)
//     {
//         CurrentXP += xpGained;
//         Debug.Log($"Gained {xpGained} XP. Current XP: {CurrentXP}");

//         while (CurrentXP >= XPToNextLevel)
//         {
//             LevelUp();
//         }
//     }

//     private void LevelUp()
//     {
//         if (CurrentLevel < MaxLevel)
//         {
//             CurrentLevel++;
//             CurrentXP -= XPToNextLevel;

//             // Scale XP required for the next level (e.g., exponential growth)
//             XPToNextLevel = Mathf.Ceil(XPToNextLevel * 1.25f);

//             // Optionally reward stat points or bonuses
//             RewardLevelUpStats();

//             Debug.Log($"Leveled up! Current Level: {CurrentLevel}. XP to Next Level: {XPToNextLevel}");
//         }
//         else
//         {
//             CurrentXP = XPToNextLevel; // Cap XP at the max level
//             Debug.Log("Reached max level. No further leveling possible.");
//         }
//     }

//     private void RewardLevelUpStats()
//     {
//         Strength += 1f;
//         Agility += 1f;
//         Endurance += 1f;
//         Intelligence += 1f;
//         Luck += 0.5f; // Optional smaller increment for Luck

//         Debug.Log($"Level-up stats rewarded: Strength: {Strength}, Agility: {Agility}, Endurance: {Endurance}, Intelligence: {Intelligence}, Luck: {Luck}");
//     }
//     public void AdjustStrength(float adjustment)
//     {
//         Strength = Mathf.Max(0, Strength + adjustment);
//         Debug.Log($"Strength adjusted to: {Strength}");
//     }

//     public void AdjustAgility(float adjustment)
//     {
//         Agility = Mathf.Max(0, Agility + adjustment);
//         Debug.Log($"Agility adjusted to: {Agility}");
//     }

//     public void AdjustEndurance(float adjustment)
//     {
//         Endurance = Mathf.Max(0, Endurance + adjustment);
//         Debug.Log($"Endurance adjusted to: {Endurance}");
//     }

//     public void AdjustIntelligence(float adjustment)
//     {
//         Intelligence = Mathf.Max(0, Intelligence + adjustment);
//         Debug.Log($"Intelligence adjusted to: {Intelligence}");
//     }

//     public void AdjustLuck(float adjustment)
//     {
//         Luck = Mathf.Max(0, Luck + adjustment);
//         Debug.Log($"Luck adjusted to: {Luck}");
//     }

//     public void ResetProgression()
//     {
//         CurrentLevel = 1;
//         CurrentXP = 0f;
//         XPToNextLevel = 100f;

//         Strength = 10f;
//         Agility = 8f;
//         Endurance = 12f;
//         Intelligence = 5f;
//         Luck = 3f;

//         Debug.Log("Progression attributes reset to default values.");
//     }

}
}