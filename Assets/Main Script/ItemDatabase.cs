using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    // Static list of all items
    public static List<ItemClass> AllItems = new List<ItemClass>();

    // Method to initialize the database
    public static void Initialize(List<ItemClass> items)
    {
        AllItems = items;
        Debug.Log($"ItemDatabase initialized with {items.Count} items.");
        foreach (var item in items)
        {
            Debug.Log($"Item added to database: {item.displayName}");
        }
    }

    // Static method to retrieve an item by name
    public static ItemClass GetItemByName(string itemName)
    {
        foreach (ItemClass item in AllItems)
        {
            Debug.Log($"Checking item: {item.displayName} against {itemName}");
            if (item.displayName == itemName)
            {
                Debug.Log($"Found item: {itemName}");
                return item;
            }
        }

        Debug.LogWarning($"Item not found in database: {itemName}");
        return null;
    }
}
