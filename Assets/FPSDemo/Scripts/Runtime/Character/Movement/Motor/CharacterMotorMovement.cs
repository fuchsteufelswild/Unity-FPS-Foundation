using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    public sealed class CharacterMotorMovement : IMotorMovement
    {
        // Components
        private CharacterMotorConfig _motorConfig;
        private Transform _cachedTransform;
        private CharacterController _characterController;

        // Systems
        private IForceHandler _forceHandler;
        private ISlopeSlider _slopeSlider;
        private IGroundDetector _groundDetector;

        private MovementInputDelegate _movementInput;

        private Vector3 _velocity;
        private Vector3 _simulatedVelocity;
        private bool _isGrounded = true;
        private float _lastGroundedChangeTime;
        private CollisionFlags _collisionFlags;

        private Vector3 _lastPosition;

        private bool _applyGravity;
        private bool _snapToGround;

        public event UnityAction<bool> GroundedChanged;
        public event UnityAction<float> FallImpact;
        public event UnityAction Teleported;

        public CharacterMotorMovement(
            Transform characterTransform, 
            CharacterController characterController,
            CharacterMotorConfig motorConfig,
            IForceHandler forceHandler,
            ISlopeSlider slopeSlider,
            IGroundDetector groundDetector)
        {
            _cachedTransform = characterTransform;
            _characterController = characterController;
            _motorConfig = motorConfig;
            _forceHandler = forceHandler;
            _slopeSlider = slopeSlider;
            _groundDetector = groundDetector;
        }

        public Vector3 Velocity => _velocity;
        public Vector3 SimulatedVelocity => _simulatedVelocity;
        public bool IsGrounded => _isGrounded;
        public float LastGroundedChangeTime => _lastGroundedChangeTime;
        public CollisionFlags CollisionFlags => _collisionFlags;

        /// <summary>
        /// Last position calculated after the end of the last frame's motor movement process.
        /// </summary>
        public Vector3 LastPosition => _lastPosition;

        /// <summary>
        /// Last the data from the save when initialized through save.
        /// </summary>
        public void LoadSaveData(Vector3 velocity, Vector3 simulatedVelocity, bool isGrounded, Vector3 lastPosition)
        {
            _velocity = velocity;
            _simulatedVelocity = simulatedVelocity;
            _isGrounded = isGrounded;
            _lastPosition = lastPosition;
        }

        public void SetMovementInputFunction(MovementInputDelegate motionInput) => _movementInput = motionInput;
        public bool HasInput() => _movementInput != null;

        /// <summary>
        /// Processes the movement for the current state.
        /// Calculates simulated velocity, tries to move the character,
        /// then updates the actual velocity and grounded state as a result of the movement.
        /// </summary>
        public void ProcessMovement(float deltaTime)
        {
            bool wasGrounded = _isGrounded;

            _simulatedVelocity = _movementInput.Invoke(_simulatedVelocity, out _applyGravity, out _snapToGround);

            if(ShouldApplyGroundEffects(wasGrounded))
            {
                _simulatedVelocity += CalculateSlideVelocity(deltaTime);
            }

            // Apply external forces
            _simulatedVelocity += _forceHandler.ConsumeExternalForce();

            ExecuteMovement(wasGrounded, deltaTime);
            UpdateMotorState(deltaTime);
        }

        /// <summary>
        /// Should apply any effects of calculation that are related to be on ground?
        /// Like sliding, grounding force etc.
        /// </summary>
        private bool ShouldApplyGroundEffects(bool wasGrounded)
        {
            return _forceHandler.ShouldDisableSnapToGround() == false
                && wasGrounded
                && _snapToGround;
        }

        private Vector3 CalculateSlideVelocity(float deltaTime)
        {
            return _slopeSlider.CalculateSlideVelocity(_groundDetector.IsOnSlope(), _groundDetector.GroundNormal, deltaTime);
        }

        /// <summary>
        /// Applies the translation using the calculated simulated velocity.
        /// </summary>
        /// <param name="wasGrounded">Was character grounded the last frame.</param>
        private void ExecuteMovement(bool wasGrounded, float deltaTime)
        {
            float groundingTranslation = 0;

            if (ShouldApplyGroundEffects(wasGrounded))
            {
                groundingTranslation = _groundDetector.CalculateGroundingTranslation(
                    _cachedTransform.position,
                    _motorConfig.GroundingForce,
                    ref _applyGravity);
            }

            if (_applyGravity)
            {
                _simulatedVelocity.y -= _motorConfig.Gravity * deltaTime;
            }

            Vector3 translation = _simulatedVelocity * deltaTime;
            translation.y -= groundingTranslation;

            _collisionFlags = _characterController.Move(translation);
        }

        /// <summary>
        /// Updates state of the motor, new velocity, grounded state etc.
        /// </summary>
        private void UpdateMotorState(float deltaTime)
        {
            bool wasGrounded = _isGrounded;

            _velocity = (_cachedTransform.position - _lastPosition) / deltaTime;
            _isGrounded = _characterController.isGrounded;

            if(wasGrounded != _isGrounded)
            {
                HandleGroundedStateChange(wasGrounded);
            }

            _lastPosition = _cachedTransform.position;
        }

        /// <summary>
        /// Handles the when grounded state of the character changed.
        /// </summary>
        /// <param name="wasGrounded">Was character grounded before this frame?</param>
        private void HandleGroundedStateChange(bool wasGrounded)
        {
            GroundedChanged?.Invoke(_isGrounded);
            _lastGroundedChangeTime = Time.time;

            if(wasGrounded == false && _isGrounded)
            {
                FallImpact?.Invoke(Mathf.Abs(_simulatedVelocity.y));
                _simulatedVelocity.y = 0;
            }
        }

        public void ResetMotorMovement()
        {
            _velocity = Vector3.zero;
            _simulatedVelocity = Vector3.zero;
            _lastPosition = _cachedTransform.position;
            _isGrounded = true;

            _slopeSlider.Reset();
            _forceHandler.Reset();
            _groundDetector.UpdateGroundNormal(Vector3.zero);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _characterController.enabled = false;

            rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
            position.WithY(position.y + _characterController.skinWidth);
            _lastPosition = position;
            _cachedTransform.SetPositionAndRotation(position, rotation);

            _characterController.enabled = true;

            Teleported?.Invoke();
        }
    }
}