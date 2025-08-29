using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Calculates the speed multiplier when character is climbing hills.
    /// </summary>
    public class SlopeSpeedMultiplierCalculator
    {
        private const float SurfaceSlopeThreshold = 5f;

        private readonly CharacterMotorConfig _motorConfig;
        private readonly CharacterController _characterController;

        public SlopeSpeedMultiplierCalculator(CharacterController characterController, CharacterMotorConfig characterMotorConfig)
        { 
            _characterController = characterController;
            _motorConfig = characterMotorConfig; 
        }

        /// <summary>
        /// Calculates the slope multiplier to modify the speed of the character when it is moving
        /// through sloped surfaces, if it is uphill it reduces speed.
        /// </summary>
        /// <param name="isGrounded">Is character currently on the ground?</param>
        /// <param name="surfaceAngle">The angle slope of the surface character is standing on.</param>
        /// <param name="slopeLimit">The limit of maximum angle of the slope character can be standing on.</param>
        /// <param name="groundNormal">Normal of the ground character is standing on.</param>
        /// <param name="velocity">Current movement vector of the character.</param>
        /// <returns>Multiplier to adjust speed to factor in moving uphill or downhill.</returns>
        public float CalculateSpeedMultiplier(
            bool isGrounded, 
            float surfaceAngle, 
            Vector3 groundNormal, 
            Vector3 velocity)
        {
            // Character airborne or the surface is nearly flat
            if(isGrounded == false || surfaceAngle <= SurfaceSlopeThreshold)
            {
                return 1f;
            }

            // -0.03f is to factor in very small uneven surfaces, that are nearly flat
            bool isAscendingSlope = Vector3.Dot(groundNormal, velocity) < -0.03f;

            if(isAscendingSlope)
            {
                // As surface angle increases towards the slope limit
                // penalty increases, and slope speed multiplier gets smaller
                float speedPenaltyScale = surfaceAngle / _characterController.slopeLimit;
                return _motorConfig.SlopedSpeedModifier.Evaluate(speedPenaltyScale);
            }

            return 1f;
        }
    }
}