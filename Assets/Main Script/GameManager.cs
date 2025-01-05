using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SG
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        // Saved Data
        private List<ObjectiveData> savedObjectiveData;
        private SG.PlayerAttributesManager.AttributesData savedAttributesData;
        private List<InventoryManager.InventoryItemData> savedInventoryData;
        private bool isPlayerDead = false;

        [SerializeField] private Animator transitionAnimation;
        [SerializeField] private float transitionDuration = 1.0f;
        private string lastSceneName;

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
            }

            if (inventoryManager != null)
            {
                savedInventoryData = inventoryManager.GetInventoryData();
            }

            if (objectiveManager != null)
            {
                savedObjectiveData = objectiveManager.GetObjectiveData();
            }

        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == lastSceneName)
            {
                var playerAttributesManager = FindObjectOfType<PlayerAttributesManager>();
                var inventoryManager = FindObjectOfType<InventoryManager>();
                var objectiveManager = FindObjectOfType<ObjectiveManager>();

                if (playerAttributesManager != null && savedAttributesData != null)
                {
                    playerAttributesManager.SetAttributesData(savedAttributesData);
                    playerAttributesManager.ResetHealth();
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
            }
            Time.timeScale = 1.0f;
        }

        public void LoadLevel(string sceneName)
        {
            StartCoroutine(LoadLevelCoroutine(sceneName));
        }

        private IEnumerator LoadLevelCoroutine(string sceneName)
        {
            if (transitionAnimation != null)
            {
                Debug.Log("Triggering End transition animation");
                transitionAnimation.SetTrigger("End");
            }

            yield return new WaitForSeconds(transitionDuration);
            SceneManager.LoadScene(sceneName);
            yield return null;

            if (transitionAnimation != null)
            {
                transitionAnimation.SetTrigger("Start");
            }
        }
        public List<InventoryManager.InventoryItemData> GetSavedInventoryData()
        {
            return savedInventoryData;
        }

        public void HandlePlayerDeath(PlayerAttributesManager playerAttributesManager, InventoryManager inventoryManager, ObjectiveManager objectiveManager)
        {
            // Save data
            SaveData(playerAttributesManager, inventoryManager, objectiveManager);

            // Record the current scene
            lastSceneName = SceneManager.GetActiveScene().name;

            // Reload the scene
            LoadLevel(lastSceneName);
        }

    }
}