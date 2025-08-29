using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Nexora.ObjectPooling
{
    /// <summary>
    /// Each scene has a dedicated <see cref="ScenePoolController"/> which categorizes pools into groups
    /// and tracks the lifetimes of <see cref="IPooledObject{T}"/> using <see cref="PooledObjectLifetimeTracker"/> and <see cref="PoolCategoryOrganizer"/>.
    /// </summary>
    public class ScenePoolController : MonoBehaviour
    {
        private readonly PooledObjectLifetimeTracker _timedPooledObjectTracker = new();
        private PoolCategoryOrganizer _categoryOrganizer;

        private static readonly Dictionary<Scene, ScenePoolController> _controllers = new();
        private readonly List<IObjectPoolBase> _pools = new();

        private bool _isActive;

        public event UnityAction OnDestroyed;

        private void Awake()
        {
            _controllers.Add(gameObject.scene, this);
            _categoryOrganizer = new PoolCategoryOrganizer(transform);
            _isActive = true;
        }

        private void OnDestroy()
        {
            _controllers.Remove(gameObject.scene);
            _isActive = false;

            foreach(var pool in _pools)
            {
                pool.Dispose();
            }

            OnDestroyed?.Invoke();
        }

        private void Update() => _timedPooledObjectTracker.Update();
        
        /// <summary>
        /// Gets a <see cref="ScenePoolController"/> in the <paramref name="scene"/>, 
        /// if <paramref name="scene"/> status is valid.
        /// <br></br>
        /// If it is not created yet, creates a new <see cref="ScenePoolController"/>.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static bool TryGetControllerInScene(Scene scene, out ScenePoolController controller)
        {
            if(UnityUtils.IsQuitting)
            {
                controller = null;
                return false;
            }

            if(_controllers.TryGetValue(scene, out controller))
            {
                return true;
            }

            if(scene.isLoaded == false)
            {
                return false;
            }

            var parent = new GameObject("[Scene Pools]");
            if(parent.scene != scene)
            {
                SceneManager.MoveGameObjectToScene(parent, scene);
            }

            controller = parent.AddComponent<ScenePoolController>();
            return true;
        }

        public Transform AddPool(IObjectPoolBase pool)
            => AddPool(pool, PoolCategory.None);

        public Transform AddPool(IObjectPoolBase pool, PoolCategory poolCategory)
        {
            if(_isActive == false)
            {
                return null;
            }

            _pools.Add(pool);
            return _categoryOrganizer.GetCategoryRoot(poolCategory);
        }

        public void RemovePool(IObjectPoolBase pool)
        {
            if(_isActive)
            {
                _pools.Remove(pool);
            }
        }

        public void TrackPooledObjectLifetime(IPooledObjectBase pooledObject, float releaseDelay)
            => _timedPooledObjectTracker.Add(pooledObject, releaseDelay);

        public void UntrackPooledObjectLifetime(IPooledObjectBase pooledObject)
            => _timedPooledObjectTracker.Remove(pooledObject);

        private class PoolCategoryOrganizer
        {
            private readonly Transform _defaultRoot;
            private readonly Dictionary<PoolCategory, Transform> _categories = new();

            public PoolCategoryOrganizer(Transform defaultRoot)
                => _defaultRoot = defaultRoot;

            public Transform GetCategoryRoot(PoolCategory category)
            {
                if(category == PoolCategory.None)
                {
                    return null;
                }

                if(_categories.TryGetValue(category, out var categoryRoot) == false)
                {
                    categoryRoot = new GameObject(category.ToString()).transform;
                    categoryRoot.SetParent(_defaultRoot);
                    _categories.Add(category, categoryRoot);
                }

                return categoryRoot;
            }
        }

        /// <summary>
        /// Manages delayed release of <see cref="IPooledObject{T}"/> registered earlier via <see cref="TrackPooledObjectLifetime(IPooledObjectBase, float)"/>
        /// </summary>
        private class PooledObjectLifetimeTracker
        {
            private readonly List<PooledObjectLifetimeData> _lifetimeData = new();

            public void Add(IPooledObjectBase pooledObject, float releaseDelay)
            {
                float releaseTime = Time.time + releaseDelay;
                int index = _lifetimeData.FindIndex(data => data.PooledSceneObject == pooledObject);

                var objectLifetime = new PooledObjectLifetimeData(pooledObject, releaseTime);

                if(index != CollectionConstants.NotFound)
                {
                    _lifetimeData[index] = objectLifetime;
                }
                else
                {
                    _lifetimeData.Add(objectLifetime);
                }
            }

            public void Remove(IPooledObjectBase pooledObject)
            {
                int index = _lifetimeData.FindIndex(data => data.PooledSceneObject == pooledObject);
                if(index != CollectionConstants.NotFound)
                {
                    _lifetimeData.RemoveAt(index);
                }
            }

            public void Update()
            {
                float currentTime = Time.time;

                for (int i = _lifetimeData.Count - 1; i >= 0; i--)
                {
                    var data = _lifetimeData[i];
                    if(data.PooledSceneObject == null || data.ReleaseTime < currentTime)
                    {
                        _lifetimeData.RemoveAt(i);
                        data.PooledSceneObject?.ReleaseOrDestroy();
                    }
                }
            }
        }

        private readonly struct PooledObjectLifetimeData
        {
            public readonly IPooledObjectBase PooledSceneObject;
            public readonly float ReleaseTime;

            public PooledObjectLifetimeData(IPooledObjectBase pooledSceneObject, float releaseTime)
            {
                PooledSceneObject = pooledSceneObject;
                ReleaseTime = releaseTime;
            }
        }
    }
}