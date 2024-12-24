using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel; // Reference to the pause menu UI
    private bool isPaused = false;

    private GameDevCW inputActions; // Input Actions reference

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
        SceneManager.LoadScene(0);
    }
}
