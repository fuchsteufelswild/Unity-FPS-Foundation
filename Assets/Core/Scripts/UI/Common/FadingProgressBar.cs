using UnityEngine;
using UnityEngine.UI;

namespace Nexora.UI
{
    /// <summary>
    /// Progress bar that is fading when not depleting, and only appears
    /// when a slight decrease happened. 
    /// </summary>
    /// <remarks>
    /// It is <see cref="MonoBehaviour"/> so to use it with a prefab.
    /// </remarks>
    [AddComponentMenu(ComponentMenuPaths.BaseUI + nameof(FadingProgressBar))]
    public sealed class FadingProgressBar : MonoBehaviour
    {
        [Tooltip("Used to fade in/out the progress bar.")]
        [SerializeField]
        private CanvasGroup _canvasGroup;

        [Tooltip("Image used to fill/deplete progress bar by fillamount.")]
        [SerializeField]
        private Image _fillBar;

        [Title("Fade Settings")]
        [Tooltip("Should this progress bar fade when not decreasing, e.g regenerating or static.")]
        [SerializeField]
        private bool _fadeWhenNotDecreasing;

        [Tooltip("Represents how long should progress bar be visible before fading.")]
        [ShowIf(nameof(_fadeWhenNotDecreasing), true)]
        [SerializeField, Range(0f, 5f)]
        private float _depletionDelay = 2f;

        [Tooltip("How fast the fade progress will be.")]
        [ShowIf(nameof(_fadeWhenNotDecreasing), true)]
        [SerializeField, Range(0f, 25f)]
        private float _fadeSpeed = 5f;

        private float _lastValue;
        private float _fadeStartTime;

        public void UpdateProgressBar(float value, float maxValue)
        {
            float progress = value / maxValue;

            _fillBar.fillAmount = progress;

            if(_fadeWhenNotDecreasing)
            {
                float targetAlpha = ShouldFade() ? 0f : 1f;
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, targetAlpha, _fadeSpeed * Time.fixedDeltaTime);

                if(value < _lastValue)
                {
                    _fadeStartTime = Time.time + _depletionDelay;
                }

                _lastValue = value;
            }


            return;
        }

        private bool ShouldFade() => _fadeStartTime < Time.time;
    }
}