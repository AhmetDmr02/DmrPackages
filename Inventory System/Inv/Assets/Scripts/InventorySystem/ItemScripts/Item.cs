using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Create New Item")]
public class Item : ScriptableObject
{
    [Header("Main Stats")]
    public string ItemID;
    public string ItemName;
    [TextArea]
    public string ItemDescription;
    public Item_Category ItemCategory;
    public bool ItemStackable;

    //This doesn't matter if item is not stackable
    public int ItemMaxStackSize;

    [Header("Art")]
    public Sprite ItemPicture;

    private void OnValidate()
    {
        if (ItemMaxStackSize < 1) ItemMaxStackSize = 1;
        if (!ItemStackable) ItemMaxStackSize = 1;
    }
}

public enum Item_Category
{
    Helmet,
    Armor,
    Weapon,
    Resource,
    Food,
    Consumable,
    Misc
}
