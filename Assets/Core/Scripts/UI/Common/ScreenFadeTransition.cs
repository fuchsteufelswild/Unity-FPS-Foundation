using Nexora.Experimental.Tweening;
using Nexora.Motion;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Nexora.UI
{
    /// <summary>
    /// Interface for sibling components that might listen to fade in/out events,
    /// e.g loading screen with special functionalities might perform when fade in 
    /// action completes.
    /// </summary>
    public interface IScreenFadeListener
    {
        void OnFadeInStarted();
        void OnFadeInCompleted();
        void OnFadeOutStarted();
        void OnFadeOutCompleted();
    }

    /// <summary>
    /// Handles screen fade animations with <see cref="AudioMixerSnapshot"/> transitions for
    /// better handling of audio between scene transitions.
    /// </summary>
    /// <remarks>
    /// Notifies <see cref="IScreenFadeListener"/> components of fade in/out operations.
    /// </remarks>
    public class ScreenFadeTransition : 
        MonoBehaviour,
        IScreenFadeTransition
    {
        private const float MinSpeedMultiplierLimit = 0.01f;

        [Tooltip("The CanvasGroup to fade in/out.")]
        [Header("Reference")]
        [SerializeField, NotNull]
        private CanvasGroup _targetCanvasGroup;

        [Tooltip("Audio snapshot applied when fully faded in. (e.g during loading)")]
        [SerializeField, NotNull]
        private AudioMixerSnapshot _activeAudioSnapshot;

        [Tooltip("Default audio snapshot to be applied after fade out.")]
        [SerializeField, NotNull]
        private AudioMixerSnapshot _defaultAudioSnapshot;

        [Tooltip("Delay before fade in starts.")]
        [Header("Timing")]
        [SerializeField, Range(0f, 5f)]
        private float _showDelay = 0.25f;

        [Tooltip("Duration of fade in animation.")]
        [SerializeField, Range(0f, 5f)]
        private float _showDuration = 0.5f;

        [Tooltip("Delay before fade out starts.")]
        [SerializeField, Range(0f, 5f)]
        private float _hideDelay = 0.25f;

        [Tooltip("Duration of fade out animation.")]
        [SerializeField, Range(0f, 5f)]
        private float _hideDuration = 0.5f;

        private Tween<float> _activeTween;

        private IScreenFadeListener[] _listeners;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _activeAudioSnapshot.TransitionTo(0f);
            _targetCanvasGroup.alpha = 1f;
            _listeners = GetListeners() ?? Array.Empty<IScreenFadeListener>();
        }

        private IScreenFadeListener[] GetListeners() => this.GetComponentsFromHierarchy<IScreenFadeListener>();

        public IEnumerator Show(float speedMultiplier = 1f)
        {
            foreach(var listener in _listeners)
            {
                listener.OnFadeInStarted();
            }

            yield return Transition(
                true,
                Mathf.Max(MinSpeedMultiplierLimit, speedMultiplier));

            foreach (var listener in _listeners)
            {
                listener.OnFadeInCompleted();
            }
        }

        public IEnumerator Hide(float speedMultiplier = 1f)
        {
            foreach (var listener in _listeners)
            {
                listener.OnFadeOutStarted();
            }

            yield return Transition(
                false, 
                Mathf.Max(MinSpeedMultiplierLimit, speedMultiplier));

            foreach (var listener in _listeners)
            {
                listener.OnFadeOutCompleted();
            }
        }

        private IEnumerator Transition(bool isShowing, float speedMultiplier)
        {
            if(isShowing)
            {
                _activeAudioSnapshot.TransitionTo(_showDuration * speedMultiplier);
            }
            else
            {
                _defaultAudioSnapshot.TransitionTo(_hideDelay * speedMultiplier);
            }

            yield return AnimateAlpha(
                targetAlpha: isShowing ? 1f : 0f,
                delay: isShowing ? _showDelay * speedMultiplier : _hideDelay * speedMultiplier,
                duration: isShowing ? _showDuration * speedMultiplier : _hideDuration * speedMultiplier,
                ease: isShowing ? Ease.SineOut : Ease.SineIn);
        }

        private IEnumerator AnimateAlpha(float targetAlpha, float delay, float duration, Ease ease)
        {
            _activeTween?.Stop();
            // Ensure frame sync
            yield return null;

            _activeTween = _targetCanvasGroup.TweenAlpha(targetAlpha, duration)
                .SetUnscaledTime(true)
                .SetDelay(delay)
                .SetEase(ease);

            yield return _activeTween.WaitForCompletion();
        }
    }
}