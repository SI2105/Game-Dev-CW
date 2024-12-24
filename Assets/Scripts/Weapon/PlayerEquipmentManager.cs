using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipmentManager : MonoBehaviour
{
    public WeaponModelInstantiationSlot rightHandSlot;
    private InventoryManager inventoryManager;
    public GameObject rightHandWeaponModel;
    private AnimationLayerController animationLayerController;
    public PlayerAnimationManager animationManager;

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

    void Start(){
        LoadRightWeapon();
    }

    public void LoadRightWeapon(){
        if (rightHandWeaponModel != null){
            Destroy(rightHandWeaponModel);
        }

        if (inventoryManager.SelectedItem !=null){
            WeaponClass weapon = inventoryManager.SelectedItem.GetWeapon();

            if (weapon != null){
                animationLayerController.ActivateWeaponOverride();
                if (weapon.weaponType == WeaponClass.WeaponType.Sword){
                    rightHandWeaponModel = Instantiate(inventoryManager.SelectedItem.prefab);
                    rightHandSlot.LoadWeapon(rightHandWeaponModel);
                    animationManager.PlayUnsheathAnimation();
                }
            } else {
                animationLayerController.DeactivateWeaponOverride();
                animationManager.PlaySheatheAnimation();
            }
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

