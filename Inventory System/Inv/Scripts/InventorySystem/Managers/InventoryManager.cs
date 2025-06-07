using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Main Stats")]
    [SerializeField] private List<Slot> _playerSlots = new List<Slot>();

    public List<Slot> PlayerSlots => _playerSlots;

    [SerializeField] private List<Item> _itemList = new List<Item>();

    public List<Item> ItemList => _itemList;

    //We Will Keep The Items On Dictionary In Order To Find
    //I Will Save Items By Their IdNames For Now
    private Dictionary<string, Item> _itemDictionary = new Dictionary<string, Item>();

    /// <summary>
    /// Always search with all lowercase
    /// </summary>
    public Dictionary<string, Item> ItemLibrary => _itemDictionary;

    /// <summary>
    /// Some slot on the players inventory is changed
    /// </summary>
    public event Action OnInventorySlotChanged;

    #region Automatic Register
    List<Item> folderItems = new List<Item>();

    [SerializeField] private bool _automaticRegister = false;
    [SerializeField] private string _automaticRegisterPath = "Assets/Items";

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!_automaticRegister) return;

        folderItems.Clear();

        var guids = AssetDatabase.FindAssets("t:Item", new[] { "Assets/Items" });

        folderItems = guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).Select(path => AssetDatabase.LoadAssetAtPath<Item>(path)).ToList();

        foreach (Item item in folderItems)
        {
            if (!_itemList.Contains(item) && item.AutomaticallyRegisterToInventoryManager)
                _itemList.Add(item);
        }

        _itemList.RemoveAll(item => item == null);
