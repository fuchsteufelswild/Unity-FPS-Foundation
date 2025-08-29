using Nexora.FPSDemo.Movement;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{

    [DisallowMultipleComponent]
    [RequireCharacterBehaviour(typeof(ICharacterMotor))]
    public sealed class LandingMotion : CharacterDataMotion<CompositeMotionData>
    {
        [SerializeField]
        private LandingImpactProcessor _landingImpactProcessor;

        private LandingMotionConfig _config = new();

        private ICharacterMotor _characterMotor;

        private LandingMotionStateController _stateController;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _characterMotor = parent.GetCC<ICharacterMotor>();

            Transform targetTransform = Mixer.Target;
            _stateController = new LandingMotionStateController(
                in _config, 
                new GeneralMotionPlayer(),
                new GeneralMotionCurveEvaluator(null));
        }

        protected override void OnBehaviourEnable(ICharacter parent) => _characterMotor.FallImpact += OnFallImpact;
        protected override void OnBehaviourDisable(ICharacter parent) => _characterMotor.FallImpact -= OnFallImpact;

        private void OnFallImpact(float landingSpeed)
        {
            float? speedFactor = _landingImpactProcessor.ProcessImpact(landingSpeed);

            if(speedFactor.HasValue)
            {
                _stateController.StartAnimation(speedFactor.Value);
            }
        }

        protected override void OnMotionDataChanged(CompositeMotionData motionData)
        {
            if(motionData?.Landing == null)
            {
                return;
            }

            _positionSpring.ChangeSpringSettings(motionData.Landing.PositionSpring);
            _rotationSpring.ChangeSpringSettings(motionData.Landing.RotationSpring);
        }

        public override void Tick(float deltaTime)
        {
            if(CurrentMotionData?.Landing == null)
            {
                HandleNoDataState();
                return;
            }

            ProcessLandingAnimation(deltaTime);
        }

        private void HandleNoDataState() => _stateController.ResetToInactive();

        private void ProcessLandingAnimation(float deltaTime)
        {
            CurveData landingData = CurrentMotionData.Landing;

            var (position, rotation, positionContinue, rotationContinue) = _stateController.Tick(deltaTime, Mixer.Target, CurrentMotionData.Landing);

            if(positionContinue)
            {
                SetTargetPosition(position);
            }
            if (rotationContinue)
            {
                SetTargetRotation(rotation);
            }

            if (positionContinue == false && rotationContinue == false)
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
            }
        }

        /// <summary>
        /// Controls the state of landing motion, responsible for starting/ticking/stopping motion
        /// and calculating the position/rotation.
        /// </summary>
        private sealed class LandingMotionStateController : GeneralMotionStateController
        {
            private readonly LandingMotionConfig _config;

            public LandingMotionStateController(in LandingMotionConfig config, GeneralMotionPlayer animationPlayer, GeneralMotionCurveEvaluator curveEvaluator)
                : base(animationPlayer, curveEvaluator)
            {
                _config = config;
            }

            public void ResetToInactive() => _animationPlayer.ResetToInactive(_config.ResetTime);
        }

        /// <summary>
        /// Class to determine how strong is the landing impact.
        /// </summary>
        [Serializable]
        private sealed class LandingImpactProcessor
        {
            [SerializeField, Range(1f, 100f)]
            private float _minLandingSpeed = 5f;

            [SerializeField, Range(1f, 100f)]
            private float _maxLandingSpeed = 15f;

            /// <summary>
            /// Processes the landing impact and returns a valid value if the landing speed was considerable.
            /// </summary>
            /// <param name="landingSpeed">How fast the character was moving downwards.</param>
            /// <returns>How strong is the landing impact? [0,1]</returns>
            public float? ProcessImpact(float landingSpeed)
            {
                if (IsSignificantImpact(landingSpeed) == false)
                {
                    return null;
                }

                float impactSpeedAbs = Mathf.Abs(landingSpeed);
                return Mathf.Clamp01(impactSpeedAbs / _maxLandingSpeed);
            }

            public bool IsSignificantImpact(float landingSpeed) => Mathf.Abs(landingSpeed) > _minLandingSpeed;
        }

        private readonly struct LandingMotionConfig
        {
            public readonly float ResetTime;
            public readonly float InactiveSpeedFactor;

            public LandingMotionConfig(float resetTime = 100f, float inactiveSpeedFactor = -1f)
            {
                ResetTime = resetTime;
                InactiveSpeedFactor = inactiveSpeedFactor;
            }
        }
    }
}