using UnityEngine;
using UnityEngine.InputSystem;
using SlimUI.ModernMenu;

public class PlayerController : MonoBehaviour
{
    private Vector2 lookInput;
    private float pitch; // X-axis rotation
    private float yaw;   // Y-axis rotation

    [SerializeField] private Transform cameraTransform;

    private void Update()
    {
        ApplyMouseLook();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // Get raw input from the Input System (mouse/gamepad)
        lookInput = context.ReadValue<Vector2>();
    }

    private void ApplyMouseLook()
    {
        // Apply mouse sensitivity multipliers
        float mouseX = lookInput.x * SettingsManager.Instance.MouseSensitivityX * Time.deltaTime;
        float mouseY = lookInput.y * SettingsManager.Instance.MouseSensitivityY * Time.deltaTime;

        // Clamp pitch to avoid unnatural vertical movement
        pitch = Mathf.Clamp(pitch - mouseY, -90f, 90f);
        yaw += mouseX;

        // Apply rotation to the camera and player
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
