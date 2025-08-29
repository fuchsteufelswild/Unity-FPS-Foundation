using Nexora.Serialization;
using System;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Generic save data for <see cref="GameObject"/> including transforms, components, and rigidbodies associated with this object.
    /// </summary>
    [Serializable]
    public class GameObjectSaveData
    {
        /// <summary>
        /// Id for the object's prefab.
        /// </summary>
        [SerializeField]
        public UnityGuid PrefabGuid;

        /// <summary>
        /// Id for the object instance.
        /// </summary>
        [SerializeField]
        public UnityGuid InstanceGuid;

        [SerializeField]
        public LocalTransformSnapshot LocalTransformSnapshot;

        /// <summary>
        /// Snapshots of all the components attached to the object.
        /// </summary>
        [SerializeField]
        public UnityComponentSnapshot[] ComponentSnapshots;

        // Children snapshots
        [SerializeField]
        public LocalTransformSnapshot[] ChildrenLocalTransformSnapshots;

        [SerializeField]
        public RigidbodySnapshot[] RigidbodySnapshots;

    }
}