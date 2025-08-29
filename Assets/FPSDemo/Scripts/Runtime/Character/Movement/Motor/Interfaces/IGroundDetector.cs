using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Interface for determining information about the surface we are standing on.
    /// Provides methods for checking how much of a downward translation should be done
    /// to ground, and provides information about the stepness of the surface.
    /// </summary>
    public interface IGroundDetector
    {
        /// <summary>
        /// Normal of the ground surface currently standing on.
        /// </summary>
        Vector3 GroundNormal { get; }

        /// <summary>
        /// Gets angle to determine stepness of the surface stayed on top of.
        /// </summary>
        float GroundSurfaceAngle { get; }

        /// <summary>
        /// The maximum distance a character might automatically climb without
        /// needing to jump.
        /// </summary>
        float StepOffset { get; }

        /// <summary>
        /// Calculates how much downward movement should be done for grounding.
        /// </summary>
        /// <remarks>
        /// Prevents character floating with small step offset adjustments.
        /// <br></br>
        /// Ensures character sticks to the ground to uneven terrains naturally.
        /// </remarks>
        /// <param name="position">Position of the character.</param>
        /// <param name="groundingForce">How aggressively character sticks to the ground.</param>
        /// <returns></returns>
        float CalculateGroundingTranslation(Vector3 position, float groundingForce, ref bool applyGravity);

        /// <summary>
        /// Is current ground is sloped?
        /// </summary>
        /// <returns></returns>
        bool IsOnSlope();

        /// <summary>
        /// Updates ground normal to be value <paramref name="normal"/>.
        /// </summary>
        void UpdateGroundNormal(Vector3 normal);
    }
}