using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using UnityEngine.Events;

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
    public GameObject[] HotBarSlot {
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
    [SerializeField] private GameObject HotbarSelector;
    public ItemClass selectedItem;


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
            recipeItem.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => Craft(recipe));
        }
    }

    
    #endregion
    public ItemClass SelectedItem
    {
        get => selectedItem;
        set
        {
            if (selectedItem != value)
            {
                selectedItem = value;
                onSelectedItemChanged?.Invoke(); // Trigger the event when the item changes
            }
        }
    }

    public ItemClass getSelectedItem(){
        return selectedItem;
    }


    public void Awake()
    {
        inputActions = new GameDevCW();

        if (onSelectedItemChanged == null)
        {
            onSelectedItemChanged = new UnityEvent();
        }
    }

    public void Start()
    {
        PopulateCraftingPanel();    
        InventoryPanel.SetActive(false);
        PlayerStatsPanel.SetActive(false);
        CraftingPanel.SetActive(false);
        Overlay.SetActive(false);
        slots = new GameObject[SlotHolder.transform.childCount];
        Items = new SlotClass[slots.Length];

        HotBarslots = new GameObject[HotBarSlotHolder.transform.childCount];

        for (int i = 0; i < HotBarslots.Length; i++) {
            HotBarslots[i] = HotBarSlotHolder.transform.GetChild(i).gameObject;
        }
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i] = new SlotClass();
        }
        for (int i = 0; i < startingItems.Length; i++)
        {
            Items[i] = startingItems[i];
        }
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = SlotHolder.transform.GetChild(i).gameObject;
        }
        foreach (ItemClass item in itemToAdd)
        {
            Add(item, 1);
        }
      
        RefreshInterface();
        foreach (ItemClass item in itemToRemove)
        {
            Remove(item);
        }
        RefreshInterface();

    }
    private void Update()
    {
        itemCursor.SetActive(isMovingItem);
        itemCursor.transform.position = Mouse.current.position.ReadValue();
        if (isMovingItem)
        {

            itemCursor.GetComponent<Image>().sprite = MovingSlot.GetItem().icon;
        }

        
        HotbarSelector.transform.position = HotBarslots[selectedSlotIndex].transform.position;
        SelectedItem = Items[selectedSlotIndex].GetItem();
       
    }

    public bool isFull() {

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].GetItem() == null)
            {
                return false;
            }
        }
        return true;
    }

    public void CraftTest() {
        CraftingRecipe recipe = craftingRecipes[0];
        Craft(recipe);
    }
    public void Craft(CraftingRecipe recipe) {
        if (recipe.CanCraft(this))
        {
            recipe.Craft(this);
            RefreshInterface();
        }

        else {
            Debug.Log("Cannot Craft");
        }

    }

    public void OnHotBarSelection(InputAction.CallbackContext context) { 
        if (context.performed)
        {
            
            float val = context.ReadValue<float>();
            selectedSlotIndex = Mathf.Clamp((int)val - 1, 0, HotBarslots.Length - 1);
        }
    }

    public void ToggleInventory() {
        InventoryPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }


    public void OnClick(InputAction.CallbackContext context)
    {
       
        if (context.performed) {

            if (isMovingItem)
            {
                EndItemMove();
            }
            else {
                BeginItemMove();
            }
           

        }

        SlotClass GetClosestSlot()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            for (int i = 0; i < slots.Length; i++) {
                if (Vector2.Distance(slots[i].transform.position, mousePos) <= 40){
                    return Items[i];

                }
            }
            return null;
        }

        bool BeginItemMove() { 
            OriginalSlot = GetClosestSlot() ;
            if (OriginalSlot != null && OriginalSlot.GetItem() != null)
            {
                MovingSlot = new SlotClass(OriginalSlot);
                OriginalSlot.RemoveItem();
                isMovingItem = true;
                RefreshInterface();
                return true;
            }
            return false;
        }

        bool EndItemMove() {
            OriginalSlot = GetClosestSlot();
            if (OriginalSlot == null)
            {
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
                        OriginalSlot.AddItem(MovingSlot.GetItem(), MovingSlot.GetQuantity());
                        MovingSlot.AddItem(TempSlot.GetItem(), TempSlot.GetQuantity());
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
    }

    public bool Add(ItemClass item, int quantity) {
        //Items.Add(item);
        SlotClass slot = Contains(item);
        if (slot != null && slot.GetItem().Stackable)
        {
            slot.AddQuantity(1);
        }
        else {


            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i].GetItem() == null)
                {
                    Items[i].AddItem(item, quantity);
                    break;
                }
            }
            //if (Items.Count < slots.Length) {
            //    Items.Add(new SlotClass(item, 1));

            //}
            //else
            //{
            //    return false;
            //}

        }
        
        RefreshInterface();
        return true;
    }

    public SlotClass Contains(ItemClass item)
    {
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].GetItem() == item)
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
        int slotCount = slots.Length;
        int itemCount = Items.Length;
        for (int i = 0; i < slots.Length; i++)
        {
            try
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = Items[i].GetItem().icon;
                if (Items[i].GetItem().Stackable)
                {
                    slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Items[i].GetQuantity().ToString();
                }
                else
                {
                    slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                }

            }
            catch
            {
                slots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
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

    public bool Remove(ItemClass item, int quantity )
    {
        SlotClass temp = Contains(item);
        if (temp != null)
        {

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


}
