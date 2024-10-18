using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform player;
    public float sensX = 100f;
    public float sensY = 100f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private InputActions inputActions;
    private Vector2 lookInput;
    private Vector3 offset;

    void Start(){
        offset = transform.position - player.transform.position;
    }

    void LateUpdate()
    {
        transform.position = player.transform.position + offset;
    }

    private void Awake()
    {
        // Initialize input actions
        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        // Enable Look action
        inputActions.Player.Look.Enable();
        // Subscribe to Look action
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;
    }

    private void OnDisable()
    {
        // Disable Look action
        inputActions.Player.Look.Disable();
        // Unsubscribe from Look action
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;
    }

    private void Update()
    {
        // Apply camera movement based on look input
        yRotation += lookInput.x * sensX * Time.deltaTime;
        xRotation -= lookInput.y * sensY * Time.deltaTime;

        // Clamp vertical rotation
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotation to the camera
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        // Get mouse movement input
        lookInput = context.ReadValue<Vector2>();
    }
}
