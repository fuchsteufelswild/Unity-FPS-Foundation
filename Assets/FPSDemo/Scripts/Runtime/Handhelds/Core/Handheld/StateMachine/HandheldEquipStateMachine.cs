using Nexora.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public readonly struct HandheldEquipConfiguration
    {
        public readonly float EquipDuration;
        public readonly float HolsterDuration;
        public readonly AudioSequence EquipAudio;
        public readonly AudioSequence HolsterAudio;

        public HandheldEquipConfiguration(float equipDuration, float holsterDuration, AudioSequence equipAudio, AudioSequence holsterAudio)
        {
            EquipDuration = equipDuration;
            HolsterDuration = holsterDuration;
            EquipAudio = equipAudio;
            HolsterAudio = holsterAudio;
        }
    }

    [Serializable]
    public sealed class HandheldEquipStateMachine
    {
        [Tooltip("The duration of the equip animation.")]
        [SerializeField, Range(0f, 10f)]
        private float _equipDuration = 0.4f;

        [Tooltip("The duration of the holster animation.")]
        [SerializeField, Range(0f, 10f)]
        private float _holsterDuration = 0.4f;

        [BeginIndent]
        [Title("Equip Audio")]
        [Tooltip("The audio sequence played during equip animation.")]
        [SerializeField]
        private AudioSequence _equipAudio;

        [Title("Holster Audio")]
        [Tooltip("The audio sequence played during holster animation.")]
        [EndIndent]
        [SerializeField]
        private AudioSequence _holsterAudio;

        private HandheldEquipConfiguration _configuration;

        private Dictionary<HandheldEquipStateType, IHandheldEquipState> _states;
        private IHandheldEquipState _currentState;

        public HandheldEquipStateType CurrentStateType => _currentState.EquipStateType;
        public event HandheldEquipStateChangedDelegate StateChanged;

        public void Initialize()
        {
            _states = new Dictionary<HandheldEquipStateType, IHandheldEquipState>
            {
                { HandheldEquipStateType.Hidden, new HiddenState() },
                { HandheldEquipStateType.Equipping, new EquippingState() },
                { HandheldEquipStateType.Equipped, new EquippedState() },
                { HandheldEquipStateType.Holstering, new HolsteringState() },
            };

            _currentState = _states[HandheldEquipStateType.Hidden];
            CreateConfiguration();
        }

        public bool CanTransitionTo(HandheldEquipStateType targetState) => _currentState.CanTransitionTo(targetState);

        private void CreateConfiguration() 
            => _configuration = new HandheldEquipConfiguration(_equipDuration, _holsterDuration, _equipAudio, _holsterAudio);

        /// <summary>
        /// Transitions from current state to <paramref name="targetState"/> and makes calls through <paramref name="context"/>
        /// playing the animations/audios etc.
        /// </summary>
        /// <param name="targetState">Target state.</param>
        /// <param name="context">Handheld context to play visuals/audios.</param>
        /// <param name="transitionSpeed">Optional speed parameter to modify the speed of animation if applicable through state.</param>
        public IEnumerator TransitionTo(HandheldEquipStateType targetState, IHandheld context, float transitionSpeed = 1f)
        {
            if(_currentState.CanTransitionTo(targetState) == false)
            {
                yield break;
            }

#if UNITY_EDITOR
            CreateConfiguration();
#endif

            IHandheldEquipState newState = _states[targetState];

            yield return _currentState.Exit(context);
            _currentState = newState;
            yield return newState.Enter(context, _configuration, transitionSpeed);

            
            StateChanged?.Invoke(targetState);
        }
    }
}