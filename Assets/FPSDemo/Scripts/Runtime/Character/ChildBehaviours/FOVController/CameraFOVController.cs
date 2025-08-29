using Nexora.Experimental.Tweening;
using Nexora.FPSDemo.Options;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    public interface ICameraFOVController
    {
        /// <summary>
        /// Current Field of View of the camera.
        /// </summary>
        float CurrentFOV { get; }

        /// <summary>
        /// Base multiplier set to effect all FOV calculations.
        /// </summary>
        float BaseMultiplier { get; }

        /// <summary>
        /// Sets base multiplier as <paramref name="multiplier"/> and restarts FOV animation
        /// using <paramref name="config"/> for a new value.
        /// </summary>
        /// <param name="config">Animation configuration to use when restarting animation.</param>
        void SetBaseMultiplier(float multiplier, in FOVAnimationConfig config);

        /// <summary>
        /// Sets dynamic multiplier as <paramref name="multiplier"/>.
        /// </summary>
        void SetDynamicMultiplier(float multiplier);

        /// <summary>
        /// Main method. Updates the camera FOV by combining two parts. Calculates current value of 
        /// the base FOV animation and applies optional dynamic multiplier onto it.
        /// </summary>
        void UpdateFOV();
    }

    /// <summary>
    /// Manages calculating the current FOV(Field of View) of the camera passed. Can set base multiplier 
    /// and dynamic multiplier.
    /// <br></br>
    /// Works mainly like this: <br></br>
    /// Always tries to moves slowly to the FOV specified in the <see cref="GraphicsOptions"/>, and uses base multiplier
    /// to scale its value. Another value of <b>dynamic multiplier</b> is used, that is for additional multiplier
    /// that can be used depending on states (aiming maybe). It is also slowly moves to target if you want to change it.
    /// </summary>
    public sealed class CameraFOVController : 
        ICameraFOVController,
        IDisposable
    {
        private const float DynamicMultiplierLerpSpeed = 3f;

        private readonly Camera _camera;

        private Tween<float> _baseFOVTween;

        private float _baseMultiplier = 1f;
        private float _currentDynamicMultiplier = 1f;
        private float _targetDynamicMultiplier = 1f;

        public float CurrentFOV => _camera.fieldOfView;
        public float BaseMultiplier => _baseMultiplier;

        public CameraFOVController(Camera camera, Ease easeType)
        {
            _camera = camera;

            InitializeTween(easeType);
            GraphicsOptions.Instance.FieldOfView.OnValueChanged += OnBaseFOVSettingChanged;
        }

        private void InitializeTween(Ease easeType)
        {
            float initialFOV = GraphicsOptions.Instance.FieldOfView;
            _camera.fieldOfView = initialFOV;

            _baseFOVTween = initialFOV.CreateAndStartTween(initialFOV, 0f, null)
                .SetEase(easeType)
                .SetUnscaledTime(true)
                .SetAutoRelease(false)
                .Stop();
        }

        public void Dispose()
        {
            _baseFOVTween?.Release();
            GraphicsOptions.Instance.FieldOfView.OnValueChanged -= OnBaseFOVSettingChanged;
        }

        private void OnBaseFOVSettingChanged(float newFOV)
        {
            float targetFOV = newFOV * _baseMultiplier;
            _baseFOVTween.SetEndValue(targetFOV)
                .SetDuration(1f)
                .Restart();
        }

        public void SetBaseMultiplier(float multiplier, in FOVAnimationConfig config)
        {
            _baseMultiplier = multiplier;
            float targetFOV = GraphicsOptions.Instance.FieldOfView.Value * multiplier;

            _baseFOVTween.SetEndValue(targetFOV)
                .SetDuration(config.Duration)
                .SetDelay(config.Delay)
                .SetEase(config.EaseType)
                .Restart();
        }

        public void SetDynamicMultiplier(float multiplier) => _targetDynamicMultiplier = multiplier;

        public void UpdateFOV()
        {
            _currentDynamicMultiplier = Mathf.Lerp(
                _currentDynamicMultiplier,
                _targetDynamicMultiplier,
                Time.deltaTime * DynamicMultiplierLerpSpeed);

            float baseFOV = _baseFOVTween.GetCurrentValue();
            _camera.fieldOfView = baseFOV * _currentDynamicMultiplier;
        }
    }
}