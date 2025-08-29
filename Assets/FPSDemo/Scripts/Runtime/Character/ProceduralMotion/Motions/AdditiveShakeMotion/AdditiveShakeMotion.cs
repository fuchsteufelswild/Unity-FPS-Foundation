using Nexora.Motion;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ExecutionOrder.EarlyGameLogic)]
    public sealed class AdditiveShakeMotion : 
        CharacterMotion, 
        IShakeHandler
    {
        private readonly ShakeChannel _positionShakeChannel = new();
        private readonly ShakeChannel _rotationShakeChannel = new();

        protected override void OnBehaviourStart(ICharacter parent) => IgnoreMixerBlendWeight = true;

        public void AddShake(ShakeInstance shakeInstance, float intensity = 1f)
        {
            if(shakeInstance.IsPlayable == false)
            {
                return;
            }

            ShakeProfile profile = shakeInstance.ShakeProfile;
            AddPositionShake(profile.PositionShake, shakeInstance.Duration, shakeInstance.Intensity * intensity);
            AddRotationShake(profile.RotationShake, shakeInstance.Duration, shakeInstance.Intensity * intensity);
        }

        public void AddPositionShake(in ShakeDefinition shakeDefinition, float duration, float intensity = 1f)
        {
            _positionShakeChannel.AddShake(in shakeDefinition, duration, intensity);
            _positionSpring.ChangeSpringSettings(shakeDefinition.Spring);
        }

        public void AddRotationShake(in ShakeDefinition shakeDefinition, float duration, float intensity = 1f)
        {
            _rotationShakeChannel.AddShake(in shakeDefinition, duration, intensity);
            _rotationSpring.ChangeSpringSettings(shakeDefinition.Spring);
        }

        public override void Tick(float deltaTime)
        {
            Vector3 targetPosition = _positionShakeChannel.Evaluate();
            Vector3 targetRotation = _rotationShakeChannel.Evaluate();

            if(targetPosition == Vector3.zero && targetRotation == Vector3.zero
            && _positionSpring.IsAtRest && _rotationSpring.IsAtRest)
            {
                return;
            }

            SetTargetPosition(targetPosition);
            SetTargetRotation(targetRotation);
        }
    }
}