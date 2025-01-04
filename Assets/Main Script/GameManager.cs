using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SG{

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        private List<ObjectiveData> savedObjectiveData;

        // Player Attributes Data
        private SG.PlayerAttributesManager.AttributesData savedAttributesData;

        // Inventory Data
        private List<InventoryManager.InventoryItemData> savedInventoryData;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void SaveData(SG.PlayerAttributesManager playerAttributesManager, InventoryManager inventoryManager, ObjectiveManager objectiveManager)
        {
            if (playerAttributesManager != null)
            {
                savedAttributesData = playerAttributesManager.GetAttributesData();
                Debug.Log("GameManager: Player attributes saved.");
            }

            if (inventoryManager != null)
            {
                savedInventoryData = inventoryManager.GetInventoryData();
                Debug.Log("GameManager: Inventory saved.");
            }

            if (objectiveManager != null)
            {
                savedObjectiveData = objectiveManager.GetObjectiveData();
                Debug.Log("GameManager: Objectives saved.");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var playerAttributesManager = FindObjectOfType<SG.PlayerAttributesManager>();
            var inventoryManager = FindObjectOfType<InventoryManager>();
            var objectiveManager = FindObjectOfType<ObjectiveManager>();
            var uiHudManager = FindObjectOfType<SG.PlayerUIHudManager>();

            if (playerAttributesManager != null && savedAttributesData != null)
            {
                playerAttributesManager.SetAttributesData(savedAttributesData);
                Debug.Log("GameManager: Player attributes restored.");
            }

            if (inventoryManager != null && savedInventoryData != null)
            {
                inventoryManager.SetInventoryData(savedInventoryData);
                Debug.Log("GameManager: Inventory restored.");
            }

            if (objectiveManager != null && savedObjectiveData != null)
            {
                objectiveManager.SetObjectiveData(savedObjectiveData);
                Debug.Log("GameManager: Objectives restored.");
            }

            if (uiHudManager != null)
            {
                uiHudManager.RefreshUI();
                Debug.Log("GameManager: UI refreshed.");
            }
        }

        public List<InventoryManager.InventoryItemData> GetSavedInventoryData()
        {
            return savedInventoryData;
        }
    }
}
