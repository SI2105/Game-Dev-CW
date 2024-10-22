using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    // Public References
    [SerializeField] private Camera mainCamera; // Main camera for raycasting
    [SerializeField] private GameObject interaction_information_ui; // UI element for interaction prompts
    
    // Text
    private TextMeshProUGUI interaction_text; // Text component to display interaction messages
    private string state; // Current state of interactable objects
    
    // Physics variables
    private Mouse mouse; // Reference to the mouse input
    private float raycastDistance = 10f; // Maximum distance for raycasting
    private InteractableObject currInteractable; // Currently selected interactable object

    // Inventory of interactable items
    private List<InteractableObject> collectedItems = new List<InteractableObject>();

    public PlayerATH playerATH; // Reference to the player's ATH (Attribute HUD)

    /// <summary>
    /// Initializes references and checks for necessary components.
    /// </summary>
    private void Start()
    {
        mouse = Mouse.current; // Get the current mouse
        interaction_text = interaction_information_ui.GetComponent<TextMeshProUGUI>(); // Get the TextMeshProUGUI component

        if (interaction_text == null)
        {
            Debug.LogError("TextMeshProUGUI component not found");
        }

        if (playerATH == null)
        {
            Debug.Log("PlayerATH is not set");
        }
    }

    /// <summary>
    /// Handles input and object detection every frame.
    /// </summary>
    void Update()
    {
        if (mouse == null || mainCamera == null)
            return;

        DetectObject(); // Detects objects under the mouse cursor

        // Check for interaction input
        if (Keyboard.current.eKey.wasPressedThisFrame && currInteractable)
        {
            CollectObject(currInteractable); // Collect the interactable object
        }
    }

    /// <summary>
    /// Detects objects under the mouse cursor using raycasting.
    /// </summary>
    void DetectObject()
    {
        Vector2 mousePosition = mouse.position.ReadValue(); // Get current mouse position
        Ray ray = mainCamera.ScreenPointToRay(mousePosition); // Create a ray from the camera through the mouse position
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            Transform selectionTransform = hit.transform;
            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();
            EnemyController enemy = selectionTransform.GetComponent<EnemyController>();
            Chest chest = selectionTransform.GetComponent<Chest>();

            if (interactable != null)
            {
                currInteractable = interactable;
                interaction_information_ui.SetActive(true);
                interaction_text.text = "Press [E] to collect " + interactable.gameObject.name; // Prompt to collect item
            }
            else if (enemy != null)
            {
                currInteractable = null;
                interaction_information_ui.SetActive(true);
                interaction_text.text = enemy.gameObject.name; // Display enemy name
            }
            else if (chest != null)
            {
                interaction_information_ui.SetActive(true);
                interaction_text.text = "Press [E] to open chest"; // Prompt to open chest
                state = chest.getState();

                if (!string.IsNullOrEmpty(state))
                {
                    interaction_text.text = state; // Display chest state
                }
                else if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    chest.OpenChest(); // Open the chest on input
                }
            }
            else
            {
                currInteractable = null;
                interaction_information_ui.SetActive(false); // Hide UI if no interactable object
            }
        }
        else
        {
            currInteractable = null;
            interaction_information_ui.SetActive(false); // Hide UI if raycast hits nothing
        }
    }

    /// <summary>
    /// Collects the specified interactable object and updates the inventory.
    /// </summary>
    void CollectObject(InteractableObject interactable)
    {
        playerATH.CollectItem(interactable.gameObject.name); // Add item to inventory
        Destroy(interactable.gameObject); // Remove the item from the scene

        // Reset interaction state
        interaction_information_ui.SetActive(false);
        currInteractable = null;
    }
}
