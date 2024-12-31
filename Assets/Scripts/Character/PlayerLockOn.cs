using UnityEngine;
using Cinemachine;

public class PlayerLockOn : MonoBehaviour
{
    [Header("Lock-On Settings")]
    public float lockOnRange = 20f;
    public float lockOnAngle = 45f;
    public Transform playerTransform;
    public CinemachineVirtualCamera lockOnCamera;
    
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
                lockOnCamera.Priority = 10;
                lockOnCamera.LookAt = currentTarget.transform;
            }
            Debug.Log($"Locked onto {currentTarget.name}");
        }
        else
        {
            Debug.Log("No enemy found within range.");
        }
    }
}