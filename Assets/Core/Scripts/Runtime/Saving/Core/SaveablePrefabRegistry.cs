using Nexora.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Registry to keep track of all prefabs that has <see cref="SaveableGameObject"/> component on them.
    /// It automatically rebuilds registry in case of initialization, and if there is a corruption/mismatch of files.
    /// Used to retrive prefabs to instantiate object before loading the save data, when loading operation occurs.
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.GameModule)]
    [CreateAssetMenu(menuName = "Nexora/Registries/" + "Saveable Prefab Registry",
                     fileName = nameof(SaveablePrefabRegistry))]
    public class SaveablePrefabRegistry :
        GameModule<SaveablePrefabRegistry>,
        IEditorValidatable
    {
        [SerializeField, ScrollableItems(0, 15), Disable]
        private SaveableGameObject[] _registeredPrefabs = Array.Empty<SaveableGameObject>();

        private Dictionary<UnityGuid, SaveableGameObject> _prefabLookupTable;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize() => LoadOrCreateInstance();

        protected override void OnInitialized()
        {
            if (_prefabLookupTable == null || _prefabLookupTable.Count != _registeredPrefabs.Length)
            {
                _prefabLookupTable = CreatePrefabLookupTable();
            }
        }

        /// <summary>
        /// Creates the lookup table for prefabs. In Editor mode, calls <see cref="RebuildRegistry"/> in the case of broken prefabs.
        /// This can happen in the following cases:
        /// <list type="number">
        ///     <item>Prefab is missing, that is, moved/deleted outside Unity.</item>
        ///     <item>.meta file become desynchronized.</item>
        ///     <item>Serialized reference become <see langword="null"/>.</item>
        /// </list>
        /// </summary>
        /// <remarks><see cref="RebuildRegistry"/> calls <see cref="CreatePrefabLookupTable"/> method again to see if any miss occurs when the method runs.</remarks>
        private Dictionary<UnityGuid, SaveableGameObject> CreatePrefabLookupTable()
        {
            var prefabLookupTable = new Dictionary<UnityGuid, SaveableGameObject>();
            foreach (var registeredPrefab in _registeredPrefabs)
            {
#if UNITY_EDITOR
                if (registeredPrefab == null)
                {
                    RebuildRegistry();
                    break;
                }
#endif

                prefabLookupTable.Add(registeredPrefab.PrefabGuid, registeredPrefab);
            }

            return prefabLookupTable;
        }

#if UNITY_EDITOR
        private void RebuildRegistry()
        {
            SetRegisteredPrefabs(FindAllSaveableGameObjectPrefabs());
            _prefabLookupTable = CreatePrefabLookupTable();
        }

        public void RunValidation()
            => SetRegisteredPrefabs(FindAllSaveableGameObjectPrefabs());

        /// <summary>
        /// Finds and returns all prefabs that has <see cref="SaveableGameObject"/> component on them.
        /// Fixes <see cref="SaveableGameObject.PrefabGuid"/> of the component if there is mismatch between 
        /// prefab's guid and the component's guid.
        /// </summary>
        public static SaveableGameObject[] FindAllSaveableGameObjectPrefabs()
        {
            var saveablePrefabs = new List<SaveableGameObject>();
            var allPrefabs = UnityEditor.AssetDatabase.FindAssets("t:prefab");

            foreach (string prefabGuid in allPrefabs)
            {
                FixAndAddPrefabIfSaveable(prefabGuid);
            }

            return saveablePrefabs.ToArray();

            void FixAndAddPrefabIfSaveable(string prefabGuid)
            {
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    UnityEditor.AssetDatabase.GUIDToAssetPath(prefabGuid));
                if (prefab.TryGetComponent<SaveableGameObject>(out var saveablePrefab))
                {
                    FixPrefabGuid(saveablePrefab, prefabGuid);
                    saveablePrefabs.Add(saveablePrefab);
                }
            }

            void FixPrefabGuid(SaveableGameObject saveablePrefab, string prefabGuid)
            {
                var guid = new Guid(prefabGuid);
                if (saveablePrefab.PrefabGuid != guid)
                {
                    UnityEditor.EditorUtility.SetDirty(saveablePrefab);
                    saveablePrefab.PrefabGuid = guid;
                }
            }
        }

        public void SetRegisteredPrefabs(SaveableGameObject[] registeredPrefabs)
        {
            _registeredPrefabs = registeredPrefabs;
            _prefabLookupTable = CreatePrefabLookupTable();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        /// <summary>
        /// Tries to get prefab with the given guid.
        /// </summary>
        public bool TryGetPrefab(UnityGuid prefabGuid, out SaveableGameObject saveablePrefab)
            => _prefabLookupTable.TryGetValue(prefabGuid, out saveablePrefab);
    }
}