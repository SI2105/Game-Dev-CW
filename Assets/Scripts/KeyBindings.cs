using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class KeyBindings : MonoBehaviour
{
    public static KeyBindings Instance { get; private set; } // Singleton instance

    [SerializeField] private GameDevCW inputActions; // Reference to the InputActions asset
    [SerializeField] private GameObject rebindingPopup; // The popup UI
    [SerializeField] private Button cancelButton; // Cancel button in the popup

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    private const string RebindsKey = "InputRebinds";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (inputActions == null)
        {
            Debug.Log("Initializing inputActions in KeyBindings...");
            inputActions = new GameDevCW();
        }

        if (rebindingPopup != null)
        {
            rebindingPopup.SetActive(false);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelRebinding);
        }

        LoadBindings();
    }

    public void StartRebindingMoveForward(TMP_Text buttonText)
    {
        StartRebinding("Player/Move", "up", buttonText);
    }

    public void StartRebindingMoveBackward(TMP_Text buttonText)
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
        StartRebinding("Player/Jump", "", buttonText);
    }

    public void StartRebindingSprint(TMP_Text buttonText)
    {
        StartRebinding("Player/Sprint", "", buttonText);
    }

    public void StartRebindingPause(TMP_Text buttonText)
    {
        StartRebinding("Player/Pause", "", buttonText);
    }

    public void StartRebindingFire(TMP_Text buttonText)
    {
        StartRebinding("Player/Fire", "", buttonText);
    }

    public void StartRebindingInventory(TMP_Text buttonText)
    {
        StartRebinding("Player/Inventory", "", buttonText);
    }

    public void StartRebinding(string actionName, string compositePart, TMP_Text buttonText)
    {
        if (inputActions == null)
        {
            Debug.LogError("InputActions is not initialized.");
            return;
        }

        var action = inputActions.asset.FindAction(actionName, true);
        if (action == null)
        {
            Debug.LogError($"Action '{actionName}' not found.");
            return;
        }

        int bindingIndex = FindBindingIndex(action, compositePart);
        if (bindingIndex == -1)
        {
            Debug.LogError($"Binding index not found for action '{actionName}' and part '{compositePart}'.");
            return;
        }

        rebindingPopup.SetActive(true);
        action.Disable();

        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(operation =>
            {
                buttonText.text = operation.selectedControl.displayName;
                rebindingPopup.SetActive(false);
                action.Enable();
                operation.Dispose();
                rebindingOperation = null;

                SaveBindings();
            })
            .OnCancel(operation =>
            {
                rebindingPopup.SetActive(false);
                operation.Dispose();
                rebindingOperation = null;
            })
            .Start();
    }

    private int FindBindingIndex(InputAction action, string compositePart)
    {
        if (string.IsNullOrEmpty(compositePart)) return 0;
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (action.bindings[i].isPartOfComposite && action.bindings[i].name == compositePart)
            {
                return i;
            }
        }
        return -1;
    }

    public void CancelRebinding()
    {
        rebindingOperation?.Cancel();
    }

    public void SaveBindings()
    {
        if (inputActions != null)
        {
            string rebinds = inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(RebindsKey, rebinds);
            PlayerPrefs.Save();
            Debug.Log("Bindings saved successfully.");
        }
    }

    public void LoadBindings()
    {
        // Initialize inputActions if it's null
        if (inputActions == null)
        {
            Debug.LogWarning("inputActions is null during LoadBindings. Initializing now.");
            inputActions = new GameDevCW();
        }

        if (PlayerPrefs.HasKey(RebindsKey))
        {
            try
            {
                string bindings = PlayerPrefs.GetString(RebindsKey);
                inputActions.LoadBindingOverridesFromJson(bindings);
                Debug.Log("Bindings loaded successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load bindings: {ex.Message}. Resetting to defaults.");
                ResetBindings();
            }
        }
        else
        {
            Debug.LogWarning("No saved bindings found in PlayerPrefs.");
        }
    }

    public void ResetBindings()
    {
        inputActions.asset.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(RebindsKey);
        Debug.Log("Bindings reset to defaults.");
    }
}
