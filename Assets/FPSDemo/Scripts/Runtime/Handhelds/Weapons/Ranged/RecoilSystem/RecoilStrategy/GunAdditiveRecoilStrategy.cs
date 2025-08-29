using Nexora.FPSDemo.ProceduralMotion;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Applies recoil to the camera, this recoil type is to be added to the recoil
    /// that builds up as gun is fired. And tries to slowly recover to the default state at all times.
    /// </summary>
    /// <remarks>
    /// Additive term here doesn't refer to the type of recoil force, it is the nature of the motion,
    /// builds up with each call and always tries to go back to the default state.
    /// </remarks>
    [Serializable]
    public abstract class GunAdditiveRecoilStrategy : IGunRecoilStrategy
    {
        protected RecoilMotion RecoilMotion { get; private set; }

        public void Initialize(IMotionMixer motionMixer, ICharacter character)
        {
            if (motionMixer.TryGetMotion(out RecoilMotion recoilMotion) == false)
            {
                recoilMotion = new RecoilMotion(character);
                motionMixer.AddMotion(recoilMotion);
            }

            RecoilMotion = recoilMotion;

            var (recoilSprings, recoverySprings) = GetSpringSettings();
            RecoilMotion.SetRecoilSpringSettings(RecoilStateType.Recoiling, recoilSprings);
            RecoilMotion.SetRecoilSpringSettings(RecoilStateType.Recovering, recoverySprings);
        }

        /// <summary>
        /// Gets unique <see cref="SpringSettings"/> for recoil and recovery stages.
        /// </summary>
        protected abstract (SpringSettings, SpringSettings) GetSpringSettings();

        public abstract void Apply(float baseRecoilIntensity, float recoilProgression, bool isAiming);
    }

    /// <summary>
    /// Applies recoil to the camera depending on the <see cref="AnimationCurve2D"/> pattern.
    /// Used for guns like <b>Assault Rifle</b>, <b>SMGs</b> that are automatic.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="RecoilMotion"/> for delegating the recoil.
    /// </remarks>
    [Serializable]
    public sealed class PatternSpringRecoil : GunAdditiveRecoilStrategy
    {
        [Tooltip("Spring settings for the recoil state.")]
        [SerializeField]
        private SpringSettings _recoilSpring = SpringSettings.Default;

        [Tooltip("Spring settings for the recovery state.")]
        [SerializeField]
        private SpringSettings _recoverySpring = SpringSettings.Default;

        [Tooltip("Curve that defines the recoil pattern.")]
        [SerializeField]
        private AnimationCurve2D _recoilPatternCurve = new();

        protected override (SpringSettings, SpringSettings) GetSpringSettings() => (_recoilSpring, _recoverySpring);

        public override void Apply(float baseRecoilIntensity, float recoilProgression, bool isAiming)
        {
            Vector2 recoilAmount = _recoilPatternCurve.Evaluate(recoilProgression);
            RecoilMotion.AddRecoil(recoilAmount *  baseRecoilIntensity);
        }
    }

    /// <summary>
    /// Applies recoil to the camera with single force. Used for guns like one-shot pistols like <b>Revolver</b>,
    /// or <b>Shotguns</b>, or <b>bolt-action Rifles</b>. 
    /// </summary>
    /// <remarks>
    /// Uses <see cref="RecoilMotion"/> for delegating the recoil.
    /// </remarks>
    [Serializable]
    public sealed class SingleForceSpringRecoil : GunAdditiveRecoilStrategy
    {
        [Tooltip("Spring settings for the recoil state.")]
        [SerializeField]
        private SpringSettings _recoilSpring = new(15f, 300f, 3f);

        [Tooltip("Spring settings for the recovery state.")]
        [SerializeField]
        private SpringSettings _recoverySpring = new(10f, 70f, 1f);

        [Tooltip("Range for the x recoil amount (recoil to sides).")]
        [SerializeField]
        private Vector2 _xRecoilAmountRange = new(-1.5f, 1.5f);

        [Tooltip("Range for the y recoil amount (recoil to up/down).")]
        [SerializeField]
        private Vector2 _yRecoilAmountRange = new(-0.2f, 0.2f);

        protected override (SpringSettings, SpringSettings) GetSpringSettings() => (_recoilSpring, _recoverySpring);

        public override void Apply(float baseRecoilIntensity, float recoilProgression, bool isAiming)
        {
            Vector2 recoilAmount = new Vector2
            {
                x = _xRecoilAmountRange.GetRandomFromRange(),
                y = _yRecoilAmountRange.GetRandomFromRange()
            } * baseRecoilIntensity;

            RecoilMotion.AddRecoil(recoilAmount);
        }
    }
}