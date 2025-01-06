using UnityEngine;
using Cinemachine;

public class PlayerLockOn : MonoBehaviour
{
    [Header("Lock-On Settings")]
    public float lockOnRange = 20f;
    public float lockOnAngle = 45f;
    public Transform playerTransform;
    public CinemachineVirtualCamera lockOnCamera;
    public CinemachineVirtualCamera mainVC;
    public Camera camera;

    [Header("Smoothing Settings")]
    public float smoothingSpeed = 5f; // Speed of the smooth transition

    private GameObject currentTarget;
    private bool targetLocked;
    private Transform smoothedLookAtTransform; // For smoother transitions

    public GameObject CurrentTarget => currentTarget;
    public bool IsTargetLocked => targetLocked;

    private void Start()
    {
        // Create a dummy object for smoother transitions
        smoothedLookAtTransform = new GameObject("SmoothedLookAt").transform;
    }

    public void LockOn()
    {
        if (targetLocked)
        {
            // Unlock the target
            targetLocked = false;
            currentTarget = null;
            if (lockOnCamera != null)
            {
                lockOnCamera.Priority = 0;
                mainVC.Priority = 60;
                lockOnCamera.LookAt = null;
            }
            Debug.Log("Target unlocked.");
            return;
        }

        GameObject closestEnemy = null;
        float closestAngle = lockOnAngle;

        // Detect enemies within range
        Collider[] hits = Physics.OverlapSphere(playerTransform.position, lockOnRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 enemyDirection = hit.transform.position - playerTransform.position;
                float angle = Vector3.Angle(playerTransform.forward, enemyDirection);
                Debug.DrawRay(playerTransform.position, enemyDirection, Color.red, 1.0f);

                if (angle < closestAngle)
                {
                    closestAngle = angle;
                    closestEnemy = hit.gameObject;
                }
            }
        }

        if (closestEnemy != null)
        {
            currentTarget = closestEnemy;
            targetLocked = true;
            if (lockOnCamera != null)
            {
                lockOnCamera.Priority = 60;

                // Find UpperHalfPoint child
                Transform upperHalfTransform = currentTarget.transform.Find("UpperHalfPoint");
                if (upperHalfTransform != null)
                {
                    smoothedLookAtTransform.position = upperHalfTransform.position; // Initialize smoothing
                    lockOnCamera.LookAt = smoothedLookAtTransform;
                }
                else
                {
                    // Fallback to the enemy's main transform if UpperHalfPoint is missing
                    smoothedLookAtTransform.position = currentTarget.transform.position; // Initialize smoothing
                    lockOnCamera.LookAt = smoothedLookAtTransform;
                    Debug.LogWarning($"UpperHalfPoint not found on {currentTarget.name}. Using main transform as LookAt.");
                }

                mainVC.Priority = 0;
            }
            Debug.Log($"Locked onto {currentTarget.name}");
        }
        else
        {
            Debug.Log("No enemy found within range.");
        }
    }

    private void LateUpdate()
    {
        if (targetLocked && currentTarget != null)
        {
            // Smoothly move the LookAt transform to the target's current position
            Vector3 targetPosition;

            Transform upperHalfTransform = currentTarget.transform.Find("UpperHalfPoint");
            if (upperHalfTransform != null)
            {
                targetPosition = upperHalfTransform.position;
            }
            else
            {
                targetPosition = currentTarget.transform.position + Vector3.up * 1.0f; // Adjust fallback offset
            }

            smoothedLookAtTransform.position = Vector3.Lerp(
                smoothedLookAtTransform.position,
                targetPosition,
                smoothingSpeed * Time.deltaTime
            );
        }
    }

    private void TransferRotationToPOV()
    {
        if (lockOnCamera == null || mainVC == null) return;
        print("TransferRotationToPOV");

        Quaternion lockOnRotation = lockOnCamera.transform.rotation;
        Vector3 eulers = lockOnRotation.eulerAngles;

        CinemachinePOV pov = mainVC.GetCinemachineComponent<CinemachinePOV>();
        if (pov != null)
        {
            pov.m_HorizontalAxis.Value = eulers.y;
            pov.m_VerticalAxis.Value = NormalizeAngle(eulers.x);
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}
