using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Provides processed movement input data and handles input state management.
    /// </summary>
    public interface IMovementInputController
    {
        /// <summary>
        /// Raw 2D input data from control device.
        /// </summary>
        Vector2 RawMovementDirection { get; }

        /// <summary>
        /// World-space processed movement vector.
        /// </summary>
        Vector3 WorldMovementDirection { get; }

        bool IsRunningHeld { get; }
        bool IsCrouchingHeld { get; }
        bool IsProningHeld { get; }
        bool IsJumpPressed { get; }

        void ConsumeRunInput();
        void ConsumeCrouchInput();
        void ConsumeProneInput();
        void ConsumeJumpInput();
    }
}