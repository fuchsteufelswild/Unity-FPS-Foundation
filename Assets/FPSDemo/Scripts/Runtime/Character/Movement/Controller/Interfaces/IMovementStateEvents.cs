using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    public interface IMovementStateEvents
    {
        /// <summary>
        /// Adds listener callback of <paramref name="transitionCallback"/> to the cases entering/exiting from <paramref name="stateType"/>.
        /// </summary>
        /// <param name="stateType">State type to add listener to.</param>
        /// <param name="transitionCallback">Listener callback for the event.</param>
        /// <param name="transitionType">Which transition event to listen to.</param>
        void AddStateTransitionListener(MovementStateType stateType, UnityAction<MovementStateType> transitionCallback, MovementStateTransitionType transitionType = MovementStateTransitionType.Enter);

        /// <summary>
        /// Removes listener callback of <paramref name="transitionCallback"/> from the cases entering/exiting from <paramref name="stateType"/>.
        /// </summary>
        /// <param name="stateType">State type to add listener to.</param>
        /// <param name="transitionCallback">Listener callback for the event.</param>
        /// <param name="transitionType">Which transition event to listen to.</param>
        void RemoveStateTransitionListener(MovementStateType stateType, UnityAction<MovementStateType> transitionCallback, MovementStateTransitionType transitionType = MovementStateTransitionType.Enter);
    }
}