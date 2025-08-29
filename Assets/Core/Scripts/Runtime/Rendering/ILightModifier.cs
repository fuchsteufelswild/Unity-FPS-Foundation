using System;
using UnityEngine;

namespace Nexora.Rendering
{
    /// <summary>
    /// Modifies the parameters of <see cref="Light"/>.
    /// </summary>
    public interface ILightModifier
    {
        /// <summary>
        /// Takes in <paramref name="current"/> modifies it then returns.
        /// </summary>
        /// <param name="current">Current light properties.</param>
        /// <returns>Modified <paramref name="current"/>.</returns>
        LightProperties Apply(LightProperties current, float deltaTime);

        /// <summary>
        /// Called when the light starts to play.
        /// </summary>
        void OnPlay(bool fadeIn);

        /// <summary>
        /// Called when the light stops playing.
        /// </summary>
        void OnStop(bool fadeOut);
    }

    /// <summary>
    /// Fades in/out the light properties.
    /// </summary>
    [Serializable]
    public sealed class FadeLightModifier : ILightModifier
    {
        [SerializeField, Range(0f, 5f)]
        private float _fadeInDuration = 0.5f;

        [SerializeField, Range(0f, 5f)]
        private float _fadeOutDuration = 0.1f;

        private float _weight;

        public void OnPlay(bool fadeIn) => _weight = fadeIn ? 1f : _weight;
        public void OnStop(bool fadeOut) { }

        public LightProperties Apply(LightProperties current, float deltaTime)
        {
            bool isOn = current.IsOn;

            float targetWeight = isOn ? 1f : 0f;
            float fadeDuration = isOn ? _fadeInDuration : _fadeOutDuration;
            float fadeDelta = fadeDuration > 0f ? deltaTime / fadeDuration : 1f;

            _weight = Mathf.MoveTowards(_weight, targetWeight, fadeDelta);

            current.MultiplyWith(_weight);

            return current;
        }
    }

    /// <summary>
    /// Makes a S-curve pulse effect to the properties.
    /// </summary>
    [Serializable]
    public sealed class PulseLightModifier : ILightModifier
    {
        private enum PlayMode
        {
            Once,
            Loop
        }

        [Tooltip("Is pulse enabled?")]
        [SerializeField]
        private bool _isEnabled;

        [Tooltip("Play pulse once, or loop?")]
        [SerializeField]
        private PlayMode _playMode;

        [SerializeField, Range(0f, 100f)]
        private float _pulseDuration;

        [SerializeField]
        private Color _pulseColor;

        [Tooltip("Scaler to intensity.")]
        [SerializeField, Range(0f, 1f)]
        private float _intensityScale;

        [Tooltip("Scaler to range.")]
        [SerializeField, Range(0f, 1f)]
        private float _rangeScale;

        [Tooltip("How fast color should change towards pulse color?")]
        [SerializeField, Range(0f, 1f)]
        private float _pulseColorWeight;

        private float _pulseTimer;

        /// <summary>
        /// Not to confuse with 'isEnabled', this parameter defines if the effect is active now,
        /// like it might play once and then stops.
        /// <br></br>
        /// Enable, on the other hand, denotes this effect can be played at all.
        /// </summary>
        private bool _isActive;

        public void OnPlay(bool fadeIn)
        {
            _pulseTimer = 0f;
            _isActive = true;
        }

        public void OnStop(bool fadeOut) => _isActive = false;

        public LightProperties Apply(LightProperties current, float deltaTime)
        {
            if (_isEnabled == false || _isActive == false)
            {
                return current;
            }

            // Normalizing the timer into [0-1] in relation with 'pulse duration',
            // [0 - 'pulseDuratoin'] -> [0 - 1]
            // Using '0.001f' as to prevent dividing '0'.
            float normalizedTime = _pulseTimer / Mathf.Max(_pulseDuration, 0.001f);
            float pulseFactor = CalculateSmoothPulse(normalizedTime);

            current.Intensity *= pulseFactor * _intensityScale;
            current.Intensity *= pulseFactor * _rangeScale;
            current.Color = Color.Lerp(current.Color, _pulseColor, _pulseColorWeight * deltaTime);

            _pulseTimer += deltaTime;

            if (_pulseTimer > _pulseDuration)
            {
                HandlePulseCompletion();
            }

            current.IsOn |= _isActive;

            return current;
        }

        /// <summary>
        /// Smooth S-curve using a Sine wave; 0 -> 1 -> 0 -> 1 ... 
        /// </summary>
        /// <param name="normalizedTime">Time -> [0,1].</param>
        private float CalculateSmoothPulse(float normalizedTime)
        {
            float sineInput = Mathf.PI * (2f * normalizedTime - 0.5f);
            return (Mathf.Sin(sineInput) + 1f) * 0.5f;
        }

        private void HandlePulseCompletion()
        {
            if (_playMode == PlayMode.Once)
            {
                _isActive = false;
            }

            // Reset for looping
            _pulseTimer = 0f;
        }
    }

    /// <summary>
    /// Adds a noise to the properties.
    /// </summary>
    [Serializable]
    public sealed class NoiseLightModifier : ILightModifier
    {
        [SerializeField]
        private bool _isEnabled;

        [SerializeField, Range(0f, 1f)]
        private float _intensity;

        [SerializeField, Range(0f, 50f)]
        private float _noiseSpeed;

        public LightProperties Apply(LightProperties current, float deltaTime)
        {
            if (_isEnabled == false)
            {
                return current;
            }

            float noise = Mathf.PerlinNoise(Time.time * _noiseSpeed, 0f) * _intensity;
            current.Add(noise);

            return current;
        }

        public void OnPlay(bool fadeIn) { }

        public void OnStop(bool fadeOut) { }
    }
}