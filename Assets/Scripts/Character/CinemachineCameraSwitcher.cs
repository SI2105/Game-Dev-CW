using UnityEngine;
using Cinemachine;

public class CinemachineCameraSwitcher : MonoBehaviour
{
    // Reference to the Cinemachine Virtual Camera
    [Tooltip("Assign the Cinemachine Virtual Camera here.")]
    public CinemachineVirtualCamera virtualCamera;

    // Enumerations for Aim types
    public enum AimType { Composer, POV }

    // Current selected Aim type
    [Tooltip("Select the initial Aim type.")]
    public AimType currentAimType = AimType.Composer;

    void Start()
    {
        // If virtualCamera is not assigned, attempt to get it from the same GameObject
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogError("CinemachineVirtualCamera not found on this GameObject. Please assign it in the Inspector.");
                return;
            }
        }

        // Apply the initial Aim component
        ApplyAim(currentAimType);
    }

    /// <summary>
    /// Switches the Aim component to the specified AimType.
    /// </summary>
    /// <param name="newAimType">The desired Aim type.</param>
    public void SwitchAim(AimType newAimType)
    {
        if (currentAimType == newAimType)
            return; // No change needed

        currentAimType = newAimType;
        ApplyAim(newAimType);
    }

    /// <summary>
    /// Applies the specified Aim component to the Virtual Camera.
    /// </summary>
    /// <param name="aimType">The Aim type to apply.</param>
    private void ApplyAim(AimType aimType)
    {
        // Remove existing Aim components to avoid conflicts
        RemoveAimComponents();

        switch (aimType)
        {
            case AimType.Composer:
                // Add CinemachineComposer with default settings
                virtualCamera.AddCinemachineComponent<CinemachineComposer>();
                Debug.Log("Switched Aim to Composer.");
                break;

            case AimType.POV:
                // Add CinemachinePOV and configure its axes
                var pov = virtualCamera.AddCinemachineComponent<CinemachinePOV>();

                // Configure Vertical Axis
                pov.m_VerticalAxis.m_MinValue = -70f;
                pov.m_VerticalAxis.m_MaxValue = 70f;
                pov.m_VerticalAxis.m_MaxSpeed = 200f;
                pov.m_VerticalAxis.m_AccelTime = 0f; // Instant acceleration
                pov.m_VerticalAxis.m_DecelTime = 0f; // Instant deceleration
                pov.m_VerticalAxis.m_InputAxisName = ""; // Disable input if not needed
                pov.m_VerticalAxis.m_InputAxisValue = 0f; // Initial value
                pov.m_VerticalAxis.m_Wrap = false;

                // Configure Horizontal Axis (Locked)
                pov.m_HorizontalAxis.m_MinValue = 0f;
                pov.m_HorizontalAxis.m_MaxValue = 0f;
                pov.m_HorizontalAxis.m_MaxSpeed = 40f;
                pov.m_HorizontalAxis.m_AccelTime = 0f;
                pov.m_HorizontalAxis.m_DecelTime = 0f;
                pov.m_HorizontalAxis.m_InputAxisName = ""; // Disable input if not needed
                pov.m_HorizontalAxis.m_InputAxisValue = 0f; // Initial value
                pov.m_HorizontalAxis.m_Wrap = false;

                Debug.Log("Switched Aim to POV with custom settings.");
                break;

            default:
                Debug.LogWarning("Unhandled AimType: " + aimType);
                break;
        }
    }

    /// <summary>
    /// Removes all existing Aim components from the Virtual Camera.
    /// </summary>
    private void RemoveAimComponents()
    {
        // Remove CinemachineComposer if it exists
        var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
        if (composer != null)
        {
            Destroy(composer);
            Debug.Log("Removed existing Composer Aim component.");
        }

        // Remove CinemachinePOV if it exists
        var pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        if (pov != null)
        {
            Destroy(pov);
            Debug.Log("Removed existing POV Aim component.");
        }
    }

    // Optional: Specific methods for UI integration

    /// <summary>
    /// Switches Aim to Composer.
    /// </summary>
    public void SetAimToComposer()
    {
        SwitchAim(AimType.Composer);
    }

    /// <summary>
    /// Switches Aim to POV.
    /// </summary>
    public void SetAimToPOV()
    {
        SwitchAim(AimType.POV);
    }
}
