using System;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    [Serializable]
    public class AirborneState : CharacterMovementState
    {
        [Title("Speed Settings")]
        [SerializeField, Range(1f, 500f)]
        private float _maxDownwardSpeed = 20f;

        [SerializeField, Range(1f, 500f)]
        private float _maxUpwardSpeed = 20f;

        [Tooltip("Minimum speed for a character in the air, horizontal speed.")]
        [SerializeField, Range(0f, 15f)]
        private float _minAirSpeed = 2f;

        [Title("Air Control")]
        [Tooltip("How much input effects horizontal movement while being airborne.")]
        [SerializeField, Range(0f, 10f)]
        private float _inputInfluence = 3f;

        [Tooltip("How fast the input takes effect while being airborne.")]
        [SerializeField, Range(0.1f, 10f)]
        private float _inputResponsiveness = 5f;

        [Tooltip("Modifier for landing velocity when landing after being airborne.")]
        [SerializeField, Range(0f, 3f)]
        private float _landingVelocityModifier = 0.75f;

        [Tooltip("How fast the initial moment decays while being airborne.")]
        [SerializeField, Range(0f, 10f)]
        private float _initialMomentumDecayRate = 3f;

        private bool _wasGroundedLastTick;
        private Vector3 _initialHorizontalVelocity;
        private Vector3 _smoothedInput;

        private float _currentMaxSpeed;

        public override MovementStateType StateType => MovementStateType.Airborne;
        public override float StepCycleLength => 1f;
        public override bool ApplyGravity => true;
        public override bool SnapToGround => false;

        public override bool CanTransitionTo() => CharacterMotor.IsGrounded == false;

        public override void OnEnter(MovementStateType previousStateType)
        {
            _currentMaxSpeed = Mathf.Max(CharacterMotor.SimulatedVelocity.Horizontal().magnitude, _minAirSpeed);
            _initialHorizontalVelocity = CharacterMotor.SimulatedVelocity.Horizontal();
            _smoothedInput = Vector3.zero;
            _wasGroundedLastTick = true;
        }

        public override void UpdateLogic()
        {
            HandleTransitions();
            _wasGroundedLastTick = CharacterMotor.IsGrounded;
            ClearUnusedInputs();
        }

        private void HandleTransitions()
        {
            if (ShouldTransitionToJump() && MovementController.TrySetState(MovementStateType.Jump)) return;

            // If not on the ground, then no need check ground states
            if (CharacterMotor.IsGrounded == false) return;

            if (ShouldTransitionToRun() && MovementController.TrySetState(MovementStateType.Run)) return;

            if (ShouldTransitionToSlide() && MovementController.TrySetState(MovementStateType.Slide)) return;

            if (MovementController.TrySetState(MovementStateType.Idle)) return;
            if (MovementController.TrySetState(MovementStateType.Walk)) return;
        }

        private bool ShouldTransitionToJump() => MovementInput.IsJumpPressed;
        private bool ShouldTransitionToRun() => MovementInput.IsRunningHeld;
        private bool ShouldTransitionToSlide() => MovementInput.IsCrouchingHeld;

        private void ClearUnusedInputs()
        {
            if(MovementInput.RawMovementDirection.sqrMagnitude < 0.01f)
            {
                MovementInput.ConsumeCrouchInput();
                MovementInput.ConsumeProneInput();
            }
        }

        public override Vector3 UpdateVelocity(Vector3 currentVelocity, float deltaTime)
        {
            UpdateSmoothedInput(deltaTime);
            DecayInitialMomentum(deltaTime);

            Vector3 horizontalVelocity = CalculateHorizontalVelocity();
            Vector3 verticalVelocity = CalculateVerticalVelocity(currentVelocity);

            return HandleSpecialCases(horizontalVelocity.Horizontal() + verticalVelocity);
        }

        /// <summary>
        /// Updates movement input using the input smoothing parameters, modifying its magnitude
        /// using the influence and lerps it smoothly to avoid abrupt movements.
        /// </summary>
        private void UpdateSmoothedInput(float deltaTime)
        {
            _smoothedInput = Vector3.Lerp(
                _smoothedInput,
                _inputInfluence * MovementInput.WorldMovementDirection,
                deltaTime * _inputResponsiveness);
        }

        /// <summary>
        /// Decays the initial momentum back to zero slowly.
        /// </summary>
        private void DecayInitialMomentum(float deltaTime)
        {
            _initialHorizontalVelocity = Vector3.Lerp(
                _initialHorizontalVelocity,
                Vector3.zero,
                deltaTime * _initialMomentumDecayRate);
        }

        private Vector3 CalculateHorizontalVelocity() 
            => Vector3.ClampMagnitude(_smoothedInput + _initialHorizontalVelocity, _currentMaxSpeed);

        private Vector3 CalculateVerticalVelocity(Vector3 currentVelocity)
        {
            float verticalSpeed = Mathf.Clamp(
                currentVelocity.y,
                -_maxDownwardSpeed,
                _maxUpwardSpeed);

            if(HitCeiling() && currentVelocity.y > 0.1f)
            {
                verticalSpeed = currentVelocity.y * -0.5f;
            }

            return Vector3.up * verticalSpeed;
        }

        private bool HitCeiling() => CharacterMotor.CollisionFlags.HasFlag(CollisionFlags.CollidedAbove);

        /// <summary>
        /// Handles any special cases that can happen.
        /// </summary>
        /// <returns>Modified velocity after processing all special cases.</returns>
        private Vector3 HandleSpecialCases(Vector3 velocity)
        {
            if(JustLanded())
            {
                velocity.x *= _landingVelocityModifier;
                velocity.z *= _landingVelocityModifier;
                // Small value to make character go into the ground a little further to ensure sticking
                velocity.y = -1;
            }

            return velocity;
        }

        private bool JustLanded() => CharacterMotor.IsGrounded && _wasGroundedLastTick == false;
    }
}