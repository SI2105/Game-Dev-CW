using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CinemachineOffsetShake : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Reference to the Cinemachine Virtual Camera
    private float shakeIntensity = 0.05f; // Intensity of the shake (only Y-axis)
    private float shakeFrequency = 10f; // Frequency of the shake
    private CinemachineCameraOffset cameraOffset; // Reference to the Camera Offset component
    private GameDevCW inputActions; // Input Actions asset reference
    private Vector2 moveInput; // Stores movement input
    private bool isSprinting; // Tracks sprint state

    private Vector3 originalOffset; // Stores the original offset

    private void Awake()
    {
        // Initialize input actions
        inputActions = new GameDevCW();
        inputActions.Player.Move.performed += HandleMove;
        inputActions.Player.Move.canceled += HandleMove;

        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        if (virtualCamera != null)
        {
            // Get the Cinemachine Camera Offset component
            cameraOffset = virtualCamera.GetComponent<CinemachineCameraOffset>();
            if (cameraOffset != null)
            {
                originalOffset = cameraOffset.m_Offset; // Save the original offset
            }
            else
            {
                Debug.LogError("Cinemachine Camera Offset component is missing!");
            }
        }
        else
        {
            Debug.LogError("Virtual Camera is not assigned!");
        }
    }

    private void Update()
    {
        // Check if the player is moving
        bool isWalking = moveInput.magnitude > 0;
        bool forwardPressed = moveInput.y > 0;
        bool leftPressed = moveInput.x < 0;
        bool rightPressed = moveInput.x > 0;
        bool backwardPressed = moveInput.y < 0;
        bool isRunning = isSprinting;

        // Set intensity and frequency based on movement type
        float shakeIntensity = isRunning ? 0.1f : 0.075f; // Higher intensity for sprinting
        float shakeFrequency = isRunning ? 13f : 11f; // Lower frequency for sprinting

        if (isWalking)
        {
            // Apply camera shake to Y-axis only
            float offsetY = Mathf.Sin(Time.time * shakeFrequency) * shakeIntensity;

            cameraOffset.m_Offset = new Vector3(
                originalOffset.x,       // Keep X-axis unchanged
                originalOffset.y + offsetY, // Modify Y-axis for shake
                originalOffset.z        // Keep Z-axis unchanged
            );
        }
        else
        {
            // Reset the Offset to its original value
            cameraOffset.m_Offset = originalOffset;
        }
    }


    private void HandleMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
