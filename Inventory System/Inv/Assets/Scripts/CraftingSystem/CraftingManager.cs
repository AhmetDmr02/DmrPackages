using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] private CraftingRecipe[] allCraftingRecipes;

    // GUI Elements
    [Header("GUI")]
    [SerializeField] private GameObject inputOutputIndicatorObject;
    [SerializeField] private GameObject recipeInstanceObject;
    [SerializeField] private Transform recipeGridObject;
    [SerializeField] private Transform recipeInputsObject;
    [SerializeField] private Transform recipeOutputsObject;
    [SerializeField] private TextMeshProUGUI buttonText;

    public CraftingRecipe CurrentRecipe { get; private set; }
    private List<GameObject> createdRecipes = new List<GameObject>();
    public static CraftingManager instance;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        // Subscribing Some Events To Refresh GUI On Inventory Changes
        InventoryManager.instance.OnItemAdded += (Item item) => { if (CurrentRecipe != null) UpdateCurrentRecipe(CurrentRecipe); };
        InventoryManager.instance.OnItemAdded += (Item item) => { refreshInputOutputIndicator(); };
        InventoryManager.instance.OnItemRemoved += (Item item) => { if (CurrentRecipe != null) UpdateCurrentRecipe(CurrentRecipe); };
        InventoryManager.instance.OnItemRemoved += (Item item) => { refreshInputOutputIndicator(); };
        InventoryManager.instance.OnInventorySlotChanged += () => { if (CurrentRecipe != null) UpdateCurrentRecipe(CurrentRecipe); };
        InventoryManager.instance.OnInventorySlotChanged += () => { if (CurrentRecipe != null) refreshInputOutputIndicator(); };

        validateRecipes();
    }

    private void OnDestroy()
    {
        // Unsubscribing from events to prevent memory leaks
        InventoryManager.instance.OnItemAdded -= (Item item) => { if (CurrentRecipe != null) UpdateCurrentRecipe(CurrentRecipe); };
        InventoryManager.instance.OnItemAdded -= (Item item) => { refreshInputOutputIndicator(); };
        InventoryManager.instance.OnItemRemoved -= (Item item) => { if (CurrentRecipe != null) UpdateCurrentRecipe(CurrentRecipe); };
        InventoryManager.instance.OnItemRemoved -= (Item item) => { refreshInputOutputIndicator(); };
        InventoryManager.instance.OnInventorySlotChanged -= () => { if (CurrentRecipe != null) UpdateCurrentRecipe(CurrentRecipe); };
        InventoryManager.instance.OnInventorySlotChanged -= () => { if (CurrentRecipe != null) refreshInputOutputIndicator(); };
    }

    /// <summary>
    /// Updates the current crafting recipe and checks if the player has enough materials and inventory space to craft.
    /// </summary>
    /// <param name="recipe">The crafting recipe to update.</param>
    public void UpdateCurrentRecipe(CraftingRecipe recipe)
    {
        // Null check for the recipe
        if (recipe == null) return;

        CurrentRecipe = recipe;

        // Check if the player has enough materials and inventory space to craft
        bool doesPlayerHaveMaterials = doesPlayerHaveEnoughMaterial(recipe.Inputs);
        refreshInputOutputIndicator();

        if (!doesPlayerHaveMaterials)
        {
            buttonText.text = "Doesn't have enough materials to craft";
            return;
        }

        bool doesPlayerHaveSpace = doesPlayerHaveEnoughSpace(recipe.Outputs);

        if (!doesPlayerHaveSpace)
        {
            buttonText.text = "Doesn't have enough inventory space";
            return;
        }

        buttonText.text = "Craft!";
    }

    /// <summary>
    /// Crafts the currently selected recipe if the player has enough materials and inventory space.
    /// </summary>
    public void CraftItem()
    {
        if (CurrentRecipe == null) return;

        bool doesPlayerHaveMaterials = doesPlayerHaveEnoughMaterial(CurrentRecipe.Inputs);

        if (!doesPlayerHaveMaterials) return;

        bool doesPlayerHaveSpace = doesPlayerHaveEnoughSpace(CurrentRecipe.Outputs);

        if (!doesPlayerHaveSpace) return;

        // Removing items and giving the crafted item
        foreach (RecipeInputDetails inputDetails in CurrentRecipe.Inputs)
        {
            int neededCount = inputDetails.Count;
            Item neededItem = inputDetails.Item;

            if (inputDetails.DestroyOnCraft)
                InventoryManager.instance.RemoveItem(neededItem, neededCount, inputDetails.RequiredSerializedData);
        }

        foreach (RecipeOutputDetails outputDetails in CurrentRecipe.Outputs)
        {
            int outputItemCount = outputDetails.Count;
            Item outputItem = outputDetails.Item;

            InventoryManager.instance.AddItem(outputItem, outputItemCount, outputDetails.SerializedData);
        }
    }

    #region VisualUpdateFunctions
    /// <summary>
    /// Refreshes the input and output indicators for the currently selected recipe.
    /// </summary>
    private List<GameObject> createdIndicators = new List<GameObject>();
    private void refreshInputOutputIndicator()
    {
        if (CurrentRecipe == null) return;

        // Deleting old indicators
        foreach (GameObject go in createdIndicators)
        {
            Destroy(go);
        }

        // Creating and validating indicators for recipe inputs
        Dictionary<Item, int> inputCalculator = new Dictionary<Item, int>();
        foreach (RecipeInputDetails inputDetails in CurrentRecipe.Inputs)
        {
            // Before setting up indicators, we need to detect the same recipes
            if (inputCalculator.ContainsKey(inputDetails.Item))
            {
                int i = inputCalculator[inputDetails.Item];
                i += inputDetails.Count;
                inputCalculator[inputDetails.Item] = i;
            }
            else
            {
                inputCalculator.Add(inputDetails.Item, inputDetails.Count);
            }
        }

        foreach (RecipeInputDetails inputDetails in CurrentRecipe.Inputs)
        {
            GameObject createdInputDetail = Instantiate(inputOutputIndicatorObject, recipeInputsObject);

            InputOutputIndicator indicator = createdInputDetail.GetComponent<InputOutputIndicator>();

            int neededCount = inputDetails.Count;
            Item neededItem = inputDetails.Item;

            int itemOnPlayerInventory = InventoryManager.instance.GetItemCountOnPlayerInventoryWithSerializedData(neededItem.ItemID,inputDetails.RequiredSerializedData);

            // True means green, False means red
            bool greenOrRed = true;

            foreach (Item item in inputCalculator.Keys)
            {
                if (item != neededItem) continue;

                if (itemOnPlayerInventory < inputCalculator[item])
                {
                    greenOrRed = false;
                    break;
                }
            }

            Color sendColor = greenOrRed ? Color.green : Color.red;

            indicator.SetupIndicator(sendColor, neededItem.ItemPicture, itemOnPlayerInventory, neededCount);

            createdIndicators.Add(createdInputDetail);
        }

        // Creating and validating indicators for recipe outputs
        foreach (RecipeOutputDetails outputDetails in CurrentRecipe.Outputs)
        {
            GameObject createdInputDetail = Instantiate(inputOutputIndicatorObject, recipeOutputsObject);

            InputOutputIndicator indicator = createdInputDetail.GetComponent<InputOutputIndicator>();

            int outputCount = outputDetails.Count;
            Item neededItem = outputDetails.Item;

            bool emptySpaceOnInventory = doesPlayerHaveEnoughSpace(CurrentRecipe.Outputs);

            // For now, all outputs are shown in gray color
            Color sendColor = Color.gray;

            indicator.SetupIndicator(sendColor, neededItem.ItemPicture, outputCount, 0);

            createdIndicators.Add(createdInputDetail);
        }
    }

    /// <summary>
    /// Validates the crafting recipes and creates instances of recipe objects in the UI.
    /// </summary>
    private void validateRecipes()
    {
        // First, delete old instances
        foreach (GameObject go in createdRecipes)
        {
            Destroy(go);
        }

        // Create instances of crafting recipes
        foreach (CraftingRecipe recipe in allCraftingRecipes)
        {
            GameObject go = Instantiate(recipeInstanceObject, recipeGridObject);

            go.GetComponent<RecipeInstance>().SetupRecipe(recipe);

            createdRecipes.Add(go);
        }
    }
    #endregion

    #region HelperFunctions
    /// <summary>
    /// Checks if the player has enough materials to craft the specified items.
    /// </summary>
    /// <param name="items">The items needed for crafting.</param>
    /// <returns>True if the player has enough materials, False otherwise.</returns>
    private bool doesPlayerHaveEnoughMaterial(RecipeInputDetails[] items)
    {
        bool toggle = true;
        Dictionary<(Item,string), int> neededCountById = new Dictionary<(Item, string), int>();
        foreach (RecipeInputDetails item in items)
        {
            if (neededCountById.ContainsKey((item.Item,item.RequiredSerializedData)))
            {
                int i = neededCountById[(item.Item, item.RequiredSerializedData)];
                i += item.Count;
                neededCountById[(item.Item, item.RequiredSerializedData)] = i;
            }
            else
            {
                neededCountById.Add((item.Item, item.RequiredSerializedData), item.Count);
            }
        }

        foreach ((Item item , string serializedData) itemTuple in neededCountById.Keys)
        {
            int totalCount = 0;

            if(itemTuple.serializedData == string.Empty)
            {
                totalCount = InventoryManager.instance.GetItemCountOnPlayerInventory(itemTuple.item.ItemID,false);
            }
            else
            {
                totalCount = InventoryManager.instance.GetItemCountOnPlayerInventoryWithSerializedData(itemTuple.item.ItemID, itemTuple.serializedData);
            }

            if (totalCount < neededCountById[(itemTuple. item, itemTuple.serializedData)])
            {
                toggle = false;
                break;
            }
        }

        return toggle;
    }

    /// <summary>
    /// Checks if the player has enough inventory space to receive the specified items.
    /// </summary>
    /// <param name="items">The items that will be added to the inventory.</param>
    /// <returns>True if the player has enough space, False otherwise.</returns>
    public bool doesPlayerHaveEnoughSpace(RecipeOutputDetails[] items)
    {
        // Create a dictionary to keep track of the required count of each item in the output
        Dictionary<(Item item, string serializedData), int> requiredItemCount = new Dictionary<(Item, string), int>();

        // Populate the required count dictionary based on the RecipeOutputDetails array
        foreach (RecipeOutputDetails itemDetails in items)
        {
            if (requiredItemCount.ContainsKey((itemDetails.Item, itemDetails.SerializedData)))
            {
                requiredItemCount[(itemDetails.Item, itemDetails.SerializedData)] += itemDetails.Count;
            }
            else
            {
                requiredItemCount.Add((itemDetails.Item, itemDetails.SerializedData), itemDetails.Count);
            }
        }

        // Iterate through the required items and check if the player has enough space for each item
        foreach (var kvp in requiredItemCount)
        {
            Item item = kvp.Key.item;
            int count = kvp.Value;

            List<Slot> slotsWithSameItem =  InventoryManager.instance.GetSlotsWithId(item.ItemID, kvp.Key.serializedData);
            int remainingSpace = 0;

            // First, try to merge items into existing stacks
            foreach (Slot slot in slotsWithSameItem)
            {
                remainingSpace += item.ItemMaxStackSize - slot.ItemCount;
            }

            // Then, add the remaining space from empty slots

            List<Slot> emptySlots = InventoryManager.instance.GetEmptySlotsForItem(item);
            foreach (Slot slot in emptySlots)
            {
                remainingSpace += item.ItemMaxStackSize;
            }

            if (remainingSpace < count)
            {
                // If the player doesn't have enough space for this item, return false immediately
                return false;
            }
        }

        // If the loop finishes without returning false, it means the player has enough space for all items
        return true;
    }

    #endregion
}
