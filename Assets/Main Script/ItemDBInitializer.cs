using System.Collections.Generic;
using UnityEngine;

public class ItemDatabaseInitializer : MonoBehaviour
{
    [SerializeField] private List<ItemClass> allItems; // Populate this in the Inspector

    private static bool isInitialized = false;

    private void Awake()
    {
        if (!isInitialized)
        {
            DontDestroyOnLoad(gameObject); // Persist this GameObject across scenes
            ItemDatabase.Initialize(allItems); // Initialize the static database
            isInitialized = true; // Prevent re-initialization
            Debug.Log("ItemDatabase initialized and set to persist.");
        }

    }
}