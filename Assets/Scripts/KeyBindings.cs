// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.UI;
// using TMPro;

// public class KeyBindings : MonoBehaviour
// {
//     public static KeyBindings Instance { get; private set; } // Singleton instance

//     [SerializeField] private GameDevCW inputActions; // Reference to the InputActions asset
//     [SerializeField] private GameObject rebindingPopup; // The popup UI
//     [SerializeField] private Button cancelButton; // Cancel button in the popup

//     private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
//     private const string RebindsKey = "InputRebinds";

//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//             return;
//         }

//         if (inputActions == null)
//         {
//             Debug.Log("Initializing inputActions in KeyBindings...");
//             inputActions = new GameDevCW();
//         }

//         if (rebindingPopup != null)
//         {
//             rebindingPopup.SetActive(false);
//         }

//         if (cancelButton != null)
//         {
//             cancelButton.onClick.AddListener(CancelRebinding);
//         }

//         LoadBindings();
//     }

//     public void StartRebindingMoveForward(TMP_Text buttonText)
//     {
//         StartRebinding("Player/Move", "up", buttonText);
//     }

//     public void StartRebindingMoveBackward(TMP_Text buttonText)
//     {
//         StartRebinding("Player/Move", "down", buttonText);
//     }

//     public void StartRebindingMoveLeft(TMP_Text buttonText)
//     {
//         StartRebinding("Player/Move", "left", buttonText);
//     }

//     public void StartRebindingMoveRight(TMP_Text buttonText)
//     {
//         StartRebinding("Player/Move", "right", buttonText);
//     }

//     public void StartRebindingJump(TMP_Text buttonText)
//     {
//         StartRebinding("Player/Jump", "", buttonText);
//     }

//     public void StartRebindingSprint(TMP_Text buttonText)
//     {
//         StartRebinding("Player/Sprint", "", buttonText);
//     }

//     public void StartRebindingPause(TMP_Text buttonText)
//     {
//         StartRebinding("Player/Pause", "", buttonText);
//     }

//     public void StartRebindingFire(TMP_Text buttonText)
//     {
//         StartRebinding("Player/Fire", "", buttonText);
//     }

//     public void StartRebindingInventory(TMP_Text buttonText)
//     {
//         StartRebinding("Player/Inventory", "", buttonText);
//     }

//     public void StartRebinding(string actionName, string compositePart, TMP_Text buttonText)
//     {
//         if (inputActions == null)
//         {
//             Debug.LogError("InputActions is not initialized.");
//             return;
//         }

//         var action = inputActions.asset.FindAction(actionName, true);
//         if (action == null)
//         {
//             Debug.LogError($"Action '{actionName}' not found.");
//             return;
//         }

//         int bindingIndex = FindBindingIndex(action, compositePart);
//         if (bindingIndex == -1)
//         {
//             Debug.LogError($"Binding index not found for action '{actionName}' and part '{compositePart}'.");
//             return;
//         }

//         rebindingPopup.SetActive(true);
//         action.Disable();

//         rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
//             .OnComplete(operation =>
//             {
//                 buttonText.text = operation.selectedControl.displayName;
//                 rebindingPopup.SetActive(false);
//                 action.Enable();
//                 operation.Dispose();
//                 rebindingOperation = null;

//                 SaveBindings();
//             })
//             .OnCancel(operation =>
//             {
//                 rebindingPopup.SetActive(false);
//                 operation.Dispose();
//                 rebindingOperation = null;
//             })
//             .Start();
//     }

//     private int FindBindingIndex(InputAction action, string compositePart)
//     {
//         if (string.IsNullOrEmpty(compositePart)) return 0;
//         for (int i = 0; i < action.bindings.Count; i++)
//         {
//             if (action.bindings[i].isPartOfComposite && action.bindings[i].name == compositePart)
//             {
//                 return i;
//             }
//         }
//         return -1;
//     }

//     public void CancelRebinding()
//     {
//         rebindingOperation?.Cancel();
//     }

//     public void SaveBindings()
//     {
//         if (inputActions != null)
//         {
//             string rebinds = inputActions.SaveBindingOverridesAsJson();
//             PlayerPrefs.SetString(RebindsKey, rebinds);
//             PlayerPrefs.Save();
//             Debug.Log("Bindings saved successfully.");
//         }
//     }

//     public void LoadBindings()
//     {
//         // Initialize inputActions if it's null
//         if (inputActions == null)
//         {
//             Debug.LogWarning("inputActions is null during LoadBindings. Initializing now.");
//             inputActions = new GameDevCW();
//         }

