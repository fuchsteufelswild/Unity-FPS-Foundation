using Nexora.FPSDemo.Options;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    public interface ISensitivityCalculator
    {
        /// <summary>
        /// Calculates the current sensitivity value using the options.
        /// </summary>
        /// <returns>Sensitivity.</returns>
        float CalculateSensitivity(float deltaTime);
    }

    public sealed class FOVCompensatedSensitivityCalculator : ISensitivityCalculator
    {
        private const float SensitivityMultiplier = 0.05f;
        private const float LerpSpeed = 5f;

        private readonly Camera _camera;

        private float _currentSensitivity;

        public FOVCompensatedSensitivityCalculator(Camera camera) => _camera = camera;

        public float CalculateSensitivity(float deltaTime)
        {
            float targetSensitivity = CalculateTargetSensitivity();
            _currentSensitivity = Mathf.Lerp(_currentSensitivity, targetSensitivity, LerpSpeed);
            return _currentSensitivity * SensitivityMultiplier;
        }

        private float CalculateTargetSensitivity()
        {
            float baseSensitivity = InputOptions.Instance.MouseSensitivity;

            if(_camera != null)
            {
                float fovRatio = _camera.fieldOfView / GraphicsOptions.Instance.FieldOfView.Value;
                return baseSensitivity * fovRatio;
            }

            return baseSensitivity;
        }
    }
}