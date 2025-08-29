using System;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    [Serializable]
    public class IdleState : CharacterMovementState
    {
        public override MovementStateType StateType => MovementStateType.Idle;
        public override float StepCycleLength => 0f;
        public override bool ApplyGravity => false;
        public override bool SnapToGround => true;

        public override bool CanTransitionTo()
        {
            return CharacterMotor.Velocity.Horizontal().sqrMagnitude < VelocityThreshold
                && CharacterMotor.CanSetHeight(CharacterMotor.DefaultHeight);
        }

        public override void OnEnter(MovementStateType previousStateType)
        {
            MovementInput.ConsumeRunInput();
            CharacterMotor.SetHeight(CharacterMotor.DefaultHeight);
        }

        public override Vector3 UpdateVelocity(Vector3 currentVelocity, float deltaTime)
        {
            return Vector3.MoveTowards(currentVelocity, Vector3.zero, deltaTime);
        }

        public override void UpdateLogic()
        {
            if (ShouldTransitionToWalk() && MovementController.TrySetState(MovementStateType.Walk)) return;

            if (ShouldTransitionToCrouch() && MovementController.TrySetState(MovementStateType.Crouch)) return;

            if (ShouldTransitionToProne() && MovementController.TrySetState(MovementStateType.Prone)) return;

            if (ShouldTransitionToJump() && MovementController.TrySetState(MovementStateType.Jump)) return;

            if (MovementController.TrySetState(MovementStateType.Airborne)) return;
        }

        private bool ShouldTransitionToWalk()
        {
            bool hasMovementInput = MovementInput.RawMovementDirection.sqrMagnitude > InputThreshold;
            bool isMoving = CharacterMotor.Velocity.Horizontal().sqrMagnitude > VelocityThreshold;

            return hasMovementInput || isMoving;
        }

        private bool ShouldTransitionToJump() => MovementInput.IsJumpPressed;
        private bool ShouldTransitionToCrouch() => MovementInput.IsCrouchingHeld;
        private bool ShouldTransitionToProne() => MovementInput.IsProningHeld;
    }
}