using UnityEngine;

namespace Nexora.FPSDemo.Footsteps
{
    /// <summary>
    /// Configuration settings used for footsteps, from the collision mask to raycast settings to audio volumes.
    /// </summary>
    [CreateAssetMenu(menuName = "Nexora/Movement/Footstep Config", fileName = "FoostepConfig_")]
    public sealed class FootstepConfig : ScriptableObject
    {
        [Title("Ground Detection")]
        [Tooltip("Layer mask of the surface where footsteps should be detected.")]
        [SerializeField]
        private LayerMask _surfaceLayerMask = Layers.SimpleSolidObjectsMask;

        [Tooltip("Raycast distance to check ground with.")]
        [SerializeField, Range(0.01f, 1f)]
        private float _groundCheckDistance = 0.3f;

        [Tooltip("Radius of the sphere ray to check ground with.")]
        [SerializeField, Range(0.01f, 0.5f)]
        private float _groundCheckRadius = 0.3f;

        [Tooltip("Offset of the raycast if needed to not shoot rays from the center.")]
        [SerializeField, Range(0f, 1f)]
        private float _raycastStartOffset = 0f;

        [Title("Footstep Audio")]
        [Tooltip("Minimum speed that is audible, that is footstep volume will be played.")]
        [SerializeField, Range(0f, 50f)]
        private float _minAudibleSpeed = 1f;

        [Tooltip("Speed above which the footstep audio will be played at full volume.")]
        [SerializeField, Range(0f, 50f)]
        private float _maxVolumeSpeed = 5f;

        [Tooltip("Minimum volume for the footstep audio.")]
        [SerializeField, Range(0f, 1f)]
        private float _minFootstepVolume = 0.1f;

        [Tooltip("Maximum volume for the footstep audio.")]
        [SerializeField, Range(0f, 1f)]
        private float _maxFootstepVolume = 1f;

        [Title("Fall Impact")]
        [Tooltip("Minimum value of speed for playing impact effect, if speed greater than that effects will be played.")]
        [SerializeField, Range(0f, 30f)]
        private float _minImpactSpeed = 4f;

        [Tooltip("Value above which impact volume will be played at full.")]
        [SerializeField, Range(0f, 30f)]
        private float _maxImpactSpeed = 11f;

        [Tooltip("Cooldown for impact effects can be triggered (to prevent extreme amount of impact effects when player is stuck in a space).")]
        [SerializeField, Range(0f, 2f)]
        private float _impactCooldown = 0.3f;

        /// <summary>
        /// Layer mask of the surface where footsteps should be detected.
        /// </summary>
        public LayerMask SurfaceLayerMask => _surfaceLayerMask;

        /// <summary>
        /// Raycast distance to check ground with.
        /// </summary>
        public float GroundCheckDistance => _groundCheckDistance;

        /// <summary>
        /// Radius of the sphere ray to check ground with.
        /// </summary>
        public float GroundCheckRadius => _groundCheckRadius;

        /// <summary>
        /// Offset of the raycast if needed to not shoot rays from the center.
        /// </summary>
        public float RaycastStartOffset => _raycastStartOffset;

        /// <summary>
        /// Minimum speed that is audible, that is footstep volume will be played.
        /// </summary>
        public float MinAudibleSpeed => _minAudibleSpeed;

        /// <summary>
        /// Speed above which the footstep audio will be played at full volume.
        /// </summary>
        public float MaxVolumeSpeed => _maxVolumeSpeed;

        /// <summary>
        /// Minimum volume for the footstep audio.
        /// </summary>
        public float MinFootstepVolume => _minFootstepVolume;

        /// <summary>
        /// Maximum volume for the footstep audio.
        /// </summary>
        public float MaxFootstepVolume => _maxFootstepVolume;

        /// <summary>
        /// Minimum value of speed for playing impact effect, if speed greater than that effects will be played.
        /// </summary>
        public float MinImpactSpeed => _minImpactSpeed;

        /// <summary>
        /// Value above which impact volume will be played at full.
        /// </summary>
        public float MaxImpactSpeed => _maxImpactSpeed;

        /// <summary>
        /// Cooldown for impact effects can be triggered (to prevent extreme amount of impact effects when player is stuck in a space).
        /// </summary>
        public float ImpactCooldown => _impactCooldown;

        public void Validate()
        {
            _maxFootstepVolume = Mathf.Max(_maxFootstepVolume, _minFootstepVolume);
            _maxVolumeSpeed = Mathf.Max(_maxVolumeSpeed, _minAudibleSpeed);
            _maxImpactSpeed = Mathf.Max(_maxImpactSpeed, _minImpactSpeed);
        }
    }
}