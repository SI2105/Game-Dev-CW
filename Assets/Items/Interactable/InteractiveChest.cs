using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractiveChest : MonoBehaviour
{
    // Reference to the chest game object or its animator
    public string chestName = "Chest"; // Customizable chest name
    public bool isOpen = false; // State to track if the chest is open
    [SerializeField] private ItemClass[] PossibleItems; // Array of possible items that can be in the chest
    public List<SlotClass> Items = new List<SlotClass>(); // List of items in the chest
    public GameObject ItemsContainer;
    public GameObject ChestPanel;
    [SerializeField]private GameObject ItemPrefab;
    public InventoryManager Inventory;
    private Button collectAllButton;
    private bool Looted;
    private float RareItemDropChance;
    private Button CloseButton;
    // Called when the object is interacted with
    public void Start()
    {
        
        Looted = false;
        RareItemDropChance = 0.0f;
        //ChestPanel = GameObject.Find("ChestPanel");
        //ItemsContainer = GameObject.Find("ItemsContainer");
        Inventory = GameObject.Find("Inventory").GetComponent<InventoryManager>();
        collectAllButton = ChestPanel.transform.GetChild(2).GetComponent<Button>();
        collectAllButton.onClick.AddListener(CollectAll);
        CloseButton = ChestPanel.transform.GetChild(3).gameObject.GetComponent<Button>();
        CloseButton.onClick.AddListener(CloseChest);
        ChestPanel.SetActive(false);

        System.Random random = new System.Random();
        int itemCount = random.Next(1, 5); // Random number of items between 1 and 4
        for (int i = 0; i < itemCount; i++)
        {
            ItemClass item = PossibleItems[random.Next(PossibleItems.Length)];
            int quantity = 1;


            // Check if the item is stackable
            if (item.Stackable)
            {
                quantity = random.Next(1, 5); // Random quantity between 1 and 4 for stackable items
            }
            // Make weapons very rare
            else if (item is WeaponClass)
            {
                if (random.NextDouble() > RareItemDropChance) // 5% chance to get a weapon, to make it rare
                {
                    continue; // Skip this iteration if the weapon is not selected
                }
            }

            Items.Add(new SlotClass(item, quantity));
        }


    }
    public void Interact()
    {
        if (isOpen)
        {
            CloseChest();
        }
        else
        {
            OpenChest();
        }
    }

    // Opens the chest
    public void OpenChest()
    {
       
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log($"{chestName} opened.");
        RefreshChest();
        ChestPanel.SetActive(true);
        isOpen = true;
    }

    // Closes the chest
    public void CloseChest()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log($"{chestName} closed.");
        ChestPanel.SetActive(false);
        isOpen = false;
    }

    // This method can be triggered by a raycast manager or selector
    public void OnSelect()
    {
        //Check if the chest is already looted, and prevent access
        if (Looted == false)
        {
            Interact();
        }

        else {
            print("Already Looted");
        }
           
    }

    public void RefreshChest()
    {
        //Used for Refreshing the Chest UI, including closing it if the chest is empty
        foreach (Transform child in ItemsContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (SlotClass item in Items)
        {
            GameObject ChestItem = Instantiate(ItemPrefab, ItemsContainer.transform);
            ChestItem.transform.GetChild(1).GetComponent<Image>().sprite = item.GetItem().icon;
            ChestItem.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = item.GetItem().displayName;
            ChestItem.transform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text = item.GetQuantity().ToString();
            ChestItem.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => {
                Inventory.Add(item.GetItem(), item.GetQuantity());
                Items.Remove(item);
                
                
                RefreshChest();
            }); //Button has a Listener added that add Item to Inventory, Removes from Chest and Refreshes the Chest once complete.

            if (item.GetItem() is ConsumableClass consumable && consumable.IsPotion && Inventory.FullForPotions())
            {
                
                ChestItem.transform.GetChild(4).GetComponent<Button>().interactable = false;
                ChestItem.transform.GetChild(4).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Inventory Full";

            }
        }

        if (Items.Count == 0)
        {
            //Chest is close upon being empty
            CloseChest();
            Looted = true;
        }
        
    }

    private void CollectAll() {
        foreach (SlotClass item in Items)
        {
            Inventory.Add(item.GetItem(), item.GetQuantity());
            Items.Remove(item);
        }
        
        RefreshChest();
        
    }

    public void SetRareItemDrop(float chance)
    {
        RareItemDropChance = chance;
    }
}
