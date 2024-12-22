using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

public class InventoryManager : MonoBehaviour
{

    [SerializeField] private GameObject itemCursor;
    [SerializeField] private ItemClass[] itemToAdd;
    [SerializeField] private ItemClass[] itemToRemove;
    //Above are temporary for testing purposes
    [SerializeField] private GameObject SlotHolder;
    private SlotClass[] Items;
  
    [SerializeField] private GameObject HotBarSlotHolder;
    [SerializeField] private SlotClass[] startingItems;

    private GameObject[] slots;
    private GameObject[] HotBarslots;
    private GameDevCW inputActions;

    private SlotClass MovingSlot;
    private SlotClass TempSlot;
    private SlotClass OriginalSlot;
    bool isMovingItem = false;

    [SerializeField] private int selectedSlotIndex = 0;
    [SerializeField] private GameObject HotbarSelector;
    public ItemClass selectedItem;
    public void Awake()
    {
        inputActions = new GameDevCW();
    }
    public void Start()
    {
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

    private void OnEnable()
    {
        inputActions.UI.Click.performed += OnClick;
        inputActions.UI.HotBarSelector.performed += OnHotBarSelection;
        inputActions.Enable();

    }

    private void OnDisable()
    {
        inputActions.UI.Click.performed -= OnClick;
        inputActions.UI.HotBarSelector.performed -= OnHotBarSelection;
        inputActions.Disable();

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
        selectedItem = Items[selectedSlotIndex].GetItem();
        print(selectedItem);
    }

    private void OnHotBarSelection(InputAction.CallbackContext context) { 
        if (context.performed)
        {
            float val = context.ReadValue<float>();
            selectedSlotIndex = Mathf.Clamp((int)val - 1, 0, HotBarslots.Length - 1);
        }
    }
   
    private void OnClick(InputAction.CallbackContext context)
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
}
