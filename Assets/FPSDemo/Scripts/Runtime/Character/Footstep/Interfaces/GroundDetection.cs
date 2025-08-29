using UnityEngine;


namespace Nexora.FPSDemo.Footsteps
{
    public readonly struct GroundDetectionData
    {
        public readonly bool IsValid;
        public readonly RaycastHit Hit;

        public GroundDetectionData(bool isValid, RaycastHit hit)
        {
            IsValid = isValid;
            Hit = hit;
        }

        public static GroundDetectionData Invalid => new(false, default);
        public static GroundDetectionData Valid(in RaycastHit hit) => new(true, hit);
    }

    public interface IGroundDetection
    {
        /// <summary>
        /// Detects the ground under the controller.
        /// </summary>
        /// <returns>Result of the ground detection.</returns>
        GroundDetectionData DetectGround();
    }

    /// <summary>
    /// Class for detecting if controller is standing on a ground.
    /// </summary>
    public sealed class GroundDetection : IGroundDetection
    {
        private readonly FootstepConfig _footstepConfig;
        private readonly Transform _cachedTransform;

        public GroundDetection(FootstepConfig footstepConfig, Transform cachedTransform)
        {
            _footstepConfig = footstepConfig;
            _cachedTransform = cachedTransform;
        }

        public GroundDetectionData DetectGround()
        {
            // A little offset through the Vector3.up so to avoid being inside ground and miss raycast
            Vector3 rayOrigin = _cachedTransform.position + Vector3.up * _footstepConfig.RaycastStartOffset;

            var ray = new Ray(rayOrigin, Vector3.down);

            // Primary detection, precise raycast
            if(Physics.Raycast(ray, out RaycastHit hit, _footstepConfig.GroundCheckDistance, _footstepConfig.SurfaceLayerMask, QueryTriggerInteraction.Ignore))
            {
                return GroundDetectionData.Valid(hit);
            }

            // Falback detection, forgiving spherecast
            if(Physics.SphereCast(ray, _footstepConfig.GroundCheckRadius, out hit, _footstepConfig.GroundCheckDistance, _footstepConfig.SurfaceLayerMask, QueryTriggerInteraction.Ignore))
            {
                return GroundDetectionData.Valid(hit);
            }

            return GroundDetectionData.Invalid;
        }
    }

    public static class FootGroundDetector
    {
        /// <summary>
        /// Detects ground below the <paramref name="origin"/> using raycast/spherecast with provided parameters.
        /// </summary>
        /// <param name="origin">Origin of the ground detection (like player's feet).</param>
        /// <param name="raycastStartOffset">A little offset through the Vector3.up so to avoid being inside ground and miss raycast.</param>
        /// <param name="groundCheckDistance">Distance to check when raycasting.</param>
        /// <param name="groundCheckRadius">Radius of the spherecast.</param>
        /// <param name="surfaceLayerMask">Which layers can be thought as <b>'ground'</b>.</param>
        /// <returns>Detection result.</returns>
        public static GroundDetectionData DetectGround(
            Transform origin, 
            float raycastStartOffset, 
            float groundCheckDistance, 
            float groundCheckRadius, 
            LayerMask surfaceLayerMask)
        {
            // A little offset through the Vector3.up so to avoid being inside ground and miss raycast
            Vector3 rayOrigin = origin.position + Vector3.up * raycastStartOffset;

            var ray = new Ray(rayOrigin, Vector3.down);

            // Primary detection, precise raycast
            if (Physics.Raycast(ray, out RaycastHit hit, groundCheckDistance, surfaceLayerMask, QueryTriggerInteraction.Ignore))
            {
                return GroundDetectionData.Valid(hit);
            }

            // Falback detection, forgiving spherecast
            if (Physics.SphereCast(ray, groundCheckRadius, out hit, groundCheckDistance, surfaceLayerMask, QueryTriggerInteraction.Ignore))
            {
                return GroundDetectionData.Valid(hit);
            }

            return GroundDetectionData.Invalid;
        }
    }
}