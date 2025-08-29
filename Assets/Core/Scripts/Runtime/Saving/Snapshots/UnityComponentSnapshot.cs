using System;
using System.Text;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Generic snapshot of a <see cref="Component"/>.
    /// Includes the path to associated <see cref="GameObject"/>, component's <see cref="Type"/>, 
    /// and the associated data which is unique as defined for the component and controlled by
    /// <see cref="ISaveableComponent"/>.
    /// </summary>
    [Serializable]
    public struct UnityComponentSnapshot
    {
        /// <summary>
        /// Path to <see cref="GameObject"/> containing the <see cref="Component"/>.
        /// </summary>
        public readonly string GameObjectPath;

        public readonly Type ComponentType;

        public readonly object ComponentData;

        public UnityComponentSnapshot(string gameObjectPath,  Type componentType, object componentData)
        {
            GameObjectPath = gameObjectPath;
            ComponentType = componentType;
            ComponentData = componentData;
        }

        public static UnityComponentSnapshot[] ExtractSnapshots(Transform source, StringBuilder path)
        {
            var saveableComponents = source.GetComponentsInChildren<ISaveableComponent>();
            if(saveableComponents.Length == 0)
            {
                return Array.Empty<UnityComponentSnapshot>();
            }

            var snapshots = new UnityComponentSnapshot[saveableComponents.Length];
            for(int i = 0; i  < saveableComponents.Length; i++)
            {
                ISaveableComponent saveableComponent = saveableComponents[i];
                string componentPath = source.BuildRelativePathFrom(saveableComponent.transform, path);
                snapshots[i] = new UnityComponentSnapshot(componentPath, saveableComponent.GetType(), saveableComponent.Save());
            }

            return snapshots;
        }

        public static void ApplySnapshots(UnityComponentSnapshot[] snapshots, Transform target)
        {
            if(target == null || snapshots == null)
            {
                return;
            }

            string targetName = target.name;
            foreach (var snapshot in snapshots)
            {
                var componentTransform = snapshot.GameObjectPath != targetName
                    ? target.Find(snapshot.GameObjectPath)
                    : target;

                if(componentTransform != null)
                {
                    var component = (ISaveableComponent)componentTransform.gameObject.GetOrAddComponent(snapshot.ComponentType);
                    component.Load(snapshot);
                }
            }
        }
    }
}