#endif
    }
    #endregion

    #region Singleton
    public static InventoryManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }
    #endregion

    private void Start()
    {
        //Registering all items to dictionary
        try
        {
            foreach (Item item in _itemList)
            {
                if (_itemDictionary.ContainsKey(item.ItemID))
                {
                    Debug.LogError("Same Item Id Is Already In The List Couldn't Added " + item.ItemName + " To Dictionary");
                    continue;
                }
                _itemDictionary.Add(item.ItemID.ToLower(), item);
            }
            Debug.Log("All Items Registered");
        }
        catch (System.Exception err)
        {
            Debug.LogError("Couldn't Load All Items Because Of Following Error: " + err);
        }
        foreach (Slot slot in _playerSlots)
        {
            slot.ItemChanged += HandleSlotItemChanged;
        }
    }
    private void OnDestroy()
    {
        //Unsubscribe
        foreach (Slot slot in _playerSlots)
        {
            slot.ItemChanged -= HandleSlotItemChanged;
        }
    }
    private void HandleSlotItemChanged(Item item)
    {
        OnInventorySlotChanged?.Invoke();
    }

    /// <summary>
    /// Checking If Player Have Enough Slots To Take Certain Amount Of Item
    /// Always Check Before Adding Item To Player Inventory
    /// </summary>
    /// <returns>Returns True if player have enough space or false if doesn't</returns>
    public bool CanPlayerTakeThisItem(Item item, int itemCount)
    {
        List<Slot> slots = GetSlotsWithId(item.ItemID, false);
        int remainingItem = itemCount;

        //First Check If We Can Merge Same Items Into Stack
        foreach (Slot slot in slots)
        {
            if (remainingItem <= 0) return true;

            int i = slot.CurrentItem.ItemMaxStackSize - slot.ItemCount;
            remainingItem -= i;
        }

        if (remainingItem <= 0) return true;

        List<Slot> emptySlots = GetEmptySlotsForItem(item);

        foreach (Slot slot in emptySlots)
        {
            if (remainingItem <= 0) return true;
            remainingItem -= item.ItemMaxStackSize;
        }

        if (remainingItem <= 0) return true;
        return false;
    }

    /// <summary>
    /// Checking If Player Have Enough Items To Remove Certain Amount Of Item
    /// Always Check Before Removing Item To Player Inventory
    /// </summary>
    /// <returns>Returns True if player have enough item or False if doesn't</returns>
    public bool CanPlayerRemoveThisItem(Item item, int itemCount, bool includeSerializedItem)
    {
        int itemCountOnInventory = GetItemCountOnPlayerInventory(item.ItemID, includeSerializedItem);

        if (itemCountOnInventory >= itemCount) return true;
        return false;
    }

    public event Action<Item> OnItemAdded;

    /// <summary>
    /// Adds Item To Player Inventory
    /// </summary>
    public void AddItem(Item item, int itemCount)
    {
        int remainingItem = itemCount;

        //Merge Same Items First
        List<Slot> mergeSlots = GetSlotsWithId(item.ItemID, false);
        foreach (Slot slots in mergeSlots)
        {
            if (remainingItem <= 0) { OnItemAdded?.Invoke(item); return; }

            int i = slots.CurrentItem.ItemMaxStackSize - slots.ItemCount;

            if (i >= remainingItem)
            {
                slots.AdjustItemCount(remainingItem);
                remainingItem = 0;
                OnItemAdded?.Invoke(item);
                return;
            }
            slots.AdjustItemCount(i);
            remainingItem -= i;
        }

        if (remainingItem <= 0) { OnItemAdded?.Invoke(item); return; }

        //If Merge Slots Is Not Enough We Will Fill Empty Slots

        List<Slot> emptySlots = GetEmptySlotsForItem(item);

        foreach (Slot slot in emptySlots)
        {
            if (remainingItem <= 0) { OnItemAdded?.Invoke(item); return; }

            if (remainingItem <= item.ItemMaxStackSize)
            {
                slot.SetItem(item, remainingItem);
                remainingItem = 0;
                OnItemAdded?.Invoke(item);
                return;
            }
            slot.SetItem(item, item.ItemMaxStackSize);
            remainingItem -= item.ItemMaxStackSize;
        }

        if (remainingItem <= 0) { OnItemAdded?.Invoke(item); return; }

        Debug.LogWarning($"From AddItem(), Couldn't added {remainingItem} amount of item since there was no inventory space left please check with CanPlayerTakeThisItem() before adding");
    }
    public void AddItem(Item item, int itemCount, string serializedData)
    {
        int remainingItem = itemCount;

        //Merge Same Items First
        List<Slot> mergeSlots = GetSlotsWithId(item.ItemID, serializedData);
        foreach (Slot slots in mergeSlots)
        {
            if (remainingItem <= 0) { OnItemAdded?.Invoke(item); return; }

            int i = slots.CurrentItem.ItemMaxStackSize - slots.ItemCount;

            if (i >= remainingItem)
            {
                slots.AdjustItemCount(remainingItem);
                remainingItem = 0;
                OnItemAdded?.Invoke(item);
                return;
            }
            slots.AdjustItemCount(i);
            remainingItem -= i;
        }

        if (remainingItem <= 0) { OnItemAdded?.Invoke(item); return; }

        //If Merge Slots Is Not Enough We Will Fill Empty Slots

        List<Slot> emptySlots = GetEmptySlotsForItem(item);

        foreach (Slot slot in emptySlots)
        {
            if (remainingItem <= 0) { OnItemAdded?.Invoke(item); return; }

            if (remainingItem <= item.ItemMaxStackSize)
            {
                slot.SetItem(item, remainingItem, serializedData);
                remainingItem = 0;
                OnItemAdded?.Invoke(item);
                return;
            }
            slot.SetItem(item, item.ItemMaxStackSize, serializedData);
            remainingItem -= item.ItemMaxStackSize;
        }

        if (remainingItem <= 0) { OnItemAdded?.Invoke(item); return; }

        Debug.LogWarning($"From AddItem(), Couldn't added {remainingItem} amount of item since there was no inventory space left please check with CanPlayerTakeThisItem() before adding");
    }

    public event Action<Item> OnItemRemoved;

    /// <summary>
    ///  Removes Item From Player Inventory
    /// </summary>
    public void RemoveItem(Item item, int itemCount, bool includeSerializedItem)
    {
        int remainingItem = itemCount;

        List<Slot> itemsSlotList = GetSlotsWithId(item.ItemID, includeSerializedItem);

        foreach (Slot slot in itemsSlotList)
        {
            if (remainingItem <= 0) { OnItemRemoved?.Invoke(item); return; }

            if (slot.ItemCount >= remainingItem)
            {
                slot.AdjustItemCount(-remainingItem);
                remainingItem = 0;
                OnItemRemoved?.Invoke(item);
                return;
            }

            remainingItem -= slot.ItemCount;
            slot.AdjustItemCount(-slot.ItemCount);
        }

        if (remainingItem > 0)
            Debug.LogWarning($"From RemoveItem(), Couldn't removed {itemCount} amount of items instead there was only {remainingItem} in players inventory please check with CanPlayerRemoveThisItem() before removing item");

        if (remainingItem != itemCount)
            OnItemRemoved?.Invoke(item);

    }
    public void RemoveItem(Item item, int itemCount, string serializedData)
    {
        int remainingItem = itemCount;

        // Get slots matching the item ID and serialized data
        List<Slot> itemsSlotList = GetSlotsWithId(item.ItemID, serializedData);

        foreach (Slot slot in itemsSlotList)
        {
            if (remainingItem <= 0)
            {
                OnItemRemoved?.Invoke(item);
                return;
            }

            if (slot.ItemCount >= remainingItem)
            {
                slot.AdjustItemCount(-remainingItem);
                remainingItem = 0;
                OnItemRemoved?.Invoke(item);
                return;
            }

            remainingItem -= slot.ItemCount;
            slot.AdjustItemCount(-slot.ItemCount);
        }

        if (remainingItem > 0)
        {
            Debug.LogWarning($"From RemoveItem(), Couldn't remove {itemCount} items with serialized data '{serializedData}' as only {itemCount - remainingItem} were available.");
        }

        if (remainingItem != itemCount)
        {
            OnItemRemoved?.Invoke(item);
        }
    }

    public void ClearSlotOfPlayer(Slot selectedSlot)
    {
        selectedSlot.ClearSlot();
    }

    /// <summary>
    /// Removes All The Items On Player Slots
    /// </summary>
    public void ClearPlayerInventory()
    {
        foreach (Slot slot in _playerSlots)
        {
            slot.ClearSlot();
        }
    }

    /// <summary>
    /// Returns Empty Slots On Player Slots
    /// Returns Lenght Of 0 If There Is No Empty Slot Left
    /// </summary>
    public List<Slot> GetEmptySlotsForItem(Item item)
    {
        List<Slot> slots = new List<Slot>();

        foreach (Slot slot in _playerSlots)
        {
            //Check for filter
            if (slot.FilterActive && slot.FilteredCategory != item.ItemCategory) continue;

            if (slot.CurrentItem == null) slots.Add(slot);
        }
        return slots;
    }

    /// <summary>
    /// Returns List Of Slots Based On ItemId
    /// Returns Empty List If Match Cannot Be Found
    /// </summary>
    public List<Slot> GetSlotsWithId(string _ItemId, bool includeSerializedItems)
    {
        List<Slot> foundSlots = new List<Slot>();
        foreach (Slot slot in _playerSlots)
        {
            if (slot.CurrentItem == null) continue;

            if (!includeSerializedItems)
                if (slot.SerializedData != string.Empty) continue;

            if (slot.CurrentItem.ItemID == _ItemId) foundSlots.Add(slot);
        }
        return foundSlots;
    }
    public List<Slot> GetSlotsWithId(string _ItemId, string serializedData)
    {
        List<Slot> foundSlots = new List<Slot>();

        foreach (Slot slot in _playerSlots)
        {
            if (slot.CurrentItem == null) continue;
            if (slot.SerializedData != serializedData) continue;
            if (slot.CurrentItem.ItemID == _ItemId) foundSlots.Add(slot);
        }
        return foundSlots;
    }
    /// <summary>
    /// Returns Item Count Of An Item Currently On Players Inventory Based On ItemId
    /// </summary>
    public int GetItemCountOnPlayerInventory(string _ItemId, bool includeSerializedItems)
    {
        List<Slot> correspondingSlot = includeSerializedItems ? GetSlotsWithId(_ItemId, true) : GetSlotsWithId(_ItemId, false);

        int count = 0;
        foreach (Slot slot in correspondingSlot)
        {
            count += slot.ItemCount;
        }
        return count;
    }
    public int GetItemCountOnPlayerInventoryWithSerializedData(string _ItemId, string serializedData)
    {
        List<Slot> correspondingSlots = GetSlotsWithId(_ItemId, true);

        int count = 0;
        foreach (Slot slot in correspondingSlots)
        {
            if (slot.SerializedData == serializedData) count += slot.ItemCount;
        }
        return count;
    }
    public void RemoveFromSlot(Slot slot, int itemCount)
    {
        if (slot.CurrentItem == null) return;

        if (slot.ItemCount < itemCount) return;

        if (slot.ItemCount == itemCount)
        {
            slot.ClearSlot();
            return;
        }
        else
        {
            slot.AdjustItemCount(-itemCount);
        }
    }
    public Item GetItemWithItemID(string itemID)
    {
        foreach (Item item in _itemList)
        {
            if (item.ItemID.ToLower() != itemID.ToLower()) continue;
            return item;
        }
        return null;
    }

    #region Validation Methods
    private bool ValidateSlotOperation(Item sourceItem, Slot sourceSlot, Slot targetSlot)
    {
        // Check target slot filter
        if (targetSlot.FilterActive && targetSlot.FilteredCategory != sourceItem.ItemCategory)
            return false;

        // Check source slot filter (for swapping)
        if (sourceSlot.FilterActive && targetSlot.CurrentItem != null &&
            sourceSlot.FilteredCategory != targetSlot.CurrentItem.ItemCategory)
            return false;

        return true;
    }

    private bool ValidateSplitOperation(Item sourceItem, Slot sourceSlot, Slot targetSlot, int splitAmount)
    {
        // Check if we have enough items to split
        if (splitAmount <= 0) return false;

        // Check target slot filter
        if (targetSlot.FilterActive && targetSlot.FilteredCategory != sourceItem.ItemCategory)
            return false;

        // Check source slot filter
        if (sourceSlot.FilterActive && sourceSlot.FilteredCategory != targetSlot.FilteredCategory)
            return false;

        return true;
    }
    #endregion

    #region Splitters Mergers with Success Returns
    public bool MergeSlots(Slot selectedSlot, Slot targetSlot)
    {
        if (!ValidateSlotOperation(selectedSlot.CurrentItem, selectedSlot, targetSlot))
            return false;

        int carryAmount = selectedSlot.ItemCount;
        int slotMaxTakeAmount = targetSlot.CurrentItem.ItemMaxStackSize - targetSlot.ItemCount;

        int AddAmount = slotMaxTakeAmount >= carryAmount ? carryAmount : slotMaxTakeAmount;
        int RemoveAmount = -AddAmount;

        targetSlot.AdjustItemCount(AddAmount);
        selectedSlot.AdjustItemCount(RemoveAmount);
        return true;
    }

    public bool SplitOne(Slot selectedSlot, Slot targetSlot)
    {
        if (!ValidateSplitOperation(selectedSlot.CurrentItem, selectedSlot, targetSlot, 1))
            return false;

        if (targetSlot.CurrentItem == null)
        {
            targetSlot.SetItem(selectedSlot.CurrentItem, 1, selectedSlot.SerializedData);
            selectedSlot.AdjustItemCount(-1);
        }
        else if (targetSlot.CurrentItem == selectedSlot.CurrentItem)
        {
            int carryAmount = 1;
            int slotMaxTakeAmount = targetSlot.CurrentItem.ItemMaxStackSize - targetSlot.ItemCount;

            int AddAmount = slotMaxTakeAmount >= carryAmount ? carryAmount : slotMaxTakeAmount;
            int RemoveAmount = -AddAmount;

            targetSlot.AdjustItemCount(AddAmount);
            selectedSlot.AdjustItemCount(RemoveAmount);
        }
        return true;
    }

    public bool SplitHalf(Slot selectedSlot, Slot targetSlot)
    {
        int halfAmount = selectedSlot.ItemCount / 2;
        if (!ValidateSplitOperation(selectedSlot.CurrentItem, selectedSlot, targetSlot, halfAmount))
            return false;

        if (targetSlot.CurrentItem == null)
        {
            targetSlot.SetItem(selectedSlot.CurrentItem, halfAmount, selectedSlot.SerializedData);
            selectedSlot.AdjustItemCount(-halfAmount);
        }
        else if (targetSlot.CurrentItem == selectedSlot.CurrentItem)
        {
            int carryAmount = halfAmount;
            int slotMaxTakeAmount = targetSlot.CurrentItem.ItemMaxStackSize - targetSlot.ItemCount;

            int AddAmount = slotMaxTakeAmount >= carryAmount ? carryAmount : slotMaxTakeAmount;
            int RemoveAmount = -AddAmount;

            targetSlot.AdjustItemCount(AddAmount);
            selectedSlot.AdjustItemCount(RemoveAmount);
        }
        return true;
    }

    (Item item, int itemCount, string serializedData) bufferSlot = (null, 0, "");
    public bool SwapSlots(Slot selectedSlot, Slot targetSlot)
    {
        if (!ValidateSlotOperation(selectedSlot.CurrentItem, selectedSlot, targetSlot))
            return false;

        bufferSlot = (targetSlot.CurrentItem, targetSlot.ItemCount, targetSlot.SerializedData);

        targetSlot.SetItem(selectedSlot.CurrentItem, selectedSlot.ItemCount, selectedSlot.SerializedData);
        selectedSlot.SetItem(bufferSlot.item, bufferSlot.itemCount, bufferSlot.serializedData);
        return true;
    }
    #endregion
}