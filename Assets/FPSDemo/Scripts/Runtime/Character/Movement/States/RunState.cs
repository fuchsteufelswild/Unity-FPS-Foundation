using Nexora.FPSDemo.Options;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    [Serializable]
    public class RunState : GroundedMovementState
    {
        [Tooltip("Minimum speed required for the movement to be considered running.")]
        [SerializeField]
        private float _minRunSpeed = 2.5f;

        public override MovementStateType StateType => MovementStateType.Run;

        public override bool CanTransitionTo()
        {
            Vector2 moveInput = MovementInput.RawMovementDirection;
            Vector3 horizontalVelocity = CharacterMotor.Velocity.Horizontal();

            return CanRunBasedOnInput(moveInput)
                && horizontalVelocity.magnitude > _minRunSpeed
                && CharacterMotor.CanSetHeight(CharacterMotor.DefaultHeight)
                && CharacterMotor.IsGrounded;
        }

        private bool CanRunBasedOnInput(Vector2 moveInput)
        {
            bool isMovingForward = moveInput.y > 0f;
            bool isNotMovingSidewaysOnly = Mathf.Abs(moveInput.x) <= 0.9f;
            bool hasMovementInput = moveInput.sqrMagnitude > 0.1f;

            return isMovingForward && isNotMovingSidewaysOnly && hasMovementInput;
        }

        public override void OnEnter(MovementStateType previousStateType)
        {
            CharacterMotor.SetHeight(CharacterMotor.DefaultHeight);
            MovementInput.ConsumeCrouchInput();
            MovementInput.ConsumeProneInput();
        }

        public override void UpdateLogic()
        {
            if(IsEnabled == false)
            {
                HandleDisabledStateTransitions();
            }
            else
            {
                HandleEnabledStateTransitions();
            }
        }

        /// <summary>
        /// Automatically transitions to these states in this order whenever running stops or blocked.
        /// </summary>
        private void HandleDisabledStateTransitions()
        {
            if (MovementController.TrySetState(MovementStateType.Walk)) return;

            if (MovementController.TrySetState(MovementStateType.Idle)) return;
        }

        private void HandleEnabledStateTransitions()
        {
            if (MovementController.TrySetState(MovementStateType.Airborne)) return;

            if (ShouldTransitionToWalk() && MovementController.TrySetState(MovementStateType.Walk)) return;

            if (ShouldTransitionToCrouchOrSlide() 
            && (MovementController.TrySetState(MovementStateType.Slide) || MovementController.TrySetState(MovementStateType.Crouch))) return;

            if (ShouldTransitionToProne() && MovementController.TrySetState(MovementStateType.Prone)) return;

            if (ShouldTransitionToJump() && MovementController.TrySetState(MovementStateType.Jump)) return;
        }

        private bool ShouldTransitionToWalk()
        {
            return (MovementInput.IsRunningHeld == false && InputOptions.Instance.AutoRunToggleMode == false)
                || CanTransitionTo() == false;
        }

        private bool ShouldTransitionToCrouchOrSlide() => MovementInput.IsCrouchingHeld;
        private bool ShouldTransitionToProne() => MovementInput.IsProningHeld;
        private bool ShouldTransitionToJump() => MovementInput.IsJumpPressed;
    }
}