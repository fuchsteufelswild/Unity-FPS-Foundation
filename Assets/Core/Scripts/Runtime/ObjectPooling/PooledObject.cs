using System;
using UnityEngine;

namespace Nexora.ObjectPooling
{
    /// <summary>
    /// Core class that defines common functionality for classes. 
    /// Mainly needed to avoid code duplication between Unity and native classes.
    /// Simply, we cannot derive from a common abstract class as Unity objects also need to derive from <see cref="MonoBehaviour"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class PooledObjectCore<T>
        where T : class
    {
        private bool _isInUse;

        // The pool, the object resides in
        private IObjectPool<T> _parentPool;
        // Listeners that listen for this object's events of Acquire/Release/Destroy
        private IObjectPoolListener[] _listeners;

        public bool IsInUse => _isInUse;

        public PooledObjectCore(IObjectPoolListener[] listeners)
        {
            _listeners = listeners ?? Array.Empty<IObjectPoolListener>();
        }

        public void SetParentPool(IObjectPool<T> pool, float newReleaseDelay, ref float currentReleaseDelay)
        {
            if (_parentPool != null)
            {
                throw new InvalidOperationException(string.Format("{0} already is in another pool", ToString()));
            }

            _parentPool = pool ?? throw new NullReferenceException("Target pool is null");
            currentReleaseDelay = newReleaseDelay > Mathf.Epsilon
                ? newReleaseDelay
                : currentReleaseDelay;
        }

        public void OnAcquired()
        {
            _isInUse = true;

            foreach (IObjectPoolListener listener in _listeners)
            {
                listener.OnAcquired();
            }
        }

        public bool Release(T pooledObject, float delay = 0f)
        {
            if (delay < Mathf.Epsilon)
            {
                ReleaseOrDestroy(pooledObject);
                return true;
            }

            return false;
        }

        public void ReleaseOrDestroy(T pooledObject)
        {
            if (_parentPool != null)
            {
                foreach (IObjectPoolListener listener in _listeners)
                {
                    listener.OnPreReleased();
                }

                _parentPool.Release(pooledObject);
            }
            else
            {
                Destroy(pooledObject);
            }

            return;

            void Destroy(T pooledObject)
            {
                var objectType = typeof(T);
                var destroyer = ObjectDestroyWrapperFactory.GetCachedDestroyer<T>();
                destroyer.Destroy(pooledObject);
            }
        }

        public void OnReleased()
        {
            _isInUse = false;
            foreach (IObjectPoolListener listener in _listeners)
            {
                listener.OnReleased();
            }
        }
    }

    /// <summary>
    /// Pooled regular C# class object that isn't tied to Unity <see cref="GameObject"/>
    /// </summary>
    public class PooledObject :
        IPooledObject<PooledObject>
    {
        private float _autoReleaseDelay;

        public float AutoReleaseDelay => _autoReleaseDelay;

        private PooledObjectCore<PooledObject> _pooledObjectCore;

        public PooledObject()
        {
            _pooledObjectCore = new PooledObjectCore<PooledObject>(null);
        }

        public void OnAcquired() 
            => _pooledObjectCore.OnAcquired();

        public void OnPreAcquired() { }

        public void OnReleased()
            => _pooledObjectCore.OnReleased();

        public void Release(float delay = 0)
        {
            if (_pooledObjectCore.Release(this, delay) == false)
            {
                
            }
        }

        public void ReleaseOrDestroy()
            => _pooledObjectCore.ReleaseOrDestroy(this);

        public void SetParentPool(IObjectPool<PooledObject> pool, float releaseDelay)
        {
            _pooledObjectCore.SetParentPool(pool, releaseDelay, ref _autoReleaseDelay);
        }
    }
}