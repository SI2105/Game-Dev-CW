using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class KeyBindings : MonoBehaviour
{
    [SerializeField] private GameDevCW inputActions; // Reference to the InputActions asset
    [SerializeField] private GameObject rebindingPopup; // The popup UI
    [SerializeField] private Button cancelButton; // Cancel button in the popup

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private void Awake()
    {
        // Initialize InputActions
        if (inputActions == null)
        {
            inputActions = new GameDevCW();
        }

        // Ensure the popup is hidden initially
        rebindingPopup.SetActive(false);

        // Add listener to cancel button
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelRebinding);
        }
    }

    public void StartRebinding(string actionName, string compositePart, TMP_Text buttonText)
    {
        // Debug the initialization of inputActions
        if (inputActions == null)
        {
            Debug.LogError("InputActions is not initialized.");
            return;
        }

        // Find the action
        var action = inputActions.asset.FindAction(actionName, true);

        if (action == null)
        {
            Debug.LogError($"Action '{actionName}' not found in InputActions.");
            return;
        }

        // Debug rebindingPopup
        if (rebindingPopup == null)
        {
            Debug.LogError("RebindingPopup is not assigned.");
            return;
        }

        Debug.Log("All checks passed. Starting rebinding process...");

        int bindingIndex = -1;
        if (compositePart != "")
        {
            bindingIndex = -1;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].isPartOfComposite && action.bindings[i].name == compositePart)
                {
                    bindingIndex = i;
                    break;
                }
            }

            if (bindingIndex == -1)
            {
                Debug.LogError($"Composite part '{compositePart}' not found for action '{actionName}'.");
                return;
            }
        }
        else
        {
            bindingIndex = 0;
        }
        
        // Show the popup and update the status text
        rebindingPopup.SetActive(true);

        // Disable the action temporarily
        action.Disable();
        Debug.Log($"Binding Index for {action.name}: {bindingIndex}");
        Debug.Log($"Rebinding Fire action, Active Devices: {InputSystem.devices.Count}");
        foreach (var device in InputSystem.devices)
        {
            Debug.Log($"Device: {device.displayName}");
        }
        // Start the rebinding operation
        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .OnMatchWaitForAnother(0.1f) // Optional: Wait for confirmation
            .OnComplete(operation =>
            {
                buttonText.text = operation.selectedControl.displayName;
                Debug.Log($"Rebinding complete: {action.bindings[bindingIndex].effectivePath}");
                rebindingPopup.SetActive(false);
                action.Enable();
                operation.Dispose();
                rebindingOperation = null;
            })
            .OnCancel(operation =>
            {
                Debug.Log("Rebinding canceled.");
                rebindingPopup.SetActive(false);
                operation.Dispose();
                rebindingOperation = null;
            })
            .Start();
    }

    public void CancelRebinding()
    {
        if (rebindingOperation != null)
        {
            rebindingOperation.Cancel();
        }
    }

    public void SaveBindings()
    {
        PlayerPrefs.SetString("InputBindings", inputActions.asset.SaveBindingOverridesAsJson());
        Debug.Log("Bindings saved.");
    }

    public void LoadBindings()
    {
        if (PlayerPrefs.HasKey("InputBindings"))
        {
            inputActions.asset.LoadBindingOverridesFromJson(PlayerPrefs.GetString("InputBindings"));
            Debug.Log("Bindings loaded.");
        }
    }

    public void ResetBindings()
    {
        inputActions.asset.RemoveAllBindingOverrides();
        Debug.Log("Bindings reset.");
    }

    public void StartRebindingMoveForward(TMP_Text buttonText)
    {
        StartRebinding("Player/Move", "up", buttonText); // Action: Move, Binding Index: 0 (Forward movement)
    }

    public void StartRebindingMoveBackwards(TMP_Text buttonText)
    {
        StartRebinding("Player/Move", "down", buttonText); 
    }
    public void StartRebindingMoveLeft(TMP_Text buttonText)
    {
        StartRebinding("Player/Move", "left", buttonText); 
    }
    public void StartRebindingMoveRight(TMP_Text buttonText)
    {
        StartRebinding("Player/Move", "right", buttonText); 
    }

    public void StartRebindingJump(TMP_Text buttonText)
    {
        StartRebinding("Player/Jump", "", buttonText); // Action: Jump, Binding Index: 0
    }

    public void StartRebindingSprint(TMP_Text buttonText)
    {
        StartRebinding("Player/Sprint", "", buttonText); // Action: Sprint, Binding Index: 0
    }

    public void StartRebindingPause(TMP_Text buttonText)
    {
        StartRebinding("Player/Pause", "", buttonText); // Action: Pause, Binding Index: 0
    }

    public void StartRebindingFire(TMP_Text buttonText)
    {
        // string actionName = "Fire"; 
        // int bindingIndex = 0;
        //  // Debug the initialization of inputActions
        // if (inputActions == null)
        // {
        //     Debug.LogError("InputActions is not initialized.");
        //     return;
        // }

        // // Find the action
        // var action = inputActions.asset.FindAction(actionName);

        // if (action == null)
        // {
        //     Debug.LogError($"Action '{actionName}' not found in InputActions.");
        //     return;
        // }

        // // Debug rebindingPopup
        // if (rebindingPopup == null)
        // {
        //     Debug.LogError("RebindingPopup is not assigned.");
        //     return;
        // }

        // Debug.Log("All checks passed. Starting rebinding process...");

        // rebindingPopup.SetActive(true);
        // action.Disable();
        // Debug.Log($"Attempting to rebind binding index: {bindingIndex}");
        // Debug.Log($"Binding Control Type: {action.expectedControlType}");

        // rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
        //     .OnComplete(operation =>
        //     {
        //         buttonText.text = operation.selectedControl.displayName;
        //         Debug.Log($"Rebinding complete: {action.bindings[bindingIndex].effectivePath}");
        //         rebindingPopup.SetActive(false);
        //         action.Enable();
        //         operation.Dispose();
        //         rebindingOperation = null;
        //     })
        //     .OnCancel(operation =>
        //     {
        //         Debug.Log("Rebinding canceled.");
        //         rebindingPopup.SetActive(false);
        //         operation.Dispose();
        //         rebindingOperation = null;
        //     })
        //     .Start();
        StartRebinding("Player/Fire", "", buttonText); // Action: Inventory, Binding Index: 0
    }

    public void StartRebindingInventory(TMP_Text buttonText)
    {
        StartRebinding("Player/Inventory", "", buttonText); // Action: Inventory, Binding Index: 0
    }
}
