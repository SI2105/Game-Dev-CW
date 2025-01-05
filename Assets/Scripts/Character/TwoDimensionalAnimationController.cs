using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using SlimUI.ModernMenu;

namespace SG{

    public class TwoDimensionalAnimationController : MonoBehaviour
    {
        #region Animator Variables
        private Animator animator;
        private Vector2 moveInput;
        private int VelocityZHash;
        private int VelocityXHash;
        private int isRunningHash;
        private int isWalkingHash;
        private int isIdleHash;
        private int isJumpingHash;
        private int isGroundedHash;
        private int RotationMismatchHash;
        private int isFallingHash;
        private int isDodgingBackwardHash;
        private int isDodgingForwardHash;
        private int isDodgingLeftHash;
        private int isDodgingRightHash;
        #endregion

        #region Player State Variables
        private bool isSprinting;
        private bool isWalking;
        private bool isIdle;
        private bool isJumping;
        private bool isRunning;
        private bool isDodgingForward;
        private bool isDodgingLeft;
        private bool isDodgingRight;
        private bool isDodgingBackward;
        private bool isDead;
        private PlayerState _playerState;
        #endregion

        #region Physics and Ground Detection
        public Transform groundCheck;
        public LayerMask groundLayer;
        private bool isGrounded;
        private bool wasGrounded;
        private Vector2 jumpMovement;
        public float maxSlopeAngle = 45f;
        #endregion

        #region Input Actions
        private GameDevCW inputActions;
        private bool forwardPressed = false;
        private bool backwardPressed = false;
        private bool leftPressed = false;
        private bool rightPressed = false;
        private bool InventoryVisible = false;
        private bool objectiveVisible = false;
        private bool PauseVisible = false;
        #endregion

        #region Velocity Variables
        private float velocityZ = 0.0f;
        private float velocityX = 0.0f;
        public float maxDownwardSpeed = 10f;
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

        #region Player Stats Display
        [SerializeField] private TextMeshProUGUI PlayerStatsText;
        public PlayerAttributesManager attributesManager;

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

                $"<align=left>Base Damage:<line-height=0>\n<align=right>{attributesManager.BaseDamage}<line-height=1em>\n" +
                $"<align=left>Critical Hit Chance:<line-height=0>\n<align=right>{attributesManager.CriticalHitChance}<line-height=1em>\n" +
                $"<align=left>Critical Hit Multiplier:<line-height=0>\n<align=right>{attributesManager.CriticalHitMultiplier}<line-height=1em>\n" +
                $"<align=left>Attack Speed:<line-height=0>\n<align=right>{attributesManager.AttackSpeed}<line-height=1em>\n" +
              
                $"<align=left>Block Chance:<line-height=0>\n<align=right>{attributesManager.BlockChance}<line-height=1em>\n" +
                $"<align=left>Dodge Chance:<line-height=0>\n<align=right>{attributesManager.DodgeChance}<line-height=1em>\n\n";
        }
        #endregion

        #region Movement Settings
        public float acceleration = 2f;
        public float deceleration = 2f;
        public float maxVelocityZ = 1.0f;
        public float maxVelocityX = 1.0f;
        public float sprintMaxVelocityZ = 2.0f;
        public float sprintMaxVelocityX = 2.0f;
        public float verticalVelocity = 0.1f;
        public float jumpSpeed = 1.0f;
        #endregion

        #region Camera Settings
        [Header("References")]
        [SerializeField] private Transform idleTransform;
        [SerializeField] private Transform cameraTransform;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 10f;
        public float verticalRotationSpeed { get; set; } = 180f;
        [SerializeField] private float smoothRotationTime = 0.05f;
        [SerializeField] private float inputSmoothTime = 0.02f;
        [SerializeField] public float mouseSensitivity  { get; set; } = 2.0f;
        public float RotationMismatch {get; private set;} = 0f;
        public bool IsRotatingToTarget {get; private set;} = false;
        public float rotateToTargetTime = 0.25f;
        private float _rotateToTargetTimer = 0f;
        #endregion

