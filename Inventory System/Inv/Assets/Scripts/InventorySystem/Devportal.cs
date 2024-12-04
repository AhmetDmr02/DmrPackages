using UnityEngine;
using TMPro;
public class Devportal : MonoBehaviour
{
    public TMP_InputField IdField, CountField;
    public void AddItemID()
    {
        string itemId = IdField.text;
        int i = int.Parse(CountField.text);

        itemId = itemId.Trim();
        itemId = itemId.ToLower();

        if (InventoryManager.instance.CanPlayerTakeThisItem(InventoryManager.instance.ItemLibrary[itemId], i))
            InventoryManager.instance.AddItem(InventoryManager.instance.ItemLibrary[itemId], i);
        else
            Debug.Log("Cannot Add Item Since There Is No Available Space Left");

    }
    public void RemoveItemID()
    {
        string itemId = IdField.text;
        int i = int.Parse(CountField.text);

        itemId = itemId.Trim();
        itemId = itemId.ToLower();

        if (InventoryManager.instance.CanPlayerRemoveThisItem(InventoryManager.instance.ItemLibrary[itemId], i,true))
            InventoryManager.instance.RemoveItem(InventoryManager.instance.ItemLibrary[itemId], i,true);
        else
            Debug.Log("Cannot Remove Item Since There Is Not Enough Material Found");
    }
}
