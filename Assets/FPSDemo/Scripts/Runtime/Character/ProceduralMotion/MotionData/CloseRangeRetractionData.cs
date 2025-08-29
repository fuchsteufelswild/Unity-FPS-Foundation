using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    [Serializable]
    public sealed class CloseRangeRetractionData : IMotionData
    {
        [Range(0.05f, 10f)]
        public float RetractionDistance;

        public Vector3 PositionOffset;
        public Vector3 RotationOffset;

        [SpaceArea]
        public SpringSettings PositionSpring;
        public SpringSettings RotationSpring;
    }
}