using UnityEngine;
using static Dmr.Utulities.AskBox.AskBox;

namespace Dmr.Utulities.AskBox
{
    public class AskBoxCreator : MonoBehaviour
    {
        [SerializeField] private Transform _parentObject;
        [SerializeField] private GameObject _askBoxObject;

        public static AskBoxCreator instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);
        }

        public void CreateAskBox(string askBoxTitle, string askBoxDescription, string firstButtonText, string secondButtonText, ButtonAction buttonActionPrimary, ButtonAction buttonActionSecondary)
        {
            GameObject go = Instantiate(_askBoxObject,_parentObject);

            go.GetComponent<AskBox>().SetupAskBox(askBoxTitle, askBoxDescription, firstButtonText, secondButtonText, buttonActionPrimary, buttonActionSecondary);
        }
    }
}
