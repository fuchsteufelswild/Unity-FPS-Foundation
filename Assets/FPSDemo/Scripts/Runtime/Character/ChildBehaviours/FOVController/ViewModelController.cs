using Nexora.Experimental.Tweening;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    public readonly struct ViewModelShaderConfig
    {
        public readonly string FOVEnabledProperty;
        public readonly string FOVValueProperty;

        public ViewModelShaderConfig(string fOVEnabledProperty, string fOVValueProperty)
        {
            FOVEnabledProperty = fOVEnabledProperty;
            FOVValueProperty = fOVValueProperty;
        }
    }

    public interface IViewModelController
    {
        /// <summary>
        /// Current FOV view model.
        /// </summary>
        float CurrentFOV { get; }

        /// <summary>
        /// Current local scale size of the view model (uniform size, same in all axis).
        /// </summary>
        float CurrentSize { get; }

        /// <summary>
        /// Sets the target view model FOV to be <paramref name="fov"/> and restarts to animation
        /// to reach that value.
        /// </summary>
        /// <param name="fov">Target FOV value.</param>
        /// <param name="animationConfig">Animation configuration for the tween.</param>
        void SetFOV(float fov, in FOVAnimationConfig animationConfig);

        /// <summary>
        /// Sets view model size.
        /// </summary>
        void SetSize(float size);

        /// <summary>
        /// Updates shader's values with given view model FOV value.
        /// </summary>
        void UpdateShaderProperties();
    }

    /// <summary>
    /// Controls the size and FOV of the view model. Always tries to slowly move to the 
    /// value set by '<see cref="SetFOV(float, in FOVAnimationConfig)"/>'. Updates the 
    /// shader values to enable view model FOV and to update its value using the tween's 
    /// value.
    /// </summary>
    public sealed class ViewModelController : 
        IViewModelController,
        IDisposable
    {
        private readonly Transform _transform;
        private readonly ViewModelShaderConfig _shaderConfig;
        private readonly int _fovValuePropertyID;

        private Tween<float> _fovTween;
        private float _baseFOV;

        public float CurrentFOV => _fovTween.GetCurrentValue();
        public float CurrentSize => _transform.localScale.x;

        public ViewModelController(Transform transform, in ViewModelShaderConfig shaderConfig, float baseFOV, Ease easeType)
        {
            _transform = transform;
            _shaderConfig = shaderConfig;
            _fovValuePropertyID = Shader.PropertyToID(shaderConfig.FOVValueProperty);
            _baseFOV = baseFOV;

            InitializeTween(easeType);
            Shader.SetGlobalFloat(_shaderConfig.FOVEnabledProperty, 1f);
        }

        private void InitializeTween(Ease easeType)
        {
            _fovTween = _baseFOV.CreateAndStartTween(_baseFOV, 0f, null)
                .SetUnscaledTime(true)
                .SetAutoRelease(false)
                .SetEase(easeType)
                .Stop();
        }

        public void SetFOV(float fov, in FOVAnimationConfig animationConfig)
        {
            _fovTween.SetEndValue(fov)
                .SetDuration(animationConfig.Duration)
                .SetDelay(animationConfig.Delay)
                .SetEase(animationConfig.EaseType)
                .Restart();
        }

        public void SetSize(float size) => _transform.localScale = Vector3.one * size;
        public void UpdateShaderProperties() => Shader.SetGlobalFloat(_fovValuePropertyID, CurrentFOV);

        /// <summary>
        /// Updates the initial configuration, <b>base FOV</b>.
        /// </summary>
        /// <param name="baseFOV">New base fov value.</param>
        /// <param name="easeType">New ease type.</param>
        public void UpdateConfiguration(float baseFOV, Ease easeType)
        {
            _baseFOV = baseFOV;
            _fovTween?.SetEase(easeType).SetStartValue(baseFOV);
        }

        public void Dispose()
        {
            _fovTween?.Release();
            Shader.SetGlobalFloat(_shaderConfig.FOVEnabledProperty, 0f);
        }
    }
}