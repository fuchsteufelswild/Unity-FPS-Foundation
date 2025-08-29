using Nexora.Motion;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nexora.FPSDemo.UI
{
    public sealed class DynamicCrosshair : CrosshairDisplay
    {
        [Tooltip("Minimum size of the crosshair as a result of accuracy change.")]
        [SerializeField, Range(0f, 1000f)]
        private float _minSize;

        [Tooltip("Maximum size of the crosshair as a result of accuracy change.")]
        [SerializeField, Range(0f, 1000f)]
        private float _maxSize;

        [Title("Recoil")]
        [SerializeField]
        private Ease _recoilEase;

        [SerializeField, Range(0f, 10f)]
        private float _recoilThreshold;

        [SerializeField, Range(0f, 100f)]
        private float _recoilSize;

        [SerializeField, Range(0f, 100f)]
        private float _recoilDuration;

        private RectTransform _cachedTransform;
        private Image[] _crosshairParts;

        private float _lastAccuracy;
        private float _recoilTime;
        private float _scale = 1f;
        private float _size = 1f;

        private void Awake()
        {
            _cachedTransform = GetComponent<RectTransform>();
            _crosshairParts = GetComponentsInChildren<Image>();
        }

        public override void SetColor(Color color)
        {
            foreach (var image in _crosshairParts)
            {
                image.color = color;
            }
        }

        public override void SetSize(float accuracy, float scale)
        {
            float size = Mathf.LerpUnclamped(_minSize, _maxSize, Mathf.Clamp01(accuracy)) + CalculateRecoilKick(accuracy);

            if(Mathf.Abs(_size - size) > 0.001f)
            {
                _cachedTransform.sizeDelta = new(size, size);
                _size = size;
            }
            else
            {
                _cachedTransform.localScale = Vector3.one * scale;
                _scale = scale;
            }
        }

        private float CalculateRecoilKick(float accuracy)
        {
            if(_lastAccuracy - accuracy > _recoilThreshold)
            {
                _recoilTime = 1f;
            }

            _recoilTime = Mathf.Clamp01(_recoilTime - Time.deltaTime * (1 / _recoilDuration));
            _lastAccuracy = accuracy;
            return _recoilSize * Easing.Evaluate(_recoilEase, _recoilTime);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditorUtils.SafeOnValidate(this, () => ((RectTransform)transform).sizeDelta = new(_maxSize, _maxSize));
        }
#endif
    }
}
