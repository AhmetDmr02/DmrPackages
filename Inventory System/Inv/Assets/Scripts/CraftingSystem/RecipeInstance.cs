using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecipeInstance : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image recipeImage;

    //This scripts kinda act like a slot but for crafting recipes
    private CraftingRecipe recipe;

    public CraftingRecipe Recipe => recipe;

    public void OnPointerClick(PointerEventData eventData)
    {
        CraftingManager.instance.UpdateCurrentRecipe(recipe);
        Debug.Log("Updated");
    }

    public void SetupRecipe(CraftingRecipe recipe_)
    {
        recipe = recipe_;

        recipeImage.sprite = recipe_.RecipeIcon;
    }
}
