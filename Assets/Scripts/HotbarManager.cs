using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro; 

public class HotbarManager : MonoBehaviour {

    // List of UI slots representing the hotbar items
    public List<Slot> slots = new List<Slot>();
    
    public PlayerATH playerATH;

    /// <summary>
    /// Initializes the hotbar UI when the game starts.
    /// </summary>
    void Start(){
        if (playerATH != null){
            UpdateHotBarUI(); // Populate hotbar with current inventory items
        }
        else{
            Debug.Log("ATH is not assigned"); // Warn if playerATH is missing
        }
    }

    /// <summary>
    /// Updates the hotbar UI to reflect the current inventory items.
    /// </summary>
    public void UpdateHotBarUI(){
        for (int i = 0; i < slots.Count; i++){
            // Get the name of the inventory item at the current index
            string iName = playerATH.GetInventoryItemName(i);

            if (!string.IsNullOrEmpty(iName)){
                slots[i].itemText.text = iName; // Display item name in slot
                Debug.Log("Updating hotbar");
            }
            else{
                slots[i].itemText.text = "Empty"; // Indicate empty slot
            }
        }
    }
}
