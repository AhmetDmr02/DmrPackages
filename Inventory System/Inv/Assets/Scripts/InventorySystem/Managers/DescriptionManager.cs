using UnityEngine;
using TMPro;

public class DescriptionManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText, descriptionText, categoryText;

    public static DescriptionManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    public void SetDescriptor(string title, string description, string category)
    {
        titleText.text = title;
        descriptionText.text = description;
        categoryText.text = $"Item Category: \n {category}";
    }
    public void ClearDescriptor()
    {
        titleText.text = "";
        descriptionText.text = "";
        categoryText.text = "";
    }
}
