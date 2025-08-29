using Nexora.FPSDemo.CharacterBehaviours;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    [DisallowMultipleComponent]
    [RequireCharacterBehaviour(typeof(ILookController))]
    public sealed class LookMotion : CharacterDataMotion<CompositeMotionData>
    {
        private ILookController _lookController;

        protected override void OnBehaviourStart(ICharacter parent) => _lookController = parent.GetCC<ILookController>();

        protected override void OnMotionDataChanged(CompositeMotionData motionData)
        {
            if(motionData?.Look == null)
            {
                return;
            }

            _positionSpring.ChangeSpringSettings(motionData.Look.RotationSpring);
            _rotationSpring.ChangeSpringSettings(motionData.Landing.RotationSpring);
        }

        public override void Tick(float deltaTime)
        {
            if(CurrentMotionData == null)
            {
                return;
            }

            var (targetPosition, targetRotation) = LookSwayCalculator.Calculate(_lookController.LookInput, CurrentMotionData.Look);

            SetTargetPosition(targetPosition);
            SetTargetRotation(targetRotation);
        }
    }

    public static class LookSwayCalculator
    {
        public static (Vector3 position, Vector3 rotation) Calculate(Vector2 lookInput, SwayData lookData)
        {
            Vector2 clampedInput = Vector2.ClampMagnitude(lookInput, lookData.MaxSway);

            Vector3 positionSway = new()
            {
                x = clampedInput.y * lookData.PositionSway.x,
                y = clampedInput.x * lookData.PositionSway.y,
                z = 0f
            };

            Vector3 rotationSway = new()
            {
                x = clampedInput.x * lookData.RotationSway.x,
                y = clampedInput.y * -lookData.RotationSway.y,
                z = clampedInput.y * -lookData.RotationSway.z
            };

            return (positionSway, rotationSway);
        }
    }
}