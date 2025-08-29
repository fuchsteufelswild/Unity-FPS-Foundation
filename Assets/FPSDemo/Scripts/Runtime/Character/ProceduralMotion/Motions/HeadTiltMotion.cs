using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    [RequireCharacterBehaviour(typeof(ILookController))]
    public sealed class HeadTiltMotion : CharacterMotion
    {
        [SerializeField]
        private HeadTiltData _data = new();

        private ILookController _lookController;

        protected override SpringSettings DefaultPositionSpringSettings => _data.PositionSpring;
        protected override SpringSettings DefaultRotationSpringSettings => _data.RotationSpring;

        protected override void OnBehaviourStart(ICharacter parent) => _lookController = parent.GetCC<ILookController>();

        public override void Tick(float deltaTime)
        {
            if(_data == null || _lookController == null)
            {
                return;
            }

            float verticalAngle = _lookController.ViewAngles.x;

            var (targetPosition, targetRotation) = HeadTiltCalculator.Calculate(verticalAngle, _data);

            SetTargetPosition(targetPosition);
            SetTargetRotation(targetRotation);
        }

        [Serializable]
        public sealed class HeadTiltData
        {
            [Tooltip("The vertical look angle at which the tilt effect starts.")]
            public float AngleThreshold = 30f;

            [Tooltip("The vertical look angle at which the tilt effect is the strongest.")]
            public float MaxEffectiveAngle = 70f;

            [Tooltip("The positional offset applied when the tilt effect is at the strongest.")]
            public Vector3 PositionOffset = new(0.01f, -0.02f, 0f);

            [Tooltip("The rotational offset applied when the tilt effect is at the strongest.")]
            public Vector3 RotationOffset = new(2f, -0.5f, -2f);

            [SpaceArea]
            public SpringSettings PositionSpring = SpringSettings.Default;
            public SpringSettings RotationSpring = SpringSettings.Default;
        }
    }

    public static class HeadTiltCalculator
    {
        public static (Vector3 position, Vector3 rotation) Calculate(float verticalLookAngle, HeadTiltMotion.HeadTiltData data)
        {
            float angle = Mathf.Abs(verticalLookAngle);

            if(angle <= data.AngleThreshold)
            {
                return (Vector3.zero, Vector3.zero);
            }

            float range = data.MaxEffectiveAngle - data.AngleThreshold;
            if(range <= 0.001f)
            {
                return (data.PositionOffset * 0.01f, data.RotationOffset);
            }

            float progress = Mathf.Clamp01((angle - data.AngleThreshold) / range);
            float factor = Easing.Evaluate(Ease.CubicOut, progress);

            Vector3 targetPosition = data.PositionOffset * factor * 0.01f;
            Vector3 targetRotation = data.RotationOffset * factor;

            return (targetPosition, targetRotation);
        }
    }
}