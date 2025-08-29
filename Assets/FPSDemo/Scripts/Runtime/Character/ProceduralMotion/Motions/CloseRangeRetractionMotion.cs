using Nexora.FPSDemo.Handhelds;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    [RequireCharacterBehaviour(typeof(IHandheldRetractionHandler))]
    public sealed class CloseRangeRetractionMotion : CharacterDataMotion<CloseRangeRetractionData>
    {
        private IHandheldRetractionHandler _retractionHandler;

        public float RetractionFactor { get; private set; }
        public bool IsRetracted => RetractionFactor > 0;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _retractionHandler = parent.GetCC<IHandheldRetractionHandler>();
            IgnoreMixerBlendWeight = true;
        }

        protected override void OnMotionDataChanged(CloseRangeRetractionData motionData)
        {
            if(motionData == null)
            {
                return;
            }

            _positionSpring.ChangeSpringSettings(motionData.PositionSpring);
            _rotationSpring.ChangeSpringSettings(motionData.RotationSpring);
        }

        public override void Tick(float deltaTime)
        {
            if(CurrentMotionData == null)
            {
                return;
            }

            float closestObjectDistance = _retractionHandler?.ClosestObjectDistance ?? float.MaxValue;

            if(closestObjectDistance >= CurrentMotionData.RetractionDistance && _positionSpring.IsAtRest && _rotationSpring.IsAtRest)
            {
                return;
            }

            var (targetPosition, targetRotation, factor) = RetractionCalculator.Calculate(closestObjectDistance, CurrentMotionData);

            RetractionFactor = factor;

            SetTargetPosition(targetPosition);
            SetTargetRotation(targetRotation);
        }
    }

    public static class RetractionCalculator
    {
        public static (Vector3 position, Vector3 rotation, float factor) Calculate(float closestObjectDistance, CloseRangeRetractionData data)
        {
            if(data == null || closestObjectDistance >= data.RetractionDistance)
            {
                return (Vector3.zero, Vector3.zero, 0f);
            }

            float factor = (data.RetractionDistance - closestObjectDistance) / data.RetractionDistance;
            factor = Mathf.Clamp01(factor);

            Vector3 retractionPosition = data.PositionOffset * factor;
            Vector3 retractionRotation = data.RotationOffset * factor;

            return (retractionPosition, retractionRotation, factor);
        }
    }
}