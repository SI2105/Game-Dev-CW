using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SG
{
    public class CombatManager : MonoBehaviour
    {
        private InventoryManager inventoryManager;

        void Awake()
        {
            inventoryManager = GetChildComponent<InventoryManager>();

            if (inventoryManager != null)
            {
                inventoryManager.onSelectedItemChanged.AddListener(LoadWeapon);
            }
        }


        void LoadWeapon()
        {
            var currentItem = inventoryManager.SelectedItem;

            if (currentItem != null)
            {
                Debug.Log("Loaded Item: " + currentItem.name);
                
            }
            else
            {
                Debug.LogWarning("No item selected in the hotbar!");
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
}