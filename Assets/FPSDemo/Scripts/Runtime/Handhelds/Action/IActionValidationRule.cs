using Nexora.FPSDemo.Movement;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Represents a single condition that must be met for an action to be valid.
    /// </summary>
    public interface IActionValidationRule
    {
        /// <summary>
        /// Initializes the rule with necessary character components.
        /// </summary>
        void Initialize(ICharacter character);

        /// <returns>If validation conditions are met.</returns>
        bool IsValid();
    }

    [Serializable]
    public sealed class GroundedRule : IActionValidationRule
    {
        [SerializeField]
        private bool _mustBeGrounded = true;

        private ICharacterMotor _characterMotor;

        public void Initialize(ICharacter character) => _characterMotor = character.GetCC<ICharacterMotor>();
        public bool IsValid() => _mustBeGrounded == _characterMotor.IsGrounded;
    }

    [Serializable]
    public sealed class MovementStateRule : IActionValidationRule
    {
        [ReorderableList]
        [SerializeField]
        private MovementStateType[] _disallowedStates = Array.Empty<MovementStateType>();

        private IMovementController _movementController;

        public void Initialize(ICharacter character) => _movementController = character.GetCC<IMovementController>();
        public bool IsValid() => _disallowedStates.Contains(_movementController.ActiveStateType) == false;
    }

    [Serializable]
    public sealed class MaxSpeedRule : IActionValidationRule
    {
        [SerializeField, Range(0f, 100f)]
        private float _maxAllowedSpeed = 5f;

        private ICharacterMotor _characterMotor;

        public void Initialize(ICharacter character) => _characterMotor = character.GetCC<ICharacterMotor>();
        public bool IsValid() => _characterMotor.Velocity.magnitude < _maxAllowedSpeed;
    }
}