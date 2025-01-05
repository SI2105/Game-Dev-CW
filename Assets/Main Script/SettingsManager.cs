using UnityEngine;

namespace SG
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance;

        private TwoDimensionalAnimationController playerController;

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
        }

        private void Start()
        {
            // Find the player controller to apply settings to
            FindPlayerController();
        }

        private void FindPlayerController()
        {
            playerController = FindObjectOfType<TwoDimensionalAnimationController>();
            if (playerController == null)
            {
                Debug.LogError("Player controller not found. Ensure the player object is active in the scene.");
            }
        }

        // Updates and applies sensitivity settings to the player
        public void UpdateAndApplySensitivity(float sensitivityX, float sensitivityY)
        {
            PlayerPrefs.SetFloat("XSensitivity", sensitivityX);
            PlayerPrefs.SetFloat("YSensitivity", sensitivityY);
            GameManager.Instance.SensitivityX = sensitivityX;
            GameManager.Instance.SensitivityY = sensitivityY;

            if (playerController == null)
            {
                FindPlayerController();
            }

            if (playerController != null)
            {
                playerController.mouseSensitivity = sensitivityX; // Assuming X sensitivity is applied to mouse sensitivity
                Debug.Log($"Applied Sensitivity: X={sensitivityX}, Y={sensitivityY} to player.");
            }
        }

        // Updates and applies mouse smoothing to the player
        public void UpdateAndApplyMouseSmoothing(float smoothing)
        {
            PlayerPrefs.SetFloat("MouseSmoothing", smoothing);
            GameManager.Instance.MouseSmoothing = smoothing;

            if (playerController == null)
            {
                FindPlayerController();
            }

            if (playerController != null)
            {
                playerController.verticalRotationSpeed = smoothing * 180f; // Example: apply smoothing to rotation speed
                Debug.Log($"Applied Mouse Smoothing: {smoothing} to player.");
            }
        }

        // Updates and applies difficulty settings to the player
        public void UpdateAndApplyDifficulty(string difficulty)
        {
            // Save difficulty in PlayerPrefs
            if (difficulty.Equals("Normal", System.StringComparison.OrdinalIgnoreCase))
            {
                PlayerPrefs.SetInt("NormalDifficulty", 1);
                PlayerPrefs.SetInt("HardCoreDifficulty", 0);
            }
            else if (difficulty.Equals("Hardcore", System.StringComparison.OrdinalIgnoreCase))
            {
                PlayerPrefs.SetInt("NormalDifficulty", 0);
                PlayerPrefs.SetInt("HardCoreDifficulty", 1);
            }

            GameManager.Instance.Difficulty = difficulty;

            if (playerController == null)
            {
                FindPlayerController();
            }

            if (playerController != null && playerController.attributesManager != null)
            {
                // Pass the difficulty string directly to the method
                playerController.attributesManager.SetDifficultyMultiplier(difficulty);
                Debug.Log($"Applied {difficulty} difficulty to player.");
            }
            else
            {
                Debug.LogError("Player controller or attributes manager not found. Cannot apply difficulty.");
            }
        }

        // Example for applying custom settings like Field of View
        public void UpdateAndApplyFieldOfView(Cinemachine.CinemachineVirtualCamera virtualCamera, float fov)
        {
            if (virtualCamera != null)
            {
                virtualCamera.m_Lens.FieldOfView = fov;
                Debug.Log($"Updated and applied Field of View: {fov}");
            }
            else
            {
                Debug.LogError("Virtual Camera is null. Cannot update Field of View.");
            }
        }

        // Applies all settings to the player dynamically
        public void ApplySettings()
        {
            if (playerController == null)
            {
                FindPlayerController();
            }

            if (playerController != null)
            {
                // Apply sensitivity
                playerController.mouseSensitivity = GameManager.Instance.SensitivityX;

                // Apply smoothing
                playerController.verticalRotationSpeed = GameManager.Instance.MouseSmoothing * 180f;

                // Apply difficulty
                playerController.attributesManager?.SetDifficultyMultiplier(GameManager.Instance.Difficulty);

                Debug.Log("All settings applied to player.");
            }
            else
            {
                Debug.LogError("Player controller not found. Settings could not be applied.");
            }
        }
    }
}
