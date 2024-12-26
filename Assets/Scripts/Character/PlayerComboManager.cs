using UnityEngine;

public class PlayerComboManager : MonoBehaviour
{
    private Animator animator;
    private float comboTimer = 0f;
    private int currentComboStep = 0;
    private bool isComboActive = false;
    private GameDevCW inputActions;

    [SerializeField] private float comboResetTime = 1f;
    [SerializeField] private float comboWindowTime = 0.3f;

    // Input queuing variables
    private bool hasQueuedAttack = false;
    private float queueTimer = 0f;
    [SerializeField] private float maxQueueTime = 0.5f; // How long to remember queued input

    // Camera manager reference
    [Header("Camera Settings")]
    [SerializeField] private CameraManager cameraManager; // Reference to CameraManager
    [SerializeField] private Cinemachine.CinemachineVirtualCamera attackCamera; // Attack camera

    private void Awake()
    {
        animator = GetComponent<Animator>();
        inputActions = new GameDevCW();
        inputActions.Player.Attack.performed += _ => QueueAttack();
    }

    private void Update()
    {
        if (isComboActive)
        {
            comboTimer += Time.deltaTime;
            if (comboTimer > comboResetTime)
            {
                ResetCombo();
            }
        }

        // Handle queued attacks
        if (hasQueuedAttack)
        {
            queueTimer += Time.deltaTime;
            if (queueTimer > maxQueueTime)
            {
                // Clear the queue if we've waited too long
                hasQueuedAttack = false;
                queueTimer = 0f;
            }
            else
            {
                // Check if we can now perform the queued attack
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
    }

    private void QueueAttack()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool canAttackNow = !stateInfo.IsName("Attack") ||
                           (stateInfo.normalizedTime > (1 - comboWindowTime) &&
                            stateInfo.normalizedTime < 1.0f);

        if (canAttackNow)
        {
            // If we can attack now, do it immediately
            PerformCombo();
        }
        else if (currentComboStep < 3)
        {
            // Queue the attack for later
            hasQueuedAttack = true;
            queueTimer = 0f;
            Debug.Log("Attack Queued");
        }
    }

    public void PerformCombo()
    {
        if (isComboActive)
        {
            if (currentComboStep < 3)
            {
                currentComboStep++;
                Debug.Log($"Combo Step Incremented: {currentComboStep}");
                animator.SetInteger("ComboIndex", currentComboStep);
                animator.SetTrigger("Attack");
                comboTimer = 0f;
            }
        }
        else
        {
            isComboActive = true;
            currentComboStep = 1;
            Debug.Log($"Combo Started: {currentComboStep}");
            animator.SetInteger("ComboIndex", currentComboStep);
            animator.SetTrigger("Attack");
            comboTimer = 0f;

            // Switch to the attack camera when the combo starts
            if (cameraManager != null && attackCamera != null)
            {
                cameraManager.SetActiveCamera(attackCamera);
            }
        }

        CancelInvoke(nameof(ResetCombo));
        Invoke(nameof(ResetCombo), comboResetTime);
    }

    private void ResetCombo()
    {
        Debug.Log("Resetting Combo");
        isComboActive = false;
        currentComboStep = 0;
        comboTimer = 0f;
        hasQueuedAttack = false; // Clear any queued attacks when resetting
        queueTimer = 0f;
        animator.SetInteger("ComboIndex", 0);
        animator.ResetTrigger("Attack");
        animator.SetTrigger("ResetCombo");

        // Switch back to the default camera
        if (cameraManager != null)
        {
            cameraManager.SetDefaultCamera();
        }
    }

    public void OnAttackAnimationComplete()
    {
        Debug.Log("Attack Animation Complete");
        if (!isComboActive || currentComboStep >= 3)
        {
            ResetCombo();
        }
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
