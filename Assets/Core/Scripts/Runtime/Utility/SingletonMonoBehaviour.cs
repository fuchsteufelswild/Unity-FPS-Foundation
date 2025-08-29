using UnityEngine.Assertions;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Singleton <see cref="MonoBehaviour"/> implementation without persistence between game scenes.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class.</typeparam>
    [DefaultExecutionOrder(ExecutionOrder.Singleton)]
    public abstract class SingletonMonoBehaviour<T> : 
        MonoBehaviour,
        IMonoBehaviour
        where T : SingletonMonoBehaviour<T>

    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                Assert.IsTrue(_instance != null);

                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if(_instance != this)
            {
                Destroy(this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}