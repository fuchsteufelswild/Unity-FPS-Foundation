using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="useGravity"></param>
    /// <param name="snapToGround"></param>
    /// <returns></returns>
    public delegate Vector3 MovementInputDelegate(Vector3 velocity, out bool useGravity, out bool snapToGround);

    public interface ICharacterMotor : 
        ICharacterBehaviour,
        ICharacterPhysics,
        IMotorMovement
    {
        /// <inheritdoc cref="IGroundDetector.GroundNormal"/>
        Vector3 GroundNormal { get; }

        /// <inheritdoc cref="IGroundDetector.GroundSurfaceAngle"/>
        float GroundSurfaceAngle { get; }

        /// <inheritdoc cref="CharacterController.slopeLimit"/>
        float SlopeLimit { get; }

        /// <inheritdoc cref="IRotationInfo.TurnSpeed"/>
        float TurnSpeed { get; }

        /// <summary>
        /// Pushing force to apply to any non-kinematic body that collides with the character.
        /// </summary>
        float PushForce { get; }

        /// <summary>
        /// Add external force to the character.
        /// </summary>
        /// <param name="force">Applied force.</param>
        /// <param name="forceMode">Mode to denote how to apply force.</param>
        /// <param name="snapToGround">Should this force snaps character to the ground?</param>
        void AddForce(Vector3 force, ForceMode forceMode, bool snapToGround = false);

        /// <inheritdoc cref="SlopeSpeedMultiplierCalculator.CalculateSpeedMultiplier(bool, float, Vector3, Vector3)"/>
        float GetSlopeSpeedMultiplier();
    }
}