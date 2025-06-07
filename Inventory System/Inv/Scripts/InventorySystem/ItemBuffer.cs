using System;
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

    public static event Action<Item> OnItemPickedUp;
    public static event Action<Item> OnItemPutDown;

    private void Update()
    {
        if (CurrentItem.Item_ != null)
            m_itemImage.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            HandleMouseUp();
        }
    }
    private void HandleMouseDown()
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
                    m_itemImage.sprite = slot.CurrentItem.GetItemSprite(slot.SerializedData);
                    m_itemImage.color = Color.white;

                    m_currentItem.Item_ = slot.CurrentItem;
                    m_currentItem.ItemCount = slot.ItemCount;
                    m_currentItem.holderSlot = slot;

                    OnItemPickedUp?.Invoke(m_currentItem.Item_);
                }
                break; // Exit the loop after processing the first valid UI element hit
            }
        }
        #endregion
    }
    private void HandleMouseUp()
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
            ClearBuffer();
            return;
        }
        #endregion

        foreach (RaycastResult hit in raycastResults)
        {
            if (hit.gameObject.GetComponent<Slot>() == null)
            {
                // Mouse dragged and released on non-slot object
                ClearBuffer();
                return;
            }

            Slot slot = hit.gameObject.GetComponent<Slot>();

            //Splitting Half
            if (Input.GetKey(KeyCode.LeftShift) && (slot.CurrentItem == null || slot.CurrentItem == m_currentItem.Item_))
            {
                bool success = InventoryManager.Instance.SplitHalf(m_currentItem.holderSlot, slot);

                if (success)
                {
                    OnItemPutDown?.Invoke(m_currentItem.Item_);
                }

                ClearBuffer();
                return;
            }

            //Splitting Only One And Merge
            if (Input.GetKey(KeyCode.LeftControl) && (slot.CurrentItem == null || slot.CurrentItem == m_currentItem.Item_))
            {
                bool success = InventoryManager.Instance.SplitOne(m_currentItem.holderSlot, slot);

                if (success)
                {
                    OnItemPutDown?.Invoke(m_currentItem.Item_);
                }

                ClearBuffer();
                return;
            }

            if (slot.CurrentItem == m_currentItem.Item_ && slot.SerializedData == m_currentItem.holderSlot.SerializedData)
            {
                bool success = InventoryManager.Instance.MergeSlots(m_currentItem.holderSlot, slot);

                if (success)
                {
                    OnItemPutDown?.Invoke(m_currentItem.Item_);
                }

                ClearBuffer();
                return;
            }

            // Default swap operation
            bool swapSuccess = InventoryManager.Instance.SwapSlots(m_currentItem.holderSlot, slot);

            if (swapSuccess)
            {
                OnItemPutDown?.Invoke(m_currentItem.Item_);
            }

            ClearBuffer();
            break; // Exit the loop after processing the first valid UI element hit
        }
    }

    private void ClearBuffer()
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