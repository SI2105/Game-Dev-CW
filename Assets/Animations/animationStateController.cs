using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationStateController : MonoBehaviour
{
    private Animator animator;
    private int isWalkingHash;
    private int isRunningHash;
    private Vector2 moveInput; // Stores movement input
    private bool isSprinting; // Tracks sprint state
    private GameDevCW inputActions; // Input Actions asset reference

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
        isWalkingHash = Animator.StringToHash("isWalking"); // Get the hash ID for the "isWalking" parameter
        isRunningHash = Animator.StringToHash("isRunning"); // Get the hash ID for the "isRunning" parameter
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
        bool isWalking = moveInput.y > 0;
        animator.SetBool(isWalkingHash, isWalking);

        // Set the animator's isRunning parameter if sprinting and moving forward
        /// <remarks>
        /// The isRunning parameter is set to true if the player is walking (isWalking is true) and sprinting (isSprinting is true).
        /// </remarks>
        bool isRunning = isWalking && isSprinting;
        animator.SetBool(isRunningHash, isRunning);
    }


    // Called when the Move action is performed or canceled
    private void HandleMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
