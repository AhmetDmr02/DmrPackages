using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemBuffer : MonoBehaviour
{
    // This script is responsible for carrying items

    [Header("GUI")]
    [SerializeField] private Image m_itemImage;

    private ItemBundle m_currentItem;
    public ItemBundle CurrentItem => m_currentItem;

    private void Update()
    {
        if (CurrentItem.Item_ != null)
            m_itemImage.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            handleMouseDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            handleMouseUp();
        }
    }
    private void handleMouseDown()
    {
        #region Creating Buffer
        // Create a pointer event data for raycasting
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        // Perform a raycast and get all the UI elements that intersect with the ray
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        foreach (RaycastResult hit in raycastResults)
        {
            if (hit.gameObject.GetComponent<Slot>() != null)
            {
                Slot slot = hit.gameObject.GetComponent<Slot>();

                if (m_currentItem.Item_ == null)
                {
                    if (slot.CurrentItem == null) return;

                    m_itemImage.transform.position = Input.mousePosition;
                    m_itemImage.sprite = slot.CurrentItem.ItemPicture;
                    m_itemImage.color = Color.white;

                    m_currentItem.Item_ = slot.CurrentItem;
                    m_currentItem.ItemCount = slot.ItemCount;
                    m_currentItem.holderSlot = slot;
                }

                break; // Exit the loop after processing the first valid UI element hit
            }
        }
        #endregion
    }
    private void handleMouseUp()
    {
        if (m_currentItem.Item_ == null) return;

        #region Raycasting
        // Create a pointer event data for raycasting
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        // Perform a raycast and get all the UI elements that intersect with the ray
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        if (raycastResults.Count < 1)
        {
            // Mouse dragged and released on nothing
            clearBuffer();
            return;
        }
        #endregion

        foreach (RaycastResult hit in raycastResults)
        {
            if (hit.gameObject.GetComponent<Slot>() == null)
            {
                // Mouse dragged and released on non-slot object
                clearBuffer();
                return;
            }

            Slot slot = hit.gameObject.GetComponent<Slot>();

            //Splitting Half
            if (Input.GetKey(KeyCode.LeftShift) && (slot.CurrentItem == null || slot.CurrentItem == m_currentItem.Item_))
            {
                splitHalf(slot);
                return;
            }

            //Splitting Only One And Merge
            if (Input.GetKey(KeyCode.LeftControl) && (slot.CurrentItem == null || slot.CurrentItem == m_currentItem.Item_))
            {
                splitOne(slot);
                return;
            }

            if (slot.CurrentItem == m_currentItem.Item_ && slot.SerializedData == m_currentItem.holderSlot.SerializedData)
            {
                mergeSlots(slot);
                return;
            }

            swapSlots(slot);

            break; // Exit the loop after processing the first valid UI element hit
        }
    }
    private void splitHalf(Slot slot)
    {
        //Empty Slot
        if (slot.CurrentItem == null)
        {
            //Checking For Filters
            if (slot.FilterActive && slot.FilteredCategory != m_currentItem.Item_.ItemCategory) { clearBuffer(); return; }
            if (m_currentItem.holderSlot.FilterActive && m_currentItem.holderSlot.FilteredCategory != slot.FilteredCategory) { clearBuffer(); return; }

            if (m_currentItem.ItemCount / 2 <= 0) { clearBuffer(); return; }
            InventoryManager.instance.SplitHalf( m_currentItem.holderSlot, slot );

            clearBuffer();
            return;
        }
        else if (slot.CurrentItem == m_currentItem.Item_)
        {
            InventoryManager.instance.SplitHalf(m_currentItem.holderSlot, slot);

            clearBuffer();
            return;
        }
    }
    private void splitOne(Slot slot)
    {
        if (slot.CurrentItem == null)
        {
            //Checking For Filters
            if (slot.FilterActive && slot.FilteredCategory != m_currentItem.Item_.ItemCategory) { clearBuffer(); return; }
            if (m_currentItem.holderSlot.FilterActive && m_currentItem.holderSlot.FilteredCategory != slot.FilteredCategory) { clearBuffer(); return; }

            if (m_currentItem.ItemCount / 2 <= 0) { clearBuffer(); return; }

            InventoryManager.instance.SplitOne(m_currentItem.holderSlot, slot);

            clearBuffer();
            return;
        }
        else if (slot.CurrentItem == m_currentItem.Item_)
        {
            InventoryManager.instance.SplitOne(m_currentItem.holderSlot, slot);

            clearBuffer();
            return;
        }
    }
    private void mergeSlots(Slot slot)
    {
        InventoryManager.instance.MergeSlots(m_currentItem.holderSlot, slot);

        clearBuffer();
        return;
    }
    private void swapSlots(Slot slot)
    {
        //Checking For Filters
        if (slot.FilterActive && slot.FilteredCategory != m_currentItem.Item_.ItemCategory) { clearBuffer(); return; }
        if (m_currentItem.holderSlot.FilterActive && m_currentItem.holderSlot.FilteredCategory != slot.FilteredCategory && slot.CurrentItem != null) { clearBuffer(); return; }

        ItemBundle bufferSlot = new ItemBundle();
        bufferSlot.Item_ = slot.CurrentItem;
        bufferSlot.ItemCount = slot.ItemCount;

        InventoryManager.instance.SwapSlots(m_currentItem.holderSlot, slot);

        clearBuffer();
        return;
    }
    private void clearBuffer()
    {
        // Clearing buffer
        m_currentItem.Item_ = null;
        m_currentItem.ItemCount = 0;
        m_itemImage.sprite = null;
        m_itemImage.color = new Color(0, 0, 0, 0);
    }

    public struct ItemBundle
    {
        public Item Item_;
        public Slot holderSlot;
        public int ItemCount;
    }
}