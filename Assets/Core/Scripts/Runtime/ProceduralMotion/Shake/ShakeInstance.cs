using System;
using UnityEngine;

namespace Nexora.Motion
{
    [Serializable]
    public sealed class ShakeInstance
    {
        public ShakeProfile ShakeProfile;

        [Clamp(0f, 8f)]
        public float Duration;

        public float Intensity;

        public bool IsPlayable => Duration > 0.001f;
    }
}