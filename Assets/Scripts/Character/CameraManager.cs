using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineVirtualCamera defaultCamera; // Default gameplay camera
    private CinemachineVirtualCamera activeCamera;

    [Header("Camera Priorities")]
    [SerializeField] private int highPriority = 10; // Priority for active camera
    [SerializeField] private int lowPriority = 0; // Priority for inactive camera

    private void Start()
    {
        // Ensure the default camera is active at the start
        SetDefaultCamera();
    }

    /// <summary>
    /// Activates the specified camera.
    /// </summary>
    /// <param name="cameraToActivate">The camera to activate.</param>
    public void SetActiveCamera(CinemachineVirtualCamera cameraToActivate)
    {
        if (cameraToActivate == null)
        {
            Debug.LogError("Attempted to activate a null camera!");
            return;
        }

        Debug.Log("activating new camera");
        // Set the priority of the new active camera
        cameraToActivate.Priority = highPriority;

        // Lower the priority of the previous active camera
        if (activeCamera != null && activeCamera != cameraToActivate)
        {
            activeCamera.Priority = lowPriority;
        }

        activeCamera = cameraToActivate;
    }

    /// <summary>
    /// Switches back to the default camera.
    /// </summary>
    public void SetDefaultCamera()
    {
        SetActiveCamera(defaultCamera);
    }
}
