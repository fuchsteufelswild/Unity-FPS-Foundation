using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.Motion
{
    /// <summary>
    /// Interface for handling shakes upon receiving, deriving from this class 
    /// means that the class will be listening to shake sources.
    /// </summary>
    public interface IShakeHandler : IMonoBehaviour
    {
        /// <summary>
        /// Called from a source that emits signal to inform a shake,
        /// with data that defines shake and an optional intensity multiplier.
        /// </summary>
        void AddShake(ShakeInstance shakeInstance, float intensity = 1f);
    }
}

namespace Nexora
{
    /// <summary>
    /// Represents a data for the shake. Defines speed, amplitude for the shake and a spring for the shake decay.
    /// </summary>
    [Serializable]
    public struct ShakeDefinition
    {
        [Title("Amplitude")]
        [Tooltip("Amplitude of the shake in the x axis. (Horizontal Shake)")]
        [Range(0, 100f)]
        public float AmplitudeX;

        [Tooltip("Amplitude of the shake in the y axis. (Vertical Shake)")]
        [Range(0, 100f)]
        public float AmplitudeY;

        [Tooltip("Amplitude of the shake in the z axis. (Depth shake)")]
        [Range(0, 100f)]
        public float AmplitudeZ;

        [Title("Speed")]
        [Range(0, 100f)]
        public float ShakeSpeed;

        [Title("Spring Settings")]
        public SpringSettings Spring;

        /// <summary>
        /// Default shake with a general spring along with speed of 20 and no amplitude on any axis.
        /// </summary>
        public static readonly ShakeDefinition Default =
            new()
            {
                AmplitudeX = 0,
                AmplitudeY = 0,
                AmplitudeZ = 0,
                ShakeSpeed = 20f,
                Spring = SpringSettings.Default
            };
    }
}
