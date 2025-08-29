using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Interface to handle collision of character with other objects.
    /// </summary>
    public interface ICollisionHandler
    {
        /// <summary>
        /// Applies push force in the event of collision with <paramref name="hit"/>.
        /// </summary>
        /// <remarks>
        /// Applies push if the <paramref name="hit"/> has valid non-kinematic rigidbody, 
        /// has layer in the collision mask, and it is not falling.
        /// </remarks>
        /// <param name="hit">Object which the character hit.</param>
        void HandleControllerCollision(ControllerColliderHit hit);
    }
}