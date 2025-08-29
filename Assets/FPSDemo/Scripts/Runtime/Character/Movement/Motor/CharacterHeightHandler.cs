using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Handles the character's height and updates the center accordingly.
    /// Makes changes on <see cref="CharacterController"/>.
    /// </summary>
    public class CharacterHeightHandler : ICharacterPhysics
    {
        private readonly CharacterController _characterController;
        private readonly CharacterMotorConfig _motorConfig;
        private readonly float _defaultHeight;

        public event UnityAction<float> HeightChanged;

        public CharacterHeightHandler(CharacterController characterController, CharacterMotorConfig motorConfig)
        {
            _characterController = characterController;
            _motorConfig = motorConfig;
            _defaultHeight = characterController.height;
        }

        public float Height
        {
            get => _characterController.height;
            set
            {
                if (Mathf.Abs(_characterController.height - value) < 0.01f)
                {
                    return;
                }

                _characterController.height = value;
                _characterController.center = Vector3.up * (value * 0.5f);
                HeightChanged?.Invoke(value);
            }
        }

        public float Radius => _characterController.radius;
        public float DefaultHeight => _defaultHeight;
        public float Gravity => _motorConfig.Gravity;
        public float Mass => _motorConfig.Mass;
        public LayerMask CollisionMask => _motorConfig.CollisionMask;

        public bool CanSetHeight(float targetHeight)
        {
            if (Mathf.Abs(Height - targetHeight) < 0.01f)
            {
                return true;
            }

            if (Height < targetHeight)
            {
                return !CheckCanStand(targetHeight - Height + 0.1f);
            }

            return true;
        }

        /// <summary>
        /// Shoots ray through the ceiling to see if any object blocks its standing path.
        /// </summary>
        /// <returns>If it is safe to stand.</returns>
        private bool CheckCanStand(float maxDistance)
        {
            Vector3 rayOrigin = _characterController.transform.position
                + Vector3.up * _characterController.height * 0.5f;

            Vector3 rayDirection = Vector3.up;

            return Physics.SphereCast(
                new Ray(rayOrigin, rayDirection),
                _characterController.radius,
                maxDistance,
                _motorConfig.CollisionMask,
                QueryTriggerInteraction.Ignore);
        }
    }
}