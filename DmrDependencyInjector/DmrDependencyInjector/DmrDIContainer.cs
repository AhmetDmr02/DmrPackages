using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DmrDependencyInjector
{
    internal static class DmrDIContainer
    {
        // Maps service Type -> Instance
        private static readonly ConcurrentDictionary<Type, object> _instances = new();
        // Maps Instance -> Set of registered types (for cleanup)
        private static readonly ConcurrentDictionary<object, HashSet<Type>> _instanceToTypes = new();

        internal static bool RegisterWithAllTypes(object instance)
        {
            if (!ValidateRegistration(instance))
                return false;

            var typesToRegister = GetAllRegistrableTypes(instance.GetType());
            return Register(instance, typesToRegister.ToArray());
        }

        internal static bool Register(object instance, params Type[] asTypes)
        {
            if (!ValidateRegistration(instance))
                return false;

            if (asTypes == null || asTypes.Length == 0)
                asTypes = new[] { instance.GetType() };

            foreach (var serviceType in asTypes)
            {
                if (!serviceType.IsAssignableFrom(instance.GetType()))
                {
                    Debug.LogError($"Instance of type {instance.GetType().Name} cannot be registered as {serviceType.Name}");
                    return false;
                }
            }

            var registeredTypes = new HashSet<Type>();
            foreach (var serviceType in asTypes)
            {
                if (_instances.TryGetValue(serviceType, out var existing) && existing != instance)
                {
                    Debug.LogWarning($"Service type {serviceType.Name} already registered. Replacing instance.");
                    CleanupOldInstance(existing, serviceType);
                }
                _instances[serviceType] = instance;
                registeredTypes.Add(serviceType);
            }

            _instanceToTypes.AddOrUpdate(instance,
                registeredTypes,
                (key, existing) => { existing.UnionWith(registeredTypes); return existing; });

            return true;
        }


        internal static List<Type> UnregisterInstance(object instance)
        {
            var removed = new List<Type>();
            if (_instanceToTypes.TryRemove(instance, out var types))
            {
                foreach (var t in types)
                {
                    if (_instances.TryGetValue(t, out var inst) && ReferenceEquals(inst, instance))
                    {
                        _instances.TryRemove(t, out _);
                        removed.Add(t);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Instance of type {instance.GetType().Name} was not registered.");
            }
            return removed;
        }

        // Resolves an instance by service type, or null if none found
        internal static object Resolve(Type serviceType)
        {
            _instances.TryGetValue(serviceType, out var inst);
            return inst;
        }

        // Helpers
        private static HashSet<Type> GetAllRegistrableTypes(Type concreteType)
        {
            var types = new HashSet<Type> { concreteType };
            foreach (var i in concreteType.GetInterfaces())
                types.Add(i);
            var baseType = concreteType.BaseType;
            while (baseType != null && baseType != typeof(object) && baseType != typeof(MonoBehaviour) && baseType != typeof(DIRegisterableObject) && baseType != typeof(DIRegisterableMonoBehaviour))
            {
                types.Add(baseType);
                baseType = baseType.BaseType;
            }
            return types;
        }

        private static void CleanupOldInstance(object oldInstance, Type serviceType)
        {
            if (_instanceToTypes.TryGetValue(oldInstance, out var types))
            {
                types.Remove(serviceType);
                if (types.Count == 0)
                    _instanceToTypes.TryRemove(oldInstance, out _);
            }
        }

        private static bool ValidateRegistration(object instance)
        {
            if (instance is MonoBehaviour && instance is not DIRegisterableMonoBehaviour)
            {
                Debug.LogError("MonoBehaviour must inherit from DIRegisterableMonoBehaviour to register.");
                return false;
            }
            if (!(instance is MonoBehaviour) && !(instance is DIRegisterableObject))
            {
                Debug.LogError($"Type {instance.GetType().Name} must implement DIRegisterableObject or IDIRegisterable.");
                return false;
            }
            return true;
        }
    }
}
