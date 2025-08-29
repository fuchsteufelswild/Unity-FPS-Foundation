using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Class that controls steps of the character using characters horizontal movement
    /// and self rotation around its Y-axis.
    /// </summary>
    /// <remarks>
    /// Can be used for any class that wants to track character's steps, like for footstep audios
    /// or to inform AI etc.
    /// </remarks>
    public sealed class StepCycleManager : IStepCycleManager
    {
        private MovementControllerConfig _controllerConfig;
        private ICharacterMotor _characterMotor;
        private Func<ICharacterMovementState> _getActiveState;

        private float _distanceMovedSinceLastCycle;
        private float _currentStepLength = 1f;

        public float StepCycle { get; private set; }
        public event UnityAction StepCycleEnded;

        public StepCycleManager() { }

        public StepCycleManager(
            MovementControllerConfig controllerConfig, 
            ICharacterMotor characterMotor, 
            Func<ICharacterMovementState> getActiveState)
        {
            _controllerConfig = controllerConfig;
            _characterMotor = characterMotor;
            _getActiveState = getActiveState;
        }

        public void Initialize(
            MovementControllerConfig controllerConfig,
            ICharacterMotor characterMotor,
            Func<ICharacterMovementState> getActiveState)
        {
            _controllerConfig = controllerConfig;
            _characterMotor = characterMotor;
            _getActiveState = getActiveState;
        }

        /// <summary>
        /// Ticks the step cycle one frame forward. Updates step based on
        /// movement and rotation, and forwards a step cycle ahead if its completed
        /// depending on the current state.
        /// </summary>
        public void TickStepCycle(float deltaTime)
        {
            // Cannot step in the air or in the water
            if(_characterMotor.IsGrounded == false)
            {
                return;
            }
            
            // Cannot step in an invalid state
            ICharacterMovementState activeState = _getActiveState();
            if(activeState == null)
            {
                return;
            }

            UpdateMovementBasedStep(deltaTime);
            UpdateTurnBasedStep(deltaTime);
            CheckCycleCompletion(deltaTime, activeState);
        }

        /// <summary>
        /// Updates the step distance using the horizontal movement of the character.
        /// </summary>
        private void UpdateMovementBasedStep(float deltaTime)
        {
            // Only take horizontal to avoid spurious step sound when falling or sliding
            float horizontalStep = _characterMotor.Velocity.Horizontal().magnitude;
            _distanceMovedSinceLastCycle += horizontalStep * deltaTime;
        }

        /// <summary>
        /// Updates the step distance using the rotation of the character.
        /// </summary>
        private void UpdateTurnBasedStep(float deltaTime)
        {
            float clampedTurnSpeed = Mathf.Clamp(_characterMotor.TurnSpeed, 0f, _controllerConfig.MaxTurnStepSpeed);
            _distanceMovedSinceLastCycle += clampedTurnSpeed * _controllerConfig.TurnStepLength * deltaTime;
        }

        /// <summary>
        /// Blends the step length when transitioning states and checks if the current step cycle
        /// is completed.
        /// </summary>
        /// <param name="activeState">Currently active movement state.</param>
        private void CheckCycleCompletion(float deltaTime, ICharacterMovementState activeState)
        {
            float targetStepLength = Mathf.Max(activeState.StepCycleLength, 1f);
            _currentStepLength = Mathf.MoveTowards(_currentStepLength, targetStepLength, deltaTime * _controllerConfig.StepLerpSpeed);

            if (_distanceMovedSinceLastCycle > _currentStepLength)
            {
                _distanceMovedSinceLastCycle -= _currentStepLength;
                StepCycleEnded?.Invoke();
            }

            StepCycle = _distanceMovedSinceLastCycle / _currentStepLength;
        }
    }
}