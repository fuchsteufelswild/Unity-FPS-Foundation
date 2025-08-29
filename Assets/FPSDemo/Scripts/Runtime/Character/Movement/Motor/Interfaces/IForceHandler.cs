using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Receives forces applied to the motor and accumulates them.
    /// </summary>
    public interface IForceHandler
    {
        /// <summary>
        /// Adds the given <paramref name="force"/> with <paramref name="forceMode"/>
        /// to the accumulated external force. Updates snap to ground setting as well.
        /// </summary>
        void AddForce(Vector3 force, ForceMode forceMode, bool snapToGround);

        /// <remarks>
        /// This might happen in the cases of explosion, when character needs to fly to the air, or ragdoll.
        /// </remarks>
        /// <returns>Last applied force requires the character to disable snapping to ground?</returns>
        public bool ShouldDisableSnapToGround();

        /// <returns>Accumulated external force.</returns>
        Vector3 ConsumeExternalForce();

        /// <summary>
        /// Resets the accumulated force.
        /// </summary>
        void Reset();
    }
}