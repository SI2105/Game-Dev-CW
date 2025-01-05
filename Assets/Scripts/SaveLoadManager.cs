using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using SG;

namespace SG
{
    [System.Serializable]
    public class PlayerData
    {
        // Basic attributes to save
        public float CurrentHealth;
        public float MaxHealth;
        public float CurrentStamina;
        public float MaxStamina;
        public float TorchFuel;
        public float MaxTorchFuel;

        // Player Transform (Position and Rotation)
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float RotationX;
        public float RotationY;
        public float RotationZ;

        // Scene
        public string CurrentScene;

        // Combat Attributes
        public float BaseDamage;
        public float CriticalHitChance;
        public float CriticalHitMultiplier;
        public float AttackSpeed;
        public float Armor;
        public float BlockChance;
        public float DodgeChance;

        // Progression Attributes
        public int CurrentLevel;
        public float CurrentXP;
        public float XPToNextLevel;
        public float Strength;
        public float Agility;
        public float Endurance;
        public float Intelligence;
        // public float Luck;

        // Exploration and Dungeon Attributes
        public float VisionRange;
        public float Hunger;
        public float MaxHunger;
        public float TrapDetectionLevel;
        public float PoisonResistance;
        public float BleedResistance;
    }

    public class SaveLoadManager : MonoBehaviour
    {
        private string savePath;

        private void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, "playerSave.json");
        }

        public void SaveGame(PlayerAttributesManager player)
        {
            PlayerData data = new PlayerData
            {
                // Copy attributes from PlayerAttributesManager
                CurrentHealth = player.CurrentHealth,
                MaxHealth = player.MaxHealth,
                CurrentStamina = player.CurrentStamina,
                MaxStamina = player.MaxStamina,
                TorchFuel = player.TorchFuel,
                MaxTorchFuel = player.MaxTorchFuel,

                // Save Transform (Position and Rotation)
                PositionX = player.transform.position.x,
                PositionY = player.transform.position.y,
                PositionZ = player.transform.position.z,
                RotationX = player.transform.eulerAngles.x,
                RotationY = player.transform.eulerAngles.y,
                RotationZ = player.transform.eulerAngles.z,

                // Save current scene
                CurrentScene = SceneManager.GetActiveScene().name,

                // Other attributes...
                BaseDamage = player.BaseDamage,
                CriticalHitChance = player.CriticalHitChance,
                CriticalHitMultiplier = player.CriticalHitMultiplier,
                AttackSpeed = player.AttackSpeed,
                Armor = player.Armor,
                BlockChance = player.BlockChance,
                DodgeChance = player.DodgeChance,
                CurrentLevel = player.CurrentLevel,
                CurrentXP = player.CurrentXP,
                XPToNextLevel = player.XPToNextLevel,
                Strength = player.Strength,
                Agility = player.Agility,
                Endurance = player.Endurance,
                Intelligence = player.Intelligence,
                // Luck = player.Luck,
                VisionRange = player.VisionRange,
                Hunger = player.Hunger,
                MaxHunger = player.MaxHunger,
                TrapDetectionLevel = player.TrapDetectionLevel,
                PoisonResistance = player.PoisonResistance,
                BleedResistance = player.BleedResistance
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"Game saved to {savePath}");
        }

        public void LoadGame(PlayerAttributesManager player)
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                PlayerData data = JsonUtility.FromJson<PlayerData>(json);

                // Load the saved scene
                if (SceneManager.GetActiveScene().name != data.CurrentScene)
                {
                    StartCoroutine(LoadSceneAndRestore(data, player));
                }
                else
                {
                    RestorePlayerData(data, player);
                }
            }
            else
            {
                Debug.LogWarning("No save file found.");
            }
        }

        private IEnumerator LoadSceneAndRestore(PlayerData data, PlayerAttributesManager player)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(data.CurrentScene);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Once the scene is loaded, restore player data
            PlayerAttributesManager newPlayer = FindObjectOfType<PlayerAttributesManager>();
            RestorePlayerData(data, newPlayer);
        }

        private void RestorePlayerData(PlayerData data, PlayerAttributesManager player)
        {
            // Restore attributes to PlayerAttributesManager
            player.CurrentHealth = data.CurrentHealth;
            player.MaxHealth = data.MaxHealth;
            player.CurrentStamina = data.CurrentStamina;
            player.MaxStamina = data.MaxStamina;
            player.TorchFuel = data.TorchFuel;
            player.MaxTorchFuel = data.MaxTorchFuel;

            // Restore Transform (Position and Rotation)
            player.transform.position = new Vector3(data.PositionX, data.PositionY, data.PositionZ);
            player.transform.eulerAngles = new Vector3(data.RotationX, data.RotationY, data.RotationZ);

            // Restore other attributes...
            player.BaseDamage = data.BaseDamage;
            player.CriticalHitChance = data.CriticalHitChance;
            player.CriticalHitMultiplier = data.CriticalHitMultiplier;
            player.AttackSpeed = data.AttackSpeed;
            player.Armor = data.Armor;
            player.BlockChance = data.BlockChance;
            player.DodgeChance = data.DodgeChance;
            player.CurrentLevel = data.CurrentLevel;
            player.CurrentXP = data.CurrentXP;
            player.XPToNextLevel = data.XPToNextLevel;
            player.Strength = data.Strength;
            player.Agility = data.Agility;
            player.Endurance = data.Endurance;
            player.Intelligence = data.Intelligence;
            // player.Luck = data.Luck;
            player.VisionRange = data.VisionRange;
            player.Hunger = data.Hunger;
            player.MaxHunger = data.MaxHunger;
            player.TrapDetectionLevel = data.TrapDetectionLevel;
            player.PoisonResistance = data.PoisonResistance;
            player.BleedResistance = data.BleedResistance;

            Debug.Log("Game loaded successfully.");
        }
    }
}