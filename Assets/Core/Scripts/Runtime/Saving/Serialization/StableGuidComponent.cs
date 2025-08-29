using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nexora.Serialization
{
    /// <summary>
    /// Provides non-replicaple unique identifier to Unity <see cref="GameObject"/>.
    /// Can be used to refer any instance of an object no matter where it is.
    /// It can be used within many systems.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public abstract class StableGuidComponent : 
        MonoBehaviour,
        ISerializationCallbackReceiver
    {
        [SerializeField]
        [UnityGuidUIOptions(disableForPrefabs: true, hasNewGuidButton: false)]
        private UnityGuid _stableGuid = new(Guid.NewGuid());

        public UnityGuid StableGuid
        {
            get => _stableGuid;
            protected set => _stableGuid = value;
        }

        /// <summary>
        /// Creation.
        /// </summary>
        protected virtual void Awake() => UpdateGuid();
        /// <summary>
        /// After deserialize validation.
        /// </summary>
        protected virtual void OnValidate() => UpdateGuid();

        /// <summary>
        /// If it is a prefab instance or asset make <see cref="Guid"/> not valid.
        /// Else create a new <see cref="Guid"/> or restore if already have one.
        /// </summary>
        private void UpdateGuid()
        {
#if UNITY_EDITOR
            if (UnityEditorUtils.IsAssetOnDisk(this))
            {
                _stableGuid.Clear();
            }
            else
#endif
            {
                CreateOrRestoreGuid();
            }
        }

        /// <summary>
        /// When after component deserialized(<see cref="OnValidate"/>) or creating(<see cref="Awake"/>) this component. 
        /// If the <see cref="Guid"/> is valid we are trying restore it agressively trying to add it to <see cref="GuidRegistry"/>
        /// (Note that <see cref="Guid"/> might be changed in this action).
        /// If it is invalid we are creating a brand new <see cref="Guid"/>.
        /// </summary>
        private void CreateOrRestoreGuid()
        {
            if (_stableGuid.IsValid() == false)
            {
                _stableGuid = new UnityGuid(Guid.NewGuid());

#if UNITY_EDITOR
                if (Application.isPlaying == false)
                {
                    if (PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                    }

                }
#endif
                return;
            }

            AddToRegistry();

            return;

            void AddToRegistry()
            {
                // Continuously trying to add the new Guid until it wouldn't result in a duplicate
                while (GuidRegistry.Add(this) == false)
                {
                    _stableGuid = new UnityGuid(Guid.NewGuid());
                }
            }
        }

        protected virtual void OnDestroy() => RemoveFromRegistry();

        public void ClearGuid()
        {
            RemoveFromRegistry();
            _stableGuid.Clear();
        }

        private void RemoveFromRegistry()
        {
            if (_stableGuid.IsValid())
            {
                GuidRegistry.Remove(this);
            }
        }

        /// <summary>
        /// Prevents prefab instances and assets from having a valid Guid.
        /// </summary>
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // Without this check, destroyed objects might throw exceptions during serialization
            // e.g when closing a scene
            if(this == null)
            {
                return;
            }

            // If the object is a prefab instance or an asset, then we clear its Guid.
            // As prefabs should not contain a Guid or it would be duplicated when instanced.
            if(UnityEditorUtils.IsAssetOnDisk(this))
            {
                _stableGuid.Clear();
            }
#endif
        }

        public void OnAfterDeserialize() { }
    }
}