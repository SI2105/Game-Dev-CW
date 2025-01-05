using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public GameDevCW inputActions { get; private set; }
    private const string PlayerPrefsKey = "InputBindings";

    public static InputManager Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        inputActions = new GameDevCW();
        LoadBindings();
    }

    public void LoadBindings()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            string bindingsJson = PlayerPrefs.GetString(PlayerPrefsKey);
            if (!string.IsNullOrEmpty(bindingsJson))
            {
                inputActions.asset.LoadBindingOverridesFromJson(bindingsJson);
                Debug.Log("Bindings loaded from PlayerPrefs.");
            }
        }
        else
        {
            Debug.Log("No saved bindings found. Using defaults.");
        }
    }

    public void SaveBindings()
    {
        string bindingsJson = inputActions.asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(PlayerPrefsKey, bindingsJson);
        PlayerPrefs.Save();
        Debug.Log("Bindings saved to PlayerPrefs.");
    }
}