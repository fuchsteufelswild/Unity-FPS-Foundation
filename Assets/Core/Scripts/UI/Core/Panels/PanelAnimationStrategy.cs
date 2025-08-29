using Nexora.Experimental.Tweening;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.UI
{
    /// <summary>
    /// Interface for defining animation behaviours for <see cref="PanelBase"/>.
    /// Can derive this class to define own animated panels, like <see cref="Animator"/>
    /// based, <see cref="AnimationCurve"/> based, tween based etc.
    /// </summary>
    public interface IPanelAnimationStrategy
    {
        /// <summary>
        /// Checks if currently any animation is running.
        /// </summary>
        bool IsAnimating { get; }

        /// <summary>
        /// Should use <see cref="Time.unscaledDeltaTime"/>?
        /// </summary>
        bool UseUnscaledTime { get; }

        /// <summary>
        /// Initializes the strategy with the given Unity context.
        /// </summary>
        void Initialize(MonoBehaviour context);

        void ShowAnimated(Action onComplete = null);
        
        void HideAnimated(Action onComplete = null);
        
        void StopAnimation();
        
        /// <summary>
        /// Cleans up the any resources upon context's destroy.
        /// </summary>
        void Cleanup();

#if UNITY_EDITOR
        /// <summary>
        /// Like <b>OnValidate</b>, will be called from <see cref="PanelBase"/>.
        /// </summary>
        void Validate(MonoBehaviour context);
#endif
    }

    /// <summary>
    /// Animation behaviour that is just shown as Null state, does not have any animations.
    /// </summary>
    [Serializable]
    public sealed class NullAnimationStrategy : IPanelAnimationStrategy
    {
        public bool IsAnimating => false;
        public bool UseUnscaledTime => false;

        public void Initialize(MonoBehaviour context) { }
        public void ShowAnimated(Action onComplete = null) => onComplete?.Invoke();
        public void HideAnimated(Action onComplete = null) => onComplete?.Invoke();

        public void StopAnimation() { }
        public void Cleanup() { }

#if UNITY_EDITOR
        public void Validate(MonoBehaviour context) { }
#endif

        public static NullAnimationStrategy Null => new();
    }

    [Serializable]
    public sealed class CanvasCrossfadeAnimationStrategy : IPanelAnimationStrategy
    {
        [Tooltip("CanvasGroup that is crossfaded.")]
        [SerializeField, NotNull]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private bool _useUnscaledTime;

        [Tooltip("Easing of the cross fade animation.")]
        [SerializeField]
        private Ease _easeType;

        [Tooltip("Length of the show animation.")]
        [SerializeField]
        private float _showAnimationDuration;

        [Tooltip("Length of the hide animation.")]
        [SerializeField]
        private float _hideAnimationDuration;

        public bool IsAnimating => _canvasGroup.HasTweens();

        public bool UseUnscaledTime => _useUnscaledTime;

        public void Initialize(MonoBehaviour context)
            => _canvasGroup = context.gameObject.GetOrAddComponent<CanvasGroup>();

        public void ShowAnimated(Action onComplete = null)
        {
            _canvasGroup.ClearTweens();
            _canvasGroup.TweenAlpha(1f, _showAnimationDuration)
                .SetEase(_easeType)
                .SetUnscaledTime(_useUnscaledTime)
                .SetOnComplete(onComplete);
        }

        public void HideAnimated(Action onComplete = null)
        {
            _canvasGroup.ClearTweens();
            _canvasGroup.TweenAlpha(0f, _hideAnimationDuration)
                .SetEase(_easeType)
                .SetUnscaledTime(_useUnscaledTime)
                .SetOnComplete(onComplete);
        }

        public void Cleanup() => _canvasGroup.ClearTweens();

        public void StopAnimation()
        {
            _canvasGroup.ClearTweens();
        }

#if UNITY_EDITOR
        public void Validate(MonoBehaviour context)
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = context.gameObject.GetOrAddComponent<CanvasGroup>();
            }
        }
#endif
    }

    /// <summary>
    /// Animation behaviour that works with <see cref="Animator"/>.
    /// </summary>
    [Serializable]
    public sealed class AnimatorAnimationStrategy : IPanelAnimationStrategy
    {
        /// <summary>
        /// Delay to disable the animator after hide animation is started playing.
        /// </summary>
        private const float DisableAnimatorDelay = 1f;

        private static readonly int _showAnimationHash = Animator.StringToHash("Show");
        private static readonly int _showAnimationSpeedHash = Animator.StringToHash("Show Speed");
        private static readonly int _hideAnimationHash = Animator.StringToHash("Hide");
        private static readonly int _hideAnimationSpeedHash = Animator.StringToHash("Hide Speed");

        [SerializeField, NotNull]
        private Animator _animator;

        [Tooltip("Multiplier of how fast show animation of this panel should run.")]
        [SerializeField, Range(0f, 5f)]
        private float _showSpeed;

        [Tooltip("Multiplier of how fast hide animation of this panel should run.")]
        [SerializeField, Range(0f, 5f)]
        private float _hideSpeed;

        [Tooltip("Should animations of this panel use unscaled time?")]
        [SerializeField]
        private bool _useUnscaledTime;

        /// <summary>
        /// Unity object this strategy is bound to.
        /// </summary>
        private MonoBehaviour _context;
        private Coroutine _animatorDisableRoutine;

        /// <summary>
        /// Returns if the <see cref="Animator"/> is playing.
        /// </summary>
        public bool IsAnimating => _animator.IsPlaying();
        public bool UseUnscaledTime => _useUnscaledTime;

        public void Initialize(MonoBehaviour context)
        {
            _context = context;

            if (_animator == null)
            {
                _animator = context.GetComponent<Animator>();
                _animator.fireEvents = false;
                _animator.keepAnimatorStateOnDisable = true;
                _animator.writeDefaultValuesOnDisable = false;
                _animator.enabled = false;
            }
        }

        public void Cleanup() => CoroutineUtils.StopCoroutine(_context, ref _animatorDisableRoutine);

        public void ShowAnimated(Action onComplete = null)
        {
            bool shouldRebind = _animator.enabled == false;
            EnsureState();

            _animator.SetFloat(_showAnimationSpeedHash, _showSpeed);
            _animator.SetTrigger(_showAnimationHash);

            if(shouldRebind)
            {
                _animator.Rebind();
            }
        }

        private void EnsureState()
        {
            CoroutineUtils.StopCoroutine(_context, ref _animatorDisableRoutine);
            _animator.enabled = true;
        }

        public void HideAnimated(Action onComplete = null)
        {
            EnsureState();

            _animator.SetFloat(_hideAnimationSpeedHash, _hideSpeed);
            _animator.SetTrigger(_hideAnimationHash);
            _animatorDisableRoutine = _context.InvokeDelayed(DisableAnimator, DisableAnimatorDelay);
        }

        private void DisableAnimator()
        {
            _animator.enabled = false;
            _animatorDisableRoutine = null;
        }

        public void StopAnimation() { }

#if UNITY_EDITOR
        public void Validate(MonoBehaviour context)
        {
            _animator = context.GetComponent<Animator>();
        }
#endif
    }
}