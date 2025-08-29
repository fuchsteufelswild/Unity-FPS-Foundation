using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Tracks the change of rotation value of the character and derives turn speed from it.
    /// </summary>
    public class RotationTracker : IRotationInfo
    {
        private readonly Transform _cachedTransform;

        private float _lastYRotation;
        private float _turnSpeed;

        public float TurnSpeed => _turnSpeed;

        public RotationTracker(Transform transform)
        {
            _cachedTransform = transform;
            _lastYRotation = transform.localEulerAngles.y;
        }

        public void UpdateTurnSpeed()
        {
            float currentYRotation = _cachedTransform.localEulerAngles.y;
            _turnSpeed = Mathf.Abs(currentYRotation - _lastYRotation);
            _lastYRotation = currentYRotation;
        }
    }
}