        #region UI and Miscellaneous
        [SerializeField] private GameObject pauseMenuPanel;
        public float movingThreashold = 0.01f;
        #endregion

        #region Slope Handling

        private bool IsOnSlope(out RaycastHit slopeHit) {
            float groundCheckRadius = 0.4f;
            if (Physics.SphereCast(transform.position + Vector3.up * groundCheckRadius, groundCheckRadius, Vector3.down, out slopeHit, 1.5f)) {
                float angle = Vector3.Angle(slopeHit.normal, Vector3.up);
                return angle > 0 && angle <= maxSlopeAngle;
            }
            slopeHit = default;
            return false;
        }

        private void AdjustGravityOnSlope() {
            if (IsOnSlope(out RaycastHit slopeHit)) {
                Vector3 gravityDirection = Vector3.ProjectOnPlane(Physics.gravity, slopeHit.normal);
                rb.AddForce(gravityDirection - Physics.gravity, ForceMode.Acceleration);
            } else {
                rb.AddForce(Physics.gravity, ForceMode.Acceleration);
            }
        }

        private void PreventSliding() {
            if (IsOnSlope(out RaycastHit slopeHit)) {
                Vector3 projectedVelocity = Vector3.ProjectOnPlane(rb.velocity, slopeHit.normal);
                if (projectedVelocity.magnitude > 0.05f) { // Sliding threshold
                    rb.velocity = new Vector3(0, rb.velocity.y, 0); // Stop all sliding
                }
            }
        }

        private void ClampVerticalVelocity() {
            if (rb.velocity.y < -maxDownwardSpeed) {
                rb.velocity = new Vector3(rb.velocity.x, -maxDownwardSpeed, rb.velocity.z);
            }
        }
        #endregion

        private PlayerLockOn playerLockOn;
        public CinemachineCameraSwitcher cameraSwitcher;


        #region Skills

        private bool isSkillTreeVisible = false;

