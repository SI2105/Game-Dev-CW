using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipmentManager : MonoBehaviour
{
    public WeaponModelInstantiationSlot rightHandSlot;
    private InventoryManager inventoryManager;
    public GameObject rightHandWeaponModel;

    void Awake()
    {
        inventoryManager = GetChildComponent<InventoryManager>();

        if (inventoryManager != null)
        {
            inventoryManager.onSelectedItemChanged.AddListener(LoadRightWeapon);
        }
    }

    void Start(){
        LoadRightWeapon();
    }

    public void LoadRightWeapon(){
        if (rightHandWeaponModel != null){
            Destroy(rightHandWeaponModel);
        }

        if (inventoryManager.SelectedItem !=null){
            rightHandWeaponModel = Instantiate(inventoryManager.SelectedItem.prefab);
            rightHandSlot.LoadWeapon(rightHandWeaponModel);
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

