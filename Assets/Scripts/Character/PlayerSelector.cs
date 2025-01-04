using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace SG
{
    public class PlayerSelector : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera; // The camera used for raycasting
        [SerializeField] private float maxRaycastDistance = 5f; // Max distance for raycasting
        [SerializeField] private TextMeshProUGUI interactableNameUI; // UI element to display the name of the interactable
        [SerializeField] private PopupMessageManager popupMessageManager; // Reference to the PopupMessageManager
        private GameDevCW inputActions; // Input action class reference
        private PlayerAttributesManager playerAttributesManager; // The player attributes manager
        private PlayerChestSensor ChestSensor;

        private void Awake()
        {
            inputActions = new GameDevCW();
            ChestSensor = GetComponent<PlayerChestSensor>();
            playerAttributesManager = GetComponent<PlayerAttributesManager>();
            inputActions.Player.Interact.performed += ctx => InteractWithInteractable();
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void Update()
        {
            CheckForInteractable();
        }

        private void CheckForInteractable()
        {
            if (ChestSensor.objects.Count > 0)
            {
                InteractiveChest chest = ChestSensor.objects[0].GetComponent<InteractiveChest>();
                CollectibleData collectible = ChestSensor.objects[0].GetComponent<CollectibleData>();

                if (chest != null)
                {
                    interactableNameUI.text = chest.chestName;
                }
                else if (collectible != null)
                {
                    interactableNameUI.text = collectible.collectible.displayName;
                }
                else
                {
                    interactableNameUI.text = "";
                }
            }
            else
            {
                interactableNameUI.text = "";
            }
            //Vector2 mousePos = Mouse.current.position.ReadValue();
            //Ray ray = playerCamera.ScreenPointToRay(mousePos);
            ////Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            //RaycastHit hit;

            //Debug.DrawRay(ray.origin, ray.direction * maxRaycastDistance, Color.green); // Debug ray visualization

            //if (Physics.Raycast(ray, out hit, maxRaycastDistance))
            //{
            //    InteractiveChest chest = hit.collider.GetComponent<InteractiveChest>();
            //    CollectibleData collectible = hit.collider.GetComponent<CollectibleData>();

            //    if (chest != null)
            //    {
            //        interactableNameUI.text = chest.chestName;

            //        // Display the name of the interactable chest on the UI

            //    }
            //    //else
            //    //{
            //    //    interactableNameUI.text = "";
            //    //}


            //    else if (collectible != null)
            //    {
            //        // Display the name of the collectible on the UI
            //        interactableNameUI.text = collectible.collectible.displayName;
            //    }
            //    else
            //    {
            //        interactableNameUI.text = "";
            //    }
            //}
            //else
            //{
            //    // Clear the UI if no chest or collectible is hit
            //    interactableNameUI.text = "";
            //}
        }

        private void InteractWithInteractable()
        {
            if (ChestSensor.objects.Count > 0)
            {
                InteractiveChest chest = ChestSensor.objects[0].GetComponent<InteractiveChest>();
                CollectibleData collectible = ChestSensor.objects[0].GetComponent<CollectibleData>();

                if (chest != null)
                {
                    chest.OnSelect();
                }
                else if (collectible != null)
                {
                    if (popupMessageManager != null)
                    {
                        popupMessageManager.ShowPopup(collectible.collectible.GetCollectible());
                    }
                    playerAttributesManager.InventoryManager.Add(collectible.collectible.GetCollectible(), 1);



                    // Destroy the associated GameObject
                    Destroy(collectible.gameObject);
                }
            
            }
        }

    }
}
