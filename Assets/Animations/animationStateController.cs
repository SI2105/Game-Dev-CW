using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationStateController : MonoBehaviour
{
    private Animator animator;
    private int isWalkingHash;
    private Vector2 moveInput; // Stores movement input
    private GameDevCW inputActions; // Input Actions asset reference

    private void Awake()
    {
        inputActions = new GameDevCW();

        // Subscribe to the Move action's performed and canceled events
        inputActions.Player.Move.performed += HandleMove;
        inputActions.Player.Move.canceled += HandleMove;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking"); // Get the hash ID for the "isWalking" parameter    
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        // Set the animator's isWalking parameter based on moveInput
        animator.SetBool(isWalkingHash, moveInput.y > 0);
    }

    // Called when the Move action is performed or canceled
    private void HandleMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
