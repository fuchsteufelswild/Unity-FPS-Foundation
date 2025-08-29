using System;
using UnityEngine;

namespace Nexora
{
    public static class MathUtils
    {
        public static Vector3 GetRandomPoint(Vector3 min, Vector3 max)
        {
            return new Vector3
            (
                UnityEngine.Random.Range(min.x, max.x),
                UnityEngine.Random.Range(min.y, max.y),
                UnityEngine.Random.Range(min.z, max.z)
            );
        }

        public static Vector3 CreateJitter(float value)
        {
            return new Vector3
            (
                UnityEngine.Random.Range(-value, value),
                UnityEngine.Random.Range(-value, value),
                UnityEngine.Random.Range(-value, value)
            );
        }
    }

    [Serializable]
    public sealed class SmoothedFloat
    {
        [Tooltip("Damping speed of the lerp.")]
        [SerializeField, Range(0f, 20f)]
        private float _dampingSpeed;

        private float _currentValue;

        public void SetInitialValue(float value) => _currentValue = value;

        public float Update(float target, float deltaTime)
        {
            return _currentValue = Mathf.Lerp(_currentValue, target, _dampingSpeed * deltaTime);
        }
    }
}