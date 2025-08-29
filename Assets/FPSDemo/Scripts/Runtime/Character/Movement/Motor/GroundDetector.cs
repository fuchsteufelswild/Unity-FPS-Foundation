using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Helper class for determining information about the surface we are standing on.
    /// Provides methods for checking how much of a downward translation should be done
    /// to ground, and provides information about the stepness of the surface.
    /// </summary>
    public sealed class GroundDetector : IGroundDetector
    {
        private readonly CharacterController _characterController;
        private readonly CharacterMotorConfig _motorConfig;

        /// <summary>
        /// The maximum distance a character might automatically climb without
        /// needing to jump.
        /// </summary>
        /// <remarks>
        /// Stores the initial value.
        /// </remarks>
        private readonly float _defaultStepOffset;

        public Vector3 GroundNormal { get; private set; }
        public float GroundSurfaceAngle => Vector3.Angle(Vector3.up, GroundNormal);
        public float StepOffset => _defaultStepOffset;

        public GroundDetector(CharacterController characterController, CharacterMotorConfig motorConfig)
        {
            _characterController = characterController;
            _motorConfig = motorConfig;
            _defaultStepOffset = characterController.stepOffset;
        }

        public void UpdateGroundNormal(Vector3 normal) => GroundNormal = normal;
        public bool IsOnSlope() => GroundSurfaceAngle > _motorConfig.SlideThresholdAngle;

        public float CalculateGroundingTranslation(Vector3 position, float groundingForce, ref bool applyGravity)
        {
            float distanceToGround = 0.001f;

            var ray = new Ray(position, Vector3.down);

            if(PhysicsUtils.RaycastOptimized(ray, _defaultStepOffset, out RaycastHit hit, _motorConfig.CollisionMask))
            {
                distanceToGround = hit.distance;
            }
            else
            {
                applyGravity = true;
            }

            return distanceToGround * groundingForce;
        }
    }
}