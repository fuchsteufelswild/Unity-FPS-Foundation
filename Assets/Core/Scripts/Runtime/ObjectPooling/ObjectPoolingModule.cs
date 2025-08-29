using System;
using System.Collections.Generic;
using UnityEngine;


namespace Nexora.ObjectPooling
{
    /// <summary>
    /// Game module that provides access to all object pools available. 
    /// Acts as an object pool registry which also provides methods to acquire elements.
    /// <br></br><br></br><b>!IMPORTANT!</b> Pools are registered into two different <see cref="Dictionary{TKey, TValue}"/> both by <see cref="Type"/> and with their <see cref="HashCode"/>.
    /// There are methods for each access type for all actions such as Register/Get/Release.
    /// Check method definition for the best usage possible.
    /// There is a code duplication, but this way it is easier to grasp and use this class,
    /// with fewer functions it is hard to understand and use.
    /// </summary>
    public sealed class ObjectPoolingModule : GameModule<ObjectPoolingModule>
    {
        private readonly Dictionary<int, IObjectPoolBase> _poolsByTemplate = new();
        private readonly Dictionary<Type, IObjectPoolBase> _poolsByType = new();

        static ObjectPoolingModule()
        {
            // Explicit static constructor to:
            // 1. Control precise type initialization timing
            // 2. Prevent beforefieldinit optimization
            // 3. Reserve space for future static initialization
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init() => LoadOrCreateInstance();

        protected override void OnInitialized()
        {
            DisposePools();
        }

        /// <summary>
        /// First clears all pools available and then <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        private void DisposePools()
        {
            ClearAllPools();
            _poolsByTemplate.Clear();
            _poolsByType.Clear();
        }

        private void ClearAllPools()
        {
            foreach (var pool in _poolsByTemplate.Values)
            {
                pool.Clear();
            }
            foreach (var pool in _poolsByType.Values)
            {
                pool.Clear();
            }
        }

        public void RegisterPool<T>(IObjectPool<T> pool)
            where T : class
        {
            _poolsByType.TryAdd(pool.GetType(), pool);
        }

        public void RegisterPool<T>(T template, IObjectPool<T> pool)
            where T : class
        {
            _poolsByTemplate.TryAdd(template.GetHashCode(), pool);
        }

        public void UnregisterPool<T>(IObjectPool<T> pool)
            where T : class
        {
            _poolsByType.Remove(typeof(T));
        }

        public void UnregisterPool<T>(T template)
            where T : class
        {
            _poolsByTemplate.Remove(template.GetHashCode());
        }

        public bool HasPool<T>()
            where T : class
        {
            return _poolsByType.ContainsKey(typeof(T));
        }

        public bool HasPool<T>(T template)
        {
            return _poolsByTemplate.ContainsKey(template.GetHashCode());
        }

        public IObjectPool<T> GetPool<T>()
            where T : class
        {
            if(_poolsByType.TryGetValue(typeof(T), out var pool))
            {
                return (IObjectPool<T>)pool;
            }

            return null;
        }

        public IObjectPool<T> GetPool<T>(T template)
            where T : class
        {
            if(template == null)
            {
                return null;
            }

            if(_poolsByTemplate.TryGetValue(template.GetHashCode(), out var pool))
            {
                return (IObjectPool<T>)pool;
            }

            return null;    
        }

        public T GetElement<T>()
            where T : class
        {
            var pool = GetPool<T>();
            var instance = GetElementFromPool<T>(pool);
            return instance;
        }

        private T GetElementFromPool<T>(IObjectPool<T> pool)
            where T : class
        {
            if (pool == null)
            {
                throw new InvalidOperationException
                    (string.Format("No pool for the type {0} has been found in the registry.", typeof(T).Name));
            }

            return pool.Acquire();
        }

        public bool TryGetElement<T>(out T instance)
            where T : class
        {
            var pool = GetPool<T>();
            instance = GetElementFromPool<T>(pool);
            return instance != null;
        }

        public T GetElement<T>(T template)
            where T : class
        {
            if(template == null)
            {
                return null;
            }

            var pool = GetPool(template);
            var instance = GetElementFromPool<T>(pool);
            return instance;
        }

        public bool TryGetElement<T>(T template, out T instance)
            where T : class
        {
            if(template == null)
            {
                instance = null;
                return false;
            }

            var pool = GetPool(template);
            instance = GetElementFromPool<T>(pool);
            return instance != null;
        }

        public T Get<T>(Vector3 position, Quaternion rotation, Transform parent = null)
            where T : UnityEngine.Object
        {
            var pool = GetPool<T>();
            var instance = GetElementFromPool(pool, position, rotation, parent);
            return instance;
        }

        private T GetElementFromPool<T>(
            IObjectPool<T> pool, 
            Vector3 position, 
            Quaternion rotation, 
            Transform parent = null)
            where T : UnityEngine.Object
        {
            if (pool == null)
            {
                throw new InvalidOperationException(string.Format("No pool for the type {0} has been found in the registry.", typeof(T).Name));
            }

            return pool.GetInWorldSpace(position, rotation, parent);
        }

        public T Get<T>(
            T template, 
            Vector3 position, 
            Quaternion rotation, 
            Transform parent = null)
            where T : UnityEngine.Object
        {
            if(template == null)
            {
                return null;
            }

            var pool = GetPool<T>(template);
            var instance = GetElementFromPool(pool, position, rotation, parent);
            return instance;
        }

        public void Release<T>(T instance) 
            where T : class
        {
            var pool = GetPool<T>();
            pool?.Release(instance);
        }

        public void Release<T>(T template, T instance)
            where T : class
        {
            var pool = GetPool<T>(template);
            pool?.Release(instance);
        }

        public int GetFreeObjectCount<T>()
            where T : class
        {
            var pool = GetPool<T>();
            return pool != null ? pool.FreeObjectCount : 0;
        }

        public int GetFreeObjectCount<T>(T template)
            where T : class
        {
            var pool = GetPool<T>(template);
            return pool != null ? pool.FreeObjectCount : 0;
        }        
    }
}