using UnityEngine;

namespace DmrEventBus
{
    public class TestFire : MonoBehaviour
    {
        TestEvent eventInstance;
        private void Start()
        {
            eventInstance = new TestEvent();

            eventInstance.Value = Random.Range(0, 100);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                eventInstance.Value = Random.Range(0, 100);
                EventBus.Publish(eventInstance);
            }
        }
    }
}
