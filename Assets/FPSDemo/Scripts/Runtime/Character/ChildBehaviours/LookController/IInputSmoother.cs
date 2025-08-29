using Nexora.FPSDemo.Options;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    public interface IInputSmoother
    {
        /// <summary>
        /// Smoothes <paramref name="input"/> using smoothness defined in settings.
        /// </summary>
        /// <returns>Smoothed <paramref name="input"/>.</returns>
        Vector2 SmoothInput(Vector2 input);

        /// <summary>
        /// Resets the inputs.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Applies exponential smoothing (exponentially weighted moving average),
    /// to smooth input, considering previous inputs up to some amount.
    /// </summary>
    /// <remarks>
    /// Last added input takes more weight.
    /// </remarks>
    public sealed class ExponentialInputSmoother : IInputSmoother
    {
        private const float SmoothnessValidThreshold = 0.01f;
        private const int DefaultMaxStepsCount = 8;

        private readonly List<Vector2> _inputBuffer = new();
        private int _maxSteps;

        public ExponentialInputSmoother(int maxSteps = DefaultMaxStepsCount) => UpdateMaxSteps(maxSteps);
        public void UpdateMaxSteps(int maxSteps = DefaultMaxStepsCount) => _maxSteps = Mathf.Clamp(maxSteps, 1, 20);
        public void Reset() => _inputBuffer.Clear();

        public Vector2 SmoothInput(Vector2 input)
        {
            float smoothness = InputOptions.Instance.MouseSmoothness;

            if(smoothness < SmoothnessValidThreshold)
            {
                return input;
            }

            // Remove excess inputs
            while(_inputBuffer.Count >= _maxSteps)
            {
                _inputBuffer.RemoveAt(0);
            }

            _inputBuffer.Add(input);

            return CalculateSmoothedInput(smoothness);
        }

        /// <summary>
        /// Apply exponential smoothing, so that the last input has the most influence,
        /// while retaining earlier inputs that have lesser and lesser influence.
        /// </summary>
        /// <param name="smoothness">Smoothness value.</param>
        /// <returns>Smoothed composite input value.</returns>
        private Vector2 CalculateSmoothedInput(float smoothness)
        {
            Vector2 smoothedInput = Vector2.zero;
            float totalWeight = 0f;
            float weight = 1f;

            for(int i = _inputBuffer.Count - 1; i >= 0; i--)
            {
                smoothedInput += _inputBuffer[i] * weight;
                totalWeight += weight;
                weight *= smoothness;
            }

            return totalWeight > 0f
                ? smoothedInput / totalWeight
                : Vector2.zero;
        }
    }
}