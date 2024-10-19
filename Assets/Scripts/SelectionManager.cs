using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private GameObject interaction_information_ui;
    [SerializeField] private string interaction_text;

    private Camera mainCamera;
    private Mouse mouse;

    private void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;
    }

    void Update()
    {
        if (mouse == null)
            return;
        
        Vector2 mousePosition = mouse.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)){
            Transform selectionTransform = hit.transform;
            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                interaction_text = interactable.GetItemName();
                interaction_information_ui.SetActive(true);
            }
            else
            {
                interaction_information_ui.SetActive(false);
            }
        }
        else{
            interaction_information_ui.SetActive(false);
        }
    }
}
