using Nexora.Experimental.Tweening;
using UnityEngine;

namespace Nexora.Experimental.Tweening
{
    public class FloatTween : Tween<float>
    {
        protected override float Interpolate(in float start, in float end, float progress)
            => Mathf.LerpUnclamped(start, end, progress);
    }

    public class IntTween : Tween<int>
    {
        protected override int Interpolate(in int start, in int end, float progress)
            => (int)Mathf.LerpUnclamped(start, end, progress);
    }

    public class Vector3Tween : Tween<Vector3>
    {
        protected override Vector3 Interpolate(in Vector3 start, in Vector3 end, float progress)
        {
            return new Vector3(
                Mathf.LerpUnclamped(start.x, end.x, progress),
                Mathf.LerpUnclamped(start.y, end.y, progress),
                Mathf.LerpUnclamped(start.z, end.z, progress));
        }
    }

    public class Vector2Tween : Tween<Vector2>
    {
        protected override Vector2 Interpolate(in Vector2 start, in Vector2 end, float progress)
        {
            return new Vector2(
                Mathf.LerpUnclamped(start.x, end.x, progress),
                Mathf.LerpUnclamped(start.y, end.y, progress));
        }
    }

    public class ColorTween : Tween<Color>
    {
        protected override Color Interpolate(in Color start, in Color end, float progress)
        {
            return new Color(
                Mathf.LerpUnclamped(start.r, end.r, progress),
                Mathf.LerpUnclamped(start.g, end.g, progress),
                Mathf.LerpUnclamped(start.b, end.b, progress),
                Mathf.LerpUnclamped(start.a, end.a, progress));
        }
    }

    public class QuaternionTween : Tween<Quaternion>
    {
        protected override Quaternion Interpolate(in Quaternion start, in Quaternion end, float progress)
            => Quaternion.LerpUnclamped(start, end, progress);
    }
}