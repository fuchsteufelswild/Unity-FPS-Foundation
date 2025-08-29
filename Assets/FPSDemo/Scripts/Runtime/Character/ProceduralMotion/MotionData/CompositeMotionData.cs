using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{

    /// <summary>
    /// Composite motion data that is made up of different data. It stores the main motions that would be
    /// needed by nearly all weapons/situations (like jump, look, fall etc.)
    /// </summary>
    [Serializable]
    public sealed class CompositeMotionData : IMotionData
    {
        [SpaceArea]
        public SingleData Fall;

        [SpaceArea]
        public SwayData Look;

        [SpaceArea]
        public SwayData Strafe;

        [SpaceArea]
        public CurveData Jump;

        [SpaceArea]
        public CurveData Landing;
    }

    [Serializable]
    public sealed class SingleData : IMotionData
    {
        public SpringSettings PositionSpring = SpringSettings.Default;
        public SpringSettings RotationSpring = SpringSettings.Default;

        [Range(0f, 100f)]
        public float PositionValue;

        [Range(0f, 100f)]
        public float RotationValue;
    }

    [Serializable]
    public sealed class SwayData : IMotionData
    {
        [Tooltip("Determines the maximum amount of sway can be made from the center.")]
        [Range(0f, 200f)]
        public float MaxSway = 10f;

        public SpringSettings PositionSpring = SpringSettings.Default;
        public SpringSettings RotationSpring = SpringSettings.Default;

        public Vector3 PositionSway;
        public Vector3 RotationSway;
    }

    [Serializable]
    public sealed class CurveData : IMotionData
    {
        public SpringSettings PositionSpring = SpringSettings.Default;
        public SpringSettings RotationSpring = SpringSettings.Default;

        public AnimationCurve3D PositionCurves;
        public AnimationCurve3D RotationCurves;
    }
}