using System.Numerics;
using UnityEngine;

[System.Serializable]

public class SlotClass
{
    [SerializeField] private ItemClass item;
    [SerializeField] private int quantity;

    public SlotClass()
    {
        this.item = null;
        this.quantity = 0;
    }

    public SlotClass(SlotClass slot)
    {
        this.item = slot.GetItem();
        this.quantity = slot.GetQuantity();
    }
    public SlotClass(ItemClass item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }


   public ItemClass GetItem()
    {
        return item;
    }

    public int GetQuantity()
    {
        return quantity;
    }
    public void AddQuantity(int quantity)
    {
        this.quantity += quantity ;
    }

   public void SubQuantity(int quantity)
    {
        this.quantity -= quantity;
    }
    public void SetQuantity(int quantity)
    {
        this.quantity = quantity;
    }

    public void AddItem(ItemClass item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
    public void SetItem(ItemClass item)
    {
        this.item = item;
    }
    public void RemoveItem()
    {
        this.item = null;
        this.quantity = 0;
    }
    //public bool IsEmpty()
    //{
    //    return item == null;
    //}
    //public bool IsFull()
    //{
    //    return item != null;
    //}
    //public bool IsStackable()
    //{
    //    return item.stackable;
    //}
    //public bool IsSameItem(ItemClass item)
    //{
    //    return this.item == item;
    //}
    //public bool IsSameItem(SlotClass slot)
    //{
    //    return this.item == slot.GetItem();
    //}
    //public bool IsSameItem(ItemClass item, SlotClass slot)
    //{
    //    return this.item == item && this.item == slot.GetItem();
    //}
    //public bool IsSameItem(SlotClass slot1, SlotClass slot2)
    //{
    //    return slot1.GetItem() == slot2.GetItem();
    //}
    //public bool IsSameItem(ItemClass item1, ItemClass item2)
    //{
    //    return item1 == item2;
    //}

}
