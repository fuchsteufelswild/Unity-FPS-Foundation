using System;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    [Serializable]
    public readonly struct StaminaThresholds
    {
        public readonly float HeavyBreathingThreshold;
        public readonly float DisableMovementThreshold;
        public readonly float EnableMovementThreshold;

        public StaminaThresholds(float? heavyBreathing, float? disableMovement, float? enableMovement)
        {
            HeavyBreathingThreshold = heavyBreathing ?? 0.05f;
            DisableMovementThreshold = disableMovement ?? 0.01f;
            EnableMovementThreshold = enableMovement ?? 0.2f;
        }
    }

    public interface IStaminaMovementHandler
    {
        /// <summary>
        /// Handles enabling/disabling movement depending on <paramref name="currentStamina"/>.
        /// <br></br>
        /// Disables movement if stamina gets below a threhsold.
        /// <br></br>
        /// Enables movement above a certain threshold, <b>when it was disabled</b>.
        /// </summary>
        /// <param name="currentStamina">Current stamina value.</param>
        /// <param name="thresholds">Threshold configuration for controlling movement depending on <paramref name="currentStamina"/>.</param>
        void HandleMovementBlocking(float currentStamina, in StaminaThresholds thresholds);
    }

    /// <summary>
    /// Handles the stamina's relationship with the movement of the character.
    /// Disables/enables movement using the stamina values.
    /// </summary>
    public sealed class StaminaMovementHandler : IStaminaMovementHandler
    {
        private readonly IMovementController _movementController;
        private readonly UnityEngine.Object _blockerSource;

        private bool _isMovementBlocked;

        public StaminaMovementHandler(IMovementController movementController, UnityEngine.Object blockerSource)
        {
            _movementController = movementController;
            _blockerSource = blockerSource;
        }

        public void HandleMovementBlocking(float currentStamina, in StaminaThresholds thresholds)
        {
            switch (_isMovementBlocked)
            {
                case false when currentStamina < thresholds.DisableMovementThreshold:
                    _movementController.AddStateBlocker(_blockerSource, MovementStateType.Jump);
                    _movementController.AddStateBlocker(_blockerSource, MovementStateType.Run);
                    _isMovementBlocked = true;
                    break;
                case true when currentStamina > thresholds.EnableMovementThreshold:
                    _movementController.RemoveStateBlocker(_blockerSource, MovementStateType.Jump);
                    _movementController.RemoveStateBlocker(_blockerSource, MovementStateType.Run);
                    _isMovementBlocked = false;
                    break;
            }
        }
    }
}