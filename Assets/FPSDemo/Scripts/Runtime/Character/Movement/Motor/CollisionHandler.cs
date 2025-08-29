using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Handles collision of character with other objects. Applies push in the event of collision.
    /// </summary>
    public class CollisionHandler : ICollisionHandler
    {
        private readonly CharacterMotorConfig _motorConfig;

        public CollisionHandler(CharacterMotorConfig characterMotorConfig) => _motorConfig = characterMotorConfig;

        public void HandleControllerCollision(ControllerColliderHit hit)
        {
            // Has no rigidbody or a kinematic body
            Rigidbody body = hit.collider.attachedRigidbody;
            if(body == null || body.isKinematic)
            {
                return;
            }

            // Not a collidable that can push
            var bodyLayerMask = 1 << body.gameObject.layer;
            if((bodyLayerMask | _motorConfig.CollisionMask) == 0)
            {
                return;
            }

            // If the character was falling or on the ground
            if(hit.moveDirection.y < -0.3f)
            {
                return;
            }

            Vector3 pushDirection = hit.moveDirection.Horizontal();
            body.AddForce(pushDirection * _motorConfig.PushForce, ForceMode.Impulse);
        }
    }
}