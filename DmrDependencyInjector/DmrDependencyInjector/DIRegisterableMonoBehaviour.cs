using UnityEngine;

namespace DmrDependencyInjector
{
    [DefaultExecutionOrder(-1)]
    public class DIRegisterableMonoBehaviour : MonoBehaviour
    {
        protected void Awake()
        {
            DIInjectorManager.Register(this);
        }

        protected InjectionResult injectResult;
        protected void Start()
        {
            DIInjectorManager.InjectClassDependencies(this,out injectResult);
        }

        protected void OnDestroy()
        {
            DIInjectorManager.Unregister(this);
        }
    }
}
