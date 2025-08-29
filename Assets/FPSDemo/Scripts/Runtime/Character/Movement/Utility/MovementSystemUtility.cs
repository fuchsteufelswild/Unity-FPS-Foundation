using System.Diagnostics.Eventing.Reader;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    public static class MovementSystemUtility
    {
        public static readonly MovementStateType[] MovementStateTypes = EnumUtility.GetAllEnumValues<MovementStateType>();
        public static readonly int MovementStateCount = MovementStateTypes.Length;
    }

    public static class MovementControllerExtensions
    {
        public static bool IsInState<T>(this IMovementStateMachine movementStateMachine)
            where T : class, ICharacterMovementState
        {
            var state = movementStateMachine.GetStateOfType<T>();
            return state != null && movementStateMachine.ActiveStateType == state.StateType;
        }

        public static void TemporarilyBlockState(
            this IMovementStateBlocker stateBlocker, 
            Object blocker, 
            MovementStateType stateType, 
            float duration, 
            MonoBehaviour coroutineRunner)
        {
            stateBlocker.AddStateBlocker(blocker, stateType);
            coroutineRunner.InvokeDelayed(() => stateBlocker.RemoveStateBlocker(blocker, stateType), duration);
        }
    }
}