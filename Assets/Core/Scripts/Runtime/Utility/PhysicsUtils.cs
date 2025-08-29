using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Physics Utility class that provides optimized Raycast and Physics casts using preallocated caches
    /// </summary>
    public static class PhysicsUtils
    {
        public static readonly LayerMask AllLayers = ~int.MinValue;

        private static readonly Collider[] _cachedColliders = new Collider[64];
        private static readonly RaycastHit[] _cachedRaycastHits = new RaycastHit[64];

        /// <summary>
        /// Generates ray originated from the position of <paramref name="transform"/> translated by <paramref name="localOffset"/>
        /// In the direction <b>transform.forward</b> tilted by <paramref name="randomSpread"/> amount.
        /// </summary>
        public static Ray GenerateRay(Transform transform, float randomSpread, Vector3 localOffset = default)
        {
            Vector3 spread = transform.TransformVector(
                new Vector3
                (
                    Random.Range(-randomSpread, randomSpread),
                    Random.Range(-randomSpread, randomSpread),
                    0f
                ));

            Vector3 rayDirection = Quaternion.Euler(spread) * transform.forward;

            return new Ray(transform.position + transform.TransformVector(localOffset), rayDirection);
        }

        /// <summary>
        /// Uses preallocated cache to Raycast without any allocations.
        /// </summary>
        public static bool RaycastOptimized(
            Ray ray,
            float maxDistance,
            out RaycastHit hit,
            int layerMask = Physics.DefaultRaycastLayers,
            Transform ignoredHierarchyRoot = null,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int hitCount = Physics.RaycastNonAlloc(ray, _cachedRaycastHits, maxDistance, layerMask, triggerInteraction);

            if (hitCount == 0)
            {
                hit = default;
                return false;
            }

            GetClosestRaycastHitFromCache(ignoredHierarchyRoot, hitCount, out hit);
            return hit.transform != null;
        }

        /// <summary>
        /// Returns the RaycastHit from the cache that has the minimum distance.
        /// <br></br>
        /// <b>! Don't call without filling the cache, like all optimized casts call this function after filling the cache !</b>
        /// </summary>
        private static void GetClosestRaycastHitFromCache(Transform ignoredHierarchyRoot, int hitCount, out RaycastHit hit)
        {
            if (hitCount <= 0)
            {
                hit = default;
                return;
            }

            float closestDistance = float.PositiveInfinity;
            bool shouldIgnoreHierarchy = ignoredHierarchyRoot != null;
            int closestHitIndex = -1;

            for (int i = 0; i < hitCount; i++)
            {
                if (shouldIgnoreHierarchy && _cachedRaycastHits[i].transform.IsChildOf(ignoredHierarchyRoot))
                {
                    continue;
                }

                if (_cachedRaycastHits[i].distance < closestDistance)
                {
                    closestDistance = _cachedRaycastHits[i].distance;
                    closestHitIndex = i;
                }
            }

            hit = closestHitIndex != -1
                ? _cachedRaycastHits[closestHitIndex]
                : default;
        }

        /// <summary>
        /// Optimized Raycast returns closestDistance, and with potential specification to ignore the specific hierarchy of elements.(root and its children)
        /// </summary>
        public static float GetClosestRaycastDistance(
            Ray ray,
            float maxDistance = float.PositiveInfinity,
            int layerMask = Physics.DefaultRaycastLayers,
            Transform ignoredHierarchyRoot = null,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int hitCount = Physics.RaycastNonAlloc(ray, _cachedRaycastHits, maxDistance, layerMask, triggerInteraction);

            if (hitCount == 0)
            {
                return float.PositiveInfinity;
            }

            RaycastHit hit;
            GetClosestRaycastHitFromCache(ignoredHierarchyRoot, hitCount, out hit);

            return hit.distance;
        }

        /// <summary>
        /// Uses preallocated cache to SphereCast without any allocations.
        /// </summary>
        public static bool SphereCastOptimized(
            Ray ray,
            float radius,
            float maxDistance,
            out RaycastHit hit,
            int layerMask = Physics.DefaultRaycastLayers,
            Transform ignoredHierarchyRoot = null,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int hitCount = Physics.SphereCastNonAlloc(ray, radius, _cachedRaycastHits, maxDistance, layerMask, triggerInteraction);

            if (hitCount == 0)
            {
                hit = default;
                return false;
            }

            GetClosestRaycastHitFromCache(ignoredHierarchyRoot, hitCount, out hit);
            return hit.transform != null;
        }

        /// <summary>
        /// Optimized SphereCast returns closestDistance, and with potential specification to ignore the specific hierarchy of elements(root and its children).
        /// </summary>
        public static float GetClosestSphereCastDistance(
            Ray ray,
            float radius,
            float maxDistance = float.PositiveInfinity,
            int layerMask = Physics.DefaultRaycastLayers,
            Transform ignoredHierarchyRoot = null,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int hitCount = Physics.SphereCastNonAlloc(ray, radius, _cachedRaycastHits, maxDistance, layerMask, triggerInteraction);

            if (hitCount == 0)
            {
                return float.PositiveInfinity;
            }

            RaycastHit hit;
            GetClosestRaycastHitFromCache(ignoredHierarchyRoot, hitCount, out hit);

            return hit.distance;
        }

        /// <summary>
        /// Uses preallocated cache to OverlapBox without any allocations.
        /// </summary>
        /// <remarks>
        /// <paramref name="colliders"/> can be assigned to another object as this is a cached optimized version.
        /// Use it the moment retrieved, if you intend to store them, duplicate the array by <b>deep copy</b>.
        /// </remarks>
        public static int OverlapBoxOptimized(
            in Bounds bounds,
            Quaternion orientation,
            out Collider[] colliders,
            int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            return OverlapBoxOptimized(bounds.center, bounds.extents, orientation, out colliders, layerMask, triggerInteraction);
        }

        /// <inheritdoc cref="OverlapBoxOptimized(in Bounds, Quaternion, out Collider[], int, QueryTriggerInteraction)"/>
        public static int OverlapBoxOptimized(
            Vector3 center,
            Vector3 extents,
            Quaternion orientation,
            out Collider[] colliders,
            int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int hitCount = Physics.OverlapBoxNonAlloc(center, extents, _cachedColliders, orientation, layerMask, triggerInteraction);
            colliders = _cachedColliders;
            return hitCount;
        }

        /// <summary>
        /// Uses preallocated cache to OverlapSphere without any allocations.
        /// </summary>
        /// <inheritdoc cref="OverlapBoxOptimized(in Bounds, Quaternion, out Collider[], int, QueryTriggerInteraction)" path="/remarks"/>
        public static int OverlapSphereOptimized(
            Vector3 center,
            float radius,
            out Collider[] colliders,
            int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(center, radius, _cachedColliders, layerMask, triggerInteraction);
            colliders = _cachedColliders;
            return hitCount;
        }
    }
}
