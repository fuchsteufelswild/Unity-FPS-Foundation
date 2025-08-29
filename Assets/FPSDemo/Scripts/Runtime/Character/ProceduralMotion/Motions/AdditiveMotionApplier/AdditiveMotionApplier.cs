using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    /// <summary>
    /// Motion that can be added multiple fixed forces, curve forces, and delayed forces for both
    /// position and rotation separately. Automatically disbands expired forces, and can configure
    /// the motion using different spring settings.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ExecutionOrder.EarlyGameLogic)]
    public sealed class AdditiveMotionApplier : CharacterMotion
    {
        [Title("Interpolation")]
        [SerializeField]
        private SpringSettingsProvider _positionSpringSettingsProvider = new();

        [SerializeField]
        private SpringSettingsProvider _rotationSpringSettingsProvider = new();

        private ForceManager _positionForceManager = new();
        private ForceManager _rotationForceManager = new();
        private MotionUpdateManager _motionUpdateManager = new();

        protected override void OnBehaviourStart(ICharacter parent) => IgnoreMixerBlendWeight = true;
        protected override SpringSettings DefaultPositionSpringSettings => _positionSpringSettingsProvider.GentleSpring;
        protected override SpringSettings DefaultRotationSpringSettings => _rotationSpringSettingsProvider.GentleSpring;

        public override void Tick(float deltaTime)
        {
            if(_motionUpdateManager.RequiresUpdate == false)
            {
                return;
            }

            float currentTime = Time.time;

            Vector3 targetPosition = _positionForceManager.EvaluateAndCleanup(currentTime);
            Vector3 targetRotation = _rotationForceManager.EvaluateAndCleanup(currentTime);

            SetTargetPosition(targetPosition);
            SetTargetRotation(targetRotation);

            _motionUpdateManager.UpdateStatus(targetPosition, targetRotation, _positionSpring, _rotationSpring);
        }

        /// <summary>
        /// Sets custom <see cref="SpringSettings"/> to be used for position and rotation motion.
        /// </summary>
        public void SetCustomSpringSettings(SpringSettings positionSpringSettings,  SpringSettings rotationSpringSettings)
        {
            _positionSpringSettingsProvider.SetCustomSettings(positionSpringSettings);
            _rotationSpringSettingsProvider.SetCustomSettings(rotationSpringSettings);
        }

        public void AddDelayedPositionForce(DelayedSpringImpulseDefinition delayedSpringImpulse, float scale = 1f, SpringType springType = SpringType.Gentle)
        {
            this.InvokeDelayed(
                () => AddPositionForce(delayedSpringImpulse.SpringImpulse, scale, springType), 
                delayedSpringImpulse.Delay);
        }

        /// <summary>
        /// Adds a constant force that is applied for a fixed duration.
        /// Both force amount and the duration is specified by <paramref name="springImpulse"/>.
        /// </summary>
        /// <param name="springImpulse">Impulse to apply.</param>
        /// <param name="scale">Scaler for the force.</param>
        /// <param name="springType">Type of the spring motion.</param>
        public void AddPositionForce(SpringImpulseDefinition springImpulse, float scale = 1f, SpringType springType = SpringType.Gentle)
        {
            if(springImpulse.IsNegligible() || Mathf.Approximately(scale, 0f))
            {
                return;
            }

            _positionForceManager.AddSpringImpulse(springImpulse, scale, Time.time);
            _motionUpdateManager.MarkForUpdate();
            ApplyPositionSpringSettings(springType);
        }

        /// <summary>
        /// Adds a force that is retrieved from <paramref name="forceCurve"/> and applied the duration of the curve.
        /// </summary>
        /// <param name="forceCurve">Animation curve that is used as a source of force.</param>
        /// <param name="springType">Type of the spring motion.</param>
        public void AddPositionCurveForce(AnimationCurve3D forceCurve, SpringType springType = SpringType.Gentle)
        {
            if (forceCurve.Duration < ForceManager.MinimumAnimationCurveDuration)
            {
                return;
            }

            _positionForceManager.AddAnimationCurveForce(forceCurve, Time.time);
            _motionUpdateManager.MarkForUpdate();
            ApplyPositionSpringSettings(springType);
        }

        /// <summary>
        /// Changes the position spring settings depending on the new spring type.
        /// </summary>
        private void ApplyPositionSpringSettings(SpringType springType)
        {
            SpringSettings settings = _positionSpringSettingsProvider.GetSpringSettings(springType);
            _positionSpring.ChangeSpringSettings(settings);
        }

        public void AddDelayedRotationForce(DelayedSpringImpulseDefinition delayedSpringImpulse, float scale = 1f, SpringType springType = SpringType.Gentle)
        {
            this.InvokeDelayed(
                () => AddRotationForce(delayedSpringImpulse.SpringImpulse, scale, springType),
                delayedSpringImpulse.Delay);
        }

        /// <inheritdoc cref="AddPositionForce(SpringImpulseDefinition, float, SpringType)"/>
        public void AddRotationForce(SpringImpulseDefinition springImpulse, float scale = 1f, SpringType springType = SpringType.Gentle)
        {
            if(springImpulse.IsNegligible() || Mathf.Approximately(scale, 0f))
            {
                return;
            }

            _rotationForceManager.AddSpringImpulse(springImpulse, scale, Time.time);
            _motionUpdateManager.MarkForUpdate();
            ApplyRotationSpringSettings(springType);
        }

        /// <inheritdoc cref="AddPositionCurveForce(AnimationCurve3D, SpringType)"/>
        public void AddRotationCurveForce(AnimationCurve3D forceCurve, SpringType springType = SpringType.Gentle)
        {
            if(forceCurve.Duration < ForceManager.MinimumAnimationCurveDuration)
            {
                return;
            }

            _rotationForceManager.AddAnimationCurveForce(forceCurve, Time.time);
            _motionUpdateManager.MarkForUpdate();
            ApplyRotationSpringSettings(springType);
        }

        /// <summary>
        /// Changes the rotation spring settings depending on the new spring type.
        /// </summary>
        private void ApplyRotationSpringSettings(SpringType springType)
        {
            SpringSettings settings = _rotationSpringSettingsProvider.GetSpringSettings(springType);
            _rotationSpring.ChangeSpringSettings(settings);
        }

        private sealed class MotionUpdateManager
        {
            private bool _requiresUpdate;

            public bool RequiresUpdate => _requiresUpdate;

            public void MarkForUpdate() => _requiresUpdate = true;

            /// <summary>
            /// Updates status by checking if springs arrived their targets and stopped moving.
            /// </summary>
            /// <param name="targetPosition">Target accumulated position.</param>
            /// <param name="targetRotation">Target accumulated rotation.</param>
            /// <param name="positionSpring">Spring that drives position motion.</param>
            /// <param name="rotationSpring">Spring that drives rotation motion.</param>
            /// <returns>If motion needs to be updating.</returns>
            public void UpdateStatus(Vector3 targetPosition, Vector3 targetRotation, PhysicsSpring3D positionSpring, PhysicsSpring3D rotationSpring)
            {
                if(targetPosition == Vector3.zero && targetRotation == Vector3.zero
                && positionSpring.IsAtRest && rotationSpring.IsAtRest)
                {
                    _requiresUpdate = false;
                }
            }

            /// <summary>
            /// Resets the update requirements.
            /// </summary>
            public void Reset() => _requiresUpdate = false;
        }

        [Serializable]
        private sealed class SpringSettingsProvider
        {
            [SerializeField]
            private SpringSettings _gentleSpring = new SpringSettings(10f, 100f, 1f);

            [SerializeField]
            private SpringSettings _bouncySpring = new SpringSettings(12f, 140f, 1.1f);

            private SpringSettings _customSpring;

            public SpringSettings GentleSpring => _gentleSpring;
            public SpringSettings BouncySpring => _bouncySpring;
            public SpringSettings CustomSpring => _customSpring;

            public void SetCustomSettings(SpringSettings settings) => _customSpring = settings;

            public SpringSettings GetSpringSettings(SpringType springType) => springType switch
            {
                SpringType.Gentle => _gentleSpring,
                SpringType.Bouncy => _bouncySpring,
                SpringType.Custom => _customSpring,
                _ => SpringSettings.Default
            };
        }
    }
}