using System;
using UnityEngine;

namespace Nexora.Audio
{
    /// <summary>
    /// Audio source wrapper, that allows audio source elements to be modulated,
    /// while played. It has optional fade dynamics, functionality of which is
    /// already built-in in the abstract class. 
    /// <br></br>There is also noise that can be used to make audio parameters more natural.
    /// </summary>
    /// <remarks>
    /// It is always playing when the object is active, updates are made through
    /// <see cref="FixedUpdate"/> to have time-independent updates.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public abstract class ModulatedAudioSource : MonoBehaviour
    {
        private const float FadeDisableEpsilon = 0.0001f;

        [SerializeField]
        [HideInInspector]
        protected AudioSource _audioSource;

        [SerializeField]
        [Range(0f, 10f)]
        private float _fadeInDuration = 0.5f;

        [SerializeField]
        [Range(0f, 10f)]
        private float _fadeOutDuration = 0.5f;

        [SerializeField, SpaceArea]
        protected Noise _noise;

        /// <summary>
        /// 0-1. 1 for playing, 0 for stopped state.
        /// </summary>
        protected float _fadeProgress;

        private bool _isPlaying;

        /// <summary>
        /// Can be modified to give multiplier from the outside class that uses this object.
        /// </summary>
        public float ExternalParameterMultiplier { get; set; } = 1f;

        private void OnEnable()
        {
            _audioSource.enabled = true;
            _audioSource.Play();
        }

        private void OnDisable()
        {
            _audioSource.Stop();
            _audioSource.enabled = false;
        }

        public void Play(bool fadeIn = true)
        {
            enabled = true;
            _isPlaying = true;

            _audioSource.Play();

            if(fadeIn == false)
            {
                _fadeProgress = 1f;
            }
        }

        public void Stop(bool fadeOut = true)
        {
            _isPlaying = false;

            if(fadeOut == false)
            {
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            UpdateFadeProgress(Time.fixedDeltaTime);
            UpdateAudioParameters(Time.fixedDeltaTime);

            if(_fadeProgress < FadeDisableEpsilon)
            {
                enabled = false;
            }
        }

        /// <summary>
        /// Updates the fade progress to 0 or to 1, depending on source is playing or not.
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdateFadeProgress(float deltaTime)
        {
            float targetFadeProgress = _isPlaying ? 1f : 0f;
            float fadeDuration = _isPlaying ? _fadeInDuration : _fadeOutDuration;
            float fadeDelta = deltaTime * (1f / fadeDuration);

            _fadeProgress = Mathf.MoveTowards(_fadeProgress, targetFadeProgress, fadeDelta);
        }

        /// <summary>
        /// Main method for modulation. Override this method to have your parameter/parameters
        /// of choice to have operations on them. E.g might use volume to fade
        /// </summary>
        protected abstract void UpdateAudioParameters(float deltaTime);

        /// <summary>
        /// Struct to define noise that can be used for making audio more natural to feel.
        /// </summary>
        [Serializable]
        protected struct Noise
        {
            public bool Enabled;

            [Range(0f, 1f)]
            public float Intensity;

            [Range(0f, 1f)]
            public float Speed;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _audioSource = gameObject.GetOrAddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = true;
            _audioSource.volume = 0f;
            _audioSource.spatialBlend = 1f;
            _audioSource.minDistance = 1f;
            _audioSource.maxDistance = 10f;
        }

        protected virtual void Validate()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
            _audioSource.enabled = enabled;
        }
#endif
    }
}
