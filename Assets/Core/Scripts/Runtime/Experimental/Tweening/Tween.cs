using System;
using System.Collections;
using UnityEngine;
using Nexora.Motion;

namespace Nexora.Experimental.Tweening
{
    public interface ITween
    {
        /// <summary>
        /// The object tween is attached to.
        /// </summary>
        UnityEngine.Object Target { get; }

        /// <summary>
        /// Updates the value on every tick call.
        /// </summary>
        void Tick();

        /// <summary>
        /// Resets and releases the tween, cleaning up all resources.
        /// </summary>
        void Release(TweenResetBehaviour tweenResetBehaviour);

        /// <summary>
        /// Resets the tween, sets its value according to the passed <paramref name="tweenResetBehaviour"/>.
        /// </summary>
        void Reset(TweenResetBehaviour tweenResetBehaviour);
    }

    /// <summary>
    /// Main class for tweens. Having an interface similar to that of DOTween's.
    /// It is a primitive imitation of DOTween tweens.
    /// </summary>
    public abstract class Tween<T> :
        ITween
        where T : struct
    {
        private const float Epsilon = 0.0001f;

        private float _delay;
        private float _duration;
        private float _direction;
        private float _progress;

        private TweenState _state;
        private LoopType _loopType;
        private bool _autoRelease;
        private bool _autoReleaseWithTarget;
        private bool _useUnscaledTime;
        // -1 for inifinite loop
        private int _loopCount;
        private float _speedMultiplier = 1f;

        private T _startValue;
        private T _endValue;
        private UnityEngine.Object _tweenTarget;

        private Action<T> _onUpdate;
        private Action _onComplete;

        private Ease _ease = Ease.SineInOut;

        public UnityEngine.Object Target => _tweenTarget;

        public bool IsPlaying() => _state == TweenState.Playing;
        public UnityEngine.Object GetTweenTarget() => _tweenTarget;
        public T GetStartValue() => _startValue;
        public T GetCurrentValue() => Interpolate(in _startValue, in _endValue, Easing.Evaluate(_ease, _progress));
        public T GetEndValue() => _endValue;

        /// <summary>
        /// Starts the tween and registers it to the <see cref="TweenSystem"/>.
        /// </summary>
        public Tween<T> Start()
        {
            if(IsPlaying() || _duration <= 0)
            {
                return this;
            }

            _progress = 0;
            _direction = 1;
            _state = TweenState.Playing;

            TweenSystem.Instance.RegisterTween(this);
            return this;
        }

        /// <summary>
        /// Stops the tween and unregisters it from the <see cref="TweenSystem"/>.
        /// </summary>
        public Tween<T> Stop()
        {
            if(_state == TweenState.Stopped)
            {
                return this;
            }

            _state = TweenState.Stopped;
            TweenSystem.Instance.UnregisterTween(this);
            return this;
        }

        public Tween<T> Pause()
        {
            _state = TweenState.Paused;
            return this;
        }

        public Tween<T> Resume()
        {
            if(_state == TweenState.Stopped)
            {
                return Restart();
            }

            if(IsPlaying() || _progress == 0f)
            {
                return this;    
            }

            _state = TweenState.Playing;
            return this;
        }

        public Tween<T> Restart()
        {
            _progress = 0f;
            _direction = 1f;
            return Start();
        }

        public Tween<T> SetAutoRelease(bool enabled)
        {
            _autoRelease = enabled;
            return this;
        }

        /// <summary>
        /// Attaches the tween to a target. If already attached to another target, then it is first detached from it.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Tween<T> AttachTo(UnityEngine.Object target)
        {
            if(target == null)
            {
                throw new ArgumentNullException(string.Format("Tween cannot be attached to a null target: {0}", nameof(target)));
            }

            UnityEngine.Object oldTarget = _tweenTarget;
            _tweenTarget = target;
            TweenSystem.Instance.RegisterTweenTarget(this, oldTarget);
            return this;
        }

        public Tween<T> SetAutoReleaseWithParent(bool enabled)
        {
            _autoReleaseWithTarget = enabled;
            return this;
        }

        /// <summary>
        /// Stops and resets the value using <paramref name="resetBehaviour"/>.
        /// </summary>
        public void Release(TweenResetBehaviour resetBehaviour = TweenResetBehaviour.KeepCurrentValue)
        {
            if (_state == TweenState.Stopped || _state == TweenState.Paused)
            {
                Debug.LogWarning("Tween is already stopped or paused. Cannot release in this state.");
                return;
            }

            Stop();
            Reset(resetBehaviour);
        }

        public void Reset(TweenResetBehaviour tweenResetBehaviour)
        {
            switch(tweenResetBehaviour)
            {
                case TweenResetBehaviour.ResetToStartValue:
                    _onUpdate?.Invoke(_startValue);
                    break;
                case TweenResetBehaviour.ResetToEndValue:
                    _onUpdate?.Invoke(_endValue);
                    break;
            }

            ResetAllValues();
        }

        private void ResetAllValues()
        {
            _direction = 1f;
            _progress = 0f;
            _state = TweenState.Stopped;
            _loopType = LoopType.Restart;
            _tweenTarget = null;
            _autoRelease = false;
            _autoReleaseWithTarget = false;
            _useUnscaledTime = false;
            _onUpdate = null;
            _onComplete = null;
            _ease = Ease.SineInOut;
            _delay = 0f;
            _loopCount = 0;
            _speedMultiplier = 1f;
        }

        /// <summary>
        /// Ticks delay and if the delay is passed updates progress and updates loops if necessary.
        /// </summary>
        public void Tick()
        {
            if(IsPlaying() == false)
            {
                return;
            }

            float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if(_delay > 0f)
            {
                _delay -= deltaTime;
                return;
            }

            if(_autoReleaseWithTarget && _tweenTarget == null)
            {
                Release();
                return;
            }


            bool finishedLoop = UpdateProgress(deltaTime);
            _onUpdate?.Invoke(GetCurrentValue());
            if(finishedLoop)
            {
                DecrementLoopCount();
            }
        }

        /// <summary>
        /// Using loop type, updates the progress value and updates direction accordingly.
        /// </summary>
        /// <returns>If the current loop has finished.</returns>
        private bool UpdateProgress(float deltaTime)
        {
            _progress += (_direction * deltaTime * _speedMultiplier) / _duration;
            bool finishedLoop = false;

            if (_progress >= 1f)
            {
                _progress = _loopType == LoopType.Restart ? 0f : 1f;
                if(_loopType == LoopType.Yoyo)
                {
                    _direction = -1f;
                }
                else
                {
                    finishedLoop = true;
                }
            }
            else if(_loopType == LoopType.Yoyo && _progress <= 0f)
            {
                _progress = 0f;
                _direction = 1f;
                finishedLoop = true;
            }

            return finishedLoop;
        }

        private void DecrementLoopCount()
        {
            if(_loopCount > 0)
            {
                _progress = 0f;
                _loopCount--;
            }
            else if(_loopCount == 0)
            {
                Stop();
                _onComplete?.Invoke();
                if(_autoRelease)
                {
                    Release();
                }
            }
            else
            {
                _progress = 0f;
            }
        }

        /// <summary>
        /// Sets the end value to <paramref name="value"/> and sets progress to 0.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setStartToCurrent">Determines if the start value to be set as current value</param>
        /// <returns></returns>
        public Tween<T> SetEndValue(T value, bool setStartToCurrent = true)
        {
            if(setStartToCurrent && _progress > Epsilon)
            {
                _startValue = GetCurrentValue();
            }

            _endValue = value;
            _progress = 0f;
            return this;
        }

        public Tween<T> SetStartValue(T value)
        {
            _startValue = value;
            return this;
        }

        public Tween<T> SetDuration(float duration)
        {
            _duration = Mathf.Max(duration, Epsilon);
            return this;
        }

        public Tween<T> SetDelay(float delay)
        {
            _delay = Mathf.Max(delay, 0f); 
            return this;
        }

        public Tween<T> SetSpeedMultiplier(float speedMultiplier)
        {
            _speedMultiplier = Mathf.Max(speedMultiplier, 0.05f);
            return this;
        }

        public Tween<T> SetProgress(float newProgress, bool instantUpdate = false)
        {
            _progress = Mathf.Clamp01(newProgress);

            if(IsPlaying() && instantUpdate)
            {
                _onUpdate?.Invoke(GetCurrentValue());
            }

            return this;
        }

        public Tween<T> SetLoops(int loopCount, LoopType loopType = LoopType.None)
        {
            _loopCount = Mathf.Max(loopCount, -1);
            _loopType = loopType;
            return this;
        }

        public Tween<T> SetEase(Ease ease)
        {
            _ease = ease;
            return this;
        }

        public Tween<T> SetUnscaledTime(bool useUnscaledTime)
        {
            _useUnscaledTime = useUnscaledTime;
            return this;
        }

        public Tween<T> SetOnComplete(Action action)
        {
            if(action == null)
            {
                return this;
            }

            _onComplete += action;
            return this;
        }

        public Tween<T> SetOnUpdate(Action<T> action)
        {
            if(action == null)
            {
                return this;
            }

            _onUpdate += action;
            return this;
        }

        /// <summary>
        /// Returns the total time tween will be running from first call to finish including the delay.
        /// </summary>
        public float GetTotalRunningTime()
        {
            float totalDuration = _duration;

            totalDuration *= (_loopCount + 1);

            if(_loopType == LoopType.Yoyo)
            {
                totalDuration *= 2;
            }

            return totalDuration + _delay;
        }

        public IEnumerator WaitForCompletion()
        {
            while(IsPlaying())
            {
                yield return null;
            }
        }

        protected abstract T Interpolate(in T start, in T end, float progress);
    }

}