using Nexora.FPSDemo.Movement;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    [RequireCharacterBehaviour(typeof(ICharacterMotor))]
    [DefaultExecutionOrder(ExecutionOrder.AfterDefaultPresentation)]
    public sealed class CharacterYPositionSmoother : CharacterBehaviour
    {
        [SerializeField, Range(0f, 50f)]
        private float _groundedLerpSpeed = 20f;

        [SerializeField, Range(50f, 200f)]
        private float _airborneLerpSpeed = 100f;

        private Transform _cachedTransform;
        private Transform _motorTransform;

        private ICharacterMotor _characterMotor;

        private AdaptiveSmoother _ySmoother;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _characterMotor = parent.GetCC<ICharacterMotor>();
            _motorTransform = _characterMotor.transform;
            _cachedTransform = transform;

            _ySmoother = new AdaptiveSmoother(_groundedLerpSpeed, _airborneLerpSpeed);

            ResetYPosition();
        }

        protected override void OnBehaviourEnable(ICharacter parent) => _characterMotor.Teleported += ResetYPosition;
        protected override void OnBehaviourDisable(ICharacter parent) => _characterMotor.Teleported -= ResetYPosition;

        private void ResetYPosition()
        {
            if(_characterMotor != null && _ySmoother != null)
            {
                _ySmoother.SetValue(_motorTransform.position.y);
            }
        }

        private void LateUpdate()
        {
            float smoothedY = _ySmoother.Update(_characterMotor.IsGrounded, _motorTransform.position.y, Time.deltaTime);

            _cachedTransform.position = _motorTransform.position.WithY(smoothedY);
        }

        private sealed class AdaptiveSmoother
        {
            private readonly float _groundedSpeed;
            private readonly float _airborneSpeed;
            private readonly float _speedChangeRate = 10f;

            private float _currentValue;
            private float _currentSpeed;

            public AdaptiveSmoother(float groundedSpeed, float airborneSpeed)
            {
                _groundedSpeed = groundedSpeed;
                _airborneSpeed = airborneSpeed;
            }

            public void SetValue(float value) => _currentValue = value;

            public float Update(bool isGrounded, float targetValue, float deltaTime)
            {
                float targetSpeed = isGrounded ? _groundedSpeed : _airborneSpeed;
                _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, deltaTime * _speedChangeRate);

                _currentValue = Mathf.Lerp(_currentValue, targetValue, _currentSpeed * deltaTime);

                return _currentValue;
            }
        }
    }

    
}