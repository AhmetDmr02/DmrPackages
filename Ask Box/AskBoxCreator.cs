using UnityEngine;
using static Core.Utulities.AskBox;

namespace Core.Utulities
{
    public class AskBoxCreator : MonoBehaviour
    {
        [SerializeField] private Transform parentObject;
        [SerializeField] private GameObject askBoxObject;

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
            GameObject go = Instantiate(askBoxObject,parentObject);

            go.GetComponent<AskBox>().SetupAskBox(askBoxTitle, askBoxDescription, firstButtonText, secondButtonText, buttonActionPrimary, buttonActionSecondary);
        }
    }
}
