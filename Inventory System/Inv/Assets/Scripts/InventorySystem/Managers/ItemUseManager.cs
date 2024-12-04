using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemUseManager : MonoBehaviour
{
    void Update()
    {
        //We will detect and activate the interface of item to use it 
        if (Input.GetKeyDown(KeyCode.E))
        {
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

                    if (slot.CurrentItem == null) return;

                    IUsableItem usableItem = slot.CurrentItem as IUsableItem;
                    if (usableItem != null)
                    {
                        usableItem.OnUse();

                        if (usableItem.DestroyOnUse())
                            slot.AdjustItemCount(-1);
                    }

                    break; // Exit the loop after processing the first valid UI element hit
                }
            }
        }

    }
}
