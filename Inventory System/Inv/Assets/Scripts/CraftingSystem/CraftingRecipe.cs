using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Crafting/Create New Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public Sprite RecipeIcon;

    public RecipeInputDetails[] Inputs;

    public RecipeOutputDetails[] Outputs;

    private void OnValidate()
    {
        foreach (RecipeInputDetails recipe in Inputs)
        {
            if (recipe.Count <= 0) recipe.Count = 1;
        }
        foreach (RecipeOutputDetails recipe in Outputs)
        {
            if (recipe.Count <= 0) recipe.Count = 1;
        }
    }
}

[System.Serializable]
public class RecipeInputDetails
{
    public Item Item;

    /// <summary>
    /// Leave it blank if you dont wanna use any serialized data
    /// </summary>
    public string RequiredSerializedData;

    public int Count;
    public bool DestroyOnCraft;
}
[System.Serializable]
public class RecipeOutputDetails
{
    public Item Item;

    /// <summary>
    /// Leave it blank if you dont wanna use any serialized data
    /// </summary>
    public string SerializedData;

    public int Count;
}
