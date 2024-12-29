using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSelector : MonoBehaviour
{
    private Vector2 lookInput;
    private GameDevCW inputActions;


    // Start is called before the first frame update
    void Start()
    {
        inputActions = new GameDevCW();
        inputActions.Player.Look.performed += HandleLook;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void HandleLook(InputAction.CallbackContext context)
    {

        lookInput = context.ReadValue<Vector2>();
    }
}
