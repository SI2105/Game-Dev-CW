using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro; 

public class HotbarManager : MonoBehaviour {

    public List<Slot> slots = new List<Slot>();
    public PlayerATH playerATH;

    void Start(){
        if (playerATH != null){
            UpdateHotBarUI();
        }
        else{
            Debug.Log("ATH is not assigned");
        }

    }

    public void UpdateHotBarUI(){
        for (int i = 0; i<slots.Count; i++){
            Debug.Log("here");

            string iName = playerATH.GetInventoryItemName(i);

            if (iName != null && iName != ""){
                slots[i].itemText.text = iName;
                Debug.Log("Updating hotbar");
            }else{
                slots[i].itemText.text = "Empty";
            }
        }
    }
}