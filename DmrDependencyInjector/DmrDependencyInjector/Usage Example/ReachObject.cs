using DmrDependencyInjector;
using System;
using UnityEngine;

public class ReachObject : MonoBehaviour
{
    [DmrInject][SerializeField] private TestRegisterable _testRegisterable;

    private void Start()
    {
        DIInjectorManager.InjectClassDependencies(this, out var injectResult);
    }

    private void Update()
    {
        _testRegisterable.Reach();
    }
}
