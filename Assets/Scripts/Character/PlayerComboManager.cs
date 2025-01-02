using UnityEngine;
using System.Collections; // Corrected namespace for IEnumerator
using UnityEngine.InputSystem;

namespace SG{

    public class PlayerComboManager : MonoBehaviour
    {
        private Animator animator;
        private float comboTimer = 0f;
        private int currentComboStep = 0;
        private bool isComboActive = false;
        private bool isSpinAttackActive = false;

        private GameDevCW inputActions;

        // hashes
        private static int isBlockingHash;
        private static int BlockHash;
        private static int spinningAttackHash;
        private static int comboResetTriggerHash;
        private static int attackTriggerHash;
        private static int comboIndexHash;
        private static int isPlayingActionHash;
        private static int isAttackingHash;

        [SerializeField] private float comboResetTime = 1f;
        [SerializeField] private float comboWindowTime = 0.4f;
        private PlayerState _playerState;
        private int staminaUsageCount = 0;
        private bool isBlocking = false; 

        // Input queuing variables
        private bool hasQueuedAttack = false;
        private float queueTimer = 0f;
        [SerializeField] private float maxQueueTime = 0.9f; // How long to remember queued input

        // Camera manager reference
        [Header("Camera Settings")]
        [SerializeField] private CameraManager cameraManager; // Reference to CameraManager
        [SerializeField] private Cinemachine.CinemachineVirtualCamera defaultCamera; // Default camera
        [SerializeField] private Cinemachine.CinemachineVirtualCamera attackCamera; // Attack camera

        // Stamina
        [Header("Stamina Settings")]
        [SerializeField] private PlayerAttributesManager attributesManager; // Reference to AttributesManager
        private TwoDimensionalAnimationController playerController;
        private PlayerEquipmentManager playerEquipmentManager;
        [SerializeField] private float staminaCostPerAttack = 10f;
        [SerializeField] private float staminaCostSpinAttack = 20f;
        
        private PlayerLockOn playerLockOn;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            inputActions = new GameDevCW();
            attributesManager = GetComponent<PlayerAttributesManager>();
            playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
            playerController = GetComponent<TwoDimensionalAnimationController>();

            inputActions.Player.Attack.performed += _ => QueueAttack();
            inputActions.Player.Block.performed += HandleBlockAndHeal;
            inputActions.Player.Block.canceled += HandleBlockAndHeal;
            inputActions.Player.SpinAttack.performed += HandleSpinAttack;
            inputActions.Player.TwoHandAttack.performed += HandleTwoHandAttack;
            inputActions.Player.PowerUp2.performed += HandlePowerUp2; 

            _playerState = GetComponent<PlayerState>();
            playerLockOn = GetComponent<PlayerLockOn>();

            isBlockingHash = Animator.StringToHash("isBlocking");
            BlockHash = Animator.StringToHash("Block");
            spinningAttackHash = Animator.StringToHash("isSpinningAttack");
            comboResetTriggerHash = Animator.StringToHash("ResetCombo");
            attackTriggerHash = Animator.StringToHash("Attack");
            comboIndexHash = Animator.StringToHash("ComboIndex");
            isPlayingActionHash = Animator.StringToHash("isPlayingAction");
            isAttackingHash = Animator.StringToHash("isAttacking");
        }

        private object EquippedItem(){
            return playerEquipmentManager.TrackEquippedItem<object>();
        }

