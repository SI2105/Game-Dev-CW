using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    // Public References
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject interaction_information_ui;
    
    // Text 
    private TextMeshProUGUI interaction_text;
    
    // Physics variables
    private Mouse mouse;
    private float raycastDistance = 10f;
    private InteractableObject currInteractable;

    // Inventory of interactable items
    private List<InteractableObject> collectedItems = new List<InteractableObject>();

    private void Start()
    {
        mouse = Mouse.current;
        interaction_text = interaction_information_ui.GetComponent<TextMeshProUGUI>();
        if (interaction_text == null)
        {
            Debug.LogError("TextMeshProUGUI component not found in interaction_information_ui");
        }
    }

    void Update()
    {
        if (mouse == null)
            return;
        
        if (mainCamera == null)
            return;
        
        DetectObject(); // Keeps track of the item the raycast hits and assigns it to currInteractable
        if (Keyboard.current.eKey.wasPressedThisFrame && currInteractable){
            CollectObject(currInteractable); // Collects the item if the raycast hits the object and user presses e at the same time
        }
    }

    void DetectObject()
    {
        
        Vector2 mousePosition = mouse.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition); // ray follows mouse position
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance)){ // checks if the ray hits an object within 100f distance
            // Debug.Log("hit object: " + hit.transform.name);
            Transform selectionTransform = hit.transform;
            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>(); // check if interacted object has an interactable component

            if (interactable != null)
            {
                currInteractable = interactable;
                interaction_information_ui.SetActive(true);
                interaction_text.text = "Press [E] to collect " + interactable.gameObject.name; // set ui text
                // Debug.Log("Interactable Object found, displaying UI.");
            }
            else
            {
                currInteractable = null;
                interaction_information_ui.SetActive(false);
                // Debug.Log("Interactable Object not found, displaying UI.");
            }
        }
        else{
            currInteractable = null;
            interaction_information_ui.SetActive(false);
        }
    }

    void CollectObject(InteractableObject interactable)
    {
        collectedItems.Add(interactable); // adds item to the inventory
        Destroy(interactable.gameObject); // destroys the item

        // reset
        interaction_information_ui.SetActive(false);
        currInteractable = null;
    }

    void Hit(){

    }
}
