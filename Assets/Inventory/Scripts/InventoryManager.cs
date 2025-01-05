using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using UnityEngine.Events;
using System;

namespace SG
{


    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();
        [SerializeField] private GameObject itemCursor;
        [SerializeField] private ItemClass[] itemToAdd;
        [SerializeField] private ItemClass[] itemToRemove;

        [SerializeField] private GameObject SlotHolder;
        public GameObject InventoryPanel;
        public GameObject PlayerStatsPanel;
        public GameObject CraftingPanel;
        public GameObject Overlay;
        public GameObject HotBar;
        public GameObject HotBarSelector;
        private SlotClass[] Items;

        [SerializeField] private GameObject HotBarSlotHolder;

        [SerializeField] private SlotClass[] startingItems;

        private GameObject[] slots;
        private GameObject[] HotBarslots;
        public GameObject[] HotBarSlot
        {
            get => HotBarslots;
            set => HotBarslots = value;
        }
        public GameDevCW inputActions;

        private SlotClass MovingSlot;
        private SlotClass TempSlot;
        private SlotClass OriginalSlot;
        bool isMovingItem = false;

        public UnityEvent onSelectedItemChanged;
        [SerializeField] private int selectedSlotIndex = 0;
        public int SelectedSlotIndex
        {
            get => selectedSlotIndex;
            set => selectedSlotIndex = value;
        }

        public ItemClass selectedItem;

        private int NumOfPotion;
        private int MaxNumOfPotion;

        public int numOfPotion
        {
            get => NumOfPotion;
            set => NumOfPotion = value;
        }

        public int maxNumOfPotion
        {
            get => MaxNumOfPotion;
            set => MaxNumOfPotion = value;
        }

        private void OnEnable()
        {

            Skill.OnSkillUnlocked += OnSkillUnlocked;
        }

        private void OnDisable()
        {
            Skill.OnSkillUnlocked -= OnSkillUnlocked;
        }

        private void OnSkillUnlocked(Skill.SkillName skillName)
        {

            if (skillName == Skill.SkillName.Carry2)
            {
                SetMaxNumOfPotion(2);
            }
            else if (skillName == Skill.SkillName.Carry3)
            {
                SetMaxNumOfPotion(3);
            }
            else if (skillName == Skill.SkillName.Carry5)
            {
                SetMaxNumOfPotion(5);
            }

            PopulateCraftingPanel();
        }

        #region Crafting
        [SerializeField] private GameObject craftingContainer;
        [SerializeField] private GameObject craftingRecipeItem;

        private void PopulateCraftingPanel()
        {
            foreach (Transform child in craftingContainer.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (CraftingRecipe recipe in craftingRecipes)
            {

                GameObject recipeItem = Instantiate(craftingRecipeItem, craftingContainer.transform);
                recipeItem.transform.GetChild(0).GetComponent<Image>().sprite = recipe.outputItem.GetItem().icon;
                recipeItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = recipe.outputItem.GetItem().displayName;
                recipeItem.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = recipe.getInputAsString();
                if (recipe.OutputIsPotion() && FullForPotions())
                {
                    recipeItem.transform.GetChild(3).GetComponent<Button>().interactable = false;
                    recipeItem.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Full";

                }
                else
                {
                    recipeItem.transform.GetChild(3).GetComponent<Button>().interactable = true;
                    recipeItem.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Craft";
                }
                recipeItem.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
                {
                    Craft(recipe);
                    PopulateCraftingPanel();
                });
            }
        }


        #endregion
        public ItemClass SelectedItem
        {
            get => selectedItem;
            set
            {
                // Compare references to see if it’s actually a new item
                if (selectedItem == value)
                    return; // Do nothing if it’s the same

                selectedItem = value;

                if (onSelectedItemChanged != null)
                {
                    Debug.Log($"SelectedItem changed to: {selectedItem?.name ?? "null"}");
                    onSelectedItemChanged.Invoke();
                }
                else
                {
                    Debug.LogError("onSelectedItemChanged is null. Ensure it is initialized.");
                }
            }
        }
        private bool isRestoringInventory = false;
        [System.Serializable]
        public class InventoryItemData
        {
            public string itemName;
            public int quantity;
        }

