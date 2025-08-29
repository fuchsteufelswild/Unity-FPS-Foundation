using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    public enum BobCalculationMethod
    {
        [Tooltip("Bob motion is calculated using step cycles, at each step cycle motion will be inversed. " +
            "Best for scenarios where depends on character steps.")]
        StepCycleBased,

        [Tooltip("Bob motion is calculated using time, that is a continuos bob that is alternating.")]
        TimeBased
    }

    [Serializable]
    public sealed class BobData : IMotionData
    {
        public BobCalculationMethod CalculationMethod;

        [Range(0.01f, 10f)]
        [ShowIf(nameof(CalculationMethod), BobCalculationMethod.TimeBased)]
        public float TimeBasedBobSpeed = 1f;

        [SpaceArea]
        public SpringSettings PositionSpring = SpringSettings.Default;

        [Tooltip("Force applied to the position, in each step on the ground.")]
        public SpringImpulseDefinition PositionStepImpulseForce = SpringImpulseDefinition.Default;

        [Tooltip("Amplitude of the position's continuous bob.")]
        public Vector3 PositionAmplitude = Vector3.zero;

        [SpaceArea]
        public SpringSettings RotationSpring = SpringSettings.Default;

        [Tooltip("Force applied to the rotation, in each step on the ground.")]
        public SpringImpulseDefinition RotationStepImpulseForce = SpringImpulseDefinition.Default;

        [Tooltip("Amplitude of the rotation's continuous bob.")]
        public Vector3 RotationAmplitude = Vector3.zero;
    }
}