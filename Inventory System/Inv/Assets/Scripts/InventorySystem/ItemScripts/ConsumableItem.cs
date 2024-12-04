using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable Item", menuName = "Items /Consumable Item")]
public class ConsumableItem : Item, IUsableItem
{
    public bool DestroyOnUse_;

    public float Water, Food;

    public bool DestroyOnUse()
    {
        return DestroyOnUse_;
    }

    public void OnUse()
    {
        Debug.Log($"I Ate Something! Water: {Water} Food: {Food}");
    }
}