        public List<InventoryItemData> GetInventoryData()
        {
            List<InventoryItemData> data = new List<InventoryItemData>();

            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i].GetItem() != null)
                {
                    Debug.Log($"Saving item: {Items[i].GetItem().displayName} x{Items[i].GetQuantity()}");
                    data.Add(new InventoryItemData
                    {
                        itemName = Items[i].GetItem().displayName,
                        quantity = Items[i].GetQuantity()
                    });
                }
            }

            Debug.Log($"Saved {data.Count} items.");
            return data;
        }

        private bool isUIInitialized = false;

        private void InitializeUI()
        {
            if (!isUIInitialized)
            {
                slots = new GameObject[SlotHolder.transform.childCount];
                Items = new SlotClass[slots.Length];
                HotBarslots = new GameObject[HotBarSlotHolder.transform.childCount];

                // Initialize HotBar slots
                for (int i = 0; i < HotBarslots.Length; i++)
                {
                    HotBarslots[i] = HotBarSlotHolder.transform.GetChild(i).gameObject;
                }

                // Initialize inventory slots
                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i] = SlotHolder.transform.GetChild(i).gameObject;
                    Items[i] = new SlotClass();
                }

                isUIInitialized = true;
            }
        }

        public void SetInventoryData(List<InventoryItemData> data)
        {
            Debug.Log("Starting inventory restoration...");
            isRestoringInventory = true;

            // Ensure UI is initialized
            InitializeUI();

            // Clear existing inventory
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] != null)
                {
                    // Items[i].RemoveItem();
                    Remove(Items[i].GetItem(), Items[i].GetQuantity());
                }
            }

            // Restore items
            NumOfPotion = 0; // Reset count
            foreach (var itemData in data)
            {
                var item = ItemDatabase.GetItemByName(itemData.itemName);
                if (item != null)
                {
                    Add(item, itemData.quantity);
                    if (item is ConsumableClass consumable && consumable.IsPotion)
                    {
                        NumOfPotion += itemData.quantity;
                    }
                }
            }


            try
            {
                RefreshInterface();
                Debug.Log("Inventory restoration complete.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error refreshing interface: {e.Message}");
            }

            isRestoringInventory = false;
        }

        public ItemClass getSelectedItem()
        {
            return selectedItem;
        }


        public void Awake()
        {
            inputActions = new GameDevCW();

            if (onSelectedItemChanged == null)
            {
                Debug.Log("selceted item");
                onSelectedItemChanged = new UnityEvent();
            }
        }

        public void Start()
        {
            InitializeUI();

            PopulateCraftingPanel();
            InventoryPanel.SetActive(false);
            PlayerStatsPanel.SetActive(false);
            CraftingPanel.SetActive(false);
            Overlay.SetActive(false);

            // Set MaxNumOfPotion before restoring inventory
            MaxNumOfPotion = 2;

            // Access saved inventory data from GameManager
            var savedData = GameManager.Instance?.GetSavedInventoryData();

            // If no saved inventory data exists, initialize with starting items
            if (!isRestoringInventory && (savedData == null || savedData.Count == 0))
            {
                Debug.Log("No saved inventory data found. Initializing with starting items.");
                for (int i = 0; i < startingItems.Length; i++)
                {
                    Items[i] = startingItems[i];
                }

                foreach (ItemClass item in itemToAdd)
                {
                    Add(item, 1);
                }

                foreach (ItemClass item in itemToRemove)
                {
                    Remove(item);
                }
            }
            else if (savedData != null && savedData.Count > 0)
            {
                Debug.Log("Saved inventory data found. Restoring...");
                // Restore saved inventory data
                SetInventoryData(savedData);
            }

            // Update potion count after setting inventory
            UpdatePotionCount();
            RefreshInterface();
        }




        private void UpdatePotionCount()
        {
            NumOfPotion = 0;
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i]?.GetItem() is ConsumableClass consumable)
                {
                    if (consumable.IsPotion)
                    {
                        NumOfPotion += Items[i].GetQuantity();
                    }
                }
            }
        }

        private void Update()
        {
            itemCursor.SetActive(isMovingItem);
            itemCursor.transform.position = Mouse.current.position.ReadValue();
            if (isMovingItem)
            {

                itemCursor.GetComponent<Image>().sprite = MovingSlot.GetItem().icon;
            }


            HotBarSelector.transform.position = HotBarslots[selectedSlotIndex].transform.position;
            SelectedItem = Items[selectedSlotIndex].GetItem();

        }

        public bool isFull()
        {

            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i].GetItem() == null)
                {
                    return false;
                }
            }
            return true;
        }

        public void CraftTest()
        {
            CraftingRecipe recipe = craftingRecipes[0];
            Craft(recipe);
        }
        public void Craft(CraftingRecipe recipe)
        {
            if (recipe.CanCraft(this))
            {
                recipe.Craft(this);
                RefreshInterface();
            }

            else
            {
                Debug.Log("Cannot Craft");
            }

        }

        public void OnHotBarSelection(InputAction.CallbackContext context)
        {
            if (context.performed)
            {

                float val = context.ReadValue<float>();
                SelectedSlotIndex = Mathf.Clamp((int)val - 1, 0, HotBarslots.Length - 1);
            }
        }


        public void OnClick(InputAction.CallbackContext context)
        {


            if (context.performed)
            {


                if (isMovingItem)
                {
                    EndItemMove();
                }
                else
                {
                    BeginItemMove();
                }


            }



        }

        SlotClass GetClosestSlot()
        {

            for (int i = 0; i < slots.Length; i++)
            {
                if (Vector2.Distance(slots[i].transform.position, Mouse.current.position.ReadValue()) <= 55)
                {

                    return Items[i];


                }
            }
            return null;
        }

        bool BeginItemMove()
        {
            OriginalSlot = GetClosestSlot();
            if (OriginalSlot == null || OriginalSlot.GetItem() == null)
            {

                return false;
            }

            MovingSlot = new SlotClass(OriginalSlot);
            Remove(MovingSlot.GetItem(), MovingSlot.GetQuantity());
            isMovingItem = true;
            RefreshInterface();
            return true;

        }

        bool EndItemMove()
        {
            OriginalSlot = GetClosestSlot();
            if (OriginalSlot == null)
            {


                Remove(MovingSlot.GetItem(), MovingSlot.GetQuantity());
                Add(MovingSlot.GetItem(), MovingSlot.GetQuantity());
                MovingSlot.RemoveItem();
            }
            else
            {


                if (OriginalSlot.GetItem() != null)
                {
                    if (OriginalSlot.GetItem() == MovingSlot.GetItem())
                    {
                        if (OriginalSlot.GetItem().Stackable)
                        {

                            OriginalSlot.AddQuantity(MovingSlot.GetQuantity());
                            MovingSlot.RemoveItem();
                        }
                        else
                        {

                            return false;
                        }

                    }
                    else
                    {

                        TempSlot = new SlotClass(OriginalSlot);

                        Remove(OriginalSlot.GetItem(), OriginalSlot.GetQuantity());

                        OriginalSlot.AddItem(MovingSlot.GetItem(), MovingSlot.GetQuantity());
                        Add(TempSlot.GetItem(), TempSlot.GetQuantity());
                        isMovingItem = false;
                        RefreshInterface();
                        return true;

                    }
                }
                else
                {

                    OriginalSlot.AddItem(MovingSlot.GetItem(), MovingSlot.GetQuantity());
                    MovingSlot.RemoveItem();

                }
            }

            isMovingItem = false;
            RefreshInterface();
            return true;

        }

        public bool Add(ItemClass item, int quantity)
        {
            //Items.Add(item);
            SlotClass slot = Contains(item);
            if (item is ConsumableClass)
            {
                ConsumableClass ToCheck = (ConsumableClass)item;
                if (ToCheck.IsPotion && NumOfPotion < MaxNumOfPotion)
                {
                    NumOfPotion += quantity;
                }
                else if (ToCheck.IsPotion && NumOfPotion >= MaxNumOfPotion)
                {
                    return false;
                }
            }
            if (slot != null && slot.GetItem().Stackable)
            {
                slot.AddQuantity(quantity);
            }
            else
            {


                for (int i = 0; i < Items.Length; i++)
                {
                    if (Items[i].GetItem() == null)
                    {
                        Items[i].AddItem(item, quantity);
                        break;
                    }
                }


            }

            RefreshInterface();
            return true;
        }

        public SlotClass Contains(ItemClass item)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] != null && Items[i].GetItem() == item) // Check for null before accessing
                {
                    return Items[i];
                }
            }
            return null;
        }

        public bool Contains(ItemClass item, int quantity)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i].GetItem() == item && Items[i].GetQuantity() >= quantity)
                {
                    return true;
                }
            }
            return false;
        }

        public void RefreshInterface()
        {
            if (!isUIInitialized)
            {
                Debug.LogWarning("Attempting to refresh interface before initialization");
                InitializeUI();
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null || Items[i] == null)
                {
                    continue;
                }

                Image itemImage = slots[i].transform.GetChild(0).GetComponent<Image>();
                TextMeshProUGUI quantityText = slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();

                if (Items[i].GetItem() != null)
                {
                    itemImage.enabled = true;
                    itemImage.sprite = Items[i].GetItem().icon;

                    quantityText.text = Items[i].GetItem().Stackable ?
                        Items[i].GetQuantity().ToString() : "";
                }
                else
                {
                    itemImage.sprite = null;
                    itemImage.enabled = false;
                    quantityText.text = "";
                }
            }

            RefreshHotBar();
        }

        public void RefreshHotBar()
        {
            for (int i = 0; i < HotBarslots.Length; i++)
            {
                try
                {
                    HotBarslots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    HotBarslots[i].transform.GetChild(0).GetComponent<Image>().sprite = Items[i].GetItem().icon;
                    if (Items[i].GetItem().Stackable)
                    {
                        HotBarslots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Items[i].GetQuantity().ToString();
                    }
                    else
                    {
                        HotBarslots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                    }
                }
                catch
                {
                    HotBarslots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                    HotBarslots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                    HotBarslots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                }
            }
        }
        public bool Remove(ItemClass item)
        {
            SlotClass temp = Contains(item);
            if (temp != null)
            {
                if (item is ConsumableClass)
                {
                    ConsumableClass ToCheck = (ConsumableClass)item;
                    if (ToCheck.IsPotion && NumOfPotion < MaxNumOfPotion)
                    {
                        NumOfPotion -= 1;
                    }
                }
                if (temp.GetQuantity() > 1)
                {
                    temp.SubQuantity(1);
                }
                else
                {
                    int SlotToRemoveIndex = -1;
                    for (int i = 0; i < Items.Length; i++)
                    {
                        if (Items[i].GetItem() == item)
                        {

                            SlotToRemoveIndex = i;
                            break;

                        }




                    }
                    if (SlotToRemoveIndex != -1)
                    {

                        Items[SlotToRemoveIndex].RemoveItem();
                    }


                }
            }
            else
            {
                return false;
            }

            RefreshInterface();
            return true;

        }

        public bool Remove(ItemClass item, int quantity)
        {
            SlotClass temp = Contains(item);
            if (temp != null)
            {
                if (item is ConsumableClass)
                {
                    ConsumableClass ToCheck = (ConsumableClass)item;
                    if (ToCheck.IsPotion)
                    {
                        NumOfPotion -= quantity;
                    }
                }

                if (temp.GetQuantity() > quantity)
                {
                    temp.SubQuantity(quantity);
                }
                else
                {
                    int SlotToRemoveIndex = -1;
                    for (int i = 0; i < Items.Length; i++)
                    {
                        if (Items[i].GetItem() == item)
                        {

                            SlotToRemoveIndex = i;
                            break;

                        }




                    }
                    if (SlotToRemoveIndex != -1)
                    {

                        Items[SlotToRemoveIndex].RemoveItem();
                    }


                }
            }
            else
            {
                return false;
            }

            RefreshInterface();
            return true;

        }

        public void SetMaxNumOfPotion(int num)
        {
            MaxNumOfPotion = num;
        }
        public void AddPotion(int num)
        {
            NumOfPotion += num;
        }
        public void RemovePotion(int num)
        {
            NumOfPotion -= num;
        }

        public bool FullForPotions()
        {
            return NumOfPotion >= MaxNumOfPotion;
        }

        public bool CanAddPotion(int quantity)
        {
            return (MaxNumOfPotion - NumOfPotion) >= quantity && !FullForPotions();
        }


    }
}