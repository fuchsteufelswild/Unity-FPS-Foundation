using Nexora.Experimental.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nexora.UI
{
    [Serializable]
    public sealed class UITweenToTarget
    {
        [SerializeField, NotNull]
        private RectTransform _initialLocation;

        [SerializeField, NotNull]
        private RectTransform _targetLocation;

        private Tween<Vector2> _tween;

        public void Show(RectTransform selfTransform, UIAnimationConfig animation)
        {
            selfTransform.ClearTweens();

            _tween = selfTransform.TweenAnchoredPosition(_targetLocation.anchoredPosition, animation.TweenDuration)
                .SetDelay(animation.TweenStartDelay)
                .SetEase(animation.TweenEaseType);
        }

        public void Hide(RectTransform selfTransform, UIAnimationConfig animation)
        {
            selfTransform.ClearTweens();

            _tween = selfTransform.TweenAnchoredPosition(_initialLocation.anchoredPosition, animation.TweenDuration)
                .SetDelay(animation.TweenStartDelay)
                .SetEase(animation.TweenEaseType);
        }
    }

    public sealed class UITweener
    {
        private IUITweenExecutor _canvasFader;
        private IUITweenExecutor _scaler;
        private IUITweenExecutor _mover;

        public UITweener(RectTransform transform, params UIAnimationConfig[] animations)
        {
            if(transform == null)
            {
                Debug.LogError("UITweener requires valid [RectTransform] component to work with.", transform);
                return;
            }

            foreach(var animation in animations)
            {
                EnsureDependencies(animation, transform);
            }
        }

        private void EnsureDependencies(UIAnimationConfig animation, RectTransform transform)
        {
            switch (animation.TweenType)
            {
                case UITweenType.CanvasFade:
                    _canvasFader = new CanvasFadeTweener(transform);
                    break;
                case UITweenType.Move:
                    _mover = new ScaleTweener(transform);
                    break;
                case UITweenType.Scale:
                    _scaler = new ScaleTweener(transform);
                    break;
            }
        }

        public void Execute(UIAnimationConfig animation)
        {
            if(animation == null)
            {
                return;
            }

            switch (animation.TweenType)
            {
                case UITweenType.CanvasFade:
                    _canvasFader?.Execute(animation);
                    break;
                case UITweenType.Scale:
                    _scaler?.Execute(animation);
                    break;
                case UITweenType.Move:
                    _mover?.Execute(animation);
                    break;
            }
        }
    }

    public interface IUITweenExecutor
    {
        void Execute(UIAnimationConfig animation);
    }

    public sealed class CanvasFadeTweener : IUITweenExecutor
    {
        private Tween<float> _tween;

        private CanvasGroup _canvasGroup;

        public CanvasFadeTweener(RectTransform transform)
        {
            _canvasGroup = transform.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                Debug.LogError("Tween type requires [CanvasGroup] component.", transform);
            }
        }

        public void Execute(UIAnimationConfig animation)
        {
            if(_canvasGroup == null)
            {
                return;
            }

            Debug.Log("Tween starts");

            _canvasGroup.ClearTweens();

            _tween = _canvasGroup.TweenAlpha(animation.TargetAlpha, animation.TweenDuration)
                .SetDelay(animation.TweenStartDelay)
                .SetEase(animation.TweenEaseType);
        }
    }

    public sealed class ScaleTweener : IUITweenExecutor
    {
        private Tween<Vector3> _tween;

        private RectTransform _transform;
        private Vector3 _initialScale;

        public ScaleTweener(RectTransform transform)
        {
            _transform = transform;

            if (_transform == null)
            {
                Debug.LogError("Tween type requires [RectTransform] component.", transform);
                return;
            }

            _initialScale = transform.localScale;
        }

        public void Execute(UIAnimationConfig animation)
        {
            if (_transform == null)
            {
                return;
            }

            _transform.ClearTweens();
            _transform.localScale = _initialScale;

            _tween = _transform.TweenScale(_initialScale + animation.ScaleOffset, animation.TweenDuration)
                .SetDelay(animation.TweenStartDelay)
                .SetEase(animation.TweenEaseType)
                .SetLoops(0, LoopType.Yoyo);
        }
    }

    public sealed class MoveTweener : IUITweenExecutor
    {
        private Tween<Vector2> _tween;

        private RectTransform _transform;

        private Vector2 _initialPosition;

        public MoveTweener(RectTransform transform)
        {
            _transform = transform;
            
            if (_transform == null)
            {
                Debug.LogError("Tween type requires [RectTransform] component.", transform);
                return;
            }

            _initialPosition = _transform.anchoredPosition;
        }

        public void Execute(UIAnimationConfig animation)
        {
            if (_transform == null)
            {
                return;
            }

            _transform.ClearTweens();

            _tween = _transform.TweenAnchoredPosition(_initialPosition + animation.PositionOffset, animation.TweenDuration)
                .SetDelay(animation.TweenStartDelay)
                .SetEase(animation.TweenEaseType);
        }
    }
}