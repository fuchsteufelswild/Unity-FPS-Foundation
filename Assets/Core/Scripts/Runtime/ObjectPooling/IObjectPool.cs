using System;
using UnityEngine;

namespace Nexora.ObjectPooling
{
    /// <summary>
    /// Base interface for object pools. Manages the size, warmup, and cleaning of the pool.
    /// </summary>
    public interface IObjectPoolBase : IDisposable
    {
        int FreeObjectCount { get; }

        int MaximumCapacity { get; }

        /// <summary>
        /// Warms up the pool with populating it with minimum number of objects by acquiring and returning objects directly.
        /// </summary>
        /// <param name="count">Number of objects to acquire then return.</param>
        void Warmup(int count);

        /// <summary>
        /// Releasing and disposing all objects in the pool.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Generic object pool interface manages acquiring/returning objects.
    /// </summary>
    /// <typeparam name="T">Object type.</typeparam>
    public interface IObjectPool<T> : 
        IObjectPoolBase 
        where T : class
    {
        /// <summary>
        /// Acquires a free object from pool.
        /// </summary>
        T Acquire();

        /// <summary>
        /// Returns object back to the pool for future use.
        /// </summary>
        void Release(T instance);
    }

    /// <summary>
    /// Object pool specific for Unity objects.
    /// </summary>
    public interface IUnityPool<T>
        : IObjectPool<T>
        where T : UnityEngine.Object
    {
        /// <summary>
        /// Acquires a free object from pool and puts them onto designated location and under parent.
        /// </summary>
        T AcquireAndPut(Vector3 position, Quaternion rotation, Transform parent = null);
    }
}