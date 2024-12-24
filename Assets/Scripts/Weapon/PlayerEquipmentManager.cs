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
        try
        {
            weapon = inventoryManager.SelectedItem.GetWeapon();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to get weapon from selected item: {e.Message}");
        }

        if (weapon != null && weapon.prefab != null)
        {
            Debug.Log($"Loading weapon: {weapon.prefab.name}");
            
            if (weapon.weaponType == WeaponClass.WeaponType.Sword)
            {
                if (rightHandSlot == null)
                {
                    Debug.LogError("RightHandSlot is not assigned in the Inspector.");
                    return;
                }

                rightHandWeaponModel = Instantiate(weapon.prefab);
                rightHandSlot.LoadWeapon(rightHandWeaponModel);
                
                animationLayerController.ActivateWeaponOverride();
                animationManager.PlayUnsheathAnimation();
            }
        }
        else
        {
            Debug.Log($"Selected item '{inventoryManager.SelectedItem.name}' is not a weapon or has no prefab");
            animationLayerController.DeactivateWeaponOverride();
            animationManager.PlaySheatheAnimation();
        }
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