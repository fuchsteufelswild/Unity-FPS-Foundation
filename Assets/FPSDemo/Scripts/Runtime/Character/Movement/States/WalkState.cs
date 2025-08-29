using Nexora.FPSDemo.Options;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    [Serializable]
    public class WalkState : GroundedMovementState
    {
        public override MovementStateType StateType => MovementStateType.Walk;

        public override bool CanTransitionTo() 
            => CharacterMotor.IsGrounded && CharacterMotor.CanSetHeight(CharacterMotor.DefaultHeight);

        public override void OnEnter(MovementStateType previousStateType) 
            => CharacterMotor.SetHeight(CharacterMotor.DefaultHeight);

        public override void UpdateLogic()
        {
            if (ShouldTransitionToIdle() && MovementController.TrySetState(MovementStateType.Idle)) return;

            if (ShouldTransitionToRun() && MovementController.TrySetState(MovementStateType.Run)) return;

            if (ShouldTransitionToCrouch() && MovementController.TrySetState(MovementStateType.Crouch)) return;

            if (ShouldTransitionToProne() && MovementController.TrySetState(MovementStateType.Prone)) return;

            if (MovementController.TrySetState(MovementStateType.Airborne)) return;

            if (ShouldTransitionToJump() && MovementController.TrySetState(MovementStateType.Jump)) return;
        }

        private bool ShouldTransitionToIdle()
        {
            return MovementInput.RawMovementDirection.sqrMagnitude < 0.1f
                && CharacterMotor.SimulatedVelocity.sqrMagnitude < 0.01f;
        }

        private bool ShouldTransitionToRun() => MovementInput.IsRunningHeld || InputOptions.Instance.AutoRunToggleMode;
        private bool ShouldTransitionToCrouch() => MovementInput.IsCrouchingHeld;
        private bool ShouldTransitionToProne() => MovementInput.IsProningHeld;
        private bool ShouldTransitionToJump() => MovementInput.IsJumpPressed;
    }
}