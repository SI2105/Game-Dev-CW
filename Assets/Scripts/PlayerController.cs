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
    [SerializeField] private Camera playerCamera;

    // Movement variables
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    private bool isSprinting = false;
    private float currSpeed;

    // Jumping variables
    private bool isGrounded;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float jumpMultiplier = 2f;

    // THA variables
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxStamina = 100f;
    private float currHealth;
    private float currStamina;

    // THA UI
    [SerializeField] public PlayerATH playerATH;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currHealth = maxHealth;
        currStamina = maxStamina;
        currSpeed = walkSpeed;

        playerATH.UpdateHealthBar(currHealth, maxHealth);
        playerATH.UpdateStaminaBar(currStamina, maxStamina);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    void OnMove(InputValue movementValue)
    {
        // Gets the movement input from the player --> A/D and W/S
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
        Vector3 movement = (right * movementX + forward * movementY) * currSpeed;
        Debug.Log("current speed" + currSpeed);

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
        Run();
    }

    private void Run(){
        if (Input.GetKey(KeyCode.LeftShift) && currStamina > 0){
            Sprint();
        }
        else{
            StopSprint();
        }
    }

    private void Sprint(){
        if (currStamina > 0){
            isSprinting = true;
            currSpeed = sprintSpeed;
            UseStamina(25f * Time.deltaTime);
        } else {
            StopSprint();
        }
    }

    private void StopSprint(){
        isSprinting = false;
        currSpeed = walkSpeed;
        RecoverStamina(5f * Time.deltaTime);
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
            GameObject otherObject = collision.collider.gameObject;

            Enemy enemy = otherObject.GetComponent<Enemy>();

            if (enemy != null){
                TakeDamage(10f);
            }

            if (contact.point.y <= transform.position.y){
                isGrounded = true;
                return;
            }
        }
        isGrounded = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    // Applies a jump force to the rigidbody player component
    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void UseStamina(float q){
        currStamina -=q;
        if (currStamina < 0){
            currStamina = 0;
        }
        playerATH.UpdateStaminaBar(currStamina, maxStamina);
        
    }

    private void RecoverStamina(float q){
        currStamina += q;
        if (currStamina > maxStamina){
            currStamina = maxStamina;
        }
        playerATH.UpdateStaminaBar(currStamina, maxStamina);

    }

    private void TakeDamage(float q){
        currHealth -= q;
        if (currHealth < 0){
            currHealth = 0;
        }
        playerATH.UpdateHealthBar(currHealth, maxHealth);

        playerATH.StartDamageEffect();
    }

    private void GainHealth(float q){
        currHealth += q;
        if (currHealth > 100){
            currHealth = 100;
        }
        playerATH.UpdateHealthBar(currHealth, maxHealth);
        
    }
    }

    // void ToggleCursorLock(){
    //     if (Cursor.lockState == CursorLockMode.Locked){
    //         Cursor.lockState = CursorLockMode.None;
    //         Cursor.visible = true;
    //     } else {
    //         Cursor.lockState = CursorLockMode.Locked;
    //         Cursor.visible = false;
    //     }
    // }
