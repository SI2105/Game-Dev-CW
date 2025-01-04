using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SG
{
    public class EnemyManager : MonoBehaviour
    {
        private List<GameObject> enemies; // List to track all enemies in the scene

        [SerializeField] private PlayerAttributesManager playerAttributesManager;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private ObjectiveManager objectiveManager;

        void Start()
        {
            // Automatically find PlayerAttributesManager and InventoryManager if not assigned
            if (playerAttributesManager == null)
            {
                playerAttributesManager = FindObjectOfType<PlayerAttributesManager>();
            }

            if (inventoryManager == null)
            {
                inventoryManager = FindObjectOfType<InventoryManager>();
            }

            if (objectiveManager == null)
            {
                objectiveManager = FindObjectOfType<ObjectiveManager>();
            }

            // Find all enemies with the "Enemy" tag
            GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            print("Searching for enemies with the 'Enemy' tag...");
            print(foundEnemies.Length + " enemies found in the scene.");

            foreach (GameObject enemy in foundEnemies)
            {
                print("Found enemy: " + enemy.name);
            }

            enemies = new List<GameObject>(foundEnemies);
        }

        void Update()
        {
            // Check if all enemies are destroyed
            CheckForEnemies();
        }

        void CheckForEnemies()
        {
            // Remove null references (destroyed enemies) from the list
            enemies.RemoveAll(enemy => enemy == null);

            if (enemies.Count == 0)
            {
                Debug.Log("All enemies are destroyed! Transitioning to the next level...");
                TransitionToNextLevel();
            }
        }

        void TransitionToNextLevel()
        {
            // Save data and transition to the next level
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveData(playerAttributesManager, inventoryManager, objectiveManager);
            }
            else
            {
                Debug.LogWarning("GameManager instance is null. Cannot save data.");
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}