using UnityEngine;
using System;

namespace Nexora.Motion
{
    public enum Ease
    {
        Linear = 0,
        SineIn = 10,
        SineOut = 11,
        SineInOut = 12,
        QuadIn = 20,
        QuadOut = 21,
        QuadInOut = 22,
        CubicIn = 30,
        CubicOut = 31,
        CubicInOut = 32,
        QuartIn = 40,
        QuartOut = 41,
        QuartInOut = 42,
        QuintIn = 50,
        QuintOut = 51,
        QuintInOut = 52,
        ExpoIn = 60,
        ExpoOut = 61,
        ExpoInOut = 62,
        CircIn = 70,
        CircOut = 71,
        CircInOut = 72,
        BackIn = 80,
        BackOut = 81,
        BackInOut = 82,
        ElasticIn = 90,
        ElasticOut = 91,
        ElasticInOut = 92,
        BounceIn = 100,
        BounceOut = 101,
        BounceInOut = 102
    }

    /// <summary>
    /// Provides in/out/inout for many different types: 
    /// Sine, Quad, Cubic, Quart, Quint, Expo, Circ, Back, Elastic, Bounce.
    /// </summary>
    /// <remarks>
    /// If you want to support more types, add new type and then implement the easing.
    /// </remarks>
    public static class Easing
    {
        private const float BackOvershoot = 1.70158f;
        private const float BackInOutOvershoot = BackOvershoot * 1.525f;
        private const float BackCubicScale = BackOvershoot + 1;

        private const float ElasticFrequency = Mathf.PI / 2f;

        private const float BounceAmplitude = 7.5625f;
        private const float BouncePhaseWidth = 2.75f;

        /// <summary>
        /// Interpolate using the specified ease function.
        /// </summary>
        public static float Evaluate(Ease ease, float time)
        {
            return ease switch
            {
                Ease.Linear => Linear(time),
                Ease.SineIn => SineIn(time),
                Ease.SineOut => SineOut(time),
                Ease.SineInOut => SineInOut(time),
                Ease.QuadIn => QuadIn(time),
                Ease.QuadOut => QuadOut(time),
                Ease.QuadInOut => QuadInOut(time),
                Ease.CubicIn => CubicIn(time),
                Ease.CubicOut => CubicOut(time),
                Ease.CubicInOut => CubicInOut(time),
                Ease.QuartIn => QuartIn(time),
                Ease.QuartOut => QuartOut(time),
                Ease.QuartInOut => QuartInOut(time),
                Ease.QuintIn => QuintIn(time),
                Ease.QuintOut => QuintOut(time),
                Ease.QuintInOut => QuintInOut(time),
                Ease.ExpoIn => ExpoIn(time),
                Ease.ExpoOut => ExpoOut(time),
                Ease.ExpoInOut => ExpoInOut(time),
                Ease.CircIn => CircIn(time),
                Ease.CircOut => CircOut(time),
                Ease.CircInOut => CircInOut(time),
                Ease.BackIn => BackIn(time),
                Ease.BackOut => BackOut(time),
                Ease.BackInOut => BackInOut(time),
                Ease.ElasticIn => ElasticIn(time),
                Ease.ElasticOut => ElasticOut(time),
                Ease.ElasticInOut => ElasticInOut(time),
                Ease.BounceIn => BounceIn(time),
                Ease.BounceOut => BounceOut(time),
                Ease.BounceInOut => BounceInOut(time),
                _ => 0f
            };
        }

        private static float Linear(float time)
        {
            return time;
        }

        private static float SineIn(float time)
        {
            return 1f - Mathf.Cos(time * Mathf.PI / 2f);
        }

        private static float SineOut(float time)
        {
            return Mathf.Sin(time * Mathf.PI / 2f);
        }

        private static float SineInOut(float time)
        {
            return -(Mathf.Cos(Mathf.PI * time) - 1f) / 2f;
        }

        private static float QuadIn(float time)
        {
            return time * time;
        }

        private static float QuadOut(float time)
        {
            return 1 - (1 - time) * (1 - time);
        }

        private static float QuadInOut(float time)
        {
            return time < 0.5f
                ? 2 * time * time
                : 1 - Mathf.Pow(-2 * time + 2, 2) / 2;
        }

        private static float CubicIn(float time)
        {
            return time * time * time;
        }

        private static float CubicOut(float time)
        {
            return 1 - Mathf.Pow(1 - time, 3);
        }

