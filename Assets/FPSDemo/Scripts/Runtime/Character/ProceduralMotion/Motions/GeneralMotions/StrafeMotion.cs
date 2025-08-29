using Nexora.FPSDemo.Movement;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    [DisallowMultipleComponent]
    [RequireCharacterBehaviour(typeof(ICharacterMotor))]
    public sealed class StrafeMotion : CharacterDataMotion<CompositeMotionData>
    {
        private ICharacterMotor _characterMotor;

        protected override void OnBehaviourStart(ICharacter parent) => _characterMotor = parent.GetCC<ICharacterMotor>();

        protected override void OnMotionDataChanged(CompositeMotionData motionData)
        {
            if(motionData?.Strafe == null)
            {
                return;
            }

            _positionSpring.ChangeSpringSettings(motionData.Strafe.PositionSpring);
            _rotationSpring.ChangeSpringSettings(motionData.Strafe.RotationSpring);
        }

        public override void Tick(float deltaTime)
        {
            if(CurrentMotionData == null || 
                (_positionSpring.IsAtRest && _rotationSpring.IsAtRest && _characterMotor.Velocity == Vector3.zero))
            {
                return;
            }

            Vector3 localVelocity = transform.InverseTransformVector(_characterMotor.Velocity);

            var (targetPosition, targetRotation) = (
                StrafeSwayCalculator.CalculatePositionSway(localVelocity, CurrentMotionData.Strafe),
                StrafeSwayCalculator.CalculateRotationSway(localVelocity, CurrentMotionData.Strafe));

            SetTargetPosition(targetPosition);
            SetTargetRotation(targetRotation);
        }
    }

    public static class StrafeSwayCalculator
    {
        public static Vector3 CalculatePositionSway(Vector3 localVelocity, SwayData strafeData)
        {
            Vector3 strafeInput = GetStrafeInput(localVelocity, strafeData);

            return new Vector3
            {
                x = strafeInput.x * strafeData.PositionSway.x,
                y = -Mathf.Abs(strafeInput.x * strafeData.PositionSway.y),
                z = -strafeInput.z * strafeData.PositionSway.z
            };
        }

        public static Vector3 CalculateRotationSway(Vector3 localVelocity, SwayData strafeData)
        {
            Vector3 strafeInput = GetStrafeInput(localVelocity, strafeData);

            return new Vector3
            {
                x = strafeInput.z * strafeData.RotationSway.x,
                y = -strafeInput.x * strafeData.RotationSway.y,
                z = strafeInput.x * strafeData.RotationSway.z
            };
        }

        private static Vector3 GetStrafeInput(Vector3 localVelocity, SwayData strafeData)
            => Vector3.ClampMagnitude(localVelocity, strafeData.MaxSway);
    }
}