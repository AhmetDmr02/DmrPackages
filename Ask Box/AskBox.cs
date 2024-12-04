using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Core.Utulities
{
    public class AskBox : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI askBoxTitleText,askBoxDescriptionText;
        [SerializeField] private TextMeshProUGUI primaryButtonText, secondaryButtonText;
        [SerializeField] private Button primaryButton, secondaryButton;

        public delegate void ButtonAction();

        private ButtonAction buttonActionPrimary;
        private ButtonAction buttonActionSecondary;


        public void SetupAskBox(string askBoxTitle,string askBoxDescription,string firstButtonText,string secondButtonText,ButtonAction buttonActionPrimary, ButtonAction buttonActionSecondary)
        {
            this.buttonActionPrimary = buttonActionPrimary;
            this.buttonActionSecondary = buttonActionSecondary;

            primaryButton.onClick.AddListener(() => invokePrimaryButton());
            secondaryButton.onClick.AddListener(() => invokeSecondaryButton());

            askBoxTitleText.text = askBoxTitle;
            askBoxDescriptionText.text = askBoxDescription;

            primaryButtonText.text = firstButtonText;
            secondaryButtonText.text = secondButtonText;

            primaryButton.interactable = true;
            secondaryButton.interactable = true;
        }
        private void invokePrimaryButton()
        {
            buttonActionPrimary?.Invoke();

            EventSystem.current.SetSelectedGameObject(null);

            Destroy(this.gameObject);
        }
        private void invokeSecondaryButton()
        {
            buttonActionSecondary?.Invoke();

            EventSystem.current.SetSelectedGameObject(null);

            Destroy(this.gameObject);
        }
    }
}
