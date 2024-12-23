using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newCraftingRecipe", menuName = "CraftingRecipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Crafting Recipe") ]
    public SlotClass[] inputItems;
    public SlotClass outputItem;


    public bool CanCraft(InventoryManager inventory)
    {

        return false;
    }
    public void Craft(InventoryManager inventory) { 

    }

}
