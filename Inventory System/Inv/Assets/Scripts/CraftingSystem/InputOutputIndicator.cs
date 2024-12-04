using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InputOutputIndicator : MonoBehaviour
{
    //Shows how much item is needed for the recipe
    [SerializeField] private TextMeshProUGUI itemCountText;
    [SerializeField] private Image itemIcon;

    public void SetupIndicator(Color color, Sprite icon, int itemCount, int neededItemCount)
    {
        Color32 transparentColor = color;
        transparentColor.a = 50;

        this.gameObject.GetComponent<Image>().color = transparentColor;

        itemIcon.sprite = icon;

        if (neededItemCount == 0)
            itemCountText.text = itemCount.ToString();
        else
            itemCountText.text = $"{itemCount}/{neededItemCount}";
    }
}
