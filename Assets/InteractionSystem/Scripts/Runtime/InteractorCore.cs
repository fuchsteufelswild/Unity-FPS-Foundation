using Nexora.Audio;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.InteractionSystem
{
    [Serializable]
    public sealed class InteractorCore
    {
        /// <summary>
        /// Method type to use use to detect interactables.
        /// </summary>
        public enum DetectionMethod
        {
            Raycast = 0,
            SphereCast = 1
        }

        [Tooltip("Transform representing the interactor's view origin (used to shoot rays for finding interactables).")]
        [SerializeField]
        private Transform _viewTransform;

        [Tooltip("Method used to detect interactables.")]
        [SerializeField]
        private DetectionMethod _detectionMethod;

        [Range(0.01f, 2.5f)]
        [SerializeField]
        private float _maxInteractionDistance = 2.5f;

        [Tooltip("Layers that contain interactable objects.")]
        [SerializeField]
        private LayerMask _interactableLayers;

        [Tooltip("Layers that block interaction.")]
        [ShowIf(nameof(_detectionMethod), DetectionMethod.SphereCast)]
        [SerializeField]
        private LayerMask _obstructionLayers;

        [Tooltip("Spherecast Radius.")]
        [ShowIf(nameof(_detectionMethod), DetectionMethod.SphereCast)]
        [SerializeField]
        private float _sphereCastRadius = 0.2f;

        [Tooltip("How to handle trigger colliders during detection.")]
        [SerializeField]
        private QueryTriggerInteraction _triggerHandling = QueryTriggerInteraction.Collide;

        private static readonly RaycastHit[] _hitsBuffer = new RaycastHit[10];
        private float _interactionProgress;

        private Transform _ignoredRoot;

        public float InteractionProgress => _interactionProgress;

        public void SetIgnoredRoot(Transform ignoredRoot) => _ignoredRoot = ignoredRoot;

        public void ResetInteractionProgress() => _interactionProgress = 0;

        public IEnumerator DelayedInteraction(
            float delayDuration, UnityAction<float> interactionProgressChanged, 
            Action delayCompletedCallback)
        {
            float startTime = Time.time;
            float duration = delayDuration;

            while(Time.time < startTime + duration)
            {
                _interactionProgress = Mathf.Clamp01((Time.time - startTime) / duration);
                interactionProgressChanged?.Invoke(_interactionProgress);
                yield return null;
            }

            delayCompletedCallback?.Invoke();
        }


        /// <summary>
        /// Raycast: Detects the closest interactable to the view.
        /// <br></br>
        /// Spherecast: Detects the interactable of the <b>closest angle</b> with respect to view.
        /// </summary>
        public IHoverable DetectInteractables()
        {
            var ray = new Ray(_viewTransform.position, _viewTransform.forward);
            return _detectionMethod == DetectionMethod.Raycast
                ? FindHoverableWithRaycast(ray)
                : FindBestHoverableWithSphereCast(ray);
        }

        private IHoverable FindHoverableWithRaycast(Ray ray)
        {
            if(PhysicsUtils.RaycastOptimized(ray, _maxInteractionDistance, out var hit, _interactableLayers, _ignoredRoot))
            {
                return hit.collider.GetComponentInParent<IHoverable>();
            }

            return null;
        }

        public IHoverable FindBestHoverableWithSphereCast(Ray ray)
        {
            int hitCount = Physics.SphereCastNonAlloc(
                ray,
                _sphereCastRadius,
                _hitsBuffer,
                _maxInteractionDistance,
                _interactableLayers | _obstructionLayers,
                _triggerHandling);

            IHoverable bestHoverable = null;
            float bestAngle = float.MaxValue;

            for(int i = 0; i < hitCount; i++)
            {
                var hit = _hitsBuffer[i];
                var hitObject = hit.collider.gameObject;

                if(IsObstruction(hitObject)
                || HasLineOfSightObstruction(ray.origin, hit.point))
                {
                    continue;
                }

                float angle = Vector3.Angle(ray.direction, (hit.point - ray.origin).normalized);
                if(angle >= bestAngle)
                {
                    continue;
                }

                var hoverable = hitObject.GetComponentInParent<IHoverable>();
                if (hoverable != null && hoverable.enabled)
                {
                    bestAngle = angle;
                    bestHoverable = hoverable;
                }
            }

            return bestHoverable;
        }

        private bool IsObstruction(GameObject hitObject)
        {
            return ((1 << hitObject.layer) & _obstructionLayers) != 0;
        }

        private bool HasLineOfSightObstruction(Vector3 origin, Vector3 target)
        {
            Vector3 direction = target - origin;
            float distance = direction.magnitude;
            return Physics.Raycast(
                origin,
                direction.normalized,
                distance,
                _obstructionLayers,
                QueryTriggerInteraction.Ignore);
        }
    }
}