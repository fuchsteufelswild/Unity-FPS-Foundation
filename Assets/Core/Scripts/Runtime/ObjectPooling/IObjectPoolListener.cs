namespace Nexora.ObjectPooling
{
    /// <summary>
    /// Notifies components when they are acquired from the pool and when returned back to it.
    /// </summary>
    public interface IObjectPoolListener
    {
        /// <summary>
        /// Called before the object is activated from the pool.
        /// </summary>
        void OnPreAcquired();

        /// <summary>
        /// Called when the object is activated from the pool.
        /// </summary>
        void OnAcquired();

        /// <summary>
        /// Called before the object is returned to the pool.
        /// </summary>
        void OnPreReleased();

        /// <summary>
        /// Called when the object is returned to the pool.
        /// </summary>
        void OnReleased();
    }

    public interface IUnityObjectPoolListener : 
        IObjectPoolListener,
        IMonoBehaviour
    {
    }
}