        private void Hit()
        {
            object equippedItem = EquippedItem();

            if (equippedItem is WeaponClass weapon)
            {
                float weaponDamage = weapon.damage;
                WeaponCollisionHandler collisionHandler = playerEquipmentManager.rightHandWeaponModel.GetComponent<WeaponCollisionHandler>();

                if (collisionHandler != null)
                {
                    collisionHandler.SetTemporaryDamage(weaponDamage);
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
            }
            else
            {
                Debug.LogWarning("No valid item equipped.");
            }
        }
        
        private void HandleBlockAndHeal(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // Track the currently equipped item
                object equippedItem = playerEquipmentManager.TrackEquippedItem<object>();

                if (playerEquipmentManager.IsEquippedItemHeal() && equippedItem is ConsumableClass consumable)
                {
                    // Handle healing logic
                    Debug.Log($"Using consumable: {consumable.name}");

                    animator.SetBool(isPlayingActionHash, true); // Prevent other actions during healing
                    animator.SetTrigger("HealTrigger");
                }
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

        private void HandlePowerUp(InputAction.CallbackContext context){
            if (context.performed)
            {
                if (attributesManager.UseStamina(staminaCostPerAttack))
                {
                    Debug.Log("Power Up Triggered");
                    animator.SetBool("PowerUp", true);
                    animator.SetBool(isPlayingActionHash, true);

                    // Update player state
                    _playerState.SetPlayerAttackState(PlayerAttackState.Idling);
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
        private bool isPowerUpOnCooldown = false;

        private void HandlePowerUp2(InputAction.CallbackContext context)
        {
            if (context.performed && powerUpLocked == false && !isPowerUpOnCooldown)
            {
                if (attributesManager.UseStamina(staminaCostPerAttack))
                {
                    Debug.Log("Power Up Triggered");
                    animator.SetBool("PowerUp2", true);
                    animator.SetBool(isPlayingActionHash, true);
                    attributesManager.BoostAllAttributesTemporarily();

                    // Update player state
                    _playerState.SetPlayerAttackState(PlayerAttackState.Idling);

                    // Start cooldown timer
                    StartCoroutine(PowerUpCooldown());
                }
                else
                {
                    Debug.Log("Not enough stamina for power up.");
                }
            }
            else if (context.canceled)
            {
                animator.SetBool("PowerUp2", false);
                animator.SetBool(isPlayingActionHash, false);
            }
        }

        private IEnumerator PowerUpCooldown()
        {
            // Set the cooldown flag to true
            isPowerUpOnCooldown = true;

            Debug.Log("Power Up cooldown started.");

            // Wait for 20 seconds
            yield return new WaitForSeconds(20);

            // Reset the cooldown flag
            isPowerUpOnCooldown = false;

            Debug.Log("Power Up is ready again.");
        }


        private void cancelPowerUp(){
            animator.SetBool("PowerUp", false);
            animator.SetBool(isPlayingActionHash, false);
        }

        private void cancelPowerUp2(){
            animator.SetBool("PowerUp2", false);
            animator.SetBool(isPlayingActionHash, false);
        }

        private void HandleSpinAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (attributesManager.UseStamina(staminaCostSpinAttack))
                {
                    print("Spin Attack Triggered");
                    isSpinAttackActive = true;
                    animator.SetBool(isPlayingActionHash, true);
                    animator.SetBool(spinningAttackHash, true);
                    animator.SetBool(isAttackingHash, true);
                    
                    // Activate the attack camera and sync transforms
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

        private void Update()
        {
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
                        PerformCombo();
                        hasQueuedAttack = false;
                        queueTimer = 0f;
                    }
                }
            }

            // Manage combo timer
            if (isComboActive)
            {
                comboTimer += Time.deltaTime;
                if (comboTimer > comboResetTime)
                {
                    ResetComboInstantly();
                }
            }

            // Check if player is no longer attacking and update state
            if (!animator.GetBool(isAttackingHash) && !isComboActive && !_playerState.IsInState(PlayerAttackState.Idling))
            {
                _playerState.SetPlayerAttackState(PlayerAttackState.Idling);
            }
        }

        private void HandleTwoHandAttack(InputAction.CallbackContext context)
        {
            if (context.performed && twoHandClubLocked == false)
            {
                if (attributesManager.UseStamina(staminaCostPerAttack))
                {
                    Debug.Log("Two-Hand Attack Triggered");
                    animator.SetBool("TwoHandAttack", true);
                    animator.SetBool(isPlayingActionHash, true);

                    // Update player state
                    _playerState.SetPlayerAttackState(PlayerAttackState.Attacking);
                }
                else
                {
                    Debug.Log("Not enough stamina for two-hand attack.");
                }
            }
        }

        private void cancelTwoHandAttack(){
            animator.SetBool("TwoHandAttack", false);
            animator.SetBool(isPlayingActionHash, false);
        }


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

            print(canAttackNow + " can attack now");

            if (canAttackNow)
            {
                // Update player state to Attacking
                _playerState.SetPlayerAttackState(PlayerAttackState.Attacking);

                Hit();
                attributesManager.LevelUp();
                PerformCombo();
            }
            else if (currentComboStep < 3)
            {
                hasQueuedAttack = true;
                queueTimer = 0f;
                Debug.Log("Attack Queued");
            }
        }

        
        public void PerformCombo()
        {
            if (isSpinAttackActive)
            {
                Debug.Log("Combo attack blocked due to spin attack.");
                return;
            }

            // Check if stamina can be used for the current combo step
            if (staminaUsageCount < 2 && attributesManager.UseStamina(staminaCostPerAttack))
            {
                if (isComboActive)
                {
                    // Continue combo within the valid range
                    if (currentComboStep < 2) // Adjust to your maximum combo step
                    {
                        currentComboStep++;

                        staminaUsageCount++; // Increment stamina usage count
                        Debug.Log($"Combo Step Incremented: {currentComboStep}, Stamina Usage: {staminaUsageCount}");
                        animator.SetInteger(comboIndexHash, currentComboStep);
                        animator.SetTrigger(attackTriggerHash);
                        animator.SetBool(isAttackingHash, true);
                        animator.SetBool(isPlayingActionHash, true);

                        // Update player state to Attacking
                        _playerState.SetPlayerAttackState(PlayerAttackState.Attacking);

                        // Reset the combo timer
                        comboTimer = 0f;
                    }
                }
                else
                {
                    // Start a new combo
                    isComboActive = true;
                    currentComboStep = 1;
                    staminaUsageCount++; // Increment stamina usage count
                    Debug.Log($"Combo Started: {currentComboStep}, Stamina Usage: {staminaUsageCount}");
                    animator.SetInteger(comboIndexHash, currentComboStep);
                    animator.SetTrigger(attackTriggerHash);
                    animator.SetBool(isAttackingHash, false);
                    animator.SetBool(isPlayingActionHash, false);

                    // Update player state to Attacking
                    _playerState.SetPlayerAttackState(PlayerAttackState.Attacking);

                    // Reset the combo timer
                    comboTimer = 0f;
                }
            }
            else
            {
                Debug.Log("Not enough stamina to perform attack or max combo stamina limit reached.");
            }
        }



        private void ResetComboInstantly()
        {
            if (isBlocking)
            {
                Debug.Log("Combo reset skipped due to active block.");
                return; // Do not reset the combo while blocking
            }
            Debug.Log("Resetting Combo Instantly");
            isComboActive = false;
            currentComboStep = 0;
            comboTimer = 0f; // Reset the combo timer
            hasQueuedAttack = false;
            queueTimer = 0f;
            staminaUsageCount = 0; // Reset stamina usage count

            // Reset animator parameters
            animator.SetInteger(comboIndexHash, 0);
            animator.ResetTrigger(attackTriggerHash);
            animator.SetTrigger(comboResetTriggerHash);
            animator.SetBool(isAttackingHash, false);
            animator.SetBool(isPlayingActionHash, false);

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

        private bool powerUpLocked = true;
        private bool twoHandClubLocked = true;

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

        public void cancelSpinAttack(){
            isSpinAttackActive = false;
            animator.SetBool(spinningAttackHash, false);
        }


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
