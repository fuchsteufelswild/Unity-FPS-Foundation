using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    public sealed class Shake
    {
        private static readonly AnimationCurve _decayCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private float _speed;
        private Vector3 _amplitude;
        private float _duration;
        private float _endTime;

        public bool IsDone => Time.time > _endTime;

        public void Init(in ShakeDefinition shakeDefinition, float duration, float intensity = 1f)
        {
            Vector3 shakeAmplitude = new(shakeDefinition.AmplitudeX, shakeDefinition.AmplitudeY, shakeDefinition.AmplitudeZ);
            _amplitude = Vector3.Scale(Vector3.one * intensity, shakeAmplitude);
            _amplitude.RandomizeSigns();
            
            _duration = duration;
            _speed = shakeDefinition.ShakeSpeed;
            _endTime = Time.fixedTime + duration;
        }

        public Vector3 Evaluate()
        {
            float currentTime = Time.fixedTime;
            float timer = (_endTime - currentTime) * _speed;
            float decay = _decayCurve.Evaluate(1f - (_endTime - currentTime) / _duration);

            return new Vector3()
            {
                x = Mathf.Sin(timer) * _amplitude.x * decay,
                y = Mathf.Cos(timer) * _amplitude.y * decay,
                z = Mathf.Sin(timer) * _amplitude.z * decay
            };
        }
    }
}