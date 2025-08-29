using Nexora.Serialization;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Manages saving/loading scene, registering <see cref="ISaveableComponent"/>.
    /// Every <see cref="UnityEngine.SceneManagement.Scene"/> has only one <see cref="SceneSaveController"/> associated with it.
    /// All saveable <see cref="GameObject"/> of the scene are registered to the designated controller.
    /// </summary>
    [SelectionBase, DisallowMultipleComponent]
    [DefaultExecutionOrder(ExecutionOrder.GameModule)]
    [AddComponentMenu(ComponentMenuPaths.SaveSystem + nameof(SceneSaveController))]
    public class SceneSaveController : MonoBehaviour
    {
        private const int ReservedMaximumSpaceForSaveableObjects = 256;
        private const int MaxPathLength = 256;

        [SerializeField, InLineEditor]
        private LevelMetaData _levelMetaData;

        [SerializeField, ReorderableList(HasLabels = false)]
        [Tooltip("These objects are saved before others.")]
        private SaveableGameObject[] _highPrioritySaveables;

        private static readonly Dictionary<Scene, SceneSaveController> _sceneSaveControllers = new();

        private readonly Dictionary<UnityGuid, SaveableGameObject> _saveableObjects = new();
        private readonly List<SaveableGameObject> _allSaveableObjects = new(ReservedMaximumSpaceForSaveableObjects);

        private StringBuilder _cachedPathBuilder = new StringBuilder(MaxPathLength);

        public Scene Scene => gameObject.scene;
        public LevelMetaData LevelMetaData => _levelMetaData;

        private void OnDisable() => _sceneSaveControllers.Remove(gameObject.scene);

        private void OnEnable()
        {
            _sceneSaveControllers.Add(gameObject.scene, this);
            foreach (var highPrioritySaveable in _highPrioritySaveables)
            {
                RegisterSaveable(highPrioritySaveable);
            }
        }

        /// <summary>
        /// Registers object to the save system.
        /// </summary>
        public void RegisterSaveable(SaveableGameObject saveable)
        {
            if(_saveableObjects.TryAdd(saveable.StableGuid, saveable))
            {
                _allSaveableObjects.Add(saveable);
            }
        }

        /// <summary>
        /// Unregisters object from the save system.
        /// </summary>
        public void UnregisterSaveable(SaveableGameObject saveable)
        {
            if(_saveableObjects.Remove(saveable.StableGuid) == false
            || _allSaveableObjects.Remove(saveable) == false)
            {
                // The objects is not registered before
            }
        }

        /// <summary>
        /// Captures states of the all <see cref="SaveableGameObject"/> in the scene.
        /// All instances use the same path builder. 
        /// If it's not feasible then <see cref="StringBuilder"/> should be constructed every time 
        /// this method is called.
        /// </summary>
        public SceneSaveData CaptureSceneState()
        {
            _cachedPathBuilder.Clear();

            var gameObjectSaveDataArray = new GameObjectSaveData[_allSaveableObjects.Count];

            for(int i = 0;  i < gameObjectSaveDataArray.Length; i++)
            {
                gameObjectSaveDataArray[i] = _allSaveableObjects[i].CaptureState(_cachedPathBuilder);
            }

            return new SceneSaveData(gameObject.scene.name, gameObjectSaveDataArray);
        }

        /// <summary>
        /// Restores the state of all <see cref="SaveableGameObject"/> in the scene.
        /// Firstly, restores all the high priority ones that are already exists in the scene.
        /// Other ones are created with the given save data.
        /// </summary>
        public void RestoreSceneState(SceneSaveData sceneSaveData)
        {
            Dictionary<UnityGuid, GameObjectSaveData> objectSaveLookupTable = FillLookupTable();

            RestoreExistingObjectsState();
            RestoreRemainingObjectState();

            return;

            Dictionary<UnityGuid, GameObjectSaveData> FillLookupTable()
            {
                var objectSaveLookupTable = new Dictionary<UnityGuid, GameObjectSaveData>(
                    sceneSaveData.GameObjectSaveDataArray.Length);
                foreach (var objectSaveData in sceneSaveData.GameObjectSaveDataArray)
                {
                    objectSaveLookupTable.Add(objectSaveData.InstanceGuid, objectSaveData);
                }

                return objectSaveLookupTable;
            }

            // Restore existing objects and delete if it is not existent
            void RestoreExistingObjectsState()
            {
                foreach (var currentSaveable in _allSaveableObjects)
                {
                    if (objectSaveLookupTable.TryGetValue(currentSaveable.StableGuid, out var saveData))
                    {
                        currentSaveable.RestoreState(saveData);
                        objectSaveLookupTable.Remove(currentSaveable.StableGuid);
                    }
                    else
                    {
                        Destroy(currentSaveable.gameObject);
                    }
                }
            }

            void RestoreRemainingObjectState()
            {
                foreach (var remainingSaveable in objectSaveLookupTable.Values)
                {
                    InstantiateSaveableGameObject(remainingSaveable);
                }
            }
        }

        private static void InstantiateSaveableGameObject(GameObjectSaveData saveData)
        {
            if(SaveablePrefabRegistry.Instance.TryGetPrefab(saveData.PrefabGuid, out var saveablePrefab))
            {
                var instance = Instantiate(saveablePrefab);
                instance.RestoreState(saveData);
            }
            else
            {
                // Prefab is not available in the registry
                // Save might be broken, outdated prefab
                // or prefab is deleted from Unity while it is included in the save
            }
        }

        public static SceneSaveController GetSceneSaveController(Scene scene)
            => _sceneSaveControllers[scene];

        public static bool TryGetSceneSaveController(Scene scene, out SceneSaveController sceneSaveController)
            => _sceneSaveControllers.TryGetValue(scene, out sceneSaveController);

        /// <summary>
        /// Might be used to collect context like difficulty, or any other specific settings attached to.
        /// </summary>
        public virtual Dictionary<string, object> CollectSceneContextData() => null;
    }
}