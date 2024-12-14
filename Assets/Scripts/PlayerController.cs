using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

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

    private float staminaCooldown = 5f;
    private bool canUseStamina = true;

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
    private bool isPaused = false;
    // taking damage
    private float damageCooldown = 1.5f;
    private bool canTakeDamage = true;

    // THA UI
    [SerializeField] public PlayerATH playerATH;

    // Player Attack
    private PlayerAttack playerAttack;

    private InputAction pauseAction;
    private GameDevCW inputActions;
    private bool iss;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();    
        inputActions = new GameDevCW();

        pauseAction = inputActions.Player.Pause; // Fixed typo: inputactions -> inputActions
        inputActions.Player.Jump.performed += OnJumpPerformed;
        inputActions.Player.Sprint.started += OnSprintStarted;
        inputActions.Player.Sprint.canceled += OnSprintCanceled;
        pauseAction.performed += context => TogglePause();
    }
    private void Update()
    {
        Debug.Log(iss);
    }
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded && canUseStamina)
        {
            Debug.Log("Jump input detected");
            Jump();
        }
    }

    private void OnSprintStarted(InputAction.CallbackContext context)
    {
        Debug.Log("sprint started");
        iss = context.started || context.performed;
        StartSprint();
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("sprint stopped");
        StopSprint();
    }
    
    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Pause.performed += OnPausePerformed; // Ensure Pause exists in the Input System action map
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        currHealth = maxHealth;
        currStamina = maxStamina;
        currSpeed = walkSpeed;

        playerATH.UpdateHealthBar(currHealth, maxHealth);
        playerATH.UpdateStaminaBar(currStamina, maxStamina);
    }

    void OnMove(InputValue movementValue)
    {
        if(!isPaused){
            // Gets the movement input from the player --> A/D and W/S
            Vector2 movementVector = movementValue.Get<Vector2>();
            movementX = movementVector.x; 
            movementY = movementVector.y; 
        }
         else
        {
            // Reset movement when paused
            movementX = 0;
            movementY = 0;
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        TogglePause();
    }


    private void FixedUpdate() 
    {
        if(!isPaused){
            MovePlayer();
        }
        UpdateStamina();
    }

    private void UpdateStamina()
{
    if (!isSprinting && canUseStamina && currStamina < maxStamina)
    {
        RecoverStamina(5f * Time.deltaTime); // Adjust recovery rate as needed
    }
}

    public float getPlayerHealth()
    {
        return currHealth;
    }

    //if the player has exited the maze, return true, otherwise return false

    public bool hasWon()
    {
        if (playerCamera.transform.position.x < 80 && 
            playerCamera.transform.position.x > 72 && 
            playerCamera.transform.position.z < 20) // Fixed & -> &&
        {
            return true;
        }

        return false;
    }

    //toggles the pause state of the game
      void TogglePause()
    {
        Debug.Log("pausing");
        //toggle the pause state of the game
        isPaused = !isPaused;

        if (isPaused)
        {
            Debug.Log("paused");
            //set the paused screen which is gray
            playerATH.PauseScreen();
            PauseGame();
        }
        else
        {
            //return the screen to normal
            playerATH.UnPauseScreen();
            ResumeGame();
        }
    }

    void PauseGame()
    {
        Rigidbody playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.velocity = Vector3.zero; // Stop the player's movement
        playerRigidbody.angularVelocity = Vector3.zero;
        // Retrieve and disable all NavMeshAgents to freeze enemies
        NavMeshAgent[] enemies = FindObjectsOfType<NavMeshAgent>();
        foreach (NavMeshAgent agent in enemies)
        {
            agent.enabled = false;
        }
        Time.timeScale = 0; // Freeze time
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true;
    }

    void ResumeGame()
    {

        // Retrieve and enable all NavMeshAgents to resume enemy movement
        NavMeshAgent[] enemies = FindObjectsOfType<NavMeshAgent>();
        foreach (NavMeshAgent agent in enemies)
        {
            agent.enabled = true;
        }
        Time.timeScale = 1; // Resume time
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        Cursor.visible = false;
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
    
    private void OnDisable()
    {
        inputActions.Player.Pause.performed -= OnPausePerformed; 
        inputActions.Player.Jump.performed -= OnJumpPerformed; 
        inputActions.Player.Sprint.started -= OnSprintStarted; 
        inputActions.Player.Sprint.canceled -= OnSprintCanceled; 
        inputActions.Disable();
    }

    private void StartSprint(){
        Debug.Log("sprint started");

        if (canUseStamina){
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

            // Check if the collision object has an EnemyController component
            EnemyController enemy = otherObject.GetComponent<EnemyController>();

            if (enemy != null && canTakeDamage){
                TakeDamage(10f);
            }

            // Check if the collision point is below the player and the surface is flat enough to be considered as a ground
            if (contact.point.y <= transform.position.y && contact.normal.y > 0.5f){
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
        UseStamina(5f);
    }
   /// <summary>
    /// Consumes stamina and initiates cooldown if depleted.
    /// </summary>
    private void UseStamina(float amount)
    {
        currStamina -= amount;
        currStamina = Mathf.Max(currStamina, 0f); // Ensure stamina doesn't go below zero

        if (currStamina == 0f && canUseStamina)
        {
            Debug.Log("Starting stamina cooldown");
            StartCoroutine(RecoverStaminaCooldown());
        }
        playerATH.UpdateStaminaBar(currStamina, maxStamina); // Update UI
    }

    /// <summary>
    /// Recovers stamina over time.
    /// </summary>
    private void RecoverStamina(float amount)
    {
        if (currStamina < maxStamina)
        {
            currStamina += amount;
            currStamina = Mathf.Min(currStamina, maxStamina);
            Debug.Log($"Stamina recovering: {currStamina}/{maxStamina}");
        }

        playerATH.UpdateStaminaBar(currStamina, maxStamina);
    }


    /// <summary>
    /// Applies damage to the player and handles knockback.
    /// </summary>
    private void TakeDamage(float amount)
    {
        currHealth -= amount;
        currHealth = Mathf.Max(currHealth, 0f); // Ensure health doesn't go below zero
        playerATH.UpdateHealthBar(currHealth, maxHealth); // Update UI

        // Apply knockback force
        rb.AddForce((Vector3.up + Vector3.right + Vector3.left) * 1f, ForceMode.Impulse);
        playerATH.StartDamageEffect(); // Trigger UI damage effect
        StartCoroutine(DamageCooldown()); // Start damage cooldown
    }

    /// <summary>
    /// Handles the cooldown period after taking damage.
    /// </summary>
    private IEnumerator DamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }

    /// <summary>
    /// Manages the cooldown period for stamina recovery.
    /// </summary>
    private IEnumerator RecoverStaminaCooldown()
    {
        canUseStamina = false;
        yield return new WaitForSeconds(staminaCooldown);
        canUseStamina = true;
    }


    /// <summary>
    /// Increases the player's health, ensuring it doesn't exceed the maximum.
    /// </summary>
    private void GainHealth(float amount)
    {
        currHealth += amount;
        currHealth = Mathf.Min(currHealth, maxHealth); // Ensure health doesn't exceed max
        playerATH.UpdateHealthBar(currHealth, maxHealth); // Update UI
    }

    /// <summary>
    /// Toggles the cursor lock state and visibility.
    /// </summary>
    public void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    }


