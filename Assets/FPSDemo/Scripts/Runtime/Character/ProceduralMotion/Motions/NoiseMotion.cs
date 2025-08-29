using Nexora.Motion;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    /// <summary>
    /// This class doesn't use <see cref="SpringSettings"/> by motion-wise, it just 
    /// defines the settings specifically for an object and uses it that way.
    /// </summary>
    public sealed class NoiseMotion : CharacterDataMotion<NoiseMotionData>
    {
        private const float JitterValidityThreshold = 0.01f;

        [SerializeField]
        private SpringSettings _positionSpringSettings = new(10f, 100f, 1f);

        [SerializeField]
        private SpringSettings _rotationSpringSettings = new(10f, 100f, 1f);

        protected override SpringSettings DefaultPositionSpringSettings => _positionSpringSettings;
        protected override SpringSettings DefaultRotationSpringSettings => _rotationSpringSettings;

        public override void Tick(float deltaTime)
        {
            if(CurrentMotionData == null)
            {
                return;
            }

            float jitter = CurrentMotionData.Jitter < JitterValidityThreshold 
                ? 0f 
                : UnityEngine.Random.Range(0f, CurrentMotionData.Jitter);

            var (targetPosition, targetRotation) = NoiseGenerator.Calculate(CurrentMotionData, Time.time, jitter);

            SetTargetPosition(targetPosition);
            SetTargetRotation(targetRotation);
        }
    }

    public static class NoiseGenerator
    {
        public static (Vector3 position, Vector3 rotation) Calculate(NoiseMotionData noiseData, float currentTime, float jitter)
        {
            float speed = currentTime * noiseData.NoiseSpeed;

            Vector3 baseNoise = GenerateBaseNoise(jitter, speed);

            Vector3 positionNoise = Vector3.Scale(baseNoise, noiseData.PositionAmplitude);
            Vector3 rotationNoise = Vector3.Scale(baseNoise, noiseData.RotationAmplitude);

            return (positionNoise, rotationNoise);
        }

        /// <summary>
        /// Generates a 3D noise using 2D Perlin noise for each of the axis.
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private static Vector3 GenerateBaseNoise(float seed, float time)
        {
            // Perlin noise returns a value between 0 and 1, by substracting 0.5f
            // we center it around '0'.
            // x, y, z are different, but correlated due to same 'time'
            return new Vector3
            {
                x = Mathf.PerlinNoise(seed, time) - 0.5f,
                y = Mathf.PerlinNoise(seed + 1f, time) - 0.5f,
                z = Mathf.PerlinNoise(seed + 2f, time) - 0.5f
            };
        }
    }
}