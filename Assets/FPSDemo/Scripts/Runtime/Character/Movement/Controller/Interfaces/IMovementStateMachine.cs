using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    public interface IMovementStateMachine
    {
        /// <summary>
        /// Currently active movement state's type.
        /// </summary>
        MovementStateType ActiveStateType { get; }

        /// <summary>
        /// Gets 
        /// </summary>
        /// <typeparam name="T">Movement state type.</typeparam>
        /// <returns>State of type <typeparamref name="T"/> registered in this state machine.</returns>
        T GetStateOfType<T>() where T : class, ICharacterMovementState;

        /// <summary>
        /// Sets the current state to <paramref name="state"/>.
        /// </summary>
        /// <returns>If state successfuly set.</returns>
        bool TrySetState(ICharacterMovementState state);

        /// <summary>
        /// Sets the current state to the state registered for the <paramref name="stateType"/> 
        /// in this state machine.
        /// </summary>
        /// <param name="stateType">Target state type.</param>
        /// <returns>If state successfuly set.</returns>
        bool TrySetState(MovementStateType stateType);
    }
}