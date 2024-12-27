using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace SG{

    public class TwoDimensionalAnimationController : MonoBehaviour
    {
    #region Animator Variables
        private Animator animator;
        private Vector2 moveInput;
        private int VelocityZHash;
        private int VelocityXHash;
        private int isJumpingHash;
        private int isRunningHash;
        private int isWalkingHash;
        private int isIdleHash;
        #endregion

        #region Player State Variables
        private bool isSprinting;
        private bool isWalking;
        private bool isIdle;
        private bool isJumping;
        private bool isRunning;
        #endregion

        #region Physics and Ground Detection
        public Transform groundCheck;
        public LayerMask groundLayer;
        private bool isGrounded;
        private bool wasGrounded;
        private Vector2 jumpMovement;
        #endregion

        #region Input Actions
        private GameDevCW inputActions;
        bool forwardPressed = false;
        bool backwardPressed = false;
        bool leftPressed = false;
        bool rightPressed = false;
        bool InventoryVisible = false;
        bool PauseVisible = false;
        #endregion

        #region Velocity Variables
        float velocityZ = 0.0f;
        float velocityX = 0.0f;
        #endregion

        #region Look Input and Rotation Variables
        private Vector2 lookInput;
        private float verticalRotation = 10f;
        private Vector2 smoothedLookInput;
        private Vector2 lookInputVelocity;
        private float currentRotationVelocity;
        private float targetRotation;
        private float lastTargetRotation;
        #endregion
        #region PlayerStats Display

        [SerializeField] private TextMeshProUGUI PlayerStatsText;

        public void UpdatePlayerStats() {
            if (PlayerStatsText != null) {
                PlayerStatsText.text = GetPlayerStatsText();
            }

        }

        public string GetPlayerStatsText() {

            
            return
                    $"<align=left>Current Level:<line-height=0>\n<align=right>{attributesManager.CurrentLevel} / {attributesManager.MaxLevel}<line-height=1em>\n" +

                    $"<align=left>Current XP:<line-height=0>\n<align=right>{attributesManager.CurrentXP}<line-height=1em>\n" +
                    $"<align=left>XP to Next Level:<line-height=0>\n<align=right> \t {attributesManager.XPToNextLevel}<line-height=1em>\n" +
                    $"<align=left>Strength:<line-height=0>\n<align=right>{attributesManager.Strength}<line-height=1em>\n" +
                    $"<align=left>Agility:<line-height=0>\n<align=right>{attributesManager.Agility}<line-height=1em>\n" +
                    $"<align=left>Endurance:<line-height=0>\n<align=right>{attributesManager.Endurance}<line-height=1em>\n" +
                    $"<align=left>Intelligence:<line-height=0>\n<align=right>{attributesManager.Intelligence}<line-height=1em>\n" +
                    $"<align=left>Luck:<line-height=0>\n<align=right>{attributesManager.Luck}<line-height=1em>" +
                    "\n \n" +
               
                    $"<align=left>Base Damage:<line-height=0>\n<align=right>{attributesManager.BaseDamage}<line-height=1em>\n" +
                    $"<align=left>Critical Hit Chance:<line-height=0>\n<align=right>{attributesManager.CriticalHitChance}<line-height=1em>\n" +
                    $"<align=left>Critical Hit Multiplier:<line-height=0>\n<align=right>{attributesManager.CriticalHitMultiplier}<line-height=1em>\n" +
                    $"<align=left>Attack Speed:<line-height=0>\n<align=right>{attributesManager.AttackSpeed}<line-height=1em>\n" +
                    $"<align=left>Armor:<line-height=0>\n<align=right>{attributesManager.Armor}<line-height=1em>\n" +
                    $"<align=left>Block Chance:<line-height=0>\n<align=right>{attributesManager.BlockChance}<line-height=1em>\n" +
                    $"<align=left>Dodge Chance:<line-height=0>\n<align=right>{attributesManager.DodgeChance}<line-height=1em>" +
                    "\n \n"
                   
                    ;
            

        }


        #endregion


        #region Movement Settings
        public float acceleration = 2f;
        public float deceleration = 2f;

        public float maxVelocityZ = 1.0f;
        public float maxVelocityX = 1.0f;
        public float sprintMaxVelocityZ = 2.0f;
        public float sprintMaxVelocityX = 2.0f;
        #endregion

        #region Camera Settings
        [Header("References")]
        [SerializeField] private Transform idleTransform;
        [SerializeField] private Transform cameraTransform;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float verticalRotationSpeed = 180f;
        [SerializeField] private float smoothRotationTime = 0.05f;
        [SerializeField] private float inputSmoothTime = 0.02f;
        [SerializeField] private float mouseSensitivity = 2.0f;
        #endregion
        private PlayerAttributesManager attributesManager;
        [SerializeField]
        GameObject pauseMenuPanel;
        public CinemachineFreeLook freeLookCamera;


        private void Awake()
        {
            inputActions = new GameDevCW();

            // Subscribe to the Move action's performed and canceled events
            inputActions.Player.Move.performed += HandleMove;
            inputActions.Player.Move.canceled += HandleMove;

            inputActions.Player.Look.performed += HandleLook;
            inputActions.Player.Look.canceled += HandleLook;

            inputActions.Player.Jump.performed += HandleJump;
            inputActions.Player.Jump.canceled += HandleJump;

            // Subscribe to the Sprint action's performed and canceled events
            inputActions.Player.Sprint.performed += ctx => isSprinting = true;
            inputActions.Player.Sprint.canceled += ctx => isSprinting = false;

            inputActions.Player.Inventory.performed += HandleInventory;
            inputActions.Player.Inventory.canceled += HandleInventory;
            inputActions.Player.Pause.performed += HandlePause;
            inputActions.Player.Pause.canceled += HandlePause;
            attributesManager = GetComponent<PlayerAttributesManager>();

            if (attributesManager) {
                Debug.Log("Inventory Manager found");
                inputActions.UI.Click.performed += attributesManager.InventoryManager.OnClick;
                inputActions.UI.HotBarSelector.performed += attributesManager.InventoryManager.OnHotBarSelection;
                inputActions.UI.Click.canceled += attributesManager.InventoryManager.OnClick;
                inputActions.UI.HotBarSelector.canceled += attributesManager.InventoryManager.OnHotBarSelection;
                UpdatePlayerStats();
            }
            

        }

            private void HandleInventory(InputAction.CallbackContext context) {

            if (context.performed) {

                ToggleInventory();

            }
        }

        private void ToggleInventory()
        {
            InventoryVisible = !InventoryVisible;
            if (InventoryVisible)
            {
                //attributesManager.InventoryManager.CraftTest();
                UpdatePlayerStats();
                attributesManager.InventoryManager.InventoryPanel.SetActive(true);
                attributesManager.InventoryManager.PlayerStatsPanel.SetActive(true);
                attributesManager.InventoryManager.CraftingPanel.SetActive(true);
                attributesManager.InventoryManager.Overlay.SetActive(true);
                attributesManager.InventoryManager.HotBar.SetActive(false);
                attributesManager.InventoryManager.HotBarSelector.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else {
                attributesManager.InventoryManager.InventoryPanel.SetActive(false);
                attributesManager.InventoryManager.PlayerStatsPanel.SetActive(false);
                attributesManager.InventoryManager.CraftingPanel.SetActive(false);
                attributesManager.InventoryManager.Overlay.SetActive(false);
                attributesManager.InventoryManager.HotBar.SetActive(true);
                attributesManager.InventoryManager.HotBarSelector.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false; 
            }
        }

        private void Start()
        {
            animator = GetComponent<Animator>();
            VelocityZHash = Animator.StringToHash("VelocityZ");
            VelocityXHash = Animator.StringToHash("VelocityX");
            isWalkingHash = Animator.StringToHash("isWalking");
            isIdleHash = Animator.StringToHash("isIdle");
            isRunningHash = Animator.StringToHash("isRunning");
            isJumpingHash = Animator.StringToHash("isJumping");

            if (idleTransform == null)
                idleTransform = transform;
                        
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            
            // HandleRotation();
            Movement();
            UpdateAnimatorParameters();
        }

        private void HandleJump(InputAction.CallbackContext context)
        {
            if (context.performed && isGrounded)
            {
                // Check if the player has enough stamina to jump
                float jumpStaminaCost = 5f; // Define the stamina cost for jumping
                if (attributesManager.UseStamina(jumpStaminaCost))
                {
                    // Player can jump
                    isJumping = true;
                    Jump();
                    animator.SetBool(isJumpingHash, isJumping);
                }
                else
                {
                    // Not enough stamina to jump
                    Debug.Log("Not enough stamina to jump.");
                }
            }
            else if (context.canceled)
            {
                // Don't set isJumping to false here - let OnLand handle that
                animator.SetBool(isJumpingHash, false);
            }
        }

        private void Jump()
        {
            rb.AddForce(new Vector3(0, jumpMovement.y), ForceMode.Impulse);
        }

        private void OnLand()
        {
            if (!isGrounded)
                return;

            // Reset jumping state
            isJumping = false;
            // Debug.Log($"Player landed. Setting isJumping to {isJumping}");
            animator.SetBool(isJumpingHash, false);
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

        private void UpdateAnimatorParameters(){
            float speed = Mathf.Abs(rb.velocity.x);
            forwardPressed = moveInput.y > 0;
            leftPressed = moveInput.x < 0;
            rightPressed = moveInput.x > 0;
            backwardPressed = moveInput.y < 0;

            isWalking = forwardPressed || leftPressed || rightPressed || backwardPressed;
            bool canRun = isGrounded && isSprinting && attributesManager.CurrentStamina > 5f;

            isRunning = isWalking && canRun;
            isIdle = !forwardPressed && !leftPressed && !rightPressed && !backwardPressed;

            // Debug.Log($"walking {isWalking}");
            animator.SetBool(isIdleHash, isIdle);
            animator.SetBool(isWalkingHash, isWalking);
            animator.SetBool(isRunningHash, isRunning);
            animator.SetBool(isIdleHash, isIdle);

            if (isGrounded && !wasGrounded)
            {
                OnLand();
                // Debug.Log("Player landed. Setting isJumping to false.");
            }
            else if (!isGrounded && wasGrounded)
            {
                Debug.Log("");
                
            }
            wasGrounded = isGrounded;
        }

        private int groundContacts = 0;

        private void OnCollisionStay(Collision collision)
        {

            bool foundValidGround = false;
            // Check if the collided object is on the ground layer
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y > 0.1f)  // Reduced threshold to be more forgiving
                {
                    foundValidGround = true;
                    groundContacts++;
                    break;  // Exit loop once we find valid ground
                }
            }
            
            if (foundValidGround)
            {
                isGrounded = true;
            }
            else
            {
                groundContacts = 0;
                isGrounded = false;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            groundContacts = Mathf.Max(groundContacts - 1, 0);
            if (groundContacts == 0)
            {
                isGrounded = false;
            }
        }

        private void HandleRotation()
        {
            if (cameraTransform == null || InventoryVisible) return;
            // Use look input for rotation
            float mouseX = lookInput.x * rotationSpeed * Time.deltaTime;
            targetRotation += mouseX * 60f; // Smooth rotation factor
            transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);
        }

        private void Movement(){
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

            if (isRunning && forwardPressed)
            {
                float staminaCost = attributesManager.StaminaCostPerSecond * Time.deltaTime;
                
                if (!attributesManager.UseStamina(staminaCost)){
                    isRunning = false;
                }
            }

            // Update the animator with the current velocities
            animator.SetFloat(VelocityZHash, velocityZ);
            animator.SetFloat(VelocityXHash, velocityX);
        }
        private int frameCount = 0;

        [SerializeField] private Rigidbody rb; // Rigidbody reference

        private void LateUpdate()
        {
            HandleRotation(); // Update camera and player rotation
        }

        private void FixedUpdate()
        {
            HandleMovement(); // Apply movement in FixedUpdate for physics consistency
        }
        
        [SerializeField] private CinemachineVirtualCamera virtualCamera; // Reference to the virtual camera

        private void HandleMovement()
        {
            if (InventoryVisible) return;

            // Calculate movement direction relative to the player's facing direction
            Vector3 moveDirection = (transform.forward * moveInput.y) + (transform.right * moveInput.x);
            moveDirection = moveDirection.normalized;

            // Apply movement to Rigidbody
            float movementSpeed = isRunning ? sprintMaxVelocityZ : maxVelocityZ;
            Vector3 targetPosition = rb.position + moveDirection * movementSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }


        // Called when the Move action is performed or canceled
        private void HandleMove(InputAction.CallbackContext context)
        {
            if (InventoryVisible)
            {
                return;
            }
            moveInput = context.ReadValue<Vector2>();
        }

        private void HandleLook(InputAction.CallbackContext context){

            if (InventoryVisible)
            {
                return;
            }
            
            lookInput = context.ReadValue<Vector2>();
            print(lookInput);
        }

        private void HandlePause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                PauseVisible = !PauseVisible;
                if (PauseVisible)
                {
                    // Enable the pause menu UI
                    pauseMenuPanel.SetActive(true);
                    // Freeze the game by setting time scale to 0
                    Time.timeScale = 0f;
                    // Unlock and show the cursor for UI interaction
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    // Disable the pause menu UI
                    pauseMenuPanel.SetActive(false);
                    // Resume the game by setting time scale to 1
                    Time.timeScale = 1f;
                    // Lock and hide the cursor to return to gameplay
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }
}
