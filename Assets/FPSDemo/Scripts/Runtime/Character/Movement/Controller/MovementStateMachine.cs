using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Movement state machine that controls entering/exiting from states, and keeps track 
    /// of the current active state along with the registry of available states.
    /// </summary>
    public sealed class MovementStateMachine : IMovementStateMachine
    {
        private Dictionary<MovementStateType, ICharacterMovementState> _states;
        private MovementStateBlocker _stateBlockingSystem;
        private readonly StateEventController _stateEventController = new();

        private ICharacterMovementState _activeState;

        public MovementStateType ActiveStateType { get; private set; }
        public MovementStateBlocker StateBlockingSystem => _stateBlockingSystem;
        public StateEventController StateEventController => _stateEventController;

        /// <returns>Currently active state of the state machine.</returns>
        public ICharacterMovementState GetActiveState() => _activeState;

        public MovementStateMachine() { }

        public void Initialize(ICharacterMovementState[] initialStates)
        {
            _states = new Dictionary<MovementStateType, ICharacterMovementState>();

            foreach (var state in initialStates)
            {
                _states[state.StateType] = state;
            }

            _stateBlockingSystem = new MovementStateBlocker(_states);
        }

        public T GetStateOfType<T>()
            where T : class, ICharacterMovementState
        {
            foreach(var state in _states.Values)
            {
                if(state is T movementState)
                {
                    return movementState;
                }
            }

            return null;
        }

        public bool TrySetState(MovementStateType stateType)
        {
            if(_states.ContainsKey(stateType) == false)
            {
                return false;
            }

            return TrySetState(_states[stateType]);
        }

        public bool TrySetState(ICharacterMovementState newState)
        {
            if(newState == null || _states.ContainsValue(newState) == false)
            {
                return false;
            }

            if(_activeState != newState && newState.IsEnabled && newState.CanTransitionTo())
            {
                TransitionToState(newState);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Transitions to the <paramref name="newState"/>, exits current state
        /// and enters to the new state.
        /// </summary>
        private void TransitionToState(ICharacterMovementState newState)
        {
            if(_activeState != null)
            {
                _activeState.OnExit();
                _stateEventController.TriggerExitEvent(MovementStateType.None, ActiveStateType);
                _stateEventController.TriggerExitEvent(ActiveStateType, ActiveStateType);
            }

            _activeState = newState;
            newState.OnEnter(ActiveStateType);
            ActiveStateType = newState.StateType;

            _stateEventController.TriggerEnterEvent(MovementStateType.None, ActiveStateType);
            _stateEventController.TriggerEnterEvent(ActiveStateType, ActiveStateType);
        }

        /// <summary>
        /// Initializes the state machine with the state registered of the type <paramref name="stateType"/>.
        /// </summary>
        public void InitializeWithState(MovementStateType stateType)
        {
            if (_states.ContainsKey(stateType))
            {
                TransitionToState(_states[stateType]);
            }
        }
    }
}