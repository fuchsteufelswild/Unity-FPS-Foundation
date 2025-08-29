using Nexora.FPSDemo.ProceduralMotion;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Interface for defining different recoil strategies for <see cref="IGun"/>.
    /// </summary>
    public interface IGunRecoilStrategy
    {
        /// <summary>
        /// Initializes the recoil strategy with <paramref name="motionMixer"/> and <paramref name="character"/>.
        /// </summary>
        /// <param name="motionMixer">Mixer to apply recoil forces to.</param>
        /// <param name="character">Character that gun belongs to.</param>
        void Initialize(IMotionMixer motionMixer, ICharacter character);

        /// <summary>
        /// Applies recoil with given parameters.
        /// </summary>
        /// <param name="baseRecoilIntensity">Base intensity multiplier of the recoil.</param>
        /// <param name="recoilProgression">How much is the recoil has progressed (with every shoot it increases).</param>
        /// <param name="isAiming">Is character currently aiming?</param>
        void Apply(float baseRecoilIntensity, float recoilProgression, bool isAiming);
    }

    /// <summary>
    /// Applies position and rotation impulse in the event of recoil (shot).
    /// </summary>
    /// <remarks>
    /// This force can be thought of one time added force and faded like any other force, this
    /// is for the feedback of the shot.
    /// </remarks>
    [Serializable]
    public sealed class SpringForceRecoil : IGunRecoilStrategy
    {
        [Tooltip("How fast recoil progression decays.")]
        [SerializeField, Range(0f, 1f)]
        private float _recoilIntensityProgression;

        [Tooltip("How much reduce in the recoil will be done when aiming?")]
        [SerializeField, Range(0f, 1f)]
        private float _aimRecoilReduction = 1f;

        [Tooltip("Type of the spring for the recoil (gentle, bouncy etc.)")]
        [SerializeField]
        private SpringType _springBehaviour = SpringType.Bouncy;

        [Tooltip("Custom spring settings to use if don't want predefined ones.")]
        [ShowIf(nameof(_springBehaviour), SpringType.Custom)]
        [SerializeField]
        private SpringSettings _customSpringSettings = SpringSettings.Default;

        [Tooltip("Random position force applied during recoil.")]
        [SerializeField]
        private RandomSpringImpulseDefinitionGenerator _positionRecoilForce = RandomSpringImpulseDefinitionGenerator.Default;

        [Tooltip("Random rotation force applied during recoil.")]
        [SerializeField]
        private RandomSpringImpulseDefinitionGenerator _rotationRecoilForce = RandomSpringImpulseDefinitionGenerator.Default;

        private AdditiveMotionApplier _forceMotionController;

        public void Initialize(IMotionMixer motionMixer, ICharacter character) 
            => _forceMotionController = motionMixer.GetMotion<AdditiveMotionApplier>();

        public void Apply(float baseRecoilIntensity, float recoilProgression, bool isAiming)
        {
            float finalRecoilIntensity = baseRecoilIntensity * Mathf.Lerp(1f, recoilProgression, _recoilIntensityProgression);
            finalRecoilIntensity *= isAiming ? _aimRecoilReduction : 1f;

            if(_springBehaviour == SpringType.Custom)
            {
                _forceMotionController.SetCustomSpringSettings(_customSpringSettings, _customSpringSettings);
            }

            _forceMotionController.AddPositionForce(_positionRecoilForce, finalRecoilIntensity, _springBehaviour);
            _forceMotionController.AddRotationForce(_rotationRecoilForce, finalRecoilIntensity, _springBehaviour);
        }
    }

    /// <summary>
    /// Applies shake in the event of recoil (shot).
    /// </summary>
    /// <remarks>
    /// This force can be thought of one time added force and faded like any other force, this
    /// is for the feedback of the shot.
    /// </remarks>
    [Serializable]
    public sealed class ShakeForceRecoil : IGunRecoilStrategy
    {
        [Tooltip("Shake properties for recoil effects.")]
        [SerializeField]
        private ShakeInstance _recoilShakeSettings;

        private AdditiveShakeMotion _shakeMotionController;

        public void Initialize(IMotionMixer motionMixer, ICharacter character)
            => _shakeMotionController = motionMixer.GetMotion<AdditiveShakeMotion>();

        public void Apply(float baseRecoilIntensity, float recoilProgression, bool isAiming)
        {
            if(_recoilShakeSettings.IsPlayable)
            {
                _shakeMotionController.AddShake(_recoilShakeSettings, baseRecoilIntensity);
            }
        }
    }
}