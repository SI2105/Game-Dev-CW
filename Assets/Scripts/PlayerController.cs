using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Player's rigidbody reference component
    private Rigidbody rb; 

    // Camera variables
    private float movementX;
    private float movementY;
    public Camera playerCamera;

    // Movement variables
    public float speed = 10f;
    public float maxWeight = 25f;
    public float maxItem = 25f; 

    // Jumping variables
    private bool isGrounded;
    public float jumpForce = 5f;
    public float fallMultiplier = 2.5f;
    public float jumpMultiplier = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnMove(InputValue movementValue)
    {
        // Gets the movement input from the player, A/D and W/S
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x; 
        movementY = movementVector.y; 
    }

    private void FixedUpdate() 
    {
        MovePlayer();
    }

    private void MovePlayer(){
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

        // prevents the movement to be vertical
        forward.y = 0f;
        right.y = 0f;

        // Ensures to keep consistent movement speed in all directions
        forward.Normalize();
        right.Normalize();

        // Creates the movement vector
        Vector3 movement = (right * movementX + forward * movementY) * speed;

        // if a movement is currently happening it updates the velocity with current values
        if (movement.magnitude > 0){
            rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        }
        // if there is no movement, keeps the velocity on the Y axis in case the player is in the air and resets X and Z axes
        else{
            rb.velocity = new Vector3(0, rb.velocity.y, 0); 
        }
    }

    void Update()
    {
        HandleJumpInput();
        SmoothenJump();
    }

    private void HandleJumpInput(){
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            Jump();

    }

    private void SmoothenJump()
    {
        // Applies a fall multiplier if the player is falling
        if (rb.velocity.y < 0){
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        // Applies a jump multipler when the space button is released
        else if (rb.velocity.y > 0 && !Keyboard.current.spaceKey.isPressed){
            rb.velocity += Vector3.up * Physics.gravity.y * (jumpMultiplier - 1) * Time.deltaTime;     
        }
    }

    // Ground check
    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.point.y <= transform.position.y)
                isGrounded = true;
                return;
        }
        isGrounded = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    // Applies a jumo force to the rigidbody player component
    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

}
