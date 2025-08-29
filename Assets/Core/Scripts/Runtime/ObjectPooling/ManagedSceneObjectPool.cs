using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nexora.ObjectPooling
{
    /// <summary>
    /// Wrapper around <see cref="UnityEngine.Pool.ObjectPool{T}"/> for managing scene objects of type <typeparamref name="T"/>. 
    /// It registers itself to the <see cref="ScenePoolController"/>.
    /// </summary>
    /// <remarks>
    /// It is using <see cref="PooledSceneObject"/> as a component
    /// to interact with the object pooling system, and creates a mechanism to get type <typeparamref name="T"/> from the pool
    /// without requiring it to derive from <see cref="PooledSceneObject"/>.
    /// </remarks>
    /// <typeparam name="T">Type of the component the pool returns when acquired.</typeparam>
    public sealed class ManagedSceneObjectPool<T> : 
        IObjectPool<T>
        where T : Component
    {
        private readonly UnityEngine.Pool.ObjectPool<PooledSceneObject> _pool;
        private readonly ScenePoolController _scenePoolController;
        private readonly T _objectPrefab;

        public int FreeObjectCount => _pool.CountInactive;
        public int MaximumCapacity => _pool.CountAll;

        public ManagedSceneObjectPool(
            T objectPrefab, 
            Scene scene, 
            PoolCategory category, 
            int initialSize, 
            int maxSize, 
            float releaseDelay = 0f, 
            Action<T> onPostProcessPrefab = null)
        {
            if(ScenePoolController.TryGetControllerInScene(scene, out _scenePoolController) == false)
            {
                throw new Exception("Scene does not contain a valid ScenePoolController");
            }

            _objectPrefab = objectPrefab;

            Transform poolCategoryRoot = _scenePoolController.AddPool(this, category);
            var objectPoolProxy = new ObjectPoolProxy(this);
            PooledSceneObject prefabCopy = CreatedPooledInstance(poolCategoryRoot, objectPoolProxy, 
                                                                 onPostProcessPrefab, releaseDelay);

            _pool = new UnityEngine.Pool.ObjectPool<PooledSceneObject>(
                createFunc: () =>
                {
                    var instance = UnityEngine.Object.Instantiate(prefabCopy, poolCategoryRoot);
                    instance.SetParentPool(objectPoolProxy, releaseDelay);
                    return instance;
                },
                actionOnGet: pooledObject =>
                {
                    pooledObject.OnPreAcquired();
                    pooledObject.OnAcquired();
                },
                actionOnRelease: pooledObject =>
                {
                    pooledObject.gameObject.SetActive(false);
                    pooledObject.OnReleased();
                },
                actionOnDestroy: pooledObject =>
                {
                    if(pooledObject != null)
                    {
                        UnityEngine.Object.Destroy(pooledObject.gameObject);
                    }
                },
                collectionCheck: false,
                defaultCapacity: initialSize,
                maxSize: maxSize
            );
        }

        private PooledSceneObject CreatedPooledInstance(
            Transform poolCategoryRoot, 
            ObjectPoolProxy objectPool,
            Action<T> onPostProcessPrefab,
            float releaseDelay)
        {
            var instance = UnityEngine.Object.Instantiate(_objectPrefab, poolCategoryRoot);
            instance.gameObject.SetActive(false);
            onPostProcessPrefab?.Invoke(instance);

            var pooledObjectPrefab = instance.gameObject.GetOrAddComponent<PooledSceneObject>();
            pooledObjectPrefab.SetParentPool(objectPool, releaseDelay);
            return pooledObjectPrefab;
        }

        public T Acquire() => _pool.Get().GetComponent<T>();
        
        public void Release(T instance)
        {
            if(instance.TryGetComponent<PooledSceneObject>(out PooledSceneObject pooledObject))
            {
                pooledObject.Release();
            }
            else
            {
                // May log warning
            }
        }

        public void Warmup(int count)
        {
            for(int i = 0; i < count; i++)
            {
                var pooledObject = _pool.Get();
                _pool.Release(pooledObject);
            }
        }

        public void Clear() => _pool.Clear();

        public void Dispose()
        {
            _pool.Dispose();
            _scenePoolController.RemovePool(this);
            ObjectPoolingModule.Instance.UnregisterPool(this);
        }

        /// <summary>
        /// Wrapper around <see cref="ManagedSceneObjectPool{T}"/> as we want to pool object as <see cref="PooledSceneObject"/>
        /// and just retrieve the elements as <typeparamref name="T"/>.
        /// <br></br>
        /// <b><typeparamref name="T"/> does not have to derive from <see cref="PooledSceneObject"/>, just have it as component.</b>
        /// </summary>
        private sealed class ObjectPoolProxy : IUnityPool<PooledSceneObject>
        {
            private readonly ManagedSceneObjectPool<T> _parent;

            public int FreeObjectCount => _parent._pool.CountInactive;

            public int MaximumCapacity => _parent._pool.CountAll;

            public ObjectPoolProxy(ManagedSceneObjectPool<T> parent)
                => _parent = parent;

            public PooledSceneObject AcquireAndPut(
                Vector3 position,
                Quaternion rotation,
                Transform parent = null)
            { 
                return this.GetInWorldSpace(position, rotation, parent);
            }

            public PooledSceneObject Acquire() => _parent._pool.Get();
            public void Clear() => _parent._pool.Clear();
            public void Dispose() => _parent._pool.Dispose();
            public void Release(PooledSceneObject instance) => _parent._pool.Release(instance);
            public void Warmup(int count) => _parent.Warmup(count);
        }
    }
}
