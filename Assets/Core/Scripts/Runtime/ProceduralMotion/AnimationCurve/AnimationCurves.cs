using System;
using UnityEngine;

namespace Nexora.Motion
{
    /// <summary>
    /// Contains logic shared by <see cref="AnimationCurve1D"/>, <see cref="AnimationCurve2D"/>
    /// and <see cref="AnimationCurve3D"/>.
    /// </summary>
    [Serializable]
    public abstract class AnimationCurveCore
    {
        protected const float MaxDuration = 100f;

        [SerializeField]
        [Range(-20f, 20f)]
        [Tooltip("Multiplier that scales the value of the curve")]
        private float _multiplier;

        protected AnimationCurve[] _curves;

        private float? _duration;

        protected AnimationCurveCore() { }

        public float Multiplier => _multiplier;

        /// <summary>
        /// Returns max duration out of all animation curves.
        /// </summary>
        public float Duration => _duration ??= GetDuration();

        /// <summary>
        /// Returns max duration out of all animation curves, that is the duration of the curve.
        /// </summary>
        private float GetDuration()
        {
            float maxDuration = 0;

            foreach (var animationCurve in _curves)
            {
                maxDuration = Mathf.Max(maxDuration, animationCurve.keys.Last().time);
            }

            return Mathf.Clamp(maxDuration, 0f, MaxDuration);
        }
    }

    /// <summary>
    /// Represents a <see cref="AnimationCurve"/> with added multiplier.
    /// </summary>
    [Serializable]
    public sealed class AnimationCurve1D : AnimationCurveCore
    {
        [SerializeField]
        private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public float Evaluate(float time) => _curve.Evaluate(time) * Multiplier;
    }

    /// <summary>
    /// Represents a 2D curve with multiplier.
    /// Contains different <see cref="AnimationCurve"/> for each of the axes.
    /// </summary>
    [Serializable]
    public sealed class AnimationCurve2D : AnimationCurveCore
    {
        [SerializeField]
        private AnimationCurve _curveX = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField]
        private AnimationCurve _curveY = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public AnimationCurve2D()
        {
            _curves = new AnimationCurve[]
            {
                _curveX,
                _curveY
            };
        }

        /// <summary>
        /// Evaluates both axes with the given unified time.
        /// </summary>
        public Vector2 Evaluate(float time)
            => Evaluate(time, time);

        /// <summary>
        /// Evaluates both axes separately with two distinct parameters.
        /// </summary>
        public Vector2 Evaluate(float timeX, float timeY)
        {
            return new Vector2
            {
                x = _curveX.Evaluate(timeX) * Multiplier,
                y = _curveY.Evaluate(timeY) * Multiplier
            };
        }
    }

    /// <summary>
    /// Represents a 3D curve with multiplier.
    /// Contains different <see cref="AnimationCurve"/> for each of the axes.
    /// </summary>
    [Serializable]
    public sealed class AnimationCurve3D : AnimationCurveCore
    {
        [SerializeField]
        private AnimationCurve _curveX = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField]
        private AnimationCurve _curveY = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField]
        private AnimationCurve _curveZ = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public AnimationCurve3D()
        {
            _curves = new AnimationCurve[]
            {
                _curveX,
                _curveY,
                _curveZ
            };
        }

        /// <summary>
        /// Evaluates all axes with the given unified time.
        /// </summary>
        public Vector3 Evaluate(float time)
            => Evaluate(time, time, time);

        /// <summary>
        /// Evaluates all axes separately with three distinct parameters.
        /// </summary>
        public Vector3 Evaluate(float timeX, float timeY, float timeZ)
        {
            return new Vector3
            {
                x = _curveX.Evaluate(timeX) * Multiplier,
                y = _curveY.Evaluate(timeY) * Multiplier,
                z = _curveZ.Evaluate(timeZ) * Multiplier
            };
        }
    }
}
