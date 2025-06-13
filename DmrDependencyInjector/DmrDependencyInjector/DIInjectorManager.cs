using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DmrDependencyInjector
{
    public static class DIInjectorManager
    {
        static readonly ConcurrentDictionary<Type, List<FieldInfo>> _cache = new();

        private static DmrFactoryService _factoryService;

        public static event Action<Type> OnServiceUnregistered;

        private static List<string> _failedFields = new List<string>();

        public static void InjectClassDependencies(object target, out InjectionResult result)
        {
            if (_factoryService == null)
            {
                _factoryService = Resources.Load<DmrFactoryService>("DmrFactoryService");

                if (_factoryService == null)
                {
                    result = InjectionResult.Failed;

                    Debug.LogError("DmrFactoryService not found in Resources folder. Please add it.");

                    return;
                }

                _factoryService.OnInitialize();
            }

            _failedFields.Clear();

            try
            {
                var fields = GetInjectableFields(target.GetType());

                foreach (var field in fields)
                {
                    var service = DmrDIContainer.Resolve(field.FieldType);
                    if (service == null)
                    {
                        if (_factoryService.ServiceObjects.TryGetValue(field.FieldType, out var prefab) && prefab != null)
                        {
                            _factoryService.CreateServiceObject(field.FieldType);

                            service = DmrDIContainer.Resolve(field.FieldType);

                            if (service != null)
                            {
                                field.SetValue(target, service);

                                continue;
                            }
                        }

                        _failedFields.Add($"{field.DeclaringType?.Name}.{field.Name} ({field.FieldType.Name})");
                        continue;
                    }
                    field.SetValue(target, service);
                }

                result = _failedFields.Count == 0 ?
                    InjectionResult.Success :
                    InjectionResult.PartialFailure(_failedFields);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Injection failed for {target.GetType().Name}: {ex.Message}");
                result = InjectionResult.Failed;
            }
        }

        public static bool CanInjectDependencies(object target)
        {
            if (_factoryService == null)
            {
                _factoryService = Resources.Load<DmrFactoryService>("DmrFactoryService");

                if (_factoryService == null)
                {
                    Debug.LogError("DmrFactoryService not found in Resources folder. Please add it.");

                    return false;
                }

                _factoryService.OnInitialize();
            }

            var fields = GetInjectableFields(target.GetType());

            bool success = false;
            foreach (var field in fields)
            {
                var service = DmrDIContainer.Resolve(field.FieldType);
                if (service == null)
                {
                    if (_factoryService.ServiceObjects.TryGetValue(field.FieldType, out var prefab) && prefab != null)
                    {
                        _factoryService.CreateServiceObject(field.FieldType);

                        service = DmrDIContainer.Resolve(field.FieldType);

                        if (service != null)
                        {
                            continue;
                        }
                    }

                    success = false;
                    break;
                }
                success = true;
            }

            return success;
        }

        private static List<FieldInfo> GetInjectableFields(Type type)
        {
            return _cache.GetOrAdd(type, t => t
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.IsDefined(typeof(DmrInjectAttribute), true))
                .ToList());
        }

        // Auto-register instance with all its types (class + interfaces + base classes)
        public static void Register(object instance)
        {
            DmrDIContainer.RegisterWithAllTypes(instance);
        }

        public static void Unregister(object instance)
        {
            var unregisteredTypes = DmrDIContainer.UnregisterInstance(instance);
            foreach (var type in unregisteredTypes)
            {
                OnServiceUnregistered?.Invoke(type);
            }
        }

        public static void ClearCache()
        {
            _cache.Clear();
        }
    }

    public class InjectionResult
    {
        public bool IsSuccess { get; private set; }
        public List<string> FailedFields { get; private set; }

        private InjectionResult(bool success, List<string> failedFields = null)
        {
            IsSuccess = success;
            FailedFields = failedFields ?? new List<string>();
        }

        public static InjectionResult Success => new(true);
        public static InjectionResult Failed => new(false);
        public static InjectionResult PartialFailure(List<string> failedFields) => new(false, failedFields);

        public override string ToString()
        {
            if (IsSuccess) return "Success";
            if (FailedFields.Count == 0) return "Failed";
            return $"Failed to inject: {string.Join(", ", FailedFields)}";
        }
    }
}
