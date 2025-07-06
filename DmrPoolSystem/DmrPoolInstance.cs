using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DmrPoolSystem
{
    public class DmrPoolInstance
    {
        private readonly Dictionary<GameObject, Queue<GameObject>> _currentPool = new(25);
        private readonly Dictionary<GameObject, GameObject> _liveInstanceMap;

        //To prevent double return protection 
        private readonly HashSet<GameObject> _activeObjects;

        public bool DisableRememberBufferCheck = false;

        private readonly int _rememberBufferMaxSize;
        private readonly bool _sendWarning;

        private Transform _mainParentTransform;

        /// <param name="rememberBufferWarningSize">
        /// If pool object remember buffer is bigger than this value,
        /// it will do a null object scan to clean it and send a warning if sendWarning is true.
        /// liveInstanceMap capacity will be set to 0.5x rememberBufferWarningSize
        /// </param>
        public DmrPoolInstance(bool dontDestroyOnLoad = false,int rememberBufferWarningSize = 500, bool sendWarning = true)
        {
            _rememberBufferMaxSize = rememberBufferWarningSize;
            _sendWarning = sendWarning;
            _liveInstanceMap = new Dictionary<GameObject, GameObject>(Mathf.CeilToInt(0.5f * rememberBufferWarningSize));
            _activeObjects = new HashSet<GameObject>(Mathf.CeilToInt(0.5f * rememberBufferWarningSize));

            _mainParentTransform = new GameObject("DmrPoolSystem").transform;

            if (dontDestroyOnLoad)
            {
                GameObject.DontDestroyOnLoad(_mainParentTransform.gameObject);
            }
        }

        #region Public Methods
        public GameObject GetPoolObject(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogWarning("GetPoolObject called with a null prefab");
                return null;
            }

            if (_mainParentTransform == null)
            {
                //DDOL is not activated but scene is changed
                _mainParentTransform = new GameObject("DmrPoolSystem").transform;
            }

            if (!_currentPool.TryGetValue(prefab, out var pool))
            {
                RegisterPoolObject(prefab);
                pool = _currentPool[prefab]; 
            }

            //if references went null we need to clean that up
            //it can happen on scene transitions
            while (pool.Count > 0 && pool.Peek() == null)
            {
                pool.Dequeue();
            }

            if (pool.Count == 0)
            {
                PopulatePool(prefab, 1);
            }

            GameObject instance = pool.Dequeue();
            instance.SetActive(true);

            if (instance.TryGetComponent(out IPoolableGameObject poolable))
            {
                poolable.OnPoolGet();
            }

            _liveInstanceMap[instance] = prefab;
            _activeObjects.Add(instance);

            if (Mathf.Max(_activeObjects.Count, _liveInstanceMap.Count) > _rememberBufferMaxSize)
            {
                CleanUpPoolGarbage();
            }

            return instance;
        }
        public void ReturnPoolObject(GameObject createdObject)
        {
            if (createdObject == null) return;

            if (!_activeObjects.Contains(createdObject))
            {
                Debug.LogWarning($"Object {createdObject.name} is not active in pool system. Destroying the object.");

                if (createdObject.TryGetComponent(out IPoolableGameObject poolableObject))
                {
                    poolableObject.OnPoolReturn();
                }

                //To safe guard against destroyed objects
                createdObject.SetActive(false);

                GameObject.Destroy(createdObject);

                return;
            }

            if (createdObject.TryGetComponent(out IPoolableGameObject poolable))
            {
                poolable.OnPoolReturn();
            }

            createdObject.SetActive(false);
            _activeObjects.Remove(createdObject);  // Remove from active tracking

            if (_liveInstanceMap.TryGetValue(createdObject, out var prefab))
            {
                // Safeguard against destroyed prefabs (rare case)
                if (prefab == null)
                {
                    Debug.LogWarning($"Prefab for {createdObject.name} is missing. Destroying instance.");
                    GameObject.Destroy(createdObject);
                    _liveInstanceMap.Remove(createdObject);

                    CheckForRemovedPrefabs();
                    return;
                }

                _currentPool[prefab].Enqueue(createdObject);
                _liveInstanceMap.Remove(createdObject);

                if (Mathf.Max(_activeObjects.Count, _liveInstanceMap.Count) > _rememberBufferMaxSize)
                {
                    CleanUpPoolGarbage();
                }
            }
            else
            {
                Debug.LogWarning("liveInstanceMap does not contain this object. Destroying: " + createdObject.name);
                GameObject.Destroy(createdObject);
            }
        }
        public void RegisterPoolObject(GameObject prefab, int warmupAmount = 1)
        {
            if (prefab == null)
            {
                Debug.LogWarning("RegisterPoolObject called with a null prefab");
                return;
            }

            if (_currentPool.ContainsKey(prefab))
            {
                Debug.LogWarning("Pool already contains " + prefab.name);
                return;
            }

            PopulatePool(prefab, warmupAmount);
        }
        #endregion

        #region Private Methods
        private void PopulatePool(GameObject prefab, int count = 1)
        {
            if (!_currentPool.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>(count);
                _currentPool[prefab] = pool;
            }

            for (int i = 0; i < count; i++)
            {
                if (_mainParentTransform == null)
                {
                    _mainParentTransform = new GameObject("DmrPoolSystem").transform;
                }

                GameObject instance = GameObject.Instantiate(prefab, _mainParentTransform);
                instance.SetActive(false);
                pool.Enqueue(instance);
            }
        }
        private void CleanUpPoolGarbage()
        {
            if (DisableRememberBufferCheck) return;

            Debug.LogWarning("liveInstanceMap reached cleanup threshold: " + _liveInstanceMap.Count);

            var keys = _liveInstanceMap.Keys.ToList();
            foreach (var instance in keys)
            {
                if (instance == null)
                {
                    _liveInstanceMap.Remove(instance);
                    if (_sendWarning)
                        Debug.LogWarning("Removed null entry from liveInstanceMap");
                }
            }

            _activeObjects.RemoveWhere(obj => obj == null);
        }
        private void CheckForRemovedPrefabs()
        {
            var nullKeys = new List<GameObject>();

            foreach (var key in _currentPool.Keys)
            {
                if (key == null)
                    nullKeys.Add(key);
            }

            foreach (var key in nullKeys)
            {
                Debug.LogWarning("Destroyed prefab has been detected all pooled objects of this main prefab will be removed");

                _currentPool.Remove(key);
            }
        }
        #endregion
    }
}