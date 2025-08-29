using System;
using System.Diagnostics;
using UnityEngine;

namespace Nexora.Rendering
{
    public struct LightProperties
    {
        public float Intensity;
        public float Range;
        public Color Color;
        public bool IsOn;

        public LightProperties(float intensity, float range, Color color, bool isOn)
        {
            Intensity = intensity;
            Range = range;
            Color = color;
            IsOn = isOn;
        }
    }

    [RequireComponent(typeof(Light))]
    public sealed class DynamicLight : MonoBehaviour
    {
        [Tooltip("Base intensity of the light without any modifiers applied.")]
        [SerializeField, Range(0f, 10f)]
        private float _baseIntensity = 1f;

        [Tooltip("Base range of the light without any modifiers applied.")]
        [SerializeField, Range(0f, 1000f)]
        private float _baseRange = 50f;

        [Tooltip("Base color of the light without any modifiers applied.")]
        [SerializeField]
        private Color _baseColor = Color.yellow;

        [ReorderableList]
        [ReferencePicker(typeof(ILightModifier), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private ILightModifier[] _dynamicModifiers = Array.Empty<ILightModifier>();

        [NonSerialized]
        private Light _ligth;

        private bool _isOn;

        /// <summary>
        /// Base multiplier of the light to be applied to its properties.
        /// </summary>
        public float BaseMultiplier { get; set; }

        private void Awake() => _ligth = GetComponent<Light>();

        public void Play(bool fadeIn = true)
        {
            enabled = true;
            _isOn = true;

            foreach(var modifier in _dynamicModifiers)
            {
                modifier.OnPlay(fadeIn);
            }
        }

        public void Stop(bool fadeOut = true)
        {
            _isOn = false;

            foreach (var modifier in _dynamicModifiers)
            {
                modifier.OnStop(fadeOut);
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            var currentProperties = new LightProperties(_baseIntensity, _baseRange, _baseColor, _isOn);
            currentProperties.MultiplyWith(BaseMultiplier);

            foreach (var modifier in _dynamicModifiers)
            {
                modifier.Apply(currentProperties, deltaTime);
            }

            _isOn = currentProperties.IsOn;
            ApplyLightProperties(currentProperties);
        }

        private void ApplyLightProperties(LightProperties properties)
        {
            _ligth.intensity = properties.Intensity;
            _ligth.range = properties.Range;
            _ligth.color = properties.Color;
        }

        private void OnEnable()
        {
            _ligth.enabled = true;
            ApplyLightProperties(new LightProperties(0f, _ligth.range, _ligth.color, true));
        }

        private void OnDisable() => _ligth.enabled = false;

        [Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            if(_ligth == null)
            {
                _ligth = GetComponent<Light>();
            }

            _ligth.enabled = enabled;
            _ligth.intensity = _baseIntensity;
            _ligth.range = _baseRange;
            _ligth.color = _baseColor;
        }
    }

    public static class DynamicLightExtensions
    {
        public static void MultiplyWith(this ref LightProperties properties, float multiplier)
        {
            properties.Intensity *= multiplier;
            properties.Range *= multiplier;
        }

        public static void Add(this ref LightProperties properties, float addition)
        {
            properties.Intensity += addition;
            properties.Range += addition;
        }
    }
}