//         if (PlayerPrefs.HasKey(RebindsKey))
//         {
//             try
//             {
//                 string bindings = PlayerPrefs.GetString(RebindsKey);
//                 inputActions.LoadBindingOverridesFromJson(bindings);
//                 Debug.Log("Bindings loaded successfully.");
//             }
//             catch (System.Exception ex)
//             {
//                 Debug.LogError($"Failed to load bindings: {ex.Message}. Resetting to defaults.");
//                 ResetBindings();
//             }
//         }
//         else
//         {
//             Debug.LogWarning("No saved bindings found in PlayerPrefs.");
//         }
//     }

//     public void ResetBindings()
//     {
//         inputActions.asset.RemoveAllBindingOverrides();
//         PlayerPrefs.DeleteKey(RebindsKey);
//         Debug.Log("Bindings reset to defaults.");
//     }
// }
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class KeyBindings : MonoBehaviour
{
    // [SerializeField] private GameDevCW inputActions; // Reference to the InputActions asset
    [SerializeField] private GameObject rebindingPopup; // The popup UI
    [SerializeField] private Button cancelButton; // Cancel button in the popup

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    // private const string PlayerPrefsKey = "InputBindings"; // Key for PlayerPrefs

    private void Awake()
    {
        // // Initialize InputActions
        // if (inputActions == null)
        // {
        //     inputActions = new GameDevCW();
        // }

        // Ensure the popup is hidden initially
        rebindingPopup.SetActive(false);

        // Add listener to cancel button
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelRebinding);
        }
    }
    // private void Start()
    // {
    //     // Load bindings on game start
    //     LoadBindings();
    // }
    public void StartRebinding(string actionName, string compositePart, TMP_Text buttonText)
    {
        var inputActions = InputManager.Instance.inputActions;

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

                // Save bindings after rebinding
                InputManager.Instance.SaveBindings();
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

    // public void SaveBindings()
    // {
    //     if (inputActions != null)
    //     {
    //         string bindingsJson = inputActions.asset.SaveBindingOverridesAsJson();
    //         PlayerPrefs.SetString(PlayerPrefsKey, bindingsJson);
    //         PlayerPrefs.Save();
    //         Debug.Log("Bindings saved.");
    //     }
    //     else
    //     {
    //         Debug.LogError("InputActions is not initialized. Unable to save bindings.");
    //     }
    // }

    // public void LoadBindings()
    // {
    //     if (PlayerPrefs.HasKey(PlayerPrefsKey))
    //     {
    //         string bindingsJson = PlayerPrefs.GetString(PlayerPrefsKey);
    //         if (!string.IsNullOrEmpty(bindingsJson))
    //         {
    //             inputActions.asset.LoadBindingOverridesFromJson(bindingsJson);
    //             Debug.Log("Bindings loaded and applied.");
    //         }
    //         else
    //         {
    //             Debug.LogWarning("Bindings JSON is empty. Using default bindings.");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("No saved bindings found. Using default bindings.");
    //     }
    // }

    // public void ResetBindings()
    // {
    //     inputActions.asset.RemoveAllBindingOverrides();
    //     Debug.Log("Bindings reset.");
    // }

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

    public void StartRebindingSprint(TMP_Text buttonText)
    {
        StartRebinding("Player/Sprint", "", buttonText); // Action: Sprint, Binding Index: 0
    }

    public void StartRebindingPause(TMP_Text buttonText)
    {
        StartRebinding("Player/Pause", "", buttonText); // Action: Pause, Binding Index: 0
    }
 
    public void StartRebindingAttack(TMP_Text buttonText)
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
        StartRebinding("Player/Attack", "", buttonText); // Action: Inventory, Binding Index: 0
    }

    public void StartRebindingToggle(TMP_Text buttonText)
    {
        StartRebinding("Player/ToggleCamera", "", buttonText); // Action: Inventory, Binding Index: 0
    }

    public void StartRebindingBlock(TMP_Text buttonText)
    {
        StartRebinding("Player/Block", "", buttonText); // Action: Inventory, Binding Index: 0
    }

    public void StartRebindingInteract(TMP_Text buttonText)
    {
        StartRebinding("Player/Interact", "", buttonText); // Action: Inventory, Binding Index: 0
    }

    public void StartRebindingObjective(TMP_Text buttonText)
    {
        StartRebinding("Player/Objective", "", buttonText); // Action: Inventory, Binding Index: 0
    }

    public void StartRebindingLockOn(TMP_Text buttonText)
    {
        StartRebinding("Player/LockOn", "", buttonText); // Action: Inventory, Binding Index: 0
    }

    public void StartRebindingDodge(TMP_Text buttonText)
    {
        //StartRebinding("Player/Dodge", "", buttonText); // Action: Inventory, Binding Index: 0
    }
}