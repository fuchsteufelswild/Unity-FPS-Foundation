using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    [CreateAssetMenu(menuName = "Nexora/Movement/Character Motor Config", fileName = "MotorConfig_")]
    public sealed class CharacterMotorConfig : ScriptableObject
    {
        [Help("Gravity affecting on the player.")]
        [SerializeField, Range(0f, 100f)]
        private float _gravity = 15f;

        [Help("Mass of the character, affects physics calculations.")]
        [SerializeField, Range(20f, 160f)]
        private float _mass = 60f;

        [Help("How strong the character should be sticked to the ground.")]
        [SerializeField, Range(0f, 100f)]
        private float _groundingForce = 2.5f;

        [Help("How step should terrain should be so that it will start sliding?")]
        [SerializeField, Range(20f, 90f)]
        private float _slideThresholdAngle = 30f;

        [Help("How fast does character slide?")]
        [SerializeField, Range(0f, 2f)]
        private float _slideSpeed = 0.1f;

        [Help("Animation curve to get speed multiplier when character is climbing an ascending slope (moving uphill).")]
        [SerializeField]
        private AnimationCurve _slopeSpeedModifier;

        [Help("Which layer masks does character collide with?")]
        [SerializeField]
        private LayerMask _collisionMask;

        [Help("Pushing force to apply to any non-kinematic body that collides with the character.")]
        [SerializeField, Range(0f, 10f)]
        private float _pushForce = 1f;

        /// <summary>
        /// Gravity affecting on the player.
        /// </summary>
        public float Gravity => _gravity;

        /// <summary>
        /// Mass of the character, affects physics calculations.
        /// </summary>
        public float Mass => _mass;

        /// <summary>
        /// How strong the character should be sticked to the ground.
        /// </summary>
        public float GroundingForce => _groundingForce;

        /// <summary>
        /// How step should terrain should be so that it will start sliding?
        /// </summary>
        public float SlideThresholdAngle => _slideThresholdAngle;

        /// <summary>
        /// How fast does character slide?
        /// </summary>
        public float SlideSpeed => _slideSpeed;

        /// <summary>
        /// Animation curve to get speed multiplier when character is climbing an ascending slope (moving uphill).
        /// </summary>
        public AnimationCurve SlopedSpeedModifier => _slopeSpeedModifier;

        /// <summary>
        /// Which layer masks does character collide with?
        /// </summary>
        public LayerMask CollisionMask => _collisionMask;

        /// <summary>
        /// Pushing force to apply to any non-kinematic body that collides with the character.
        /// </summary>
        public float PushForce => _pushForce;
    }
}