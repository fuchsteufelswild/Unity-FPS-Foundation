using Nexora.FPSDemo.ProceduralMotion;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunRecoilBehaviour
    {
        /// <inheritdoc cref="GunRecoilBehaviour.RecoilProcessorConfig.HeatPerShot"/>
        float HeatPerShot { get; }

        /// <inheritdoc cref="GunRecoilBehaviour.RecoilProcessorConfig.HeatRecoveryRate"/>
        float HeatRecoveryRate { get; }

        /// <inheritdoc cref="GunRecoilBehaviour.RecoilProcessorConfig.HeatRecoveryDelay"/>
        float HeatRecoveryDelay { get; }

        /// <summary>
        /// Multiplier for the recoil.
        /// </summary>
        float RecoilMultiplier { get; }

        /// <summary>
        /// Gets accuracy kick depending on <paramref name="isAiming"/> status.
        /// </summary>
        float GetAccuracyKick(bool isAiming);

        /// <summary>
        /// Gets accuracy recovery rate depending on <paramref name="isAiming"/> status.
        /// </summary>
        float GetAccuracyRecoveryRate(bool isAiming);

        /// <summary>
        /// Applies recoil with given parameters.
        /// </summary>
        /// <param name="baseRecoilIntensity">Base intensity multiplier of the recoil.</param>
        /// <param name="recoilProgression">How much is the recoil has progressed (with every shoot it increases).</param>
        /// <param name="isAiming">Is character currently aiming?</param>
        void Apply(float baseRecoilIntensity, float recoilProgression, bool isAiming);

        /// <summary>
        /// Sets multiplier to enhance the recoil effects.
        /// </summary>
        /// <param name="recoilMultiplier">New multiplier.</param>
        void SetRecoilMultiplier(float recoilMultiplier);
    }

    public sealed class NullGunRecoil : IGunRecoilBehaviour
    {
        public static readonly NullGunRecoil Instance = new();

        public float HeatRecoveryRate => 0f;
        public float HeatRecoveryDelay => 0f;
        public float HeatPerShot => 0f;
        public float RecoilMultiplier => 1f;

        public void Apply(float baseRecoilIntensity, float recoilProgression, bool isAiming) { }
        public float GetAccuracyKick(bool isAiming) => 0f;
        public float GetAccuracyRecoveryRate(bool isAiming) => 0f;
        public void SetRecoilMultiplier(float recoilMultiplier) { }
    }

    public delegate void GunHeatChangedDelegate(float previousHeat, float newHeat);

    // TODO: Add GunRecoilEffect classes to react to gun recoil and apply effects like disabling the gun when it is
    // heated to max value.
    public sealed class GunRecoilBehaviour :
        GunBehaviour,
        IGunRecoilBehaviour
    {
        [SerializeField]
        private RecoilProcessorConfig _recoilConfig;

        [Tooltip("How recoil strength changes with heat increasing.")]
        [SerializeField]
        private AnimationCurve _strengthOverHeat = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1f);

        [ReorderableList(ElementLabel = "Recoil")]
        [ReferencePicker(typeof(IGunRecoilStrategy), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private IGunRecoilStrategy[] _headRecoils = Array.Empty<IGunRecoilStrategy>();

        [ReorderableList(ElementLabel = "Recoil")]
        [ReferencePicker(typeof(IGunRecoilStrategy), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private IGunRecoilStrategy[] _handsRecoils = Array.Empty<IGunRecoilStrategy>();

        public float RecoilMultiplier { get; private set; } = 1f;

        public float HeatPerShot => _recoilConfig.HeatPerShot;
        public float HeatRecoveryRate => _recoilConfig.HeatRecoveryRate;
        public float HeatRecoveryDelay => _recoilConfig.HeatRecoveryDelay;

        public float GetAccuracyKick(bool isAiming) => _recoilConfig.GetAccuracyKick(isAiming);
        public float GetAccuracyRecoveryRate(bool isAiming) => _recoilConfig.GetAccuracyRecoveryRate(isAiming);
        public void SetRecoilMultiplier(float recoilMultiplier) => RecoilMultiplier = recoilMultiplier;

        private void OnEnable()
        {
            if(Gun != null)
            {
                Gun.RecoilMechanism = this;

                IHandheldMotionTargets motionTargets = Handheld.MotionTargets;
                foreach(var recoil in _headRecoils)
                {
                    recoil.Initialize(motionTargets.HeadMotion.MotionMixer, Handheld.Character);
                }

                foreach(var recoil in _handsRecoils)
                {
                    recoil.Initialize(motionTargets.HandsMotion.MotionMixer, Handheld.Character);
                }
            }
        }

        public void Apply(float baseRecoilIntensity, float recoilProgression, bool isAiming)
        {
            float heat = recoilProgression;
            float recoilStrength = Gun.TriggerMechanism.RecoilMultiplier * RecoilMultiplier * _strengthOverHeat.Evaluate(heat);

            foreach(IGunRecoilStrategy recoil in _handsRecoils)
            {
                recoil.Apply(recoilStrength, heat, isAiming);
            }

            foreach(IGunRecoilStrategy recoil in _headRecoils)
            {
                recoil.Apply(recoilStrength, heat, isAiming);
            }
        }

        [Serializable]
        private sealed class RecoilProcessorConfig
        {
            [Title("Heat")]
            [Tooltip("Heat of the gun increased by per shot.")]
            [SerializeField, Range(0f, 2f)]
            private float _heatPerShot = 1f;

            [Tooltip("How fast gun recovers the heat.")]
            [SerializeField, Range(0f, 10f)]
            private float _heatRecoveryRate;

            [Tooltip("Delay, before gun starts to recover heat.")]
            [SerializeField, Range(0f, 10f)]
            private float _heatRecoveryDelay;

            [Title("Accuracy Bloom")]
            [Tooltip("Accuracy kick resulted by each shot that is done without aiming.")]
            [SerializeField, Range(0f, 1f)]
            private float _hipfireAccuracyKick;

            [Tooltip("Recover rate of the accuracy while not aiming.")]
            [SerializeField, Range(0f, 10f)]
            private float _hipfireAccuracyRecoveryRate;

            [Tooltip("Accuracy kick resulted by each shot that is done with aiming.")]
            [SerializeField, Range(0f, 1f)]
            private float _aimAccuracyKick;

            [Tooltip("Recover rate of the accuracy while aiming.")]
            [SerializeField, Range(0f, 10f)]
            private float _aimAccuracyRecoveryRate;

            /// <summary>
            /// Heat of the gun increased by per shot.
            /// </summary>
            public float HeatPerShot => _heatPerShot;

            /// <summary>
            /// How fast gun recovers the heat.
            /// </summary>
            public float HeatRecoveryRate => _heatRecoveryRate;

            /// <summary>
            /// Delay, before gun starts to recover heat.
            /// </summary>
            public float HeatRecoveryDelay => _heatRecoveryDelay;

            /// <summary>
            /// Accuracy kick resulted by each shot that is done without aiming.
            /// </summary>
            public float HipfireAccuracyKick => _hipfireAccuracyKick;

            /// <summary>
            /// Recover rate of the accuracy while not aiming.
            /// </summary>
            public float HipfireAccuracyRecoveryRate => _hipfireAccuracyRecoveryRate;

            /// <summary>
            /// Accuracy kick resulted by each shot that is done with aiming.
            /// </summary>
            public float AimAccuracyKick => _aimAccuracyKick;

            /// <summary>
            /// Recover rate of the accuracy while aiming.
            /// </summary>
            public float AimAccuracyRecoveryRate => _aimAccuracyRecoveryRate;

            public float GetAccuracyKick(bool isAiming) => isAiming ? AimAccuracyKick : HipfireAccuracyKick;
            public float GetAccuracyRecoveryRate(bool isAiming) => isAiming ? AimAccuracyRecoveryRate : HipfireAccuracyRecoveryRate;
        }
    }
}