using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] private CraftingRecipe[] _allCraftingRecipes;

    // GUI Elements
    [Header("GUI")]
    [SerializeField] private GameObject _inputOutputIndicatorObject;
    [SerializeField] private GameObject _recipeInstanceObject;
    [SerializeField] private Transform _recipeGridObject;
    [SerializeField] private Transform _recipeInputsObject;
    [SerializeField] private Transform _recipeOutputsObject;
    [SerializeField] private TextMeshProUGUI _buttonText;

    public CraftingRecipe CurrentRecipe { get; private set; }
    private List<GameObject> _createdRecipes = new List<GameObject>();
    public static CraftingManager Instance;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        // Subscribing Some Events To Refresh GUI On Inventory Changes
        InventoryManager.Instance.OnItemAdded += HandleItemEvent;
        InventoryManager.Instance.OnItemRemoved += HandleItemEvent;
        InventoryManager.Instance.OnInventorySlotChanged += HandleSlotChanged;

        ValidateRecipes();
    }

    private void OnDestroy()
    {
        // Proper unsubscription
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemAdded -= HandleItemEvent;
            InventoryManager.Instance.OnItemRemoved -= HandleItemEvent;
            InventoryManager.Instance.OnInventorySlotChanged -= HandleSlotChanged;
        }
    }

    private void HandleItemEvent(Item item)
    {
        if (CurrentRecipe != null)
        {
            UpdateCurrentRecipe(CurrentRecipe);
            RefreshInputOutputIndicator();
        }
    }

    private void HandleSlotChanged()
    {
        if (CurrentRecipe != null)
        {
            UpdateCurrentRecipe(CurrentRecipe);
            RefreshInputOutputIndicator();
        }
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
        bool doesPlayerHaveMaterials = DoesPlayerHaveEnoughMaterial(recipe.Inputs);
        RefreshInputOutputIndicator();

        if (!doesPlayerHaveMaterials)
        {
            _buttonText.text = "Doesn't have enough materials to craft";
            return;
        }

        bool doesPlayerHaveSpace = DoesPlayerHaveEnoughSpace(recipe.Outputs);

        if (!doesPlayerHaveSpace)
        {
            _buttonText.text = "Doesn't have enough inventory space";
            return;
        }

        _buttonText.text = "Craft!";
    }

    /// <summary>
    /// Crafts the currently selected recipe if the player has enough materials and inventory space.
    /// </summary>
    public void CraftItem()
    {
        if (CurrentRecipe == null) return;

        bool doesPlayerHaveMaterials = DoesPlayerHaveEnoughMaterial(CurrentRecipe.Inputs);

        if (!doesPlayerHaveMaterials) return;

        bool doesPlayerHaveSpace = DoesPlayerHaveEnoughSpace(CurrentRecipe.Outputs);

        if (!doesPlayerHaveSpace) return;

        // Removing items and giving the crafted item
        foreach (RecipeInputDetails inputDetails in CurrentRecipe.Inputs)
        {
            int neededCount = inputDetails.Count;
            Item neededItem = inputDetails.Item;

            if (inputDetails.DestroyOnCraft)
                InventoryManager.Instance.RemoveItem(neededItem, neededCount, inputDetails.RequiredSerializedData);
        }

        foreach (RecipeOutputDetails outputDetails in CurrentRecipe.Outputs)
        {
            int outputItemCount = outputDetails.Count;
            Item outputItem = outputDetails.Item;

            InventoryManager.Instance.AddItem(outputItem, outputItemCount, outputDetails.SerializedData);
        }
    }

    #region VisualUpdateFunctions
    /// <summary>
    /// Refreshes the input and output indicators for the currently selected recipe.
    /// </summary>
    private List<GameObject> _createdIndicators = new List<GameObject>();
    private void RefreshInputOutputIndicator()
    {
        if (CurrentRecipe == null) return;

        // Deleting old indicators
        foreach (GameObject go in _createdIndicators)
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
            GameObject createdInputDetail = Instantiate(_inputOutputIndicatorObject, _recipeInputsObject);

            InputOutputIndicator indicator = createdInputDetail.GetComponent<InputOutputIndicator>();

            int neededCount = inputDetails.Count;
            Item neededItem = inputDetails.Item;

            int itemOnPlayerInventory = InventoryManager.Instance.GetItemCountOnPlayerInventoryWithSerializedData(neededItem.ItemID, inputDetails.RequiredSerializedData);

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

            indicator.SetupIndicator(sendColor, neededItem.GetItemSprite(inputDetails.RequiredSerializedData), itemOnPlayerInventory, neededCount);

            _createdIndicators.Add(createdInputDetail);
        }

        // Creating and validating indicators for recipe outputs
        foreach (RecipeOutputDetails outputDetails in CurrentRecipe.Outputs)
        {
            GameObject createdInputDetail = Instantiate(_inputOutputIndicatorObject, _recipeOutputsObject);

            InputOutputIndicator indicator = createdInputDetail.GetComponent<InputOutputIndicator>();

            int outputCount = outputDetails.Count;
            Item neededItem = outputDetails.Item;

            bool emptySpaceOnInventory = DoesPlayerHaveEnoughSpace(CurrentRecipe.Outputs);

            // For now, all outputs are shown in gray color
            Color sendColor = Color.gray;

            indicator.SetupIndicator(sendColor, neededItem.GetItemSprite(outputDetails.SerializedData), outputCount, 0);

            _createdIndicators.Add(createdInputDetail);
        }
    }

    /// <summary>
    /// Validates the crafting recipes and creates instances of recipe objects in the UI.
    /// </summary>
    private void ValidateRecipes()
    {
        // First, delete old instances
        foreach (GameObject go in _createdRecipes)
        {
            Destroy(go);
        }

        // Create instances of crafting recipes
        foreach (CraftingRecipe recipe in _allCraftingRecipes)
        {
            GameObject go = Instantiate(_recipeInstanceObject, _recipeGridObject);

            go.GetComponent<RecipeInstance>().SetupRecipe(recipe);

            _createdRecipes.Add(go);
        }
    }
    #endregion

    #region HelperFunctions
    /// <summary>
    /// Checks if the player has enough materials to craft the specified items.
    /// </summary>
    /// <param name="items">The items needed for crafting.</param>
    /// <returns>True if the player has enough materials, False otherwise.</returns>
    private bool DoesPlayerHaveEnoughMaterial(RecipeInputDetails[] items)
    {
        bool toggle = true;
        Dictionary<(Item, string), int> neededCountById = new Dictionary<(Item, string), int>();
        foreach (RecipeInputDetails item in items)
        {
            if (neededCountById.ContainsKey((item.Item, item.RequiredSerializedData)))
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

        foreach ((Item item, string serializedData) itemTuple in neededCountById.Keys)
        {
            int totalCount = 0;

            if (itemTuple.serializedData == string.Empty)
            {
                totalCount = InventoryManager.Instance.GetItemCountOnPlayerInventory(itemTuple.item.ItemID, false);
            }
            else
            {
                totalCount = InventoryManager.Instance.GetItemCountOnPlayerInventoryWithSerializedData(itemTuple.item.ItemID, itemTuple.serializedData);
            }

            if (totalCount < neededCountById[(itemTuple.item, itemTuple.serializedData)])
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
    public bool DoesPlayerHaveEnoughSpace(RecipeOutputDetails[] items)
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

            List<Slot> slotsWithSameItem = InventoryManager.Instance.GetSlotsWithId(item.ItemID, kvp.Key.serializedData);
            int remainingSpace = 0;

            // First, try to merge items into existing stacks
            foreach (Slot slot in slotsWithSameItem)
            {
                remainingSpace += item.ItemMaxStackSize - slot.ItemCount;
            }

            // Then, add the remaining space from empty slots

            List<Slot> emptySlots = InventoryManager.Instance.GetEmptySlotsForItem(item);
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