using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using SG;
public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel; // Reference to the pause menu UI
    private bool isPaused = false;
    private GameDevCW inputActions; // Input Actions reference
    private SaveLoadManager saveLoadManager; // Save/load manager reference
    public void Awake()
    {
        inputActions = new GameDevCW(); // Initialize input actions
    }
    public void Start()
    {
        pauseMenuPanel.SetActive(false);
    }
    public void OnEnable()
    {
        inputActions.Player.Pause.performed += HandlePause; // Subscribe to the Pause input
        inputActions.Enable(); // Enable the input actions
    }
    public void OnDisable()
    {
        inputActions.Player.Pause.performed -= HandlePause; // Unsubscribe from the Pause input
        inputActions.Disable(); // Disable the input actions
    }
    public void HandlePause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TogglePauseMenu();
        }
    }
    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }
        if (isPaused)
        {
            // Freeze the game and unlock the cursor
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Resume the game and lock the cursor
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void ResumeGame()
    {
        Debug.Log("Resume button clicked!");
        isPaused = false;
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void goBackToMain()
    {
        // Save the game before returning to the main menu
        if (saveLoadManager != null)
        {
            PlayerAttributesManager player = FindObjectOfType<PlayerAttributesManager>();
            if (player != null)
            {
                saveLoadManager.SaveGame(player);
                Debug.Log("Game saved before returning to the main menu.");
            }
            else
            {
                Debug.LogWarning("PlayerAttributesManager not found! Game save skipped.");
            }
        }
        else
        {
            Debug.LogWarning("SaveLoadManager not found! Game save skipped.");
        }

        // Load the main menu scene
        SceneManager.LoadScene("New menu");
    }
}