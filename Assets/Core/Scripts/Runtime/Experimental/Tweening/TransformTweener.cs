using Nexora.Animation;
using Nexora.Audio;
using Nexora.Motion;
using Nexora.PostProcessing;
using Nexora.SaveSystem;
using Nexora.Serialization;
using System;
using UnityEngine;

namespace Nexora.Experimental.Tweening
{
    /// <summary>
    /// Component to set up tweens in the inspector window for <see cref="GameObject"/>.
    /// </summary>
    public sealed class TransformTweener : MonoBehaviour
    {
        [SerializeField]
        private bool _playOnAwake;

        [SerializeField]
        private bool _useUnscaledTime;

        [SerializeField]
        private bool _useCurrentValueAsStart;

        [SerializeField]
        [Tooltip("If checked, the end value is set relative to the current value. E.g endValue = current + endValue")]
        private bool _useRelativeEndValue;

        [SerializeField]
        private LoopType _loopType;

        [SerializeField]
        [Tooltip("How many loops will be played, -1 means infinite.")]
        private int _loopCount;

        [SerializeField]
        private TweenData[] _tweenData;

        private Transform _cachedTransform;

        private void Awake() => _cachedTransform = transform;

        private void OnEnable()
        {
            if(_playOnAwake)
            {
                Play();
            }
        }

        private void OnDisable() => Stop(TweenResetBehaviour.KeepCurrentValue);

        public void Play()
        {
            _cachedTransform.ClearTweens();

            foreach(TweenData tweenData in _tweenData)
            {
                Vector3 transformComponentValue = GetTransformComponent(tweenData.Mode);

                Vector3 startValue = _useCurrentValueAsStart
                    ? transformComponentValue
                    : tweenData.StartValue;

                Vector3 endValue = tweenData.EndValue + transformComponentValue;

                if(tweenData.Mode == TweenMode.RotateEuler)
                {
                    _cachedTransform.TweenLocalRotation(Quaternion.Euler(endValue), tweenData.Duration)
                        .SetStartValue(Quaternion.Euler(startValue))
                        .SetDelay(tweenData.Delay)
                        .SetUnscaledTime(_useUnscaledTime)
                        .SetLoops(_loopCount, _loopType);
                }
                else
                {
                    (tweenData.Mode == TweenMode.Move ? _cachedTransform.TweenLocalPosition(endValue, tweenData.Duration)
                        : _cachedTransform.TweenScale(endValue, tweenData.Duration))
                        .SetStartValue(startValue)
                        .SetDelay(tweenData.Delay)
                        .SetUnscaledTime(_useUnscaledTime)
                        .SetLoops(_loopCount, _loopType);
                }
            }
        }

        public void Stop(TweenResetBehaviour tweenResetBehaviour)
            => _cachedTransform.ClearTweens(tweenResetBehaviour);

        private Vector3 GetTransformComponent(TweenMode mode)
        {
            return mode switch
            {
                TweenMode.Move => _cachedTransform.localPosition,
                TweenMode.RotateEuler => _cachedTransform.localEulerAngles,
                TweenMode.Scale => _cachedTransform.localScale,
                _ => Vector3.zero
            };
        }

        [Serializable]
        private struct TweenData
        {
            public TweenMode Mode;
            public Vector3 StartValue;
            public Vector3 EndValue;
            [Range(0, 20f)]
            public float Delay;
            [Range(0, 100f)]
            public float Duration;
            public Ease Ease;
        }

        private enum TweenMode
        {
            Move = 0,
            RotateEuler = 1,
            Scale = 2
        }
    }
}