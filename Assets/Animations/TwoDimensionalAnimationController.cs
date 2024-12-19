using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TwoDimensionalAnimationController : MonoBehaviour
{
    private Animator animator;
    private Vector2 moveInput; // Stores movement input
    private int VelocityZHash;
    private int VelocityXHash;
    private bool isSprinting; // Tracks sprint state
    private GameDevCW inputActions; // Input Actions asset reference
    float velocityZ = 0.0f;
    float velocityX = 0.0f;
    
    private Vector2 lookInput;
    private float verticalRotation = 10f;
    public float acceleration = 2f;
    public float deceleration = 2f;

    // Define maximum velocities
    public float maxVelocityZ = 1.0f;
    public float maxVelocityX = 1.0f;
    public float sprintMaxVelocityZ = 2.0f;
    public float sprintMaxVelocityX = 2.0f;

    // Camera movement
    [Header("References")]
    [SerializeField] private Transform idleTransform;
    [SerializeField] private Transform cameraTransform;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f;         // Significantly increased
    [SerializeField] private float verticalRotationSpeed = 180f; // Separate speed for vertical
    [SerializeField] private float smoothRotationTime = 0.05f;   // Reduced for responsiveness
    [SerializeField] private float inputSmoothTime = 0.02f;      // Reduced for faster input
    [SerializeField] private float mouseSensitivity = 2.0f;      // Added sensitivity multiplier // Added to control acceleration
    
    private Vector2 smoothedLookInput;
    private Vector2 lookInputVelocity;
    private float currentRotationVelocity;
    private float targetRotation;
    private float lastTargetRotation;

    private void Awake()
    {
        inputActions = new GameDevCW();

        // Subscribe to the Move action's performed and canceled events
        inputActions.Player.Move.performed += HandleMove;
        inputActions.Player.Move.canceled += HandleMove;

        inputActions.Player.Look.performed += HandleLook;
        inputActions.Player.Look.canceled += HandleLook;

        // Subscribe to the Sprint action's performed and canceled events
        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        VelocityZHash = Animator.StringToHash("VelocityZ");
        VelocityXHash = Animator.StringToHash("VelocityX");
        if (idleTransform == null)
            idleTransform = transform;
                    
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    /// <summary>
    /// Updates the animation state based on player input and sprint state.
    /// </summary>

    private void HandleRotation()
    {
        if (idleTransform == null)
        {
            Debug.LogError("idleTransform is not assigned!");
            return;
        }

        // Apply sensitivity to input
        Vector2 sensitiveInput = lookInput * mouseSensitivity;
        
        // Quick input smoothing
        smoothedLookInput = Vector2.Lerp(
            smoothedLookInput,
            sensitiveInput,
            1f - Mathf.Exp(-inputSmoothTime * 60f)
        );

        // Handle vertical camera rotation with increased speed
        float mouseY = smoothedLookInput.y * verticalRotationSpeed * Time.deltaTime;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -89f, 89f);
       
        if (cameraTransform != null)
        {
            // Direct camera rotation for vertical movement
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        // Handle horizontal character rotation
        float mouseX = smoothedLookInput.x * rotationSpeed * Time.deltaTime;
        
        // Update target rotation more directly
        targetRotation += mouseX * 60f; // Multiplied by 60 to normalize for framerate
        
        // Quick smooth interpolation
        float currentRotationY = Mathf.SmoothDampAngle(
            idleTransform.eulerAngles.y,
            targetRotation,
            ref currentRotationVelocity,
            smoothRotationTime
        );

        // Apply the rotation
        idleTransform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
    }

    // public void OnLook(Vector2 input)
    // {
    //     lookInput = value.Get<Vector2>();
    // }   

    private void Update()
    {
        // Detect input directions
        bool forwardPressed = moveInput.y > 0;
        bool leftPressed = moveInput.x < 0;
        bool rightPressed = moveInput.x > 0;
        bool backwardPressed = moveInput.y < 0;
        bool isRunning = isSprinting;

        HandleRotation();
        // Determine current maximum velocities based on sprinting
        float currentMaxVelocityZ = isRunning ? sprintMaxVelocityZ : maxVelocityZ;
        float currentMaxVelocityX = isRunning ? sprintMaxVelocityX : maxVelocityX;

        // Handle forward movement
        if (forwardPressed && velocityZ < currentMaxVelocityZ)
        {
            velocityZ += Time.deltaTime * acceleration;
            velocityZ = Mathf.Min(velocityZ, currentMaxVelocityZ);
        }
        else if (!forwardPressed && velocityZ > 0.0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
            velocityZ = Mathf.Max(velocityZ, 0.0f);
        }

        // Left Movement
        if (leftPressed && velocityX > -currentMaxVelocityX)
        {
            velocityX -= Time.deltaTime * acceleration;
            velocityX = Mathf.Max(velocityX, -currentMaxVelocityX);
        }
        else if (!leftPressed && velocityX < 0.0f)
        {
            velocityX += Time.deltaTime * deceleration;
            velocityX = Mathf.Min(velocityX, 0.0f);
        }

        // Right Movement
        if (rightPressed && velocityX < currentMaxVelocityX)
        {
            velocityX += Time.deltaTime * acceleration;
            velocityX = Mathf.Min(velocityX, currentMaxVelocityX);
        }
        else if (!rightPressed && velocityX > 0.0f)
        {
            velocityX -= Time.deltaTime * deceleration;
            velocityX = Mathf.Max(velocityX, 0.0f);
        }

        // Backward Movement
        if (backwardPressed)
        {
            velocityZ -= Time.deltaTime * acceleration;
            velocityZ = Mathf.Max(velocityZ, -currentMaxVelocityZ);
        }
        else if (!backwardPressed && velocityZ < 0.0f)
        {
            velocityZ += Time.deltaTime * deceleration;
            velocityZ = Mathf.Min(velocityZ, 0.0f);
        }

        // Clamp small residual velocities to zero
        if (!leftPressed && !rightPressed && Mathf.Abs(velocityX) < 0.05f)
        {
            velocityX = 0.0f;
        }

        if (!forwardPressed && !backwardPressed && Mathf.Abs(velocityZ) < 0.05f)
        {
            velocityZ = 0.0f;
        }

        // Handle overshooting velocity limits
        if (forwardPressed && isRunning && velocityZ > currentMaxVelocityZ)
        {
            velocityZ = currentMaxVelocityZ;
        }
        else if (forwardPressed && velocityZ > currentMaxVelocityZ)
        {
            velocityZ -= Time.deltaTime * deceleration;
            if (velocityZ > currentMaxVelocityZ && velocityZ < (currentMaxVelocityZ + 0.05f))
            {
                velocityZ = currentMaxVelocityZ;
            }
        }
        else if (forwardPressed && velocityZ < currentMaxVelocityZ && velocityZ > (currentMaxVelocityZ - 0.05f))
        {
            velocityZ = currentMaxVelocityZ;
        }

        // **Dynamic Movement Direction Calculation**
        // Use the camera's forward and right vectors to determine the direction
        Vector3 cameraForward = cameraTransform.forward; // Forward vector of the camera
        cameraForward.y = 0f; // Ignore vertical component
        cameraForward.Normalize(); // Ensure the vector is normalized

        Vector3 cameraRight = cameraTransform.right; // Right vector of the camera
        cameraRight.y = 0f; // Ignore vertical component
        cameraRight.Normalize(); // Ensure the vector is normalized

        // Calculate movement direction based on input and camera orientation
        Vector3 moveDirection = (cameraForward * velocityZ) + (cameraRight * velocityX);

        // Apply the movement to the character's position
        transform.position += moveDirection * Time.deltaTime;

        // Debugging movement direction
        Debug.Log($"Move Direction: {moveDirection}, VelocityX: {velocityX}, VelocityZ: {velocityZ}");

        // Debugging movement

        // Update the animator with the current velocities
        animator.SetFloat(VelocityZHash, velocityZ);
        animator.SetFloat(VelocityXHash, velocityX);
    }


    // Called when the Move action is performed or canceled
    private void HandleMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void HandleLook(InputAction.CallbackContext context){
        lookInput = context.ReadValue<Vector2>();
    }
}