        private static float CubicInOut(float time)
        {
            return time < 0.5f
                ? 4 * time * time * time
                : 1 - Mathf.Pow(-2 * time + 2, 3) / 2;
        }

        private static float QuartIn(float time)
        {
            return time * time * time * time;
        }

        private static float QuartOut(float time)
        {
            return 1 - Mathf.Pow(1 - time, 4);
        }

        private static float QuartInOut(float time)
        {
            return time < 0.5
                ? 8 * time * time * time * time
                : 1 - Mathf.Pow(-2 * time + 2, 4) / 2;
        }

        private static float QuintIn(float time)
        {
            return time * time * time * time * time;
        }

        private static float QuintOut(float time)
        {
            return 1 - Mathf.Pow(1 - time, 5);
        }

        private static float QuintInOut(float time)
        {
            return time < 0.5f
                ? 16 * time * time * time * time * time
                : 1 - Mathf.Pow(-2 * time + 2, 5) / 2;
        }

        private static float ExpoIn(float time)
        {
            return time == 0
                ? 0
                : Mathf.Pow(2, 10 * time - 10);
        }

        private static float ExpoOut(float time)
        {
            return Math.Abs(time - 1) < 0.001f
                ? 1
                : 1 - Mathf.Pow(2, -10 * time);
        }

        private static float ExpoInOut(float time)
        {
            return time == 0
                ? 0
                : Math.Abs(time - 1) < 0.001f
                    ? 1
                    : time < 0.5
                        ? Mathf.Pow(2, 20 * time - 10) / 2
                        : (2 - Mathf.Pow(2, -20 * time + 10)) / 2;
        }

        private static float CircIn(float time)
        {
            return 1 - Mathf.Sqrt(1 - Mathf.Pow(time, 2));
        }

        private static float CircOut(float time)
        {
            return Mathf.Sqrt(1 - Mathf.Pow(time - 1, 2));
        }

        private static float CircInOut(float time)
        {
            return time < 0.5
                ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * time, 2))) / 2
                : (Mathf.Sqrt(1 - Mathf.Pow(-2 * time + 2, 2)) + 1) / 2;
        }

        private static float BackIn(float time)
        {
            return BackCubicScale * time * time * time - BackOvershoot * time * time;
        }

        private static float BackOut(float time)
        {
            return 1f + BackCubicScale * Mathf.Pow(time - 1, 3) + BackOvershoot * Mathf.Pow(time - 1, 2);
        }

        private static float BackInOut(float time)
        {
            return time < 0.5 ?
                Mathf.Pow(2 * time, 2) * ((BackInOutOvershoot + 1) * 2 * time - BackInOutOvershoot) / 2 :
                (Mathf.Pow(2 * time - 2, 2) * ((BackInOutOvershoot + 1) * (time * 2 - 2) + BackInOutOvershoot) + 2) / 2;
        }

        private static float ElasticIn(float p)
        {
            return Mathf.Sin(13 * ElasticFrequency * p) * Mathf.Pow(2, 10 * (p - 1));
        }

        private static float ElasticOut(float p)
        {
            return Mathf.Sin(-13 * ElasticFrequency * (p + 1)) * Mathf.Pow(2, -10 * p) + 1;
        }

        private static float ElasticInOut(float p)
        {
            if (p < 0.5f)
                return 0.5f * Mathf.Sin(13 * ElasticFrequency * (2 * p)) * Mathf.Pow(2, 10 * (2 * p - 1));

            return 0.5f * (Mathf.Sin(-13 * ElasticFrequency * (2 * p - 1 + 1)) * Mathf.Pow(2, -10 * (2 * p - 1)) + 2);
        }

        private static float BounceIn(float time)
        {
            return 1 - BounceOut(1 - time);
        }

        private static float BounceOut(float time)
        {
            if (time < 1 / BouncePhaseWidth)
                return BounceAmplitude * time * time;

            if (time < 2 / BouncePhaseWidth)
                return BounceAmplitude * (time -= 1.5f / BouncePhaseWidth) * time + 0.75f;

            if (time < 2.5f / BouncePhaseWidth)
                return BounceAmplitude * (time -= 2.25f / BouncePhaseWidth) * time + 0.9375f;

            return BounceAmplitude * (time -= 2.625f / BouncePhaseWidth) * time + 0.984375f;
        }

        private static float BounceInOut(float time)
        {
            return time < 0.5f
                ? (1 - BounceOut(1 - 2 * time)) / 2
                : (1 + BounceOut(2 * time - 1)) / 2;
        }
    }
}