using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace DmrDependencyInjector
{
    [CreateAssetMenu(menuName = "DmrDependencyInjector/DmrFactoryService")]
    public class DmrFactoryService : ScriptableObject 
    {
        [SerializeField] private GameObject[] _serviceObjectPrefabs;

        [SerializeField] private bool _closeWarning = false;

        //To fetch needed prefab
        private ConcurrentDictionary<Type, GameObject> _serviceObjects = new ConcurrentDictionary<Type, GameObject>();
        public ConcurrentDictionary<Type, GameObject> ServiceObjects => _serviceObjects;

        //To register all the types it resides if needed
        private ConcurrentDictionary<GameObject, Type[]> _serviceObjectTypes = new ConcurrentDictionary<GameObject, Type[]>();

        public void OnInitialize()
        {
            if (_serviceObjectPrefabs == null || _serviceObjectPrefabs.Length == 0)
                return;

            foreach (GameObject prefab in _serviceObjectPrefabs)
            {
                if (prefab == null) continue;

                var components = prefab.GetComponents<MonoBehaviour>();
                var validTypes = new List<Type>();

                foreach (var comp in components)
                {
                    if (comp == null) continue; // Missing script

                    var type = comp.GetType();

                    if (comp is not DIRegisterableMonoBehaviour)
                    {
                        if (!_closeWarning)
                            Debug.LogWarning($"Type {type.Name} must inherit from DIRegisterableMonoBehaviour to register.");

                        continue;
                    }

                    _serviceObjects.TryAdd(type, prefab);
                    validTypes.Add(type);
                }

                _serviceObjectTypes.TryAdd(prefab, validTypes.ToArray());
            }
        }

        public GameObject GetServiceObject(Type type)
        {
            return _serviceObjects.TryGetValue(type, out var prefab) ? prefab : null;
        }

        public bool CreateServiceObject(Type type)
        {
            if (_serviceObjects.TryGetValue(type, out var prefab))
            {
                Instantiate(prefab);
                return true;
            }

            return false;
        }
    }
}
