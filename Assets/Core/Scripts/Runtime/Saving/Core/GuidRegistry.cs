using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.Serialization
{
    /// <summary>
    /// Global registry that manages adding/removing objects to registry through their <see cref="Guid"/>.
    /// </summary>
    /// <remarks>
    /// Via this registry we ensure that the GUIDs of <see cref="StableGuidComponent"/> don't clash with 
    /// each other.
    /// </remarks>
    public sealed class GuidRegistry
    {
        private static readonly GuidRegistry _instance = new();
        private readonly Dictionary<Guid, StableGuidComponent> _guidToComponent = new();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() => _instance._guidToComponent.Clear();
#endif

        /// <summary>
        /// <inheritdoc cref="InternalAdd" path="/summary"/>
        /// </summary>
        /// <inheritdoc cref="InternalAdd" path="/returns"/>
        public static bool Add(StableGuidComponent guid) => _instance.InternalAdd(guid);

        /// <summary>
        /// Tries to add the <paramref name="guidComponent"/> to the registry.
        /// If there are no <see cref="Guid"/> collisions, replaces/adds it.
        /// Else it asserts in the Runtime and Logs warning in the Editor as their cases are different.
        /// </summary>
        /// <returns>There were no <see cref="Guid"/> collision.</returns>
        private bool InternalAdd(StableGuidComponent guidComponent)
        {
            Guid guid = guidComponent.StableGuid.Guid;

            if(_guidToComponent.TryAdd(guid, guidComponent))
            {
                // Guid does not already exist
                return true;
            }

            var existingComponent = _guidToComponent[guid];
            if (existingComponent == guidComponent)
            {
                return true;
            }

            if(existingComponent == null)
            {
                _guidToComponent[guid] = guidComponent;
                return true;
            }

            OnDuplicateGuidDetected();

            return false;

            void OnDuplicateGuidDetected()
            {
                if (Application.isPlaying)
                {
                    // A serious problem in the play mode as duplicate Guid means that one of the objects is corrupted
                    // and you won't get what you want
                    Debug.AssertFormat(false, guidComponent, "Duplicate Guid detected between {0} and {1}",
                        _guidToComponent[guid] != null ? _guidToComponent[guid].name : "NULL",
                        guidComponent != null ? guidComponent.name : "NULL");
                }
                else
                {
                    // In Editor, copying an object will result in a duplicate Guid which will be repaired.
                    // This is just a warning so that you can know what copying that object results in.
                    Debug.LogWarningFormat(guidComponent, "Duplicate Guid detected while creating {0}",
                        guidComponent != null ? guidComponent.name : "NULL");
                }
            }
        }

        public static bool Remove(StableGuidComponent guidComponent) => Remove(guidComponent.StableGuid.Guid);
        public static bool Remove(Guid guid) => _instance.InternalRemove(guid);

        private bool InternalRemove(Guid guid) => _guidToComponent.Remove(guid);
    }
}
