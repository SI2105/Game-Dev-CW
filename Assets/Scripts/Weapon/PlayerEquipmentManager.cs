using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipmentManager : MonoBehaviour
{
    public WeaponModelInstantiationSlot rightHandSlot;
    private InventoryManager inventoryManager;
    public GameObject rightHandWeaponModel;
    private AnimationLayerController animationLayerController;
    private PlayerAnimationManager animationManager;

    void Awake()
    {
        inventoryManager = GetChildComponent<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.onSelectedItemChanged.AddListener(LoadRightWeapon);
        }
        animationLayerController = GetComponent<AnimationLayerController>();
        animationManager = GetComponent<PlayerAnimationManager>();
    }

    void Start()
    {
        LoadRightWeapon();
    }

    private object currentEquippedItem; // Tracks the currently equipped item

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

    public void LoadRightWeapon()
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
            animationManager.PlaySheatheAnimation();
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

        try{
            consumable = inventoryManager.SelectedItem.GetConsumable();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to get weapon from selected item: {e.Message}");
        }

        if ((weapon != null && weapon.prefab != null) || (consumable != null && consumable.prefab != null))
        {
            Debug.Log($"Loading weapon: {(weapon != null && weapon.prefab != null ? weapon.prefab.name : "None")}");
            Debug.Log($"Loading consumable: {(consumable != null && consumable.prefab != null ? consumable.prefab.name : "None")}");

            if (weapon != null && weapon.weaponType == WeaponClass.WeaponType.Sword)
            {
                if (rightHandSlot == null)
                {
                    Debug.LogError("RightHandSlot is not assigned in the Inspector.");
                    return;
                }

                GameObject toLoad = (weapon != null && weapon.prefab != null) 
                    ? weapon.prefab 
                    : consumable.prefab;

                rightHandWeaponModel = Instantiate(toLoad);
                rightHandSlot.LoadWeapon(rightHandWeaponModel);

                animationLayerController.ActivateWeaponOverride();
                animationManager.PlayUnsheathAnimation();
            }
        }
    }

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

        // Check if the consumable is a healing item
        if (consumable != null) // Assuming IsHealingItem is a bool property of ConsumableClass
        {
            return true;
        }

        return false;
    }


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