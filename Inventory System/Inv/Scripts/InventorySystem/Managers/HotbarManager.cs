using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarManager : MonoBehaviour
{
    [Header("Main Stats")]
    [SerializeField] private List<Slot> _hotbarSlots = new List<Slot>();
    [Header("UI Stats")]
    [SerializeField] private Image _indicator;

    private bool _isHandlingScroll = false; // Flag to prevent multiple calls within a single scroll event

    private enum ScrollDirection { None, Up, Down }

    private ScrollDirection _currentScrollDirection = ScrollDirection.None;

    /// <summary>
    /// Fires when hotbar slot changes
    /// </summary>
    public static event Action<Slot> OnHotbarSlotChanged;

    private Slot _currentHotbarSlot;

    private void Start()
    {
        if (_hotbarSlots[0] == null) return;

        _currentHotbarSlot = _hotbarSlots[0];
        setIndicatorPosition(_currentHotbarSlot);
        OnHotbarSlotChanged?.Invoke(_currentHotbarSlot);
    }

    void Update()
    {
        // Check for mouse scroll input
        float scrollAmountY = Input.mouseScrollDelta.y;

        // Call a method or execute an action when the scroll amount changes
        if (scrollAmountY != 0f && !_isHandlingScroll)
        {
            // Determine the scroll direction
            if (scrollAmountY > 0)
            {
                _currentScrollDirection = ScrollDirection.Up;
            }
            else if (scrollAmountY < 0)
            {
                _currentScrollDirection = ScrollDirection.Down;
            }

            // Example: Call a method to handle the scroll event with sensitivity applied
            handleMouseScroll();

            _isHandlingScroll = true;
        }
        else if (scrollAmountY == 0f)
        {
            _isHandlingScroll = false;
        }
    }

    // Example method to handle the scroll event
    void handleMouseScroll()
    {
        if (_currentScrollDirection == ScrollDirection.Down)
        {
            int currentIndex = _hotbarSlots.IndexOf(_currentHotbarSlot);
            currentIndex = (currentIndex + 1) % _hotbarSlots.Count;
            _currentHotbarSlot = _hotbarSlots[currentIndex];
        }
        else if (_currentScrollDirection == ScrollDirection.Up)
        {
            int currentIndex = _hotbarSlots.IndexOf(_currentHotbarSlot);
            currentIndex = (currentIndex - 1 + _hotbarSlots.Count) % _hotbarSlots.Count;
            _currentHotbarSlot = _hotbarSlots[currentIndex];
        }

        setIndicatorPosition(_currentHotbarSlot);
        OnHotbarSlotChanged?.Invoke(_currentHotbarSlot);

        _currentScrollDirection = ScrollDirection.None;
    }
    void setIndicatorPosition(Slot slot)
    {
        float x = slot.transform.position.x;
        _indicator.transform.position = new Vector3(x, _indicator.transform.position.y, _indicator.transform.position.z);
    }
}
