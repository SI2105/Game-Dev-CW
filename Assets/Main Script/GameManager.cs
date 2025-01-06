using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SlimUI.ModernMenu;

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
        [SerializeField] private float transitionDuration = 10.0f;
        private string lastSceneName;

        // Settings Management
        public float SensitivityX { get; set; }
        public float SensitivityY { get; set; }
        public float MouseSmoothing { get; set; }
        public string Difficulty { get; set; }
        private GameObject debugLoaderCanvOptions;

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
            LoadPlayerSettings();
            
            debugLoaderCanvOptions = FindCanvasOptionsInDontDestroyOnLoad();

            if (debugLoaderCanvOptions != null)
            {
                Debug.Log("Canvas_Options found in DontDestroyOnLoad.");
            }
            else
            {
                Debug.LogWarning("Canvas_Options not found in DontDestroyOnLoad.");
            }

        }

        private GameObject FindCanvasOptionsInDontDestroyOnLoad()
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject obj in allGameObjects)
            {
                if (obj.name == "Canvas_Options" && obj.hideFlags == HideFlags.DontUnloadUnusedAsset)
                {
                    return obj;
                }
            }

            return null; // Not found
        }

        private void Start()
        {
            if (UISettingsManager.Instance != null)
            {
                Debug.Log($"Platform: {UISettingsManager.Instance.platform}");
                // UISettingsManager.Instance.FullScreen();
            }
            else
            {
                Debug.LogError("UISettingsManager instance not found!");
            }

            ApplyPlayerSettings();
            ApplyDifficultySettings();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // Save player data
        public void SaveData(SG.PlayerAttributesManager playerAttributesManager, InventoryManager inventoryManager, ObjectiveManager objectiveManager)
        {
            if (playerAttributesManager != null)
            {
                savedAttributesData = playerAttributesManager.GetAttributesData();
                Debug.Log($"GameManager: Player attributes saved: {savedAttributesData.MaxHealth}, {savedAttributesData.CurrentHealth}, {savedAttributesData.Strength}");
            }
            else
            {
                Debug.LogWarning("GameManager: PlayerAttributesManager is null. Cannot save attributes.");
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
            StartCoroutine(DelayedRestore(scene.name));
        }


        private IEnumerator DelayedRestore(string sceneName)
        {
            yield return null;

            RestoreSavedData();

            if (sceneName == lastSceneName)
            {
                var playerAttributesManager = FindObjectOfType<SG.PlayerAttributesManager>();
                if (playerAttributesManager != null)
                {
                    playerAttributesManager.ResetHealth();
                    Debug.Log("Player health refreshed for the same scene reload.");
                }
            }

            ApplyDifficultySettings();
            Time.timeScale = 1.0f;
        }


        private void RestoreSavedData()
        {
            var playerAttributesManager = FindObjectOfType<PlayerAttributesManager>();
            var inventoryManager = FindObjectOfType<InventoryManager>();
            var objectiveManager = FindObjectOfType<ObjectiveManager>();
            var uiHudManager = FindObjectOfType<PlayerUIHudManager>();

            if (playerAttributesManager != null && savedAttributesData != null)
            {
                playerAttributesManager.SetAttributesData(savedAttributesData);
                playerAttributesManager.ResetHealth();
                Debug.Log($"GameManager: Restored Player attributes: {savedAttributesData.MaxHealth}, {savedAttributesData.CurrentHealth}, {savedAttributesData.Strength}");
            }
            else
            {
                Debug.LogWarning("GameManager: Player attributes could not be restored. Either manager or saved data is null.");
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

        // Load settings from PlayerPrefs
        private void LoadPlayerSettings()
        {
            SensitivityX = PlayerPrefs.GetFloat("XSensitivity", 1.0f);
            SensitivityY = PlayerPrefs.GetFloat("YSensitivity", 1.0f);
            MouseSmoothing = PlayerPrefs.GetFloat("MouseSmoothing", 0.5f);

            if (PlayerPrefs.GetInt("NormalDifficulty", 1) == 1)
                Difficulty = "Normal";
            else if (PlayerPrefs.GetInt("HardCoreDifficulty", 0) == 1)
                Difficulty = "Hardcore";

            Debug.Log($"Loaded Player Settings - SensitivityX: {SensitivityX}, SensitivityY: {SensitivityY}, MouseSmoothing: {MouseSmoothing}, Difficulty: {Difficulty}");
        }

        // Apply settings to the player
        private void ApplyPlayerSettings()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.UpdateAndApplySensitivity(SensitivityX, SensitivityY);
                SettingsManager.Instance.UpdateAndApplyMouseSmoothing(MouseSmoothing);
            }
            else
            {
                Debug.LogError("SettingsManager instance not found! Could not apply player settings.");
            }
        }

        // Apply difficulty settings
        private void ApplyDifficultySettings()
        {
            var playerAttributesManager = FindObjectOfType<PlayerAttributesManager>();
            if (playerAttributesManager != null)
            {
                playerAttributesManager.SetDifficultyMultiplier(Difficulty);
                Debug.Log($"Applied difficulty settings: {Difficulty}");
            }
            else
            {
                Debug.LogError("PlayerAttributesManager instance not found! Could not apply difficulty settings.");
            }
        }

        // Update settings dynamically
        public void UpdateSettings(float newSensitivityX, float newSensitivityY, float newMouseSmoothing, string newDifficulty)
        {
            SensitivityX = newSensitivityX;
            SensitivityY = newSensitivityY;
            MouseSmoothing = newMouseSmoothing;
            Difficulty = newDifficulty;

            PlayerPrefs.SetFloat("XSensitivity", SensitivityX);
            PlayerPrefs.SetFloat("YSensitivity", SensitivityY);
            PlayerPrefs.SetFloat("MouseSmoothing", MouseSmoothing);

            if (newDifficulty == "Normal")
            {
                PlayerPrefs.SetInt("NormalDifficulty", 1);
                PlayerPrefs.SetInt("HardCoreDifficulty", 0);
            }
            else if (newDifficulty == "Hardcore")
            {
                PlayerPrefs.SetInt("NormalDifficulty", 0);
                PlayerPrefs.SetInt("HardCoreDifficulty", 1);
            }

            ApplyPlayerSettings();
            ApplyDifficultySettings();
            Debug.Log("Updated and applied new settings.");
        }
    }
}