using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    // Player reference in Dev Mode
    public Transform player;

    // Mouse sensitivity
    public float sensX = 100f;
    public float sensY = 100f;

    // X, Y axis position 
    private float xRotation = 0f;
    private float yRotation = 0f;

    private InputActions inputActions;
    private Vector2 lookInput; // Mouse movement
    private Vector3 offset; // position to maintain the camera's position follow the player

    void Start(){
        offset = transform.position - player.transform.position; // initialize the camera offset based on the position relative to the player
    }

    void LateUpdate()
    {
        transform.position = player.transform.position + offset; // ensures the camera will follow the player
    }

    private void Awake()
    {
        inputActions = new InputActions(); // Input Actions initialization
    }

    // Keeps track of the look input action when the script is enabled
    private void OnEnable()
    {
        inputActions.Player.Look.Enable();
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;
    }

    // Disables the look input action when the script is disabled
    private void OnDisable()
    {
        inputActions.Player.Look.Disable();
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;
    }

    // Handles camera rotation based on mouse input
    private void Update()
    {
        // Updates the look rotation based on the mouse input
        yRotation += lookInput.x * sensX * Time.deltaTime; 
        xRotation -= lookInput.y * sensY * Time.deltaTime;

        // blocks the X rotation on the X axis to a 90degrees
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // creates a quaternion from scratch to rotate the camera
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    // called on look 
    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>(); // Reads the look input as a V2 (X, Y)
    }
}
