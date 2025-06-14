using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DmrDependencyInjector
{
    public class DIInjectOnlyMonoBehaviour : MonoBehaviour
    {
        [Header("DI Settings")]
        [SerializeField] protected bool _autoInjectOnStart = true;
        [SerializeField] protected bool _tryAutoInjectOnReferenceLose = true;
        [Space(15)]

        protected InjectionResult _injectResult;

        protected HashSet<Type> InjectedTypes = new();

        protected void Awake()
        {
            if (_tryAutoInjectOnReferenceLose)
            {
                InjectedTypes.Clear();

                List<FieldInfo> injectableFields = DIInjectorManager.GetInjectableFields(GetType());

                foreach (FieldInfo field in injectableFields)
                {
                    InjectedTypes.Add(field.FieldType);
                }
            }
        }

        protected void Start()
        {
            DIInjectorManager.InjectClassDependencies(this, out _injectResult);

            DIInjectorManager.OnServiceUnregistered += OnServiceDestroyed;
        }

        protected void OnDestroy()
        {
            DIInjectorManager.OnServiceUnregistered -= OnServiceDestroyed;

            StopAllCoroutines();
        }

        private void OnServiceDestroyed(Type type)
        {
            if (_tryAutoInjectOnReferenceLose)
            {
                if (InjectedTypes.Contains(type))
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

            Debug.Log("Injection recovered");
        }
    }
}
