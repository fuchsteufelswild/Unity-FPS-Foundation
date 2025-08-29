using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Provides information about the motor's current motor rotation.
    /// </summary>
    public interface IRotationInfo
    {
        /// <summary>
        /// How fast is motor is turning.
        /// </summary>
        float TurnSpeed { get; }

        /// <summary>
        /// Updates turn speed as how much Y-rotation changed since the last call.
        /// </summary>
        void UpdateTurnSpeed();
    }
}