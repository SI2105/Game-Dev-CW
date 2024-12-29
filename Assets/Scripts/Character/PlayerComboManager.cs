using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] private float comboResetTime = 1f;
    [SerializeField] private float comboWindowTime = 0.4f;

    // Input queuing variables
    private bool hasQueuedAttack = false;
    private float queueTimer = 0f;
    [SerializeField] private float maxQueueTime = 0.9f; // How long to remember queued input

    // Camera manager reference
    [Header("Camera Settings")]
    [SerializeField] private CameraManager cameraManager; // Reference to CameraManager
    [SerializeField] private Cinemachine.CinemachineVirtualCamera defaultCamera; // Default camera
    [SerializeField] private Cinemachine.CinemachineVirtualCamera attackCamera; // Attack camera

    private void Awake()
    {
        animator = GetComponent<Animator>();
        inputActions = new GameDevCW();
        inputActions.Player.Attack.performed += _ => QueueAttack();
        inputActions.Player.Block.performed += HandleBlock;
        inputActions.Player.Block.canceled += HandleBlock;
        inputActions.Player.SpinAttack.performed += HandleSpinAttack;

        isBlockingHash = Animator.StringToHash("isBlocking");
        BlockHash = Animator.StringToHash("Block");
        spinningAttackHash = Animator.StringToHash("isSpinningAttack");
        comboResetTriggerHash = Animator.StringToHash("ResetCombo");
        attackTriggerHash = Animator.StringToHash("Attack");
        comboIndexHash = Animator.StringToHash("ComboIndex");
    }

    private void HandleBlock(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            print("Blocking");
            animator.SetBool(BlockHash, true);
            animator.SetBool(isBlockingHash, true);
        }
        else if (context.canceled)
        {
            print("Stopped blocking");
            animator.SetBool(BlockHash, false);
            animator.SetBool(isBlockingHash, false);
        }
    }

    private void HandleSpinAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            print("Spin Attack Triggered");
            isSpinAttackActive = true;
            animator.SetBool(spinningAttackHash, true);
            // Activate the attack camera and sync transforms
            CameraOn();
        }
    }

    public void CameraOn()
    {
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

    public void SyncCameraWithActiveOne(Cinemachine.CinemachineVirtualCamera activeCamera, Cinemachine.CinemachineVirtualCamera otherCamera)
    {
        if (activeCamera != null && otherCamera != null)
        {
            // Sync Follow and LookAt targets dynamically
            otherCamera.Follow = activeCamera.Follow;
            otherCamera.LookAt = activeCamera.LookAt;
        }
    }

    private void Update()
    {
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

                if (currentComboStep >= 2)
                {
                    ResetComboInstantly();
                }
            }
        }
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

        if (canAttackNow)
        {
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

        if (isComboActive)
        {
            if (currentComboStep < 2)
            {
                currentComboStep++;
                Debug.Log($"Combo Step Incremented: {currentComboStep}");
                animator.SetInteger(comboIndexHash, currentComboStep);
                animator.SetTrigger(attackTriggerHash);
            }
            else
            {
                ResetComboInstantly();
            }
        }
        else
        {
            isComboActive = true;
            currentComboStep = 1;
            Debug.Log($"Combo Started: {currentComboStep}");
            animator.SetInteger(comboIndexHash, currentComboStep);
            animator.SetTrigger(attackTriggerHash);
        }
    }


    private void ResetComboInstantly()
    {
        Debug.Log("Resetting Combo Instantly");
        isComboActive = false;
        currentComboStep = 0;
        hasQueuedAttack = false;
        queueTimer = 0f;
        
        // Reset animator parameters
        animator.SetInteger(comboIndexHash, 0);
        animator.ResetTrigger(attackTriggerHash);
        animator.SetTrigger(comboResetTriggerHash);
    }

    

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
}
