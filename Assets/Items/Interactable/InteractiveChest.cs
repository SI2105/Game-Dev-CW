using UnityEngine;
using UnityEngine.InputSystem;

public class InteractiveChest : MonoBehaviour
{
    // Reference to the chest game object or its animator
    public string chestName = "Chest"; // Customizable chest name
    public bool isOpen = false; // State to track if the chest is open

    // Called when the object is interacted with
    public void Interact()
    {
        if (isOpen)
        {
            CloseChest();
        }
        else
        {
            OpenChest();
        }
    }

    // Opens the chest
    private void OpenChest()
    {
        Debug.Log($"{chestName} opened.");
        isOpen = true;
    }

    // Closes the chest
    private void CloseChest()
    {
        Debug.Log($"{chestName} closed.");
        isOpen = false;
    }

    // This method can be triggered by a raycast manager or selector
    public void OnSelect()
    {
        Debug.Log($"{chestName} was selected.");
        Interact();
    }
}
