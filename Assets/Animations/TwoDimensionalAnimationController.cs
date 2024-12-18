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
    
    public float acceleration = 2f;
    public float deceleration = 2f;

    // Define maximum velocities
    public float maxVelocityZ = 1.0f;
    public float maxVelocityX = 1.0f;
    public float sprintMaxVelocityZ = 2.0f;
    public float sprintMaxVelocityX = 2.0f;


    private void Awake()
    {
        inputActions = new GameDevCW();

        // Subscribe to the Move action's performed and canceled events
        inputActions.Player.Move.performed += HandleMove;
        inputActions.Player.Move.canceled += HandleMove;

        // Subscribe to the Sprint action's performed and canceled events
        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        VelocityZHash = Animator.StringToHash("VelocityZ");
        VelocityXHash = Animator.StringToHash("VelocityX");
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
    private void Update()
    {
        // Detect input directions
        bool forwardPressed = moveInput.y > 0;
        bool leftPressed = moveInput.x < 0;
        bool rightPressed = moveInput.x > 0;
        bool backwardPressed = moveInput.y < 0;
        bool isRunning = isSprinting;

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

        if (forwardPressed && isRunning && velocityZ > currentMaxVelocityZ){
            velocityZ = currentMaxVelocityZ;
        } else if (forwardPressed && velocityZ > currentMaxVelocityZ){
            velocityZ -= Time.deltaTime * deceleration;
            if (velocityZ > currentMaxVelocityZ && velocityZ < (currentMaxVelocityZ + 0.05f)){
                velocityZ = currentMaxVelocityZ;
            }
        } else if (forwardPressed && velocityZ < currentMaxVelocityZ && velocityZ > (currentMaxVelocityZ - 0.05f)){
            velocityZ = currentMaxVelocityZ;
        }

        Debug.Log($"VelocityZ: {velocityZ}, VelocityX: {velocityX}, IsRunning: {isRunning}");

        // Update the animator with the current velocities
        animator.SetFloat(VelocityZHash, velocityZ);
        animator.SetFloat(VelocityXHash, velocityX);
    }

    // Called when the Move action is performed or canceled
    private void HandleMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
