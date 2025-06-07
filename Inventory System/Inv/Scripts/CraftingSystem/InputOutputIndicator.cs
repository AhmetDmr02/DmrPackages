using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InputOutputIndicator : MonoBehaviour
{
    //Shows how much item is needed for the recipe
    [SerializeField] private TextMeshProUGUI _itemCountText;
    [SerializeField] private Image _itemIcon;

    public void SetupIndicator(Color color, Sprite icon, int itemCount, int neededItemCount)
    {
        Color32 transparentColor = color;
        transparentColor.a = 50;

        this.gameObject.GetComponent<Image>().color = transparentColor;

        _itemIcon.sprite = icon;

        if (neededItemCount == 0)
            _itemCountText.text = itemCount.ToString();
        else
            _itemCountText.text = $"{itemCount}/{neededItemCount}";
    }
}
