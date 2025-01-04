using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace SG
{
    public class PlayerComboManager : MonoBehaviour
    {
        // Animator and Input
        private Animator animator;
        private GameDevCW inputActions;

        // Animator Hashes
        private static int isBlockingHash;
        private static int BlockHash;
        private static int spinningAttackHash;
        private static int comboResetTriggerHash;
        private static int attackTriggerHash;
        private static int comboIndexHash;
        private static int isPlayingActionHash;
        private static int isAttackingHash;

        // Combo Variables
        [SerializeField] private float comboResetTime = 1f;       // Time to reset combo if no input
        [SerializeField] private float comboWindowTime = 0.4f;    // Time window to accept next combo input
        [SerializeField] private int maxComboSteps = 2;           // Maximum number of combo steps

        private bool isComboActive = false;                      // Is a combo currently active
        private float comboTimer = 0f;                           // Timer to track combo window
        public int currentComboStep = 0;                        // Current step in the combo

        [SerializeField] private float staminaCostPerAttack = 10f;
        [SerializeField] private float staminaCostSpinAttack = 20f;

        // Player State and Components
        [SerializeField] private PlayerState playerState;
        [SerializeField] private PlayerAttributesManager attributesManager;
        [SerializeField] private PlayerEquipmentManager playerEquipmentManager;
        [SerializeField] private TwoDimensionalAnimationController playerController;
        [SerializeField] private PlayerLockOn playerLockOn;

        // Stamina and Lockout Variables
        private int staminaUsageCount = 0;
        private bool isBlocking = false;
        private bool isSpinAttackActive = false;
        private bool powerUpLocked = true;
        private bool twoHandClubLocked = true;
        private bool isPowerUpOnCooldown = false;

        // Input Queueing Variables
        private bool hasQueuedAttack = false;
        private float queueTimer = 0f;
        [SerializeField] private float maxQueueTime = 0.9f; // How long to remember queued input

        // Camera Manager Reference
        [Header("Camera Settings")]
        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private Cinemachine.CinemachineVirtualCamera defaultCamera;
        [SerializeField] private Cinemachine.CinemachineVirtualCamera attackCamera;

        private void Awake()
        {
            // Initialize components
            animator = GetComponent<Animator>();
            inputActions = new GameDevCW();
            attributesManager = GetComponent<PlayerAttributesManager>();
            playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
            playerController = GetComponent<TwoDimensionalAnimationController>();
            playerLockOn = GetComponent<PlayerLockOn>();
            playerState = GetComponent<PlayerState>();

            // Initialize animator hashes
            isBlockingHash = Animator.StringToHash("isBlocking");
            BlockHash = Animator.StringToHash("Block");
            spinningAttackHash = Animator.StringToHash("isSpinningAttack");
            comboResetTriggerHash = Animator.StringToHash("ResetCombo");
            attackTriggerHash = Animator.StringToHash("Attack");
            comboIndexHash = Animator.StringToHash("ComboIndex");
            isPlayingActionHash = Animator.StringToHash("isPlayingAction");
            isAttackingHash = Animator.StringToHash("isAttacking");

            // Setup input callbacks
            inputActions.Player.Attack.performed += _ => OnAttackInput();
            inputActions.Player.Block.performed += HandleBlockAndHeal;
            inputActions.Player.Block.canceled += HandleBlockAndHeal;
            inputActions.Player.SpinAttack.performed += HandleSpinAttack;
            inputActions.Player.TwoHandAttack.performed += HandleTwoHandAttack;
            inputActions.Player.PowerUp2.performed += HandlePowerUp2;
        }

        private void OnEnable()
        {
            inputActions.Enable();
            Skill.OnSkillUnlocked += Skill_OnSkillUnlocked;
        }

        private void OnDisable()
        {
            inputActions.Disable();
            Skill.OnSkillUnlocked -= Skill_OnSkillUnlocked;
        }

        /// <summary>
        /// Handles the attack input, managing single attacks and combo chains.
        /// </summary>
        private void OnAttackInput()
        {
            if (isSpinAttackActive)
            {
                Debug.Log("Attack input ignored due to active spin attack.");
                return;
            }

            if (isComboActive && comboTimer < comboWindowTime && currentComboStep < maxComboSteps)
            {
                // Continue the combo
                ContinueCombo();
            }
            else
            {
                // Start a new attack
                StartAttack();
            }
        }

        /// <summary>
        /// Starts a new attack or the first step of a combo.
        /// </summary>
        private void StartAttack()
        {
            isComboActive = true;
            currentComboStep = 0;
            comboTimer = 0f;
            staminaUsageCount = 1;

            // Update player state
            playerState.SetPlayerAttackState(PlayerAttackState.Attacking);
            playerState.SetPlayerAttackStatusState(PlayerAttackStatusState.InProgress);

            // Trigger attack animation
            animator.SetInteger(comboIndexHash, currentComboStep);
            animator.SetTrigger(attackTriggerHash);
            animator.SetBool(isAttackingHash, true);
            animator.SetBool(isPlayingActionHash, true);

            // Apply damage or other attack logic
            Hit();

            Debug.Log($"Attack started. Combo Step: {currentComboStep}");
        }

        /// <summary>
        /// Continues an ongoing combo by advancing to the next step.
        /// </summary>
        private void ContinueCombo()
        {
            if (currentComboStep >= 2) return;
            currentComboStep++;
            comboTimer = 0f;
            staminaUsageCount++;

            // Trigger next attack animation
            animator.SetInteger(comboIndexHash, currentComboStep);
            animator.SetTrigger(attackTriggerHash);
            animator.SetBool(isAttackingHash, true);
            animator.SetBool(isPlayingActionHash, true);

            // Apply damage or other attack logic
            Hit();

            Debug.Log($"Combo continued. Combo Step: {currentComboStep}");
        }

        /// <summary>
        /// Resets the combo system to idle state.
        /// </summary>
        private void ResetCombo()
        {
            isComboActive = false;
            currentComboStep = 0;
            comboTimer = 0f;
            staminaUsageCount = 0;

            // Reset player state
            playerState.SetPlayerAttackState(PlayerAttackState.Idling);
            playerState.SetPlayerAttackStatusState(PlayerAttackStatusState.Idling);

            // Reset animator parameters
            animator.SetInteger(comboIndexHash, 0);
            animator.ResetTrigger(attackTriggerHash);
            animator.SetTrigger(comboResetTriggerHash);
            animator.SetBool(isAttackingHash, false);
            animator.SetBool(isPlayingActionHash, false);

            Debug.Log("Combo reset to idle.");
        }

        /// <summary>
        /// Performs the attack logic, such as applying damage.
        /// </summary>
        private void Hit()
        {
            object equippedItem = EquippedItem();

            if (equippedItem is WeaponClass weapon)
            {
                float weaponDamage = weapon.damage;
                WeaponCollisionHandler collisionHandler = playerEquipmentManager.rightHandWeaponModel.GetComponent<WeaponCollisionHandler>();
                attributesManager.UseStamina(10f);
                if (collisionHandler != null)
                {
                    collisionHandler.SetTemporaryDamage(weaponDamage);
                    collisionHandler.SetCurrentComboStep(currentComboStep);
                    collisionHandler.TriggerAttack(); // Ensure you have a method to trigger the collider
                    
                    Debug.Log($"Weapon collider ready with damage: {weaponDamage}");
                }
                else
                {
                    Debug.LogWarning("WeaponCollisionHandler not found on equipped weapon model.");
                }
            }
            else if (equippedItem is ConsumableClass consumable)
            {
                float consumableDamage = consumable.damage;
                Debug.Log($"Using consumable with damage: {consumableDamage}");
                // Handle consumable usage
            }
            else
            {
                Debug.LogWarning("No valid item equipped.");
            }
        }

        /// <summary>
        /// Retrieves the currently equipped item.
        /// </summary>
        /// <returns>The equipped item as an object.</returns>
        private object EquippedItem()
        {
            return playerEquipmentManager.TrackEquippedItem<object>();
        }

        /// <summary>
        /// Updates the combo timer and handles combo reset if necessary.
        /// </summary>
        private void Update()
        {
            if (isComboActive)
            {
                comboTimer += Time.deltaTime;

                if (comboTimer >= comboResetTime)
                {
                    ResetCombo();
                }
            }

            // Manage queued attacks
            if (hasQueuedAttack)
            {
                queueTimer += Time.deltaTime;
                if (queueTimer > maxQueueTime)
                {
                    hasQueuedAttack = false;
                    queueTimer = 0f;
                }
                else
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    bool canAttackNow = !stateInfo.IsName("Attack") ||
                                        (stateInfo.normalizedTime > (1 - comboWindowTime) &&
                                        stateInfo.normalizedTime < 1.0f);

                    if (canAttackNow)
                    {
                        OnAttackInput();
                        hasQueuedAttack = false;
                        queueTimer = 0f;
                    }
                }
            }

            // Check if player is no longer attacking and update state
            if (!animator.GetBool(isAttackingHash) && !isComboActive && !playerState.IsInState(PlayerAttackState.Attacking))
            {
                playerState.SetPlayerAttackState(PlayerAttackState.Idling);
            }
        }

        /// <summary>
        /// Handles queued attack inputs.
        /// </summary>
        private void QueueAttack()
        {
            if (isSpinAttackActive)
            {
                Debug.Log("Normal attack blocked due to spin attack.");
                return;
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool canAttackNow = !stateInfo.IsName("Attack") ||
                                (stateInfo.normalizedTime > (1 - comboWindowTime) &&
                                stateInfo.normalizedTime < 1.0f);

            Debug.Log($"{canAttackNow} can attack now");

            if (canAttackNow)
            {
                // Directly handle attack input
                OnAttackInput();
            }
            else if (currentComboStep < maxComboSteps)
            {
                // Queue the attack if within combo step limit
                hasQueuedAttack = true;
                queueTimer = 0f;
                Debug.Log("Attack Queued");
                if (currentComboStep == 1)
                {
                    playerState.Attack1_progress = true;
                    
                    playerState.SetPlayerAttackStatusState(PlayerAttackStatusState.Attack_progress_1);
                }
                else if (currentComboStep == 2)
                {
                    playerState.Attack2_progress = true;
                    playerState.SetPlayerAttackStatusState(PlayerAttackStatusState.Attack_progress_2);
                }
                else if (currentComboStep == 3)
                {
                    playerState.Attack3_progress = true;
                    playerState.SetPlayerAttackStatusState(PlayerAttackStatusState.Attack_progress_3);
                }
            }
        }

        /// <summary>
        /// Resets the combo immediately, typically when the combo window expires.
        /// </summary>
        private void ResetComboInstantly()
        {
            if (isBlocking)
            {
                Debug.Log("Combo reset skipped due to active block.");
                return; // Do not reset the combo while blocking
            }

            ResetCombo();
        }

        /// <summary>
        /// Handles block and heal actions based on input context.
        /// </summary>
        private void HandleBlockAndHeal(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // Track the currently equipped item
                object equippedItem = playerEquipmentManager.TrackEquippedItem<object>();

                if (playerEquipmentManager.IsEquippedItemHeal() && equippedItem is ConsumableClass consumable)
                {
                    attributesManager.InventoryManager.Remove(consumable, 1);

                    // Handle healing logic
                    Debug.Log($"Using consumable: {consumable.name}");
                    float healthToGain = consumable.healAmount;
                    attributesManager.GainHealth(healthToGain);

                    animator.SetBool(isPlayingActionHash, true); // Prevent other actions during healing
                    animator.SetTrigger("HealTrigger");
                }
                // Uncomment and implement blocking logic if needed
                // else if (equippedItem is WeaponClass weapon)
                // {
                //     // Handle blocking logic
                //     Debug.Log($"Blocking with weapon: {weapon.name}");
                //     isBlocking = true;

                //     // Update animator for blocking
                //     animator.SetBool(BlockHash, true);
                //     animator.SetBool(isBlockingHash, true);
                // }
            }
            else if (context.canceled)
            {
                // Reset blocking and action states when the context is canceled
                isBlocking = false;
                animator.SetBool(BlockHash, false);
                animator.SetBool(isBlockingHash, false);

                // Stop healing and other playing actions
                animator.SetBool(isPlayingActionHash, false);
            }
        }

        /// <summary>
        /// Handles PowerUp actions based on input context.
        /// </summary>
        private void HandlePowerUp(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (attributesManager.UseStamina(staminaCostPerAttack))
                {
                    Debug.Log("Power Up Triggered");
                    animator.SetBool("PowerUp", true);
                    animator.SetBool(isPlayingActionHash, true);

                    // Update player state
                    playerState.SetPlayerAttackState(PlayerAttackState.Idling);
                }
                else
                {
                    Debug.Log("Not enough stamina for power up.");
                }
            }
            else if (context.canceled)
            {
                animator.SetBool("PowerUp", false);
                animator.SetBool(isPlayingActionHash, false);
            }
        }

        /// <summary>
        /// Handles PowerUp2 actions based on input context.
        /// </summary>
        private void HandlePowerUp2(InputAction.CallbackContext context)
        {
            if (context.performed && !powerUpLocked && !isPowerUpOnCooldown)
            {
                if (attributesManager.UseStamina(staminaCostPerAttack))
                {
                    Debug.Log("Power Up2 Triggered");
                    animator.SetBool("PowerUp2", true);
                    animator.SetBool(isPlayingActionHash, true);
                    attributesManager.BoostAllAttributesTemporarily();

                    // Update player state
                    playerState.SetPlayerAttackState(PlayerAttackState.Idling);

                    // Start cooldown timer
                    StartCoroutine(PowerUpCooldown());
                }
                else
                {
                    Debug.Log("Not enough stamina for power up2.");
                }
            }
            else if (context.canceled)
            {
                animator.SetBool("PowerUp2", false);
                animator.SetBool(isPlayingActionHash, false);
            }
        }

        /// <summary>
        /// Coroutine to handle PowerUp2 cooldown.
        /// </summary>
        private IEnumerator PowerUpCooldown()
        {
            // Set the cooldown flag to true
            isPowerUpOnCooldown = true;

            Debug.Log("Power Up2 cooldown started.");

            // Wait for 20 seconds
            yield return new WaitForSeconds(20);

            // Reset the cooldown flag
            isPowerUpOnCooldown = false;

            Debug.Log("Power Up2 is ready again.");
        }

        /// <summary>
        /// Handles SpinAttack actions based on input context.
        /// </summary>
        private void HandleSpinAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (attributesManager.UseStamina(staminaCostSpinAttack))
                {
                    Debug.Log("Spin Attack Triggered");
                    isSpinAttackActive = true;
                    animator.SetBool(isPlayingActionHash, true);
                    animator.SetBool(spinningAttackHash, true);
                    animator.SetBool(isAttackingHash, true);

                    // Activate the attack camera and sync transforms if needed
                    // CameraOn();
                }
                else
                {
                    animator.SetBool(isPlayingActionHash, false);
                    animator.SetBool(isAttackingHash, false);
                    Debug.Log("Not enough stamina for spin attack.");
                }
            }
        }

        /// <summary>
        /// Handles Two-Hand Attack actions based on input context.
        /// </summary>
        private void HandleTwoHandAttack(InputAction.CallbackContext context)
        {
            if (context.performed && !twoHandClubLocked)
            {
                if (attributesManager.UseStamina(staminaCostPerAttack))
                {
                    Debug.Log("Two-Hand Attack Triggered");
                    animator.SetBool("TwoHandAttack", true);
                    animator.SetBool(isPlayingActionHash, true);

                    // Update player state
                    playerState.SetPlayerAttackState(PlayerAttackState.Attacking);
                }
                else
                {
                    Debug.Log("Not enough stamina for two-hand attack.");
                }
            }
        }

        /// <summary>
        /// Cancels the Two-Hand Attack.
        /// </summary>
        private void cancelTwoHandAttack()
        {
            animator.SetBool("TwoHandAttack", false);
            animator.SetBool(isPlayingActionHash, false);
        }

        /// <summary>
        /// Handles skill unlocks.
        /// </summary>
        private void Skill_OnSkillUnlocked(Skill.SkillName skillName)
        {
            if (skillName == Skill.SkillName.PowerUpAction)
            {
                powerUpLocked = false;
            }
            else if (skillName == Skill.SkillName.TwoHandClubComboAction)
            {
                twoHandClubLocked = false;
            }
        }

        /// <summary>
        /// Activates the attack camera.
        /// </summary>
        public void CameraOn()
        {
            // If the player is locking on, do nothing
            if (playerLockOn != null && playerLockOn.IsTargetLocked)
                return;

            Debug.Log("Attack camera activated.");
            if (cameraManager != null && attackCamera != null && defaultCamera != null)
            {
                cameraManager.SetActiveCamera(attackCamera);
                SyncCameraWithActiveOne(attackCamera, defaultCamera);
            }
            else
            {
                Debug.LogWarning("CameraManager or cameras are not assigned.");
            }
        }

        /// <summary>
        /// Deactivates the attack camera.
        /// </summary>
        public void CameraOff()
        {
            // If the player is locking on, do nothing
            if (playerLockOn != null && playerLockOn.IsTargetLocked)
                return;

            Debug.Log("Switched back to default camera.");
            if (cameraManager != null && defaultCamera != null && attackCamera != null)
            {
                cameraManager.SetDefaultCamera();
                SyncCameraWithActiveOne(defaultCamera, attackCamera);
            }
            else
            {
                Debug.LogWarning("CameraManager or cameras are not assigned.");
            }

            isSpinAttackActive = false;
            animator.SetBool(spinningAttackHash, false);
        }

        /// <summary>
        /// Cancels the spin attack.
        /// </summary>
        public void cancelSpinAttack()
        {
            isSpinAttackActive = false;
            animator.SetBool(spinningAttackHash, false);
        }

        /// <summary>
        /// Syncs the follow and look-at targets between cameras.
        /// </summary>
        /// <param name="activeCamera">The active camera.</param>
        /// <param name="otherCamera">The other camera to sync.</param>
        public void SyncCameraWithActiveOne(Cinemachine.CinemachineVirtualCamera activeCamera, Cinemachine.CinemachineVirtualCamera otherCamera)
        {
            if (activeCamera != null && otherCamera != null)
            {
                // Sync Follow and LookAt targets dynamically
                otherCamera.Follow = activeCamera.Follow;
                otherCamera.LookAt = activeCamera.LookAt;
            }
        }
    }
}
