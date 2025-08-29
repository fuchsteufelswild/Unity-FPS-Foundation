using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Class that handles blocking transition <see cref="ICharacterMovementState"/> through <see cref="Object"/>,
    /// controls enabled/disabled.
    /// </summary>
    /// <remarks>
    /// <see cref="Object"/> should not do anything here, it is just a simple object or placeholder stating that
    /// it is kind of a blocker blocks transition onto the state.
    /// </remarks>
    public sealed class MovementStateBlocker : IMovementStateBlocker
    {
        private readonly Dictionary<MovementStateType, List<Object>> _stateBlockers = new();
        private readonly Dictionary<MovementStateType, ICharacterMovementState> _states;

        public MovementStateBlocker(Dictionary<MovementStateType, ICharacterMovementState> states)
        {
            _states = states;
        }

        public bool IsStateBlocked(MovementStateType stateType)
        {
            return _stateBlockers.ContainsKey(stateType) 
                && _stateBlockers[stateType]?.IsEmpty() == false;
        }

        public void AddStateBlocker(Object blocker, MovementStateType stateType)
        {
            if(_stateBlockers.ContainsKey(stateType) == false)
            {
                _stateBlockers.Add(stateType, new List<Object>());
            }

            _stateBlockers[stateType].Add(blocker);

            if(_states.ContainsKey(stateType))
            {
                _states[stateType].IsEnabled = false;
            }
        }

        public void RemoveStateBlocker(Object blocker, MovementStateType stateType)
        {
            if(_stateBlockers.ContainsKey(stateType))
            {
                _stateBlockers[stateType].Remove(blocker);

                if (_stateBlockers[stateType].IsEmpty() && _states.ContainsKey(stateType))
                {
                    _states[stateType].IsEnabled = true;
                    _stateBlockers.Remove(stateType);
                }
            }
        }
    }
}