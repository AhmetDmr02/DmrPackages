using System;
using UnityEngine;

namespace DmrDependencyInjector
{
    public abstract class DIRegisterableObject : IDisposable
    {
        protected bool _isDisposed = false;

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            DisposeInternal(); //Must be called 

            DIInjectorManager.Unregister(this);
        }

        protected abstract void DisposeInternal();
    }
}
