using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Interface for calculating sliding velocity given the ground information.
    /// </summary>
    public interface ISlopeSlider
    {
        /// <summary>
        /// Applies slide velocity to go down when character is going uphill and
        /// fades the velocity when character in on the flat ground.
        /// </summary>
        /// <param name="isOnSlope">Is currently standing on a slope?</param>
        /// <param name="groundNormal">Normal of the ground character is standing on.</param>
        /// <returns>How much velocity does sliding generates.</returns>
        Vector3 CalculateSlideVelocity(bool isOnSlope, Vector3 groundNormal, float deltaTime);

        /// <summary>
        /// Reset sliding velocity.
        /// </summary>
        void Reset();
    }
}