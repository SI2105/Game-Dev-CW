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

        private void Awake()
        {
            inputActions = new GameDevCW();
            playerAttributesManager = GetComponent<PlayerAttributesManager>();
            inputActions.Player.Interact.performed += ctx => InteractWithChest();
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
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * maxRaycastDistance, Color.green); // Debug ray visualization

            if (Physics.Raycast(ray, out hit, maxRaycastDistance))
            {
                InteractiveChest chest = hit.collider.GetComponent<InteractiveChest>();
                if (chest != null)
                {
                    // Display the name of the interactable chest on the UI
                    interactableNameUI.text = chest.chestName;
                }
                else
                {
                    interactableNameUI.text = "";
                }

                CollectibleData collectible = hit.collider.GetComponent<CollectibleData>();
                if (collectible != null)
                {
                    // Display the name of the collectible on the UI
                    interactableNameUI.text = collectible.collectible.displayName;
                }
                else
                {
                    interactableNameUI.text = "";
                }
            }
            else
            {
                // Clear the UI if no chest or collectible is hit
                interactableNameUI.text = "";
            }
        }

        private void InteractWithChest()
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxRaycastDistance))
            {
                InteractiveChest chest = hit.collider.GetComponent<InteractiveChest>();

                if (chest != null)
                {
                    chest.OnSelect();
                }

                CollectibleData collectible = hit.collider.GetComponent<CollectibleData>();
                if (collectible != null)
                {
                    print("collectible found");
                    // Add the collectible to the inventory
                    if (popupMessageManager != null)
                    {
                        popupMessageManager.ShowPopup(collectible.collectible.GetCollectible());
                    }
                    playerAttributesManager.InventoryManager.Add(collectible.collectible.GetCollectible(), 1);



                    // Destroy the associated GameObject
                    Destroy(hit.collider.gameObject);
                }
            }
        }

    }
}
