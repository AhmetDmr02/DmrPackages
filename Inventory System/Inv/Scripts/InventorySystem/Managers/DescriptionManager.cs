using UnityEngine;
using TMPro;

public class DescriptionManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText, _descriptionText, _categoryText;

    public static DescriptionManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    public void SetDescriptor(string title, string description, string category)
    {
        _titleText.text = title;
        _descriptionText.text = description;
        _categoryText.text = $"Item Category: \n {category}";
    }
    public void ClearDescriptor()
    {
        _titleText.text = "";
        _descriptionText.text = "";
        _categoryText.text = "";
    }
}
