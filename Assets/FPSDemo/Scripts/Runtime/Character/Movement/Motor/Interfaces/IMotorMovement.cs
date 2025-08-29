using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Interface that is to handle functionality of movement of the motor.
    /// </summary>
    public interface IMotorMovement
    {
        /// <summary>
        /// The actual velocity of the character (how much moved in the last frame).
        /// </summary>
        /// <remarks>
        /// This is different from the simulated velocity, like simulated velocity
        /// is what we want to do with the input give (e.g move 5 unit right). But if
        /// there is a wall, then our actual velocity is (0,0,0), even though simulated velocity is (5,0,0)
        /// </remarks>
        Vector3 Velocity { get; }

        /// <summary>
        /// The intended movement of the character, calculated through environment and state of the character.
        /// </summary>
        Vector3 SimulatedVelocity { get; }

        /// <summary>
        /// Is currently touching the ground?
        /// </summary>
        bool IsGrounded { get; }

        /// <summary>
        /// Last time the 'grounded' was changed?
        /// </summary>
        float LastGroundedChangeTime { get; }

        /// <summary>
        /// Collision flags of the character when it hits with something in the last frame.
        /// </summary>
        CollisionFlags CollisionFlags { get; }

        /// <summary>
        /// Sets the movement input function where we feed in our current velocity
        /// and it will return modified velocity along with status information
        /// to update the motor.
        /// </summary>
        /// <param name="motionInput">Delegate that handles movement.</param>
        void SetMovementInputFunction(MovementInputDelegate motionInput);

        /// <returns>If has a valid input feed?</returns>
        bool HasInput();

        /// <summary>
        /// Clears all movement of the character, and stands still.
        /// </summary>
        void ResetMotorMovement();

        /// <summary>
        /// Teleports character to <paramref name="position"/> with
        /// having <paramref name="rotation"/> at the end position.
        /// </summary>
        void Teleport(Vector3 position, Quaternion rotation);

        event UnityAction<bool> GroundedChanged;
        event UnityAction<float> FallImpact;
        event UnityAction Teleported;
    }
}