        public void HandleSkills(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ToggleSkilltree();
            }


        }
        public void ToggleSkilltree()
        {
            isSkillTreeVisible = !isSkillTreeVisible;

            if (isSkillTreeVisible)
            {

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
            }
            else
            {

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
            }

            attributesManager.SkillTreeManager.ToggleSkillTree();
        }

        #endregion
        private CinemachinePOV mainCamPov;  // reference to your main camera's POV
        public CinemachineVirtualCamera mainVC;

        #region KeybindPage
        private GameObject KeybindsPanel;
        private void HandleKeybinds(InputAction.CallbackContext context)
        {
            // Toggle the visibility of the KeybindsPanel
            if (KeybindsPanel != null)
            {
                KeybindsPanel.SetActive(!KeybindsPanel.activeSelf);
            }
        }

        #endregion
        // e.g. in Awake/Start:

        private void Awake()
        {
            inputActions = new GameDevCW();

            KeybindsPanel =GameObject.Find("KeybindsPanel");
            KeybindsPanel.SetActive(false);
            
            // Subscribe to the Move action's performed and canceled events
            inputActions.Player.Move.performed += HandleMove;
            inputActions.Player.Move.canceled += HandleMove;

            inputActions.Player.Look.performed += HandleLook;
            inputActions.Player.Look.canceled += HandleLook;

            // Subscribe to the Sprint action's performed and canceled events
            inputActions.Player.Sprint.performed += ctx => isSprinting = true;
            inputActions.Player.Sprint.canceled += ctx => isSprinting = false;

            inputActions.Player.Inventory.performed += HandleInventory;
            inputActions.Player.Inventory.canceled += HandleInventory;
            inputActions.Player.Pause.performed += HandlePause;
            inputActions.Player.Pause.canceled += HandlePause;

            inputActions.Player.LockOn.performed += HandleLockOn;

            inputActions.Player.DodgeForward.performed += HandleDodgeForward;
            inputActions.Player.DodgeBackward.performed += HandleDodgeBackward;
            inputActions.Player.DodgeLeft.performed += HandleDodgeLeft;
            inputActions.Player.DodgeRight.performed += HandleDodgeRight;

            attributesManager = GetComponent<PlayerAttributesManager>();
            _playerState = GetComponent<PlayerState>();
            playerLockOn = GetComponent<PlayerLockOn>();
            mainCamPov = mainVC.GetCinemachineComponent<CinemachinePOV>();

            if (attributesManager) {
                Debug.Log("Inventory Manager found");
                inputActions.UI.HotBarSelector.performed += attributesManager.InventoryManager.OnHotBarSelection;
                inputActions.UI.HotBarSelector.canceled += attributesManager.InventoryManager.OnHotBarSelection;
                inputActions.Player.Objective.performed += onObjective;
                inputActions.Player.Objective.canceled += onObjective;
                inputActions.UI.Skills.performed += HandleSkills;
                inputActions.UI.Skills.canceled += HandleSkills;
                inputActions.UI.Keybinds.performed += HandleKeybinds;
                inputActions.UI.Keybinds.canceled += HandleKeybinds;
                UpdatePlayerStats();
            }
        }



        public void onObjective(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                ToggleObjective();
            }


        }

        public void ToggleObjective() {
            objectiveVisible = !objectiveVisible;

            if (objectiveVisible)
            {
               
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
            }
            else {
                
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
            }

            attributesManager.ObjectiveManager.ToggleObjectivePanel();
        }
        private void HandleInventory(InputAction.CallbackContext context) {

            if (context.performed) {

                ToggleInventory();

            }
        }
        private bool isLockedOn = false;
        private void HandleLockOn(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (isLockedOn)
                {
                    // If already locked on, cancel lock-on
                    playerLockOn.LockOn();
                    isLockedOn = false;
                    Debug.Log("Lock-on canceled.");
                }
                else
                {
                    // If not locked on, activate lock-on
                    playerLockOn.LockOn();
                    isLockedOn = true;
                    Debug.Log("Lock-on activated.");
                }
            }
        }

        private void ToggleInventory()
        {
            InventoryVisible = !InventoryVisible;
            if (InventoryVisible)
            {
                inputActions.UI.Click.performed += attributesManager.InventoryManager.OnClick;
                //attributesManager.InventoryManager.CraftTest();
                inputActions.UI.Click.performed += attributesManager.InventoryManager.OnClick;
                inputActions.UI.Click.canceled += attributesManager.InventoryManager.OnClick;
                UpdatePlayerStats();
                attributesManager.InventoryManager.InventoryPanel.SetActive(true);
                attributesManager.InventoryManager.PlayerStatsPanel.SetActive(true);
                attributesManager.InventoryManager.CraftingPanel.SetActive(true);
                attributesManager.InventoryManager.Overlay.SetActive(true);
                attributesManager.InventoryManager.HotBar.SetActive(false);
                attributesManager.InventoryManager.HotBarSelector.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
            }
            else {
                
                inputActions.UI.Click.performed -= attributesManager.InventoryManager.OnClick;
                inputActions.UI.Click.canceled -= attributesManager.InventoryManager.OnClick;
                attributesManager.InventoryManager.InventoryPanel.SetActive(false);
                attributesManager.InventoryManager.PlayerStatsPanel.SetActive(false);
                attributesManager.InventoryManager.CraftingPanel.SetActive(false);
                attributesManager.InventoryManager.Overlay.SetActive(false);
                attributesManager.InventoryManager.HotBar.SetActive(true);
                attributesManager.InventoryManager.HotBarSelector.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
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
            isGroundedHash = Animator.StringToHash("isGrounded");
            isFallingHash = Animator.StringToHash("isFalling");
            RotationMismatchHash = Animator.StringToHash("rotationMismatch");
            isDodgingBackwardHash = Animator.StringToHash("isDodgingBackward");
            isDodgingForwardHash = Animator.StringToHash("isDodgingForward");
            isDodgingLeftHash = Animator.StringToHash("isDodgingLeft");
            isDodgingRightHash = Animator.StringToHash("isDodgingRight");

            if (idleTransform == null)
                idleTransform = transform;
                        
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            cameraSwitcher = FindObjectOfType<CinemachineCameraSwitcher>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            LoadGameDifficulty();
            SetGameParametersBasedOnDifficulty();
        }

        private string currentDifficulty;

        private void LoadGameDifficulty()
        {
            if (PlayerPrefs.GetInt("EasyDifficulty") == 1)
            {
                currentDifficulty = "Easy";
            }
            else if (PlayerPrefs.GetInt("NormalDifficulty") == 1)
            {
                currentDifficulty = "Normal";
            }
            else if (PlayerPrefs.GetInt("AdvancedDifficulty") == 1)
            {
                currentDifficulty = "Advanced";
            }
            else if (PlayerPrefs.GetInt("HardCoreDifficulty") == 1)
            {
                currentDifficulty = "Hardcore";
            }
            else
            {
                currentDifficulty = "Unknown"; // Default or fallback value
            }
        }

        private void SetGameParametersBasedOnDifficulty()
        {
            switch (currentDifficulty)
            {
                case "Easy":
                    Debug.Log("Setting game parameters for Easy difficulty...");
                    // Set parameters for Easy difficulty
                    break;

                case "Normal":
                    Debug.Log("Setting game parameters for Normal difficulty...");
                    // Set parameters for Normal difficulty
                    break;

                case "Advanced":
                    Debug.Log("Setting game parameters for Advanced difficulty...");
                    // Set parameters for Advanced difficulty
                    break;

                case "Hardcore":
                    Debug.Log("Setting game parameters for Hardcore difficulty...");
                    // Set parameters for Hardcore difficulty
                    break;

                default:
                    Debug.LogWarning("No valid difficulty found. Default settings applied.");
                    // Apply default or fallback parameters
                    break;
            }
        }

        private void Update()
        {
            
            // HandleRotation();
            Movement();
            UpdateAnimatorParameters();
            UpdateMovementState();
            if (checkIsDead())
            {
                if (!isDead)
                {
                    isDead = true;
                    OnPlayerDeath();
                }
            }
        }
        private void OnPlayerDeath()
        {

            // Reset input states
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            isSprinting = false;
            forwardPressed = false;
            backwardPressed = false;
            leftPressed = false;
            rightPressed = false;
            InventoryVisible = false;
            objectiveVisible = false;
            PauseVisible = false;
            isLockedOn = false;

            // Reset Animator parameters (optional)
            animator.SetBool(isIdleHash, true);
            animator.SetBool(isWalkingHash, false);
            animator.SetBool(isRunningHash, false);
            animator.SetBool(isJumpingHash, false);
            animator.SetBool(isDodgingForwardHash, false);
            animator.SetBool(isDodgingBackwardHash, false);
            animator.SetBool(isDodgingLeftHash, false);
            animator.SetBool(isDodgingRightHash, false);

            // Unsubscribe from input actions
            inputActions.Player.Move.performed -= HandleMove;
            inputActions.Player.Move.canceled -= HandleMove;
            inputActions.Player.Look.performed -= HandleLook;
            inputActions.Player.Look.canceled -= HandleLook;
            inputActions.Player.Sprint.performed -= ctx => isSprinting = true;
            inputActions.Player.Sprint.canceled -= ctx => isSprinting = false;
            inputActions.Player.Inventory.performed -= HandleInventory;
            inputActions.Player.Inventory.canceled -= HandleInventory;
            inputActions.Player.Pause.performed -= HandlePause;
            inputActions.Player.Pause.canceled -= HandlePause;
            inputActions.Player.LockOn.performed -= HandleLockOn;
            inputActions.Player.DodgeForward.performed -= HandleDodgeForward;
            inputActions.Player.DodgeBackward.performed -= HandleDodgeBackward;
            inputActions.Player.DodgeLeft.performed -= HandleDodgeLeft;
            inputActions.Player.DodgeRight.performed -= HandleDodgeRight;

            if (attributesManager)
            {
                inputActions.UI.HotBarSelector.performed -= attributesManager.InventoryManager.OnHotBarSelection;
                inputActions.UI.HotBarSelector.canceled -= attributesManager.InventoryManager.OnHotBarSelection;
                inputActions.Player.Objective.performed -= onObjective;
                inputActions.Player.Objective.canceled -= onObjective;
                inputActions.UI.Skills.performed -= HandleSkills;
                inputActions.UI.Skills.canceled -= HandleSkills;
            }

            var inventoryManager = attributesManager.InventoryManager;
            var objectiveManager = attributesManager.ObjectiveManager;

            GameManager.Instance.HandlePlayerDeath(attributesManager, inventoryManager, objectiveManager);


            // Disable the input system
            // inputActions.Disable();
        }

        private void OnPlayerRespawn()
        {
            Debug.Log("Player respawned. Re-enabling inputs and resetting variables.");

            // Reset player states
            isDead = false;
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            isSprinting = false;
            forwardPressed = false;
            backwardPressed = false;
            leftPressed = false;
            rightPressed = false;
            InventoryVisible = false;
            objectiveVisible = false;
            PauseVisible = false;
            isLockedOn = false;

            // Reset Animator parameters
            animator.SetBool(isIdleHash, true);
            animator.SetBool(isWalkingHash, false);
            animator.SetBool(isRunningHash, false);
            animator.SetBool(isJumpingHash, false);
            animator.SetBool(isDodgingForwardHash, false);
            animator.SetBool(isDodgingBackwardHash, false);
            animator.SetBool(isDodgingLeftHash, false);
            animator.SetBool(isDodgingRightHash, false);

            // Re-subscribe to input actions
            inputActions.Player.Move.performed += HandleMove;
            inputActions.Player.Move.canceled += HandleMove;
            inputActions.Player.Look.performed += HandleLook;
            inputActions.Player.Look.canceled += HandleLook;
            inputActions.Player.Sprint.performed += ctx => isSprinting = true;
            inputActions.Player.Sprint.canceled += ctx => isSprinting = false;
            inputActions.Player.Inventory.performed += HandleInventory;
            inputActions.Player.Inventory.canceled += HandleInventory;
            inputActions.Player.Pause.performed += HandlePause;
            inputActions.Player.Pause.canceled += HandlePause;
            inputActions.Player.LockOn.performed += HandleLockOn;
            inputActions.Player.DodgeForward.performed += HandleDodgeForward;
            inputActions.Player.DodgeBackward.performed += HandleDodgeBackward;
            inputActions.Player.DodgeLeft.performed += HandleDodgeLeft;
            inputActions.Player.DodgeRight.performed += HandleDodgeRight;

            if (attributesManager)
            {
                inputActions.UI.HotBarSelector.performed += attributesManager.InventoryManager.OnHotBarSelection;
                inputActions.UI.HotBarSelector.canceled += attributesManager.InventoryManager.OnHotBarSelection;
                inputActions.Player.Objective.performed += onObjective;
                inputActions.Player.Objective.canceled += onObjective;
                inputActions.UI.Skills.performed += HandleSkills;
                inputActions.UI.Skills.canceled += HandleSkills;
            }

            // Re-enable the input system
            inputActions.Enable();

            // Reset any player-specific states or attributes
            _playerState.SetPlayerMovementState(PlayerMovementState.Idling);

            Debug.Log("Inputs re-enabled and player state reset.");
        }



        private bool checkIsDead(){
            return attributesManager.CurrentHealth <= 0f;
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

        private void UpdateAnimatorParameters() {
            forwardPressed = moveInput.y > 0;
            leftPressed = moveInput.x < 0;
            rightPressed = moveInput.x > 0;
            backwardPressed = moveInput.y < 0;

            isWalking = forwardPressed || leftPressed || rightPressed || backwardPressed;
            bool canRun = isSprinting && attributesManager.CurrentStamina > 5f;
            isRunning = isWalking && canRun;
            isIdle = !forwardPressed && !leftPressed && !rightPressed && !backwardPressed;

            animator.SetBool(isIdleHash, isIdle);
            animator.SetBool(isWalkingHash, isWalking);
            animator.SetBool(isRunningHash, isRunning);
        }

        private void UpdateMovementState(){
            bool isMoveInput = moveInput != Vector2.zero;
            bool isMovingLateral = IsMovingLateral();
            PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Running : 
                                               isMovingLateral || isMoveInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            _playerState.SetPlayerMovementState(lateralState);

            if (!isGrounded && rb.velocity.y > 0f){
                _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
            }
        }

        private bool IsMovingLateral(){
            Vector3 lateralVelocity = new Vector3(velocityX, 0f, velocityZ);
            return lateralVelocity.magnitude > 0;
        }

        private int groundContacts = 0;

        private void CheckGrounded()
        {
            // Use a small radius for the sphere cast
            float groundCheckRadius = 0.4f;
            // Use a small distance for the check
            float groundCheckDistance = 0.5f;
            
            // Perform a SphereCast from slightly above the groundCheck position
            isGrounded = Physics.SphereCast(
                groundCheck.position + Vector3.up * groundCheckRadius, 
                groundCheckRadius,
                Vector3.down, 
                out RaycastHit hit,
                groundCheckDistance,
                groundLayer
            );
        }

        private bool wasLocked; // Tracks if the player was previously locked on
        [SerializeField] private float verticalClampMin = -80f;
        [SerializeField] private float verticalClampMax = 80f;
        [SerializeField] private float lockOnRotationSpeed = 5f;
        // Tracks the current yaw and pitch
        private float currentYaw;
        private float currentPitch;
        private float oldPitch;
        private float oldYaw;
        // Add this variable at the top of the class to track the lock-on state change


        private bool hasPrintedTransformAfterLocking = false;

       private void HandleRotation()
        {
            // Safety check
            if (cameraTransform == null || InventoryVisible) 
                return;

            if (playerLockOn != null && playerLockOn.IsTargetLocked && playerLockOn.CurrentTarget != null)
            {
                // --- Lock-On Logic ---
                // Find the UpperHalfPoint child transform
                Transform upperHalfTransform = playerLockOn.CurrentTarget.transform.Find("UpperHalfPoint");
                if (upperHalfTransform != null)
                {
                    // Set the camera's LookAt to the UpperHalfPoint
                    playerLockOn.lockOnCamera.LookAt = upperHalfTransform;
                }
                else
                {
                    // Fallback: Calculate an upper half position
                    Vector3 upperHalfPosition = GetUpperHalfPosition(playerLockOn.CurrentTarget);
                    playerLockOn.lockOnCamera.LookAt = CreateTemporaryTarget(upperHalfPosition);
                }

                // Optional: Smoothly rotate the player to face the target
                // If you still want the player to face the target without snapping
                Vector3 directionToTarget = playerLockOn.CurrentTarget.transform.position - transform.position;
                directionToTarget.y = 0f; // Ignore vertical difference
                if (directionToTarget != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
                    transform.rotation = targetRotation;
                }

                wasLocked = true;
            }
            else
            {
                // --- Transition Logic ---
                if (wasLocked)
                {
                    // Unlock the target
                    wasLocked = false;
                    if (playerLockOn.lockOnCamera != null)
                    {
                        playerLockOn.lockOnCamera.Priority = 0;
                        mainVC.Priority = 60;
                        playerLockOn.lockOnCamera.LookAt = null; // Reset LookAt
                    }
                    Debug.Log("Target unlocked.");
                }

                // --- Free-Look Logic ---
                if (!wasLocked)
                {
                    hasPrintedTransformAfterLocking = false;

                    float mouseX = lookInput.x * rotationSpeed * Time.deltaTime;
                    currentYaw += mouseX * 60f; 
                    transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

                    float mouseY = lookInput.y * rotationSpeed * Time.deltaTime;
                    currentPitch -= mouseY * 60f;
                    currentPitch = Mathf.Clamp(currentPitch, verticalClampMin, verticalClampMax);

                    cameraTransform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
                }
            }
        }

        /// <summary>
        /// Calculates an upper half position based on the target's Renderer bounds.
        /// </summary>
        /// <param name="target">The target GameObject.</param>
        /// <returns>The upper half position.</returns>
        private Vector3 GetUpperHalfPosition(GameObject target)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.center + Vector3.up * (renderer.bounds.size.y / 4);
            }
            else
            {
                // Fallback position if Renderer is not found
                return target.transform.position + Vector3.up * 1.0f;
            }
        }

        /// <summary>
        /// Creates a temporary GameObject at the specified position for the camera to look at.
        /// </summary>
        /// <param name="position">The position to place the temporary target.</param>
        /// <returns>The Transform of the temporary target.</returns>
        private Transform CreateTemporaryTarget(Vector3 position)
        {
            GameObject tempTarget = new GameObject("TempLookAtTarget");
            tempTarget.transform.position = position;
            Destroy(tempTarget, 2f); // Destroy after 2 seconds or when no longer needed
            return tempTarget.transform;
        }


        private void OverrideCinemachineRotation(CinemachineVirtualCamera vCam, Quaternion newRotation, bool reattach = true)
        {
            // Temporarily detach LookAt and Follow
            Transform originalLookAt = vCam.LookAt;
            Transform originalFollow = vCam.Follow;

            vCam.LookAt = null;
            vCam.Follow = originalFollow;

            // Manually set the rotation
            vCam.enabled = false;
            // Set the rotation
            vCam.transform.rotation = newRotation;
            // Re-enable
            vCam.enabled = true;

            // Optionally reattach LookAt and Follow targets
            if (reattach)
            {
                vCam.Follow = originalFollow;
                vCam.LookAt = originalLookAt;

                // Reapply the rotation after reattaching
                vCam.transform.rotation = newRotation;
            }
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
            CheckGrounded();
            ClampVerticalVelocity();
            PreventSliding();
            Movement();
            HandleMovement();
            
        }

        
        [SerializeField] private CinemachineVirtualCamera virtualCamera; // Reference to the virtual camera

        private void HandleMovement() {
            if (InventoryVisible) return;

            Vector3 moveDirection;
            
            // Check if we have an active lock-on target
            if (playerLockOn != null && playerLockOn.IsTargetLocked && playerLockOn.CurrentTarget != null) {
                // Get direction to target
                Vector3 targetDirection = (playerLockOn.CurrentTarget.transform.position - transform.position).normalized;
                targetDirection.y = 0; // Keep movement on the horizontal plane
                
                // Calculate movement direction relative to target
                Vector3 forward = targetDirection;
                Vector3 right = Vector3.Cross(Vector3.up, forward);
                
                // Combine input with target-relative directions
                moveDirection = (forward * moveInput.y) + (right * moveInput.x);
                moveDirection.Normalize();
            } else {
                // Regular movement using player's transform when not locked on
                Vector3 forward = transform.forward;
                Vector3 right = transform.right;

                // Combine input with player's facing directions
                moveDirection = (forward * moveInput.y) + (right * moveInput.x);

                // Normalize the direction to prevent overscaling
                if (moveDirection != Vector3.zero)
                    moveDirection.Normalize();
            }

            // Handle slope movement
            // RaycastHit slopeHit;
            // if (IsOnSlope(out slopeHit)) {
            //     moveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
            // }

            float movementSpeed = isRunning ? sprintMaxVelocityZ : maxVelocityZ;
            Vector3 targetPosition = rb.position + moveDirection * movementSpeed * Time.deltaTime;
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

            if (InventoryVisible || isSkillTreeVisible || objectiveVisible)
            {
                return;
            }
            
            // lookInput = context.ReadValue<Vector2>();
            // Read raw input
            Vector2 rawLookInput = context.ReadValue<Vector2>();

            // Apply mouse sensitivity from SettingsManager
            float sensitivityX = SettingsManager.Instance?.MouseSensitivityX ?? 1.0f; // Fallback to 1.0f if null
            float sensitivityY = SettingsManager.Instance?.MouseSensitivityY ?? 1.0f;

            // Apply mouse smoothing
            float smoothing = SettingsManager.Instance?.MouseSmoothing ?? 0.05f;

            // Smooth look input using Vector2.SmoothDamp
            smoothedLookInput = Vector2.SmoothDamp(
                smoothedLookInput,
                rawLookInput * new Vector2(sensitivityX, sensitivityY),
                ref lookInputVelocity,
                smoothing
            );

            lookInput = smoothedLookInput;
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

        [SerializeField] private float dodgeDistance = 0.75f; // Reduced by half (was 3.0f)
        [SerializeField] private float dodgeDuration = 0.075f; // Reduced by half (was 0.3f)
        [SerializeField] private float dodgeForce = 4f; // Adjusted proportionally to fit the reduced distance
        [SerializeField] private float dodgeStaminaCost = 1f; // Optional: lower stamina cost if desired
        [SerializeField] private float dodgeCooldown = 0.05f; // Slightly shorter cooldown to match reduced dodge

        private bool canDodge = true; // Tracks whether the player can dodge

        private void HandleDodge(Vector3 direction, string animationHash)
        {
            // 1. Check if player can dodge at all
            if (!canDodge)
            {
                Debug.Log("Dodge is on cooldown!");
                return;
            }
            // attributesManager.TakeDamage(20f);
            attributesManager.LevelUp();

            // 2. Check stamina
            if (attributesManager.CurrentStamina < dodgeStaminaCost)
            {
                Debug.Log("Not enough stamina to dodge!");
                return;
            }

            // 3. Player can dodge here
            attributesManager.UseStamina(dodgeStaminaCost);

            // 4. Mark that we’re now in dodge cooldown
            canDodge = false;

            // 5. Trigger dodge animation
            SetDodgeState(direction, animationHash, true);

            // 6. Perform the dodge
            StartCoroutine(DodgeRoutine(direction, animationHash));
        }

        private IEnumerator DodgeRoutine(Vector3 direction, string animationHash)
        {
            // Optional small delay before movement
            yield return new WaitForSeconds(0.1f);

            Vector3 startPosition = rb.position;
            Vector3 targetPosition = CalculateFinalPosition(startPosition, direction, dodgeDistance);
            float elapsedTime = 0f;

            while (elapsedTime < dodgeDuration)
            {
                rb.MovePosition(Vector3.Lerp(startPosition, targetPosition, elapsedTime / dodgeDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the final position is exact
            rb.MovePosition(targetPosition);

            // Reset dodge state
            SetDodgeState(direction, animationHash, false);

            // 7. After the dodge completes, wait for the remaining cooldown
            yield return new WaitForSeconds(dodgeCooldown);

            // 8. Re-enable dodging
            canDodge = true;
        }

        private Vector3 CalculateFinalPosition(Vector3 startPosition, Vector3 direction, float distance)
        {
            Vector3 targetPosition = startPosition + direction.normalized * distance;

            // Optional: use raycast for obstacles
            if (Physics.Raycast(startPosition, direction, out RaycastHit hit, distance))
            {
                targetPosition = hit.point - direction.normalized * 0.1f;
            }

            return targetPosition;
        }

        private void SetDodgeState(Vector3 direction, string animationHash, bool state)
        {
            if (direction == transform.forward)      isDodgingForward  = state;
            if (direction == -transform.forward)     isDodgingBackward = state;
            if (direction == transform.right)        isDodgingRight    = state;
            if (direction == -transform.right)       isDodgingLeft     = state;

            animator.SetBool(animationHash, state);
        }

        // Individual dodge handlers
        private void HandleDodgeForward(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                HandleDodge(transform.forward, "isDodgingForward");
            }
        }

        private void HandleDodgeBackward(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                HandleDodge(-transform.forward, "isDodgingBackward");
            }
        }

        private void HandleDodgeLeft(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                HandleDodge(-transform.right, "isDodgingLeft");
            }
        }

        private void HandleDodgeRight(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                HandleDodge(transform.right, "isDodgingRight");
            }
        }

    }
}