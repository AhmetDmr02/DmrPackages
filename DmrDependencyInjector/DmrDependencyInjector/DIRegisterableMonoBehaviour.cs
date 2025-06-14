using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DmrDependencyInjector
{
    [DefaultExecutionOrder(-1)]
    public class DIRegisterableMonoBehaviour : MonoBehaviour
    {
        [SerializeField] protected bool _tryAutoInjectOnReferenceLose = true;

        protected InjectionResult _injectResult;

        protected HashSet<Type> _injectedTypes = new();
        protected void Awake()
        {
            DIInjectorManager.Register(this);

            DIInjectorManager.OnServiceUnregistered += OnServiceDestroyed;

            if (_tryAutoInjectOnReferenceLose)
            {
                _injectedTypes.Clear();

                List<FieldInfo> injectableFields = DIInjectorManager.GetInjectableFields(GetType());

                foreach (FieldInfo field in injectableFields)
                {
                    _injectedTypes.Add(field.FieldType);
                }
            }
        }
        
        protected void Start()
        {
            DIInjectorManager.InjectClassDependencies(this,out _injectResult);
        }

        protected void OnDestroy()
        {
            DIInjectorManager.OnServiceUnregistered -= OnServiceDestroyed;

            DIInjectorManager.Unregister(this);

            StopAllCoroutines();
        }

        private void OnServiceDestroyed(Type type)
        {
            if (_tryAutoInjectOnReferenceLose)
            {
                if (_injectedTypes.Contains(type))
                {
                    DIInjectorManager.InjectClassDependencies(this, out _injectResult);

                    StartCoroutine(WaitUntilInjectionIsValid());
                }
            }
        }

        private IEnumerator WaitUntilInjectionIsValid()
        {
            yield return new WaitUntil(() => DIInjectorManager.CanInjectDependencies(this));

            DIInjectorManager.InjectClassDependencies(this, out _injectResult);
        }
    }
}
