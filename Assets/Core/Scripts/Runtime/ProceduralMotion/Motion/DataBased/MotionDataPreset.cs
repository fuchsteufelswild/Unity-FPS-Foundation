using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nexora.Motion
{
    /// <summary>
    /// Interface for motion data preset. 
    /// Depending on the state type and motion type it returns the appropriate <see cref="IMotionData"/>.
    /// </summary>
    public interface IMotionDataPreset
    {
        /// <summary>
        /// Returns a motion data for the given pair of state type and motion data type.
        /// </summary>
        /// <typeparam name="T">Enum type of the state type.</typeparam>
        /// <param name="state">State type enum.</param>
        /// <param name="motionDataType">Type of the motion data.</param>
        /// <returns>Motion data, if found, else null.</returns>
        public IMotionData GetMotionData<T>(T state, Type motionDataType) where T : Enum;

        /// <summary>
        /// Type of the <see cref="Enum"/> for the system this preset is used in.
        /// </summary>
        public Type SystemStateType { get; }
    }

    /// <summary>
    /// Abstract class to define <see cref="IMotionDataPreset"/>. It keeps cache for <see cref="IMotionData"/>,
    /// the key for the cache is <typeparamref name="StateType"/> and <see cref="Type"/>(motion data type).
    /// That is, for each state type and motion data type, there can be only one motion data.
    /// </summary>
    /// <remarks>
    /// For each state, it has a list of motion data available. <b>Each state can have only one <see cref="IMotionData"/>
    /// of the same type.</b>
    /// <br></br> It might have fallback preset, which is used when no <see cref="IMotionData"/> is found for the given
    /// state type and motion data type.
    /// </remarks>
    /// <typeparam name="StateType">The state type enum of the using system. 
    /// <b>It must have a value of None = 0</b>
    /// </typeparam>
    /* Example derived class from a MotionDataPreset
     [CreateAssetMenu(menuName = AssetMenuPath + nameof(UIMotionDataPreset), fileName = "Motion")]
     public class UIMotionDataPreset :
     MotionDataPreset<UIState>
     {
     }
    */
    public abstract class MotionDataPreset<StateType> :
        ScriptableObject,
        IMotionDataPreset
        where StateType : Enum
    {
        protected const string AssetMenuPath = "Nexora/Motion/Motion Presets/";

        [Tooltip("Fallback preset. If a certain type of data is not found in preset, fallback preset will be used.")]
        [SerializeField, SerializeReference]
        private MotionDataPreset<StateType> _fallbackPreset;

        [ReorderableList]
        [SerializeField, LabelByChild("State")]
        private StateMotionData[] _stateMotionData = Array.Empty<StateMotionData>();

        private Dictionary<MotionDataKey, IMotionData> _motionDataCache;

        public Type SystemStateType => typeof(StateType);

        /// <summary>
        /// Returns the appropriate motion for the given state type and motion data type.
        /// First checks if it exists in the cache, if not then checks for the None type 
        /// which acts as a fallback on the state type.
        /// If it is not found still, returns if it is found in the fallback preset.
        /// </summary>
        /// <typeparam name="T"><inheritdoc cref="IMotionDataPreset.GetMotionData{T}(T, Type)" path="/typeparam"/></typeparam>
        /// <param name="state"><inheritdoc cref="IMotionDataPreset.GetMotionData{T}(T, Type)" path="/param[@name='state']"/></param>
        /// <param name="motionDataType"><inheritdoc cref="IMotionDataPreset.GetMotionData{T}(T, Type)" path="/param[@name='motionDataType']"/></param>
        /// <returns><inheritdoc cref="IMotionDataPreset.GetMotionData{T}(T, Type)" path="/returns"/></returns>
        public IMotionData GetMotionData<T>(T state, Type motionDataType) where T : Enum
        {
            Assert.IsTrue(typeof(T) == typeof(StateType),
                "The state type for the requesting class and the preset must match!" +
                string.Format("Incompatible {0}-{1}", typeof(T).Name, typeof(StateType).Name));


            _motionDataCache ??= BuildMotionDataCache();

            StateType typedState = (StateType)(object)state;
            var key = new MotionDataKey(typedState, motionDataType);
            if (_motionDataCache.TryGetValue(key, out IMotionData motionData))
            {
                return motionData;
            }

            key = new MotionDataKey(EnumNoneValidator<StateType>.None, motionDataType);
            if (_motionDataCache.TryGetValue(key, out motionData))
            {
                return motionData;
            }

            return _fallbackPreset != null 
                ? _fallbackPreset.GetMotionData(state, motionDataType) 
                : null;
        }

        /// <summary>
        /// Builds a cache for the motion data. It creates keys using state type 
        /// and motion data type. It reads all <see cref="StateMotionData"/> set in the inspector,
        /// and creates matches for <see cref="StateMotionData.State"/>-type of the motion -> <see cref="IMotionData"/>.
        /// </summary>
        private Dictionary<MotionDataKey, IMotionData> BuildMotionDataCache()
        {
            var cache = new Dictionary<MotionDataKey, IMotionData>();

            foreach (StateMotionData data in _stateMotionData)
            {
                foreach (IMotionData motion in data.MotionData)
                {
                    if (motion == null)
                    {
                        Debug.LogError("Motion is null for the state");
                        continue;
                    }

                    var key = new MotionDataKey(data.State, motion.GetType());
                    if (cache.TryAdd(key, motion) == false)
                    {
                        Debug.LogError("Duplicate data is not allowed. State-MotionType pair can only have " +
                            "one motion data");
                    }
                }
            }

            return cache;
        }

        /// <summary>
        /// Class to store, list of <see cref="IMotionData"/> assigned
        /// to a specific <typeparamref name="StateType"/>.
        /// </summary>
        [Serializable]
        private sealed class StateMotionData
        {
            public StateType State;

            [SerializeReference, SpaceArea]
            [ReorderableList(ElementLabel = "Motion")]
            [ReferencePicker(typeof(IMotionData), TypeGrouping.ByFlatName)]
            public IMotionData[] MotionData = Array.Empty<IMotionData>();
        }

        /// <summary>
        /// Dictionary key that is combining both <typeparamref name="StateType"/> and
        /// <see cref="_motionDataType"/>. It is used as a motion data is determined by both
        /// current <typeparamref name="StateType"/> and the type of the motion data.
        /// In the constructor, <see cref="Enum"/> is converted to <see langword="int"/>
        /// so to avoid repetitive casts in <see cref="GetHashCode"/> and <see cref="Equals(object)"/> calls.
        /// </summary>
        private readonly struct MotionDataKey
        {
            private readonly int _stateType;
            private readonly Type _motionDataType;

            public MotionDataKey(StateType stateType, Type motionType)
            {
                _stateType = Convert.ToInt32(stateType);
                _motionDataType = motionType;
            }

            public override int GetHashCode()
                => HashCode.Combine(_stateType, _motionDataType.GetHashCode());

            public override bool Equals(object obj)
            {
                return obj is MotionDataKey other
                    && _stateType == other._stateType
                    && _motionDataType == other._motionDataType;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditorUtils.SafeOnValidate(this, () =>
            {
                // Fallback preset should not be same as this class,
                // the same way this class should not be the fallback preset
                // of the fallback preset of this class.
                // Or it will be infinite loop of references.
                // Sets fallback preset to null if loop is found.
                if (_fallbackPreset != null)
                {
                    if (_fallbackPreset == this || _fallbackPreset._fallbackPreset == this)
                    {
                        EditorUtility.SetDirty(this);
                        _fallbackPreset = null;
                    }
                }

                // Rebuilld cache if reload occurs when editor plays.
                if (Application.isPlaying)
                {
                    _motionDataCache = BuildMotionDataCache();
                }
            });
        }
#endif
    }

    /// <summary>
    /// Used to get None field of an <see cref="Enum"/> of type <typeparamref name="T"/>.
    /// In the data motion system, all state types must have a field called None with value 0.
    /// This is like a fallback state for all different kind of systems using motion.
    /// This class automatically checks if it is existent, throws if the field does not exist.
    /// </summary>
    /// <typeparam name="T">Enum to check.</typeparam>
    /// <exception cref="InvalidOperationException">
    /// </exception>
    internal static class EnumNoneValidator<T>
        where T : Enum
    {
        private static readonly T _none = (T)Enum.Parse(typeof(T), "None");
        public static T None => _none;

        /// <summary>
        /// Exception thrown if the types don't match. Safe.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        static EnumNoneValidator()
        {
            if (Enum.IsDefined(typeof(T), None) == false
            || Convert.ToInt32(None) != 0)
            {
                throw new InvalidOperationException($"Enum {typeof(T).Name} doesn't have a value for None with value 0");
            }
        }
    }
}