using UnityEngine;

namespace DmrEventBus
{
    public class TestListener : MonoBehaviour
    {
        void Start()
        {
            EventBus.Subscribe<TestEvent>(this, (e) => Debug.Log($"TestEvent fired with value: {e.Value}"));
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                EventBus.Unsubscribe<TestEvent>(this);
            }
        }
    }
}
