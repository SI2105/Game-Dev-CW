using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG
{

    public class PlayerEquipmentManager : MonoBehaviour
    {
        public WeaponModelInstantiationSlot rightHandSlot;
        private InventoryManager inventoryManager;
        public GameObject rightHandWeaponModel;
        private AnimationLayerController animationLayerController;
        private PlayerAnimationManager animationManager;
        private PlayerState playerState;

        // How long to wait for the unsheath animation (if not using animation events).
        [SerializeField] private float unsheathAnimationDuration = 0.24f;

        private object currentEquippedItem; // Tracks the currently equipped item

        void Awake()
        {
            // Grab references
            inventoryManager = GetChildComponent<InventoryManager>();
            animationLayerController = GetComponent<AnimationLayerController>();
            animationManager = GetComponent<PlayerAnimationManager>();
            playerState = GetComponent<PlayerState>();
            Debug.Log("player state" + playerState);
            // Listen for item changes
            if (inventoryManager != null)
            {
                // Instead of directly calling LoadRightWeapon, we’ll call LoadWeaponWithUnsheath
                inventoryManager.onSelectedItemChanged.AddListener(LoadWeaponWithUnsheath);
            }
        }

        void Start()
        {
            // On start, do the unsheath/load sequence
            LoadWeaponWithUnsheath();
        }

        /// <summary>
        /// Call this to start the full sequence:
        /// 1) Unsheath animation
        /// 2) Wait
        /// 3) Load weapon
        /// </summary>
        public void LoadWeaponWithUnsheath()
        {
            // If no item or inventory manager, just deactivate and stop
            if (inventoryManager == null || inventoryManager.SelectedItem == null)
            {
                animationLayerController.DeactivateWeaponOverride();
                return;
            }

            // Start coroutine to unsheath, wait, then load
            StartCoroutine(LoadWeaponSequence());
        }

        /// <summary>
        /// Coroutine to play the unsheath animation, wait for it to complete, then load the weapon.
        /// </summary>
        private IEnumerator LoadWeaponSequence()
        {
            // 1) Play the unsheath animation
            animationManager.PlayUnsheathAnimation();

            // 2) Wait for unsheath to finish.
            yield return new WaitForSeconds(unsheathAnimationDuration);

            // 3) Now that unsheath is done, load the weapon
            LoadRightWeapon_Internal();

            // 4) (Optional) Reset or stop the unsheath animation if needed
            animationManager.PlaySheatheAnimation();
        }

        /// <summary>
        /// Internal method that actually creates and attaches the weapon.
        /// This is the old LoadRightWeapon() logic.
        /// </summary>
        private void LoadRightWeapon_Internal()
        {
            // Clean up existing weapon model
            if (rightHandWeaponModel != null)
            {
                Destroy(rightHandWeaponModel);
            }

            // Safety check for inventory manager and selected item
            if (inventoryManager == null || inventoryManager.SelectedItem == null)
            {
                animationLayerController.DeactivateWeaponOverride();
                return;
            }

            // Try to get weapon from selected item
            WeaponClass weapon = null;
            ConsumableClass consumable = null;

            try
            {
                weapon = inventoryManager.SelectedItem.GetWeapon();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get weapon from selected item: {e.Message}");
            }

            try
            {
                consumable = inventoryManager.SelectedItem.GetConsumable();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get consumable from selected item: {e.Message}");
            }

            if ((weapon != null && weapon.prefab != null) ||
                (consumable != null && consumable.prefab != null))
            {
                Debug.Log($"Loading weapon: {(weapon != null && weapon.prefab != null ? weapon.prefab.name : "None")}");
                Debug.Log($"Loading consumable: {(consumable != null && consumable.prefab != null ? consumable.prefab.name : "None")}");

                // Example: only load swords. Adjust this if logic differs in your game.
                if (weapon != null && weapon.weaponType == WeaponClass.WeaponType.Sword)
                {
                    if (rightHandSlot == null)
                    {
                        Debug.LogError("RightHandSlot is not assigned in the Inspector.");
                        return;
                    }

                    // Choose whether to load the weapon or the consumable prefab
                    GameObject toLoad = (weapon != null && weapon.prefab != null)
                        ? weapon.prefab
                        : consumable.prefab;

                    rightHandWeaponModel = Instantiate(toLoad);
                    rightHandSlot.LoadWeapon(rightHandWeaponModel);

                    // Get component from the instantiated weapon model
                    WeaponCollisionHandler weaponHandler = rightHandWeaponModel.GetComponent<WeaponCollisionHandler>();

                    // Assign the playerState to the weapon's WeaponCollisionHandler
                    if (weaponHandler != null)
                    {
                        Debug.Log($"PlayerState type: {playerState.GetType()}");
                        Debug.Log($"WeaponHandler._playerState type: {weaponHandler._playerState?.GetType()}");
                        weaponHandler._playerState = playerState;
                    }
                }
            }
        }

        /// <summary>
        /// Tracks the currently equipped item of a given type (WeaponClass, ConsumableClass, etc.).
        /// </summary>
        public T TrackEquippedItem<T>() where T : class
        {
            if (inventoryManager == null || inventoryManager.SelectedItem == null)
            {
                if (currentEquippedItem != null)
                {
                    currentEquippedItem = null;
                }
                return null; // No item equipped
            }

            // Try to get the item as either a consumable or weapon
            T newEquippedItem = inventoryManager.SelectedItem.GetConsumable() as T
                                ?? inventoryManager.SelectedItem.GetWeapon() as T;

            // Check if the equipped item has changed
            if (!EqualityComparer<T>.Default.Equals(currentEquippedItem as T, newEquippedItem))
            {
                currentEquippedItem = newEquippedItem; // Update the equipped item
            }

            return newEquippedItem; // Return the equipped item
        }

        /// <summary>
        /// Checks if the currently equipped item is a heal item (consumable).
        /// </summary>
        public bool IsEquippedItemHeal()
        {
            if (inventoryManager == null || inventoryManager.SelectedItem == null)
            {
                Debug.Log("No item is equipped.");
                return false;
            }

            ConsumableClass consumable = null;
            try
            {
                consumable = inventoryManager.SelectedItem.GetConsumable();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get consumable from selected item: {e.Message}");
            }

            // Example check if the consumable is a healing item. 
            // Adjust if your class has a different property, e.g. consumable.IsHealingItem
            if (consumable != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Helper to retrieve a component from a child GameObject.
        /// </summary>
        private T GetChildComponent<T>() where T : Component
        {
            foreach (Transform child in transform)
            {
                var component = child.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
            return null;
        }
    }
}