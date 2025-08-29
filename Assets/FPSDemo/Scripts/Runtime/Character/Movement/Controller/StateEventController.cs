using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Class that handles adding/removing events and triggering them when state transitions occur.
    /// </summary>
    public sealed class StateEventController : IMovementStateEvents
    {
        private readonly Dictionary<MovementStateType, UnityAction<MovementStateType>> _enterEvents = new();
        private readonly Dictionary<MovementStateType, UnityAction<MovementStateType>> _exitEvents = new();

        public void TriggerEnterEvent(MovementStateType stateType, MovementStateType currentState)
        {
            if(_enterEvents.ContainsKey(stateType))
            {
                _enterEvents[stateType]?.Invoke(currentState);
            }
        }

        public void TriggerExitEvent(MovementStateType stateType, MovementStateType currentState)
        {
            if (_exitEvents.ContainsKey(stateType))
            {
                _exitEvents[stateType]?.Invoke(currentState);
            }
        }

        public void AddStateTransitionListener(
            MovementStateType stateType, 
            UnityAction<MovementStateType> transitionCallback, 
            MovementStateTransitionType transitionType = MovementStateTransitionType.Enter)
        {
            switch (transitionType)
            {
                case MovementStateTransitionType.Enter:
                    AddToEventDictionary(_enterEvents, stateType, transitionCallback);
                    break;
                case MovementStateTransitionType.Exit:
                    AddToEventDictionary(_exitEvents, stateType, transitionCallback);
                    break;
                case MovementStateTransitionType.Both:
                    AddToEventDictionary(_enterEvents, stateType, transitionCallback);
                    AddToEventDictionary(_exitEvents, stateType, transitionCallback);
                    break;
            }
        }

        private void AddToEventDictionary(Dictionary<MovementStateType, UnityAction<MovementStateType>> dict,
            MovementStateType stateType, UnityAction<MovementStateType> transitionCallback)
        {
            if (dict.ContainsKey(stateType) == false)
            {
                dict[stateType] = transitionCallback;
            }
            else
            {
                dict[stateType] += transitionCallback;
            }
        }

        public void RemoveStateTransitionListener(
            MovementStateType stateType, 
            UnityAction<MovementStateType> transitionCallback, 
            MovementStateTransitionType transitionType = MovementStateTransitionType.Enter)
        {
            switch (transitionType)
            {
                case MovementStateTransitionType.Enter:
                    RemoveFromEventDictionary(_enterEvents, stateType, transitionCallback);
                    break;
                case MovementStateTransitionType.Exit:
                    RemoveFromEventDictionary(_exitEvents, stateType, transitionCallback);
                    break;
                case MovementStateTransitionType.Both:
                    RemoveFromEventDictionary(_enterEvents, stateType, transitionCallback);
                    RemoveFromEventDictionary(_exitEvents, stateType, transitionCallback);
                    break;
            }
        }

        private void RemoveFromEventDictionary(Dictionary<MovementStateType, UnityAction<MovementStateType>> dict, 
            MovementStateType stateType, UnityAction<MovementStateType> transitionCallback)
        {
            if(dict.ContainsKey(stateType) == false)
            {
                return;
            }

            dict[stateType] -= transitionCallback;
            if (dict[stateType] == null)
            {
                dict.Remove(stateType);
            }
        }
    }
}