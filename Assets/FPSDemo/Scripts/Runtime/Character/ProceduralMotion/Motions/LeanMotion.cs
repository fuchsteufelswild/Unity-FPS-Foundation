using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    public enum LeanState
    {
        Left = -1,
        Center = 0,
        Right = 1
    }

    [DisallowMultipleComponent]
    public sealed class LeanMotion : CharacterMotion
    {
        [SerializeField, NotNull]
        private AdditiveMotionApplier _motionApplier;

        [SerializeField]
        private LeanData _data = new();

        private float _maxLeanPercent = 1f;
        private LeanState _currentLeanState = LeanState.Center;

        protected override SpringSettings DefaultPositionSpringSettings => _data.PositionSpring;
        protected override SpringSettings DefaultRotationSpringSettings => _data.RotationSpring;

        public LeanData Data => _data;

        public float MaxLeanPercent
        {
            get => _maxLeanPercent;
            set => Mathf.Clamp01(_maxLeanPercent);
        }

        protected override void Awake()
        {
            base.Awake();
            IgnoreMixerBlendWeight = true;
            
        }

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            if (_motionApplier == null)
            {
                _motionApplier = Mixer.GetMotion<AdditiveMotionApplier>();

            }
        }

        public void SetLeanState(LeanState leanState)
        {
            if(_currentLeanState == leanState)
            {
                return;
            }

            ApplyLeanForces(leanState);

            _currentLeanState = leanState;
            UpdateTargetOffsets();
        }

        private void ApplyLeanForces(LeanState newState)
        {
            float forceFactor = (newState == LeanState.Center ? 0.5f : 1f);

            _motionApplier.SetCustomSpringSettings(_data.PositionSpring, _data.RotationSpring);
            _motionApplier.AddPositionForce(_data.PositionEnterForce, forceFactor, SpringType.Custom);
            _motionApplier.AddRotationForce(_data.RotationEnterForce, forceFactor, SpringType.Custom);
        }

        public override void Tick(float deltaTime)
        {
            if(_currentLeanState == LeanState.Center)
            {
                return;
            }

            UpdateTargetOffsets();
        }

        private void UpdateTargetOffsets()
        {
            var (targetPosition, targetRotation) = LeanMotionCalculator.Calculate(_currentLeanState, _maxLeanPercent, _data);

            SetTargetPosition(targetPosition);
            SetTargetRotation(targetRotation);
        }


        [Serializable]
        public sealed class LeanData
        {
            [Range(-90f, 90f)]
            public float LeanAngle = 15f;

            [Range(-3f, 3f)]
            public float LeanSideOffset = 0.4f;

            [Range(0f, 10f)]
            public float LeanHeightOffset = 0.25f;

            [SpaceArea]
            public SpringSettings PositionSpring = SpringSettings.Default;
            public SpringSettings RotationSpring = SpringSettings.Default;

            [SpaceArea]
            [Tooltip("Force applied on position when changing lean states.")]
            public SpringImpulseDefinition PositionEnterForce;
            [Tooltip("Force applied on rotation when changing lean states.")]
            public SpringImpulseDefinition RotationEnterForce;
        }
    }

    public static class LeanMotionCalculator
    {
        public static (Vector3 position, Vector3 rotation) Calculate(LeanState leanState, float leanPercent, LeanMotion.LeanData data)
        {
            float direction = (float)leanState;

            Vector3 targetPosition = new()
            {
                x = data.LeanSideOffset * direction * leanPercent,
                y = -data.LeanHeightOffset * Mathf.Abs(direction) * leanPercent,
                z = 0f
            };

            Vector3 targetRotation = new()
            {
                x = 0f,
                y = 0f,
                z = -data.LeanAngle * direction * leanPercent,
            };

            return (targetPosition, targetRotation);
        }
    }
}