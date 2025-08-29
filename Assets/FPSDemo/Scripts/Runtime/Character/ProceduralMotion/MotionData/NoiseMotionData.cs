using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    [Serializable]
    public sealed class NoiseMotionData : IMotionData
    {
        [Range(0f, 10f)]
        public float NoiseSpeed = 2f;

        [Range(0f, 1f)]
        public float Jitter = 0.5f;

        public Vector3 PositionAmplitude;
        public Vector3 RotationAmplitude;
    }
}