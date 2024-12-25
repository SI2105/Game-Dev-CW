using UnityEngine;
using Cinemachine;

public class CameraHeadTracker : MonoBehaviour
{
    [SerializeField] private Transform headBone;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Vector3 offsetFromHead = new Vector3(0, 0.2f, 0); // Adjust this to position camera relative to head

    private void LateUpdate()
    {
        if (headBone != null)
        {
            Debug.Log("following ");
            // Update the virtual camera's transform position to follow the head
            virtualCamera.transform.position = headBone.position + offsetFromHead;
        }
    }
}