using System.Linq;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// A base class for all game modules, manages the root object of all modules
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.GameModule)]
    public abstract class GameModule : ScriptableObject
    {
        public const string GameModulesPath = "GameModules/";
        protected const string CreateMenuPath = "Nexora/Game Modules/";

        // Root transform which is the highest parent of all game modules
        private static Transform _modulesRoot;

        protected static Transform ModulesRoot
        {
            get
            {
                if (_modulesRoot == null)
                {
                    var root = new GameObject("GameModules")
                    {
                        // tag = Tags.ModulesRoot
                    };

                    _modulesRoot = root.transform;
                    DontDestroyOnLoad(root);
                }

                return _modulesRoot;
            }
        }

        /// <summary>
        /// Removes root on reload to be able to reinitialize.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reload()
        {
            if (_modulesRoot != null)
            {
                DestroyImmediate(_modulesRoot.gameObject);
                _modulesRoot = null;
            }
        }

        protected static Transform CreateChildUnderModulesRoot(string childName)
        {
            var child = new GameObject(childName).transform;
            child.parent = ModulesRoot;
            return child;
        }
    }

    /// <summary>
    /// Base generic singleton game module class that handles load/creation logic and provides hooks for extension
    /// </summary>
    /// <remarks>
    /// <b>Note that in derived modules, <see cref="LoadOrCreateInstance"/> is called 
    /// depending on the preprocessor command (EDITOR or RUNTIME) this is to make initialization more robust 
    /// in the Editor as just after domain reload(<see cref="RuntimeInitializeLoadType.AfterAssembliesLoaded"/>) 
    /// it is the cleaniest state to run just like <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/> in the runtime.</b>
    /// </remarks>
    /// <typeparam name="T">Self singleton type.</typeparam>
    [DefaultExecutionOrder(ExecutionOrder.GameModule)]
    public abstract class GameModule<T> : 
        GameModule 
        where T : GameModule<T>
    {
        public static T Instance { get; private set; }

        /// <summary>
        /// Called when an instance is loaded or created
        /// </summary>
        protected virtual void OnInitialized() { }

        protected static void LoadOrCreateInstance()
        {
            if (Instance == null)
            {
                Instance = LoadInstance() ?? CreateInstance<T>();
            }

            Instance.OnInitialized();
        }

        private static T LoadInstance()
        {
            string path = GameModulesPath + typeof(T).Name;
            var instance = Resources.Load<T>(path);

            return instance ?? Resources.LoadAll<T>(path).FirstOrDefault();
        }

        protected static void CreateInstance()
        {
            if(Instance == null)
            {
                Instance = CreateInstance<T>();
            }

            Instance.OnInitialized();
        }

    }
}
