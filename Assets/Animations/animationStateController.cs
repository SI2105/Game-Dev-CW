using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationStateController : MonoBehaviour
{
    private Animator animator;
    private int VelocityHash;
    private Vector2 moveInput; // Stores movement input
    private bool isSprinting; // Tracks sprint state
    private GameDevCW inputActions; // Input Actions asset reference

    float velocity = 0.0f;
    public float acceleration = 0.1f;
    public float deceleration = 0.1f;

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
        VelocityHash = Animator.StringToHash("Velocity"); // Get the hash ID for the "isRunning" parameter
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
        // Set the animator's isWalking parameter based on moveInput
        /// <remarks>
        /// The isWalking parameter is set to true if the player's vertical movement input (moveInput.y) is greater than 0.
        /// </remarks>
        bool forwardPressed = moveInput.y > 0;
        if (forwardPressed && velocity < 1.0f){
            velocity += Time.deltaTime * acceleration;
        }

        if (!forwardPressed && velocity > 0.0f){
            velocity -= Time.deltaTime * deceleration;
        }

        if (!forwardPressed && velocity < 0.0f){
            velocity = 0.0f;
        }

        animator.SetFloat(VelocityHash, velocity);
    }


    // Called when the Move action is performed or canceled
    private void HandleMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
