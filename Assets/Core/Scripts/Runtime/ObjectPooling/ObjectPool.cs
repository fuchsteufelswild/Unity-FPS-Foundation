using System;
using System.Collections.Concurrent;
using System.Reflection;
using UnityEngine;

namespace Nexora.ObjectPooling
{
    /// <summary>
    /// Generic wrapper around Unity's <see cref="UnityEngine.Pool.ObjectPool{T}"/> with additional functionality.
    /// <b>NOT thread-safe, use only from Unity's main thread.</b>
    /// </summary>
    public sealed class ObjectPool<T> : 
        IObjectPool<T>
        where T : class
    {
        private readonly UnityEngine.Pool.ObjectPool<T> _pool;

        private int _maxSize;

        public ObjectPool(
            Func<T> createFunc, 
            Action<T> actionOnGet, 
            Action<T> actionOnRelease, 
            Action<T> actionOnDestroy, 
            int initialSize, 
            int maxSize)
        {
            if(createFunc == null)
            {
                throw new ArgumentNullException(nameof(createFunc));
            }

            if(maxSize <= 0)
            {
                throw new ArgumentException(nameof(maxSize) + " must be greater than zero");
            }

            _maxSize = maxSize;
            _pool = new UnityEngine.Pool.ObjectPool<T>(
                    createFunc : createFunc,
                    actionOnGet : actionOnGet,
                    actionOnRelease : actionOnRelease,
                    actionOnDestroy : instance =>
                    {
                        ObjectDestroyWrapperFactory.GetCachedDestroyer<T>().Destroy(instance, actionOnDestroy);
                    },
                    collectionCheck : false,
                    defaultCapacity : initialSize,
                    maxSize : maxSize
                );

            Warmup(initialSize);
        }

        public int FreeObjectCount => _pool.CountInactive;

        public int MaximumCapacity => _maxSize;

        public T Acquire() => _pool.Get();

        public void Clear() => _pool.Clear();

        public void Release(T instance) => _pool.Release(instance);

        public void Warmup(int count)
        {
            for(int i = 0; i < count; i++)
            {
                var instance = _pool.Get();
                _pool.Release(instance);
            }
        }

        public void Dispose() => _pool.Clear();
    }

    #region
    public static class PoolExtensions
    {
        public static T GetInWorldSpace<T>(
            this IObjectPool<T> pool, 
            Vector3 position, 
            Quaternion rotation, 
            Transform parent = null)
            where T : UnityEngine.Object
        {
            T instance = pool.Acquire();

            var instanceTransform = GetTransform(instance);

            // Not a valid unity object that has transform, erroneous action. Silently return
            if(instanceTransform == null)
            {
                // May log warning
                return instance;
            }

            if(parent != null)
            {
                instanceTransform.SetParent(parent);
            }

            instanceTransform.SetPositionAndRotation(position, rotation);
            return instance;

            Transform GetTransform(T instance)
            {
                return instance switch
                {
                    GameObject go => go.transform,
                    Component comp => comp.transform,
                    _ => null
                };
            }
        }
    }
    #endregion

    #region Object Destroyer
    /// <summary>
    /// Generic wrapper to safely destroy an object in case of extra cleanup needed. 
    /// Like for regular classes we don't need to do anything, but for Unity classes we need to destroy the component.
    /// Created through <see cref="ObjectDestroyWrapperFactory"/>.
    /// </summary>
    public abstract class ObjectDestroyWrapper<TObj>
        where TObj : class
    {
        public void Destroy(TObj obj, Action<TObj> actionOnDestroy = null)
        {
            if (obj == null)
            {
                return;
            }

            if (actionOnDestroy != null)
            {
                actionOnDestroy.Invoke(obj);
            }
            else
            {
                DestroyAction(obj);
            }
        }

        protected abstract void DestroyAction(TObj obj);
    }

    public class NoOpDestroyer<TObj> :
        ObjectDestroyWrapper<TObj>
        where TObj : class
    {
        protected override void DestroyAction(TObj obj) { }
    }

    public class UnityObjectDestroyer<TObj> :
        ObjectDestroyWrapper<TObj>
        where TObj : UnityEngine.Object
    {
        protected override void DestroyAction(TObj obj)
            => UnityEngine.Object.Destroy(obj);
    }

    /// <summary>
    /// Factory that creates and caches respective destroyer given the generic Type
    /// </summary>
    public static class ObjectDestroyWrapperFactory
    {
        private static readonly ConcurrentDictionary<Type, object> _destroyerCache = new();

        /// <summary>
        /// Use <see cref="GetCachedDestroyer(Type)"/> for most cases. 
        /// Use this method if you specifically don't want to use cached destroyers.
        /// </summary>
        public static object GetDestroyerWithoutCache(Type type)
        {
            MethodInfo genericMethod = typeof(ObjectDestroyWrapperFactory).GetMethod(
                nameof(GetCachedDestroyer),
                BindingFlags.Public | BindingFlags.Static,
                null,
                Type.EmptyTypes,
                null);

            MethodInfo concreteMethod = genericMethod.MakeGenericMethod(type);

            return concreteMethod.Invoke(null, null);
        }

        public static object GetCachedDestroyer(Type type)
        {
            return _destroyerCache.GetOrAdd(type,
                _ => CreateDestroyer(type));
        }

        private static object CreateDestroyer(Type type)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                var unityDestroyerType = typeof(UnityObjectDestroyer<>).MakeGenericType(type);
                return Activator.CreateInstance(unityDestroyerType);
            }

            var noopDestroyerType = typeof(NoOpDestroyer<>).MakeGenericType(type);
            return Activator.CreateInstance(noopDestroyerType);
        }

        public static ObjectDestroyWrapper<TObj> GetCachedDestroyer<TObj>() 
            where TObj : class
        {
            return (_destroyerCache.GetOrAdd(typeof(TObj),
                _ => CreateDestroyer<TObj>())) as ObjectDestroyWrapper<TObj>;
        }

        private static ObjectDestroyWrapper<TObj> CreateDestroyer<TObj>()
            where TObj : class
        {
            if(typeof(UnityEngine.Object).IsAssignableFrom(typeof(TObj)))
            {
                var unityDestroyerType = typeof(UnityObjectDestroyer<>).MakeGenericType(typeof(TObj));
                return (ObjectDestroyWrapper<TObj>)Activator.CreateInstance(unityDestroyerType);
            }

            return new NoOpDestroyer<TObj>();
        }
    }
    #endregion
}

