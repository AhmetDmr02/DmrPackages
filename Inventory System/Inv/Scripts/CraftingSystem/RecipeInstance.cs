using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecipeInstance : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _recipeImage;

    //This scripts kinda act like a slot but for crafting recipes
    private CraftingRecipe _recipe;

    public CraftingRecipe Recipe => _recipe;

    public void OnPointerClick(PointerEventData eventData)
    {
        CraftingManager.Instance.UpdateCurrentRecipe(_recipe);
        Debug.Log("Updated");
    }

    public void SetupRecipe(CraftingRecipe recipe_)
    {
        _recipe = recipe_;

        _recipeImage.sprite = recipe_.RecipeIcon;
    }
}
