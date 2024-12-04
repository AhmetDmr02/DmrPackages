using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarManager : MonoBehaviour
{
    [Header("Main Stats")]
    [SerializeField] private List<Slot> hotbarSlots = new List<Slot>();
    [Header("UI Stats")]
    [SerializeField] private Image indicator;

    private bool isHandlingScroll = false; // Flag to prevent multiple calls within a single scroll event

    private enum ScrollDirection { None, Up, Down }

    private ScrollDirection currentScrollDirection = ScrollDirection.None;

    /// <summary>
    /// Fires when hotbar slot changes
    /// </summary>
    public static event Action<Slot> hotbarSlotChanged;

    private Slot currentHotbarSlot;

    private void Start()
    {
        if (hotbarSlots[0] == null) return;

        currentHotbarSlot = hotbarSlots[0];
        setIndicatorPosition(currentHotbarSlot);
        hotbarSlotChanged?.Invoke(currentHotbarSlot);
    }

    void Update()
    {
        // Check for mouse scroll input
        float scrollAmountY = Input.mouseScrollDelta.y;

        // Call a method or execute an action when the scroll amount changes
        if (scrollAmountY != 0f && !isHandlingScroll)
        {
            // Determine the scroll direction
            if (scrollAmountY > 0)
            {
                currentScrollDirection = ScrollDirection.Up;
            }
            else if (scrollAmountY < 0)
            {
                currentScrollDirection = ScrollDirection.Down;
            }

            // Example: Call a method to handle the scroll event with sensitivity applied
            handleMouseScroll();

            isHandlingScroll = true;
        }
        else if (scrollAmountY == 0f)
        {
            isHandlingScroll = false;
        }
    }

    // Example method to handle the scroll event
    void handleMouseScroll()
    {
        if (currentScrollDirection == ScrollDirection.Down)
        {
            int currentIndex = hotbarSlots.IndexOf(currentHotbarSlot);
            currentIndex = (currentIndex + 1) % hotbarSlots.Count;
            currentHotbarSlot = hotbarSlots[currentIndex];
        }
        else if (currentScrollDirection == ScrollDirection.Up)
        {
            int currentIndex = hotbarSlots.IndexOf(currentHotbarSlot);
            currentIndex = (currentIndex - 1 + hotbarSlots.Count) % hotbarSlots.Count;
            currentHotbarSlot = hotbarSlots[currentIndex];
        }

        setIndicatorPosition(currentHotbarSlot);
        hotbarSlotChanged?.Invoke(currentHotbarSlot);

        currentScrollDirection = ScrollDirection.None;
    }
    void setIndicatorPosition(Slot slot)
    {
        float x = slot.transform.position.x;
        indicator.transform.position = new Vector3(x, indicator.transform.position.y, indicator.transform.position.z);
    }
}
