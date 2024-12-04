using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[Serializable]
public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Assignments")]
    [SerializeField] private Image _slotImage;
    [SerializeField] private TextMeshProUGUI _slotCountText;

    private Item currentItem;
    public Item CurrentItem => currentItem;

    private int _itemCount;
    public int ItemCount => _itemCount;

    [SerializeField] private string serializedData;
    public string SerializedData => serializedData;

    public static Action<Slot, string> OnSerializedDataChanged;

    [SerializeField] private bool _filterActive;
    public bool FilterActive => _filterActive;


    [SerializeField] private Item_Category filteredCategory;
    public Item_Category FilteredCategory => filteredCategory;

    public event Action<Item> ItemChanged;

    private void Start()
    {
        recalculateVisuals();

        OnSerializedDataChanged += onSerializedDataChanged;
    }
    private void OnDestroy()
    {
        OnSerializedDataChanged -= onSerializedDataChanged;
    }

    /// <summary>
    /// You can set the slot properties,
    /// keep that in mind if this slot is not clean that means old slot will be replaced with the new properties
    /// </summary>
    /// <param name="item">Insert the item you want to use</param>
    /// <param name="ItemCount">Don't exceed the max stack size or it will automatically set ItemCount to max stack size</param>
    public void SetItem(Item item, int ItemCount)
    {
        currentItem = item;
        if (currentItem == null)
        {
            ClearSlot();
            return;
        }
        if (ItemCount < 1) _itemCount = 1;
        if (!item.ItemStackable) _itemCount = 1;
        //this is ? operator checks the condition and if true sets the first variable before ":"
        //if not set the right side of ":"
        else _itemCount = ItemCount > item.ItemMaxStackSize ? item.ItemMaxStackSize : ItemCount;


        if (_slotImage != null) _slotImage.sprite = item.ItemPicture;

        recalculateVisuals();

        ItemChanged?.Invoke(item);
    }

    /// <summary>
    /// You can set the slot properties,
    /// keep that in mind if this slot is not clean that means old slot will be replaced with the new properties
    /// </summary>
    /// <param name="item">Insert the item you want to use</param>
    /// <param name="ItemCount">Don't exceed the max stack size or it will automatically set ItemCount to max stack size</param>
    public void SetItem(Item item, int ItemCount,string serializedData)
    {
        currentItem = item;
        if (currentItem == null)
        {
            ClearSlot();
            return;
        }
        if (ItemCount < 1) _itemCount = 1;
        if (!item.ItemStackable) _itemCount = 1;
        //this is ? operator checks the condition and if true sets the first variable before ":"
        //if not set the right side of ":"
        else _itemCount = ItemCount > item.ItemMaxStackSize ? item.ItemMaxStackSize : ItemCount;


        if (_slotImage != null) _slotImage.sprite = item.ItemPicture;

        recalculateVisuals();

        this.serializedData = serializedData;

        OnSerializedDataChanged?.Invoke(this,serializedData);

        ItemChanged?.Invoke(item);
    }

    /// <summary>
    /// Sets the item count and returns any excess amount beyond the maximum stack size.
    /// If you set it to 0 it will clear the slot.
    /// </summary>
    /// <param name="ItemCount">The desired item count to set.</param>
    /// <returns>
    /// The overflow amount after increasing the item count to the maximum stack size. For example:
    /// If an item 'x' has a maximum stack size of 3, and you try to increase its count by 10, it will set the slot item count to the maximum stack size (which is 3) and return the overflow amount (which is 7).
    /// </returns>
    public int SetItemCount(int ItemCount_)
    {
        int OverflowInt = ItemCount_;

        if (currentItem == null) return OverflowInt;

        if (ItemCount_ < 1)
        {
            ClearSlot();
            recalculateVisuals();
            return OverflowInt;
        }

        if (!currentItem.ItemStackable)
        {
            _itemCount = 1;
            recalculateVisuals();
            return OverflowInt;
        }
        else
        {
            _itemCount = currentItem.ItemMaxStackSize > ItemCount_ ? ItemCount_ : currentItem.ItemMaxStackSize;

            // Calculate the overflow amount after setting the item count to the maximum stack size.
            OverflowInt = ItemCount_ - _itemCount;

            recalculateVisuals();

            // Return 0 if the overflow amount is less than 0.
            return OverflowInt < 0 ? 0 : OverflowInt;
        }
    }

    /// <summary>
    /// Increases or decreases the item count.
    /// </summary>
    public void AdjustItemCount(int value)
    {
        if (currentItem == null) return;

        if (_itemCount + value < 1)
        {
            ClearSlot();
            recalculateVisuals();
            return;
        }

        if (!currentItem.ItemStackable) _itemCount = 1;
        //this is ? operator checks the condition and if true sets the first variable before ":"
        //if not set the right side of ":"
        else _itemCount = _itemCount + value > currentItem.ItemMaxStackSize ? currentItem.ItemMaxStackSize : _itemCount + value;

        ItemChanged?.Invoke(currentItem);

        recalculateVisuals();
    }

    public void ChangeSerializedData(string newSerializedData)
    {
        serializedData = newSerializedData;
        OnSerializedDataChanged?.Invoke(this, newSerializedData);
    }
    /// <summary>
    /// Cleans the slot completely
    /// </summary>
    public void ClearSlot()
    {
        currentItem = null;
        _itemCount = 0;
        serializedData = string.Empty;
        OnSerializedDataChanged?.Invoke(this,string.Empty);
        ItemChanged?.Invoke(null);
        recalculateVisuals();
    }

    /// <summary>
    /// Recalculating Text, Image, Etc.
    /// </summary>
    private void recalculateVisuals()
    {
        if (currentItem == null)
        {
            //Setting the slot image to transparent
            _slotImage.color = new Color(0, 0, 0, 0);
            _slotCountText.enabled = false;
            return;
        }
        else
        {
            //Setting to non transparent
            if (_slotImage == null || _slotCountText == null) return;
            _slotImage.color = Color.white;
            _slotCountText.enabled = false;
        }

        if (!currentItem.ItemStackable)
            _slotCountText.enabled = false;
        else
            _slotCountText.enabled = true;

        _slotCountText.text = _itemCount.ToString();
    }
    private void onSerializedDataChanged(Slot slot, string newSerializedData)
    {
        if (slot != this) return;
        if (this.currentItem == null) return;

        if (this.currentItem is ISerializedDataChangedListener)
        {
            ISerializedDataChangedListener serializedDataChangedListener = (ISerializedDataChangedListener)this.currentItem;
            serializedDataChangedListener.OnSerializedDataChanged(this, newSerializedData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DescriptionManager.instance == null || CurrentItem == null) return;
        DescriptionManager.instance.SetDescriptor(CurrentItem.ItemName, CurrentItem.ItemDescription, CurrentItem.ItemCategory.ToString());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (DescriptionManager.instance == null || CurrentItem == null) return;
        DescriptionManager.instance.ClearDescriptor();
    }
}
