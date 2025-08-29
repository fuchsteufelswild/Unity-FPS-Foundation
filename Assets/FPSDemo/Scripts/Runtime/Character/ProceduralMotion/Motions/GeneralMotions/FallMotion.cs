using Nexora.FPSDemo.Movement;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    [DisallowMultipleComponent]
    [RequireCharacterBehaviour(typeof(ICharacterMotor))]
    public sealed class FallMotion : CharacterDataMotion<CompositeMotionData>
    {
        [Tooltip("Maximum speed of fall we take for motion.")]
        [SerializeField, Range(0f, 100f)]
        private float _terminalSpeed;

        private ICharacterMotor _characterMotor;

        protected override void OnBehaviourStart(ICharacter parent) => _characterMotor = parent.GetCC<ICharacterMotor>();

        protected override void OnMotionDataChanged(CompositeMotionData motionData)
        {
            if(motionData?.Fall == null)
            {
                return;
            }

            _positionSpring.ChangeSpringSettings(motionData.Fall.PositionSpring);
            _rotationSpring.ChangeSpringSettings(motionData.Fall.RotationSpring);
        }

        public override void Tick(float deltaTime)
        {
            if(CurrentMotionData == null ||
                (_characterMotor.IsGrounded && _positionSpring.IsAtRest && _rotationSpring.IsAtRest))
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
                return;
            }

            float verticalSpeed = _characterMotor.Velocity.y;

            var (targetPosition, targetRotation) = FallMotionCalculator.Calculate(verticalSpeed, CurrentMotionData.Fall, _terminalSpeed);

            SetTargetPosition(targetPosition);
            SetTargetRotation(targetRotation);
        }
    }

    public static class FallMotionCalculator
    {
        public static (Vector3 position, Vector3 rotation) Calculate(float verticalSpeed, SingleData fallData, float terminalSpeed)
        {
            float downwardSpeed = Mathf.Min(0f, verticalSpeed);
            float factor = Mathf.Max(downwardSpeed, -terminalSpeed);

            Vector3 position = new(0f, factor * fallData.PositionValue, 0f);
            Vector3 rotation = new(factor * fallData.RotationValue, 0f, 0f);

            return (position, rotation);
        }
    }
}