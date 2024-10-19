using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private GameObject interaction_information_ui;
    private TextMeshProUGUI interaction_text;
    public Camera mainCamera;
    private Mouse mouse;
    
    private float raycastDistance = 100f;

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

        Vector2 mousePosition = mouse.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance)){
            Debug.Log("hit object: " + hit.transform.name);
            Transform selectionTransform = hit.transform;
            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                interaction_information_ui.SetActive(true);
                interaction_text.text = interactable.gameObject.name;
                Debug.Log("Interactable Object found, displaying UI.");
            }
            else
            {
                interaction_information_ui.SetActive(false);
                Debug.Log("Interactable Object not found, displaying UI.");
            }
        }
        else{
            Debug.Log("No object hit by raycast.");
            interaction_information_ui.SetActive(false);
        }
    }
}
