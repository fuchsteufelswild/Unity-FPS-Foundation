using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora
{
    public static class CoroutineUtils
    {
        private static float EpsilonDelay = 0.0001f;
        private static CoroutineRunner _globalCoroutineRunner;
        private static WaitForSeconds _cachedWaitForEpsilon;

        private static MonoBehaviour GlobalCoroutineRunner
        {
            get
            {
                if(_globalCoroutineRunner == null)
                {
                    var runner = new GameObject("[GlobalCoroutineRunner]")
                    {
                        hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave
                    };

                    UnityEngine.Object.DontDestroyOnLoad(runner);
                    _globalCoroutineRunner = runner.AddComponent<CoroutineRunner>();
                }

                return _globalCoroutineRunner;
            }
        }

        public static Coroutine StartGlobalCoroutine(IEnumerator routine)
            => GlobalCoroutineRunner.StartCoroutine(routine);

        public static Coroutine StartGlobalCoroutineDelayed(UnityAction action, float delay)
        {
            if (delay < EpsilonDelay)
            {
                action?.Invoke();
                return null;
            }

            return StartGlobalCoroutine((RunActionDelayed(action, delay)));
        }

        public static void StopGlobalCoroutine(ref Coroutine coroutine)
        {
            StopCoroutine(GlobalCoroutineRunner, ref coroutine);
        }

        public static void StopAndReplaceCoroutine(
            MonoBehaviour context,
            ref Coroutine oldRoutine,
            IEnumerator newRoutine
            )
        {
            StopCoroutine(context, ref oldRoutine);
            oldRoutine = context.StartCoroutine(newRoutine);
        }

        public static void StopCoroutine(MonoBehaviour context, ref Coroutine coroutine)
        {
            if (coroutine == null)
            {
                return;
            }

            (context != null ? context : _globalCoroutineRunner).StopCoroutine(coroutine);
            coroutine = null;
        }

        #region EXTENSION METHODS
        public static Coroutine InvokeDelayed(this MonoBehaviour context, UnityAction action, float delay)
        {
            if (delay < EpsilonDelay)
            {
                action?.Invoke();
                return null;
            }

            return context.StartCoroutine(RunActionDelayed(action, delay));
        }

        public static Coroutine InvokeDelayed<T>(
            this MonoBehaviour context,
            UnityAction<T> action,
            T value,
            float delay)
        {
            if (delay < EpsilonDelay)
            {
                action?.Invoke(value);
                return null;
            }

            return context.StartCoroutine(RunActionDelayed(action, value, delay));
        }

        public static Coroutine InvokeNextFrame(this MonoBehaviour context, UnityAction action)
            => context.StartCoroutine(RunActionNextFrame(action));

        public static Coroutine InvokeNextFrame<T>(this MonoBehaviour context, UnityAction<T> action, T value)
            => context.StartCoroutine(RunActionNextFrame(action, value));
        #endregion

        public static IEnumerator RunActionNextFrame(UnityAction action)
        {
            yield return null;
            action?.Invoke();
        }

        public static IEnumerator RunActionNextFrame<T>(UnityAction<T> action, T value)
        {
            yield return null;
            action?.Invoke(value);
        }

        public static IEnumerator RunActionDelayed(UnityAction action, float delay)
        {
            yield return WaitFor(delay);
            action?.Invoke();
        }

        public static IEnumerator RunActionDelayed<T>(UnityAction<T> action, T value, float delay)
        {
            yield return WaitFor(delay);
            action?.Invoke(value);
        }

        public static IEnumerator WaitFor(float delay)
        {
            if (delay <= EpsilonDelay)
            {
                yield return _cachedWaitForEpsilon ??= new WaitForSeconds(EpsilonDelay);
            }
            else
            {
                yield return new Delay(delay);
            }
        }

        public static IEnumerator RunActionAfterRoutine(UnityAction action, IEnumerator routine)
        {
            yield return routine;
            action?.Invoke();
        }

        public static IEnumerator RunActionAfterRoutine<T>(UnityAction<T> action, T value, IEnumerator routine)
        {
            yield return routine;
            action?.Invoke(value);
        }

        private sealed class CoroutineRunner : MonoBehaviour
        {
            private void OnDestroy()
            {
                CoroutineUtils._globalCoroutineRunner = null;

                if (gameObject != null)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Custom Enumerator to use in place of Unity's WaitForSeconds to allocate no garbage
    /// </summary>
    public struct Delay : IEnumerator
    {
        private BaseDelay _delay;

        public Delay(float seconds) => _delay = new BaseDelay(seconds, false);
        public bool MoveNext() => _delay.MoveNext();
        public object Current => null;
        public void Reset() { }
    }

    /// <summary>
    /// <inheritdoc cref="Delay" path="/summary"/> and without time scale
    /// </summary>
    public struct UnscaledDelay : IEnumerator
    {
        private BaseDelay _delay;

        public UnscaledDelay(float seconds) => _delay = new BaseDelay(seconds, true);
        public bool MoveNext() => _delay.MoveNext();
        public object Current => null;
        public void Reset() { }
    }

    public struct BaseDelay : IEnumerator
    {
        private float _remainingTime;
        private bool _useUnscaledTime;

        public BaseDelay(float seconds, bool unscaled)
        {
            _remainingTime = seconds;
            _useUnscaledTime = unscaled;
        }

        public object Current => null;

        public bool MoveNext()
        {
            _remainingTime -= _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            return _remainingTime > 0;
        }

        public void Reset()
        {

        }
    }
}
