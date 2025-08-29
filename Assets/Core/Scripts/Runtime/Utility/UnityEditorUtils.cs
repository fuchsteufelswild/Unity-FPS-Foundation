using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Nexora
{
    public static class UnityEditorUtils
    {
#if UNITY_EDITOR
        /// <summary>
        /// Unity's OnValidate function may lead to problems or spurious warning messages in the cases below:
        /// <list type="number">
        ///     <item>Modifying serialized fields in the method.</item>
        ///     <item>Destroyed object errors when during operations like Enter/Exit Play mode, Save Prefabs, Reload Assemblies etc.</item>
        ///     <item>If the method change values when a scene loads, Unity marks the scene as dirty unnecessarily.</item>
        ///     <item>When multiple scripts use OnValidate, Unity executes them in non-deterministic order which may lead to race conditions.</item>
        /// </list>
        /// This method solves the problem stated above by waiting for all inspectors and makes the necessary checks.
        /// </summary>
        public static void SafeOnValidate(Object obj, UnityAction validateAction)
        {
            // Waiting for all inspector updates to finish leads to 
            // Preventing re-validation loops as stated in the 1st problem
            // Queues actions sequentially solving the 4th problem
            EditorApplication.delayCall += Validate;
            return;

            void Validate()
            {
                EditorApplication.delayCall -= Validate;

                // We are checking if the object is null to prevent the 2nd problem
                // And checking is dirty to prevent the 3rd problem
                if (obj == null || EditorUtility.IsDirty(obj) == false)
                {
                    return;
                }

                validateAction?.Invoke();
            }
        }
#endif

        /// <returns>
        /// If <paramref name="obj"/> is an asset?
        /// In builds, all are considered instances.
        /// </returns>
        public static bool IsAsset(this UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.Contains(obj);
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Returns if the given asset is prefab/part of a prefab, or being edited in prefab mode.
        /// </summary>
        public static bool IsAssetOnDisk(Component asset)
            => PrefabUtility.IsPartOfPrefabAsset(asset) || IsEditingInPrefabMode(asset);

        private static bool IsEditingInPrefabMode(Component asset)
        {
            // Invalid
            if (asset == null || asset.gameObject == null)
            {
                return false;
            }

            if (IsVariantOrNestedPrefab(asset))
            {
                return true;
            }

            return IsBeingEditedInPrefabWindow(asset);


            bool IsVariantOrNestedPrefab(Component asset)
            {
                return EditorUtility.IsPersistent(asset);
            }

            // Checks to see if the asset is being edited in the prefab mode window
            bool IsBeingEditedInPrefabWindow(Component asset)
            {
                StageHandle mainStage = StageUtility.GetMainStageHandle();
                StageHandle currentStage = StageUtility.GetCurrentStageHandle();
                if (currentStage != mainStage)
                {
                    PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(asset.gameObject);
                    if (prefabStage != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
#endif
    }
}