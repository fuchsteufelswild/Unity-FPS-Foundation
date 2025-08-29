using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.Motion
{
    /// <summary>
    /// Abstract base class for a broadcaster, main responsibility is broadcasting to <see cref="IMotionDataReceiver"/>
    /// that for a motion data type a specific <see cref="IMotionData"/> should be set. Has two criteria, first it checks
    /// override by type map which takes the precedence. e.g It can be set due to a specific <typeparamref name="StateType"/>
    /// may want to override specific motion type. If the override is not found then it sets the data from the first preset
    /// that contains the motion data type(latest added preset takes precedence).
    /// <br></br> Presets can be added/removed from the stack and <see cref="IMotionDataReceiver"/> can register/unregister.
    /// </summary>
    /// <remarks>
    /// Type of a <see cref="IMotionData"/> and the current <typeparamref name="StateType"/> are the main effectors to determine
    /// what data should be chosen from the preset.
    /// <br></br>It updates all receivers whenever a new data override is set, '<b><see cref="_currentState"/></b>' is changed, 
    /// or a new preset added/removed.
    /// <br></br> Derive from this class with concrete Enum the have your own broadcaster.
    /// </remarks>
    /// /// <typeparam name="StateType">
    /// <see cref="Enum"/> to determine the state of the main object to determine
    /// the motion. Like if it is a Player, it can be in state of (Moving, Jumping, etc.).
    /// And if it is UI (Transitioning, Sliding, Fading etc.). Using these states, we retrieve
    /// appropriate <see cref="IMotionData"/> to use.
    /// </typeparam>
    public abstract class MotionDataBroadcaster<StateType> : 
        MonoBehaviour,
        IMotionDataBroadcaster
        where StateType : Enum
    {
        [Tooltip("Base preset that will be always active as the first added preset in the stack.")]
        [SerializeField]
        private MotionDataPreset<StateType> _basePreset;

        private readonly Dictionary<Type, HashSet<IMotionDataReceiver>> _receiversByMotionDataType = new();

        // Each type might have its own data override, if set it takes precedence over presets.
        // Can be used for cases when you completely want to override some motion type, 
        // like when using first person aim, you might want to some motions when changing state
        // that you would like to see when not aiming
        private readonly Dictionary<Type, IMotionData> _motionDataOverrides = new();
        private readonly PresetStack _presetStack = new();

        private StateType _currentState;

        private void Start() => AddPreset<StateType>(_basePreset);

        /// <summary>
        /// <inheritdoc/>
        /// Updates all receivers as new added preset may result in a new motion data.
        /// </summary>
        public void AddPreset<T>(IMotionDataPreset preset)
            where T : Enum
        {
            if(preset == null)
            {
                return;
            }

            AssertStateType<T>(preset);

            if(_presetStack.TryAdd(preset))
            {
                UpdateReceivers();
            }
        }

        /// <summary>
        /// Asserts that preset's state type must be same with broadcaster's state type,
        /// and also typed T should be same as the type. This two factor check done so that
        /// it won't be forgotten, as it is a crucial part of the system.
        /// </summary>
        private void AssertStateType<T>(IMotionDataPreset preset)
            where T : Enum
        {
            Assert.IsTrue((preset == null || preset.SystemStateType == typeof(T))
                && typeof(T) == typeof(StateType), 
                "Preset and the broadcaster should have the same Enum" +
                " for defining the state type");
        }

        /// <summary>
        /// <inheritdoc/>
        /// Updates all receivers as removed preset may result in a different motion data.
        /// </summary>
        public void RemovePreset<T>(IMotionDataPreset preset)
            where T : Enum
        {
            AssertStateType<T>(preset);

            if (_presetStack.Remove(preset))
            {
                UpdateReceivers();
            }
        }

        /// <summary>
        /// Updates all receivers with the <see cref="_currentState"/> and 
        /// the corresponding <see cref="IMotionData"/> retrieved from the dictionary.
        /// </summary>
        private void UpdateReceivers()
        {
            foreach (var (motionDataType, _) in _receiversByMotionDataType)
            {
                IMotionData motionData = GetMotionData(motionDataType, _currentState);
                Broadcast(motionDataType, motionData);
            }
        }

        /// <summary>
        /// Gets a motion first checking if override already exists, this takes the furthest precedence.
        /// <br></br>If it is not found -> <inheritdoc cref="PresetStack.GetMotionData(StateType, Type)" path="/summary"/>
        /// </summary>
        /// <returns>Motion data if found, null otherwise.</returns>
        private IMotionData GetMotionData(Type motionDataType, StateType stateType)
        {
            if (_motionDataOverrides.TryGetValue(motionDataType, out var motionData))
            {
                return motionData;
            }

            return _presetStack.GetMotionData(stateType, motionDataType);
        }

        /// <summary>
        /// Broadcast <paramref name="motionData"/> to all compatible receivers, ones that has 
        /// <see cref="IMotionDataReceiver.MotionDataType"/> of type <paramref name="motionDataType"/>.
        /// </summary>
        private void Broadcast(Type motionDataType, IMotionData motionData)
        {
            if (_receiversByMotionDataType.TryGetValue(motionDataType, out var motionDataReceivers))
            {
                foreach (var receiver in motionDataReceivers)
                {
                    receiver.SetMotionData(motionData);
                }
            }
        }

        /// <summary>
        /// Adds data receiver to the appropriate HashSet depending on its <see cref="IMotionDataReceiver.MotionDataType"/>.
        /// </summary>
        public void AddDataReceiver(IMotionDataReceiver receiver)
        {
            if(receiver == null)
            {
                return;
            }

            if(_receiversByMotionDataType.TryGetValue(receiver.MotionDataType, out var list))
            {
                list.Add(receiver);
            }
            else
            {
                _receiversByMotionDataType.Add(receiver.MotionDataType,
                    new HashSet<IMotionDataReceiver> { receiver });
            }
        }

        public void RemoveDataReceiver(IMotionDataReceiver receiver)
        {
            if(receiver == null)
            {
                return;
            }

            if(_receiversByMotionDataType.TryGetValue(receiver.MotionDataType, out var motionDataReceivers))
            {
                motionDataReceivers.Remove(receiver);
            }
        }

        /// <summary>
        /// Sets a new data override for the motion data type of <typeparamref name="T"/>,
        /// removes the override if passed value <paramref name="motionDataOverride"/> is <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">Motion data type.</typeparam>
        /// <param name="motionDataOverride">New override.</param>
        public void SetDataOverride<T>(T motionDataOverride) 
            where T : IMotionData
        {
            if(motionDataOverride == null)
            {
                _motionDataOverrides.Remove(typeof(T));
            }
            else
            {
                _motionDataOverrides[typeof(T)] = motionDataOverride;
            }

            Broadcast(typeof(T), motionDataOverride);
        }

        /// <summary>
        /// Changes the current state. Updates all receivers.
        /// </summary>
        public void SetCurrentState<T>(T state) 
            where T : Enum
        {
            if (state is StateType specificState)
            {
                _currentState = specificState;
                UpdateReceivers();
            }
            else
            {
                Debug.LogError("Passed enum type and broadcaster state does not match," +
                    " use only compatible types");
            }
        }


        /// <summary>
        /// Stack for managing operations Add/Remove/Get for presets.
        /// </summary>
        private sealed class PresetStack
        {
            private readonly List<IMotionDataPreset> _stack = new();

            /// <returns>If preset successfully added.</returns>
            public bool TryAdd(IMotionDataPreset preset)
            {
                if (preset == null)
                {
                    return false;
                }

                _stack.Add(preset);
                return true;
            }

            /// <returns>If preset successfully removed.</returns>
            public bool Remove(IMotionDataPreset preset) => _stack.Remove(preset);

            /// <summary>
            /// Starting from latest added preset to the first added, checks if there exists
            /// a motion data for the given state type and motion type pair.
            /// </summary>
            /// <returns>Motion data if found, null otherwise.</returns>
            public IMotionData GetMotionData(StateType stateType, Type motionType)
            {
                // Check latest added preset first
                foreach (IMotionDataPreset preset in _stack.AsReverseEnumerator())
                {
                    IMotionData data = preset.GetMotionData(stateType, motionType);
                    if (data != null)
                    {
                        return data;
                    }
                }

                return null;
            }
        }
    }

    /* Example derived class to use
    public class PlayerMotionDataBroadcaster :
        MotionDataBroadcaster<MyEnum>
    {

    }
    */
}