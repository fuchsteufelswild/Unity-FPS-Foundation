using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nexora.Experimental.Tweening
{
    /// <summary>
    /// Extensions to provide DOTween like interface when tweening objects.
    /// Provides main functionality which would be needed in any game,
    /// new extensions can be added in the specific project by having the 
    /// same structure of the existing extensions.
    /// </summary>
    public static partial class TweenExtensions
    {
        public static bool ClearTweens(
            this UnityEngine.Object obj, 
            TweenResetBehaviour resetBehaviour = TweenResetBehaviour.KeepCurrentValue)
        {
            TweenSystem.Instance.StopAndClearTweensFromTarget(obj, resetBehaviour);
            return true;
        }

        public static bool HasTweens(this UnityEngine.Object obj)
            => TweenSystem.Instance.HasTweensAttached(obj);

        public static Tween<T> CreateAndStartTween<T>(
            this T startValue,
            T endValue,
            float duration,
            Action<T> onUpdate)
            where T : struct
        {
            return TweenSystem.GetTween<T>()
                .SetStartValue(startValue)
                .SetEndValue(endValue)
                .SetDuration(duration)
                .SetOnUpdate(onUpdate)
                .Start();
        }

        #region Transform
        public static Tween<Vector3> TweenPosition(this Transform self, Vector3 to, float duration)
            => CreateAndStartTween(self.position, to, duration, value => self.position = value).AttachTo(self);

        public static Tween<Vector3> TweenLocalPosition(this Transform self, Vector3 to, float duration)
            => CreateAndStartTween(self.localPosition, to, duration, value => self.localPosition = value).AttachTo(self);

        public static Tween<Vector3> TweenEuler(this Transform self, Vector3 to, float duration)
            => CreateAndStartTween(self.eulerAngles, to, duration, value => self.eulerAngles = value).AttachTo(self);

        public static Tween<Vector3> TweenLocalEulerAngles(this Transform self, Vector3 to, float duration)
            => CreateAndStartTween(self.localEulerAngles, to, duration, value => self.localEulerAngles = value).AttachTo(self);

        public static Tween<Quaternion> TweenRotation(this Transform self, Quaternion to, float duration)
            => CreateAndStartTween(self.rotation, to, duration, value => self.rotation = value).AttachTo(self);

        public static Tween<Quaternion> TweenLocalRotation(this Transform self, Quaternion to, float duration)
            => CreateAndStartTween(self.localRotation, to, duration, value => self.localRotation = value).AttachTo(self);

        public static Tween<Vector3> TweenScale(this Transform self, Vector3 to, float duration)
            => CreateAndStartTween(self.localScale, to, duration, value => self.localScale = value).AttachTo(self);
        #endregion

        #region RectTransform
        public static Tween<Vector2> TweenAnchoredPosition(this RectTransform self, Vector2 to, float duration)
            => CreateAndStartTween(self.anchoredPosition, to, duration, value => self.anchoredPosition = value).AttachTo(self);
        #endregion

        // For all graphic types (Image, Text, TextMeshProText, etc.)
        #region Graphic
        public static Tween<float> TweenAlpha(this Graphic self, float to, float duration)
            => CreateAndStartTween(self.color.a, to, duration, value => self.color = self.color.WithAlpha(value)).AttachTo(self);

        public static Tween<Color> TweenColor(this Graphic self, Color to, float duration)
            => CreateAndStartTween(self.color, to, duration, value => self.color = value).AttachTo(self);

        #endregion

        #region SpriteRenderer 
        public static Tween<float> TweenAlpha(this SpriteRenderer self, float to, float duration)
            => CreateAndStartTween(self.color.a, to, duration, value => self.color = self.color.WithAlpha(value)).AttachTo(self);

        public static Tween<Color> TweenColor(this SpriteRenderer self, Color to, float duration)
            => CreateAndStartTween(self.color, to, duration, value => self.color = value).AttachTo(self);
        #endregion

        #region CanvasRenderer
        public static Tween<float> TweenAlpha(this CanvasRenderer self, float to, float duration)
            => CreateAndStartTween(self.GetAlpha(), to, duration, self.SetAlpha).AttachTo(self);
        #endregion

        #region CanvasGroup
        public static Tween<float> TweenAlpha(this CanvasGroup self, float to, float duration)
            => CreateAndStartTween(self.alpha, to, duration, value => self.alpha = value).AttachTo(self);
        #endregion

        #region Material
        public static Tween<Color> TweenColor(this Material self, Color to, float duration)
            => CreateAndStartTween(self.color, to, duration, value => self.color = value).AttachTo(self);
        #endregion
    }
}
