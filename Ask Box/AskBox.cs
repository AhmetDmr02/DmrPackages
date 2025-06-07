using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Dmr.Utulities.AskBox
{
    public class AskBox : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _askBoxTitleText,_askBoxDescriptionText;
        [SerializeField] private TextMeshProUGUI _primaryButtonText, _secondaryButtonText;
        [SerializeField] private Button _primaryButton, _secondaryButton;

        public delegate void ButtonAction();

        private ButtonAction _buttonActionPrimary;
        private ButtonAction _buttonActionSecondary;


        public void SetupAskBox(string askBoxTitle,string askBoxDescription,string firstButtonText,string secondButtonText,ButtonAction buttonActionPrimary, ButtonAction buttonActionSecondary)
        {
            this._buttonActionPrimary = buttonActionPrimary;
            this._buttonActionSecondary = buttonActionSecondary;

            _primaryButton.onClick.RemoveAllListeners();
            _secondaryButton.onClick.RemoveAllListeners();

            _primaryButton.onClick.AddListener(() => invokePrimaryButton());
            _secondaryButton.onClick.AddListener(() => invokeSecondaryButton());

            _askBoxTitleText.text = askBoxTitle;
            _askBoxDescriptionText.text = askBoxDescription;

            _primaryButtonText.text = firstButtonText;
            _secondaryButtonText.text = secondButtonText;

            _primaryButton.interactable = true;
            _secondaryButton.interactable = true;
        }
        private void invokePrimaryButton()
        {
            _buttonActionPrimary?.Invoke();

            EventSystem.current.SetSelectedGameObject(null);

            Destroy(this.gameObject);
        }
        private void invokeSecondaryButton()
        {
            _buttonActionSecondary?.Invoke();

            EventSystem.current.SetSelectedGameObject(null);

            Destroy(this.gameObject);
        }
    }
}
