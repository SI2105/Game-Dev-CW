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
    
    private GameObject currentTarget;
    private bool targetLocked;

    // Public property to access current target
    public GameObject CurrentTarget => currentTarget;
    public bool IsTargetLocked => targetLocked;

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
                    lockOnCamera.LookAt = upperHalfTransform;
                }
                else
                {
                    // Fallback to the enemy's main transform if UpperHalfPoint is missing
                    lockOnCamera.LookAt = currentTarget.transform;
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

    Vector3 GetUpperHalfPosition(GameObject target)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Calculate the upper half position
            return renderer.bounds.center + Vector3.up * (renderer.bounds.size.y / 4);
        }
        else
        {
            // Fallback if Renderer is not found
            return target.transform.position + Vector3.up * 1.0f; // Adjust as needed
        }
    }

    private void TransferRotationToPOV()
    {
        // 1) Make sure cameras are valid
        if (lockOnCamera == null || mainVC == null) return;
        print("TransferRotationToPOV");
        // 2) Get the lock-on camera's world rotation
        Quaternion lockOnRotation = lockOnCamera.transform.rotation;
        Vector3 eulers = lockOnRotation.eulerAngles;
        
        // 3) Grab the POV component from the main camera
        CinemachinePOV pov = mainVC.GetCinemachineComponent<CinemachinePOV>();
        if (pov != null)
        {
            // Typically, the CinemachinePOV interprets:
            //   - m_HorizontalAxis as yaw (around Y)
            //   - m_VerticalAxis as pitch (around X)
            // 
            // We'll do a simple assignment, but you might want
            // to clamp or adjust angles to match your game logic.

            // Yaw
            pov.m_HorizontalAxis.Value = eulers.y;  
            // Pitch
            pov.m_VerticalAxis.Value   = NormalizeAngle(eulers.x);
        }
    }

    // Optional: A helper to bring angles into the -180..180 range
    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

}