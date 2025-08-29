using Nexora.Motion;
using UnityEngine;

namespace Nexora.UI
{
    public enum UITweenType
    {
        None = 0,
        [Tooltip("Fade via CanvasGroup.")]
        CanvasFade = 1,
        Scale = 2,
        Move = 3
    }

    [CreateAssetMenu(menuName = "Nexora/UI/UI Animation Cofig", fileName = "UIAnimation_")]
    public sealed class UIAnimationConfig : ScriptableObject
    {
        [SerializeField]
        private UITweenType _tweenType;

        [SerializeField, Range(0f, 10f)]
        private float _tweenDuration;

        [SerializeField, Range(0f, 10f)]
        private float _tweenStartDelay;

        [SerializeField]
        private Ease _tweenEaseType;

        [SerializeField]
        [ShowIf(nameof(_tweenType), UITweenType.CanvasFade)]
        private float _targetAlpha;

        [SerializeField]
        [ShowIf(nameof(_tweenType), UITweenType.Scale)]
        private Vector3 _scaleOffset;

        [SerializeField]
        [ShowIf(nameof(_tweenType), UITweenType.Move)]
        private Vector2 _positionOffset;

        public UITweenType TweenType => _tweenType;
        public float TweenDuration => _tweenDuration;
        public float TweenStartDelay => _tweenStartDelay;
        public Ease TweenEaseType => _tweenEaseType;
        public float TargetAlpha => _targetAlpha;
        public Vector3 ScaleOffset => _scaleOffset;
        public Vector2 PositionOffset => _positionOffset;
    }
}