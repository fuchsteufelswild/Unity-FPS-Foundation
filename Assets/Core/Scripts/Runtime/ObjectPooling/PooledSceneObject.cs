using System.Collections.Generic;
using System;
using UnityEngine;

namespace Nexora.ObjectPooling
{
    /// <summary>
    /// Represents pooled Unity scene object(<see cref="GameObject"/>). 
    /// Manages the lifetime of the object along with destruction notifiers, as well as notifying list of <see cref="IObjectPoolListener"/> on actions.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("ID: {GetInstanceID()} Name: {name}")]
    public sealed class PooledSceneObject :
        MonoBehaviour,
        IPooledObject<PooledSceneObject>
    {
        [SerializeField, Range(0, 1000f)]
        [Tooltip("Time for object to automatically return to the pool or self-destruct if it is not bound to any pool.")]
        private float _autoReleaseDelay;

        // Core object that implements common functionality for different class types
        private PooledObjectCore<PooledSceneObject> _pooledObjectCore;

        public float AutoReleaseDelay => _autoReleaseDelay;

        private void Awake()
        {
            _pooledObjectCore ??= new PooledObjectCore<PooledSceneObject>(
                GetObjectPoolListeners());
        }

        /// <summary>
        /// Gets listeners from the components that are on the object
        /// </summary>
        private IUnityObjectPoolListener[] GetObjectPoolListeners()
        {
            const int DefaultListenerSize = 128;

            var listeners = new List<IUnityObjectPoolListener>(DefaultListenerSize);
            GetComponents(listeners);
            return listeners.Count > 0 ? listeners.ToArray() : Array.Empty<IUnityObjectPoolListener>();
        }

        private void OnDestroy()
        {
            if (_pooledObjectCore.IsInUse == false)
            {
                return;
            }

            if(ScenePoolController.TryGetControllerInScene(gameObject.scene, out var scenePoolController))
            {
                scenePoolController.UntrackPooledObjectLifetime(this);
            }
        }

        public override string ToString()
        {
            return base.ToString() + gameObject.name;
        }

        public void SetParentPool(IObjectPool<PooledSceneObject> pool, float releaseDelay)
        {
            _pooledObjectCore ??= new PooledObjectCore<PooledSceneObject>(
                GetObjectPoolListeners());

            _pooledObjectCore.SetParentPool(pool, releaseDelay, ref _autoReleaseDelay);
        }

        public void OnPreAcquired()
        {
            gameObject.SetActive(true);
        }

        /// <inheritdoc/>
        public void OnAcquired()
        {
            _pooledObjectCore.OnAcquired();

            if (AutoReleaseDelay > Mathf.Epsilon)
            {
                if(ScenePoolController.TryGetControllerInScene(gameObject.scene, out var scenePoolController))
                {
                    scenePoolController.TrackPooledObjectLifetime(this, _autoReleaseDelay);
                }
            }
        }

        /// <inheritdoc/>
        public void ReleaseOrDestroy()
        {
            _pooledObjectCore.ReleaseOrDestroy(this);
        }

        /// <inheritdoc/>
        public void OnReleased()
        {
            _pooledObjectCore.OnReleased();
            // gameObject.SetActive(false);
        }

        /// <inheritdoc/>
        public void Release(float delay = 0)
        {
            if (_pooledObjectCore.Release(this, delay) == false)
            {
                if (ScenePoolController.TryGetControllerInScene(gameObject.scene, out var scenePoolController))
                {
                    scenePoolController.TrackPooledObjectLifetime(this, _autoReleaseDelay);
                }
            }
        }
    }
}