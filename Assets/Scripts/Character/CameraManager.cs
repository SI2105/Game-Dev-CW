using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cameraA; // First camera
    [SerializeField] private CinemachineVirtualCamera cameraB; // Second camera
    [SerializeField] private int highPriority = 10; // High priority value
    [SerializeField] private int lowPriority = 0;  // Low priority value

    private GameDevCW inputActions;

    private void Awake()
    {
        // Initialize input actions
        inputActions = new GameDevCW();
    }

    private void OnEnable()
    {
        // Enable the input action
        inputActions.Enable();
        inputActions.Player.ToggleCamera.performed += OnToggleCamera; // Bind the action
    }

    private void OnDisable()
    {
        // Disable the input action
        inputActions.Player.ToggleCamera.performed -= OnToggleCamera; // Unbind the action
        inputActions.Disable();
    }

    private void OnToggleCamera(InputAction.CallbackContext context)
    {
        // Call the toggle camera method when the input is performed
        ToggleCameras();
    }

    private void ToggleCameras()
    {
        if (cameraA.Priority == highPriority)
        {
            // Swap priorities
            cameraA.Priority = lowPriority;
            cameraB.Priority = highPriority;
        }
        else
        {
            // Swap priorities
            cameraA.Priority = highPriority;
            cameraB.Priority = lowPriority;
        }
    }
}
