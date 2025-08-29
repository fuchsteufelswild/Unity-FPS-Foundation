using System;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    [Serializable]
    public abstract class GroundedMovementState : CharacterMovementState
    {
        protected const float IsMovingThreshold = 0.001f;

        [Tooltip("The distance the character needs to cover to complete a step cycle.")]
        [SerializeField, Range(0.2f, 15f)]
        private float _stepCycleLength = 1.5f;

        [Tooltip("Movement speed when moving forward.")]
        [SerializeField, Range(0.2f, 15f)]
        private float _forwardSpeed = 2f;

        [Tooltip("Movement speed when moving backwards.")]
        [SerializeField, Range(0.2f, 15f)]
        private float _backwardSpeed = 2f;

        [Tooltip("Movement speed when strafing sideways.")]
        [SerializeField, Range(0.2f, 15f)]
        private float _strafeSpeed = 2f;

        public override bool ApplyGravity => false;
        public override bool SnapToGround => true;
        public override float StepCycleLength => _stepCycleLength;

        public override bool CanTransitionTo() => CharacterMotor.IsGrounded;

        public override Vector3 UpdateVelocity(Vector3 currentVelocity, float deltaTime)
        {
            Vector3 targetVelocity = CalculateTargetVelocity(MovementInput.WorldMovementDirection, currentVelocity);

            float accelerationRate = 1f;
            if (targetVelocity.sqrMagnitude > IsMovingThreshold)
            {
                targetVelocity *= MovementController.SpeedModifier.Evaluate() * CharacterMotor.GetSlopeSpeedMultiplier();
                accelerationRate = MovementController.AccelerationModifier.Evaluate();
            }
            else
            {
                accelerationRate = MovementController.DecelerationModifier.Evaluate();
            }

            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, accelerationRate * deltaTime);

            return currentVelocity;
        }

        protected virtual Vector3 CalculateTargetVelocity(Vector3 moveDirection, Vector3 currentVelocity)
        {
            bool isMoving = moveDirection.sqrMagnitude > 0f;
            if (isMoving == false)
            {
                return Vector3.zero;
            }

            float movementSpeed = GetMovementSpeed(MovementInput.RawMovementDirection);
            return moveDirection * movementSpeed;
        }

        private float GetMovementSpeed(Vector2 rawInput)
        {
            if(rawInput.y < 0f)
            {
                return _backwardSpeed;
            }

            if(Mathf.Abs(rawInput.x) > 0.01f)
            {
                return _strafeSpeed;
            }

            return _forwardSpeed;
        }
    }
}