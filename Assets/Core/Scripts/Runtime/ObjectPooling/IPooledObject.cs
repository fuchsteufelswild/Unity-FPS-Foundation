using UnityEngine;

namespace Nexora.ObjectPooling
{
    public interface IPooledObjectBase
    {
        /// <summary>
        /// Called before object acquired from the pool, preparing the object beforehand.
        /// </summary>
        void OnPreAcquired();

        /// <summary>
        /// Acquire object from pool.
        /// </summary>
        void OnAcquired();

        /// <summary>
        /// If bound to pool release, if not self-destruct.
        /// </summary>
        void ReleaseOrDestroy();

        /// <summary>
        /// Release object after given delay.
        /// </summary>
        void Release(float delay = 0f);

        void OnReleased();
    }

    /// <summary>
    /// Interface for pooled objects, manages the lifetime and acquiring/releasing object from/into the parent pool.
    /// Implement if you need to perform any action on the object upon these hooks.
    /// </summary>
    public interface IPooledObject<T> :
        IPooledObjectBase
        where T : class
    {
        /// <summary>
        /// Delay to automatically release the object.
        /// </summary>
        public float AutoReleaseDelay { get; }

        /// <summary>
        /// Sets pool for object to be pooled.
        /// </summary>
        void SetParentPool(IObjectPool<T> pool, float releaseDelay);
    }
}