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
        private PlayerAttributesManager attributesManager;

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
                $"<align=left>Base Damage:<line-height=0>\n<align=right>{attributesManager.BaseDamage}<line-height=1em>\n" +
                $"<align=left>Critical Hit Chance:<line-height=0>\n<align=right>{attributesManager.CriticalHitChance}<line-height=1em>\n" +
                $"<align=left>Critical Hit Multiplier:<line-height=0>\n<align=right>{attributesManager.CriticalHitMultiplier}<line-height=1em>\n" +
                $"<align=left>Attack Speed:<line-height=0>\n<align=right>{attributesManager.AttackSpeed}<line-height=1em>\n" +
                $"<align=left>Armor:<line-height=0>\n<align=right>{attributesManager.Armor}<line-height=1em>\n" +
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
        [SerializeField] private float verticalRotationSpeed = 180f;
        [SerializeField] private float smoothRotationTime = 0.05f;
        [SerializeField] private float inputSmoothTime = 0.02f;
        [SerializeField] private float mouseSensitivity = 2.0f;
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
            }
            else
            {

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            attributesManager.SkillTreeManager.ToggleSkillTree();
        }

        #endregion
        private CinemachinePOV mainCamPov;  // reference to your main camera's POV
        public CinemachineVirtualCamera mainVC;

// e.g. in Awake/Start:

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

            inputActions.Player.Inventory.performed += HandleInventory;
            inputActions.Player.Inventory.canceled += HandleInventory;
            inputActions.Player.Pause.performed += HandlePause;
            inputActions.Player.Pause.canceled += HandlePause;

            inputActions.Player.LockOn.performed += HandleLockOn;
            inputActions.Player.LockOn.canceled += HandleLockOn;

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
                inputActions.UI.Click.performed += attributesManager.InventoryManager.OnClick;
                inputActions.UI.HotBarSelector.performed += attributesManager.InventoryManager.OnHotBarSelection;
                inputActions.UI.Click.canceled += attributesManager.InventoryManager.OnClick;
                inputActions.UI.HotBarSelector.canceled += attributesManager.InventoryManager.OnHotBarSelection;
                inputActions.Player.Objective.performed += onObjective;
                inputActions.Player.Objective.canceled += onObjective;
                inputActions.UI.Skills.performed += HandleSkills;
                inputActions.UI.Skills.canceled += HandleSkills;

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
            }
            else {
                
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            attributesManager.ObjectiveManager.ToggleObjectivePanel();
        }
        private void HandleInventory(InputAction.CallbackContext context) {

            if (context.performed) {

                ToggleInventory();

            }
        }

        private void HandleLockOn(InputAction.CallbackContext context) {
            if (context.performed) {
                playerLockOn.LockOn();
            } else {
                playerLockOn.LockOn();
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
        }

        private void Update()
        {
            
            // HandleRotation();
            Movement();
            UpdateAnimatorParameters();
            UpdateMovementState();
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
                Vector3 horizontalDirection = playerLockOn.CurrentTarget.transform.position - transform.position;
                horizontalDirection.y = 0; // Rotate player horizontally
                if (horizontalDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(horizontalDirection, Vector3.up);
                }

                // Rotate the camera to look directly at the target (with pitch)
                Vector3 fullDirection = playerLockOn.CurrentTarget.transform.position - cameraTransform.position;
                if (fullDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion worldRotation = Quaternion.LookRotation(fullDirection, Vector3.up);
                    
                    // Override Cinemachine camera rotation
                    OverrideCinemachineRotation(playerLockOn.mainVC, worldRotation);

                    // Update local rotation for cameraTransform
                    cameraTransform.localRotation = Quaternion.Inverse(transform.rotation) * worldRotation;

                    // Print the transform once after locking
                    if (!hasPrintedTransformAfterLocking)
                    {
                        print($"Transform after locking: {playerLockOn.mainVC.transform.localRotation}");
                        hasPrintedTransformAfterLocking = true; // Ensure it's printed only once
                    }
                }

                wasLocked = true;
            }
            else
            {
                // --- Transition Logic ---
                if (wasLocked)
                {
                    // Step 1: Capture and Apply Final Lock-On Rotation
                    Quaternion lockOnFinalRotation = playerLockOn.lockOnCamera.transform.rotation;

                    // Override Cinemachine camera rotation to the final lock-on rotation
                    OverrideCinemachineRotation(playerLockOn.mainVC, lockOnFinalRotation);

                    // Apply the lock-on camera's final world rotation as local rotation
                    cameraTransform.localRotation = Quaternion.Inverse(transform.rotation) * lockOnFinalRotation;

                    print("Captured lock-on final rotation: " + lockOnFinalRotation);
                    print("Lock-on world rotation: " + playerLockOn.lockOnCamera.transform.rotation);
                    print("Lock-on local rotation: " + cameraTransform.localRotation);

                    // Optional: Smoothly blend to free-look
                    Quaternion targetRotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );

                    if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
                        wasLocked = false;
                    print($"Transform after unlocking: {playerLockOn.mainVC.transform.localRotation}");
                }

                // --- Free-Look Logic ---
                if (!wasLocked)
                {
                    hasPrintedTransformAfterLocking = false; // Reset the flag after leaving lock-on

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
            HandleMovement(); // your rb.MovePosition in here
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
                moveDirection = (transform.forward * moveInput.y) + (transform.right * moveInput.x);
                moveDirection.Normalize();
            }

            // Handle slope movement
            RaycastHit slopeHit;
            if (IsOnSlope(out slopeHit)) {
                moveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
            }

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

            if (InventoryVisible)
            {
                return;
            }
            
            lookInput = context.ReadValue<Vector2>();
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
        [SerializeField] private float dodgeForce = 2.5f; // Adjusted proportionally to fit the reduced distance
        [SerializeField] private float dodgeStaminaCost = 10f; // Optional: lower stamina cost if desired
        [SerializeField] private float dodgeCooldown = 0.4f; // Slightly shorter cooldown to match reduced dodge

        private bool canDodge = true; // Tracks whether the player can dodge

        private void HandleDodge(Vector3 direction, string animationHash)
        {
            // 1. Check if player can dodge at all
            if (!canDodge)
            {
                Debug.Log("Dodge is on cooldown!");
                return;
            }

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