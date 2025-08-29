using System;
using UnityEngine;

namespace Nexora.UI
{
    /// <summary>
    /// Component that controls position and orientation of a UI component to follow a world space transform
    /// with smooth interpolation.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DefaultExecutionOrder(ExecutionOrder.LatePresentation)]
    public sealed class WorldSpaceUIFollower : MonoBehaviour
    {
        /// <summary>
        /// Defines how the target determines target position.
        /// </summary>
        public enum FollowMode
        {
            FollowTransform = 0,
            FollowPosition = 1
        }

        /// <summary>
        /// Flags to determine which transform components to update.
        /// </summary>
        [Flags]
        public enum TransformUpdateFlags
        {
            None = 0,
            Position = 1 << 0,
            Rotation = 1 << 1,
            Scale = 1 << 2,
        }

        [Tooltip("Flags that denote which of the transform's components will be changed " +
            "when following a target.")]
        [SerializeField]
        private TransformUpdateFlags _transformUpdateFlags = TransformUpdateFlags.Position;

        [SerializeField]
        private InterpolationData _interpolationData = new();

        [SerializeField]
        private TargetTracker _targetTracker = new();

        [Tooltip("Duration after which the object will be disable if no target is present.")]        
        [HideIf(nameof(_transformUpdateFlags), TransformUpdateFlags.None, Comparison = UnityComparisonMethod.Equal)]
        [Range(0f, 10f)]
        [SerializeField]
        private float _autoDisableDelay = 0.25f;

        [Tooltip("Screen space offset applied to the UI element position.")]
        [ShowIf(nameof(_transformUpdateFlags), TransformUpdateFlags.Position, Comparison = UnityComparisonMethod.Equal)]
        [SerializeField]
        private Vector3 _screenOffset;

        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;

        private float _disableTimeThreshold;

        public bool IsTrackingTarget => enabled && _targetTracker.HasValidTarget();

        public Vector3 CurrentWorldPosition => _targetTracker.CalculateWorldPosition();

        private void Awake()
        {
            InitializeComponents();
            enabled = false;
        }

        private void InitializeComponents()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRectTransform = _rectTransform.parent as RectTransform;

            if(_parentRectTransform == null)
            {
                Debug.LogErrorFormat("[{0}] must have a valid parent with 'RectTransform' component.", nameof(WorldSpaceUIFollower));
            }
        }

        private void FixedUpdate()
        {
            if(_disableTimeThreshold < Time.unscaledTime)
            {
                enabled = false;
                return;
            }

            UpdateTargetValues();
        }

        /// <summary>
        /// Updates target values every frame in the <b>FixedUpdate</b> before calculating
        /// the interpolation in the <b>LateUpdate</b> to provide better unstuttering time-frame independent
        /// movement in the case of interpolation.
        /// </summary>
        private void UpdateTargetValues()
        {
            Vector3 worldPosition = _targetTracker.CalculateWorldPosition();
            Quaternion worldRotation = _targetTracker.CalculateWorldRotation(worldPosition);
            float targetScale = _targetTracker.CalculateTargetScale(worldPosition);

            _interpolationData.UpdateTargets(worldPosition, worldRotation, targetScale);
        }

        private void LateUpdate() => ApplyInterpolatedTransform();

        private void ApplyInterpolatedTransform()
        {
            var (position, rotation, scale) = _interpolationData.GetUpdatedValues();

            ApplyTransformValues(position, rotation, scale);
        }

        /// <summary>
        /// Applies the transform component values depending on the transform update flags.
        /// </summary>
        private void ApplyTransformValues(Vector3 worldPosition, Quaternion worldRotation, float scale)
        {
            if(EnumFlagsComparer.HasFlag(_transformUpdateFlags, TransformUpdateFlags.Position))
            {
                ApplyScreenPosition(worldPosition);
            }

            if(EnumFlagsComparer.HasFlag(_transformUpdateFlags, TransformUpdateFlags.Rotation))
            {
                _rectTransform.rotation = worldRotation;
            }

            if(EnumFlagsComparer.HasFlag(_transformUpdateFlags, TransformUpdateFlags.Scale))
            {
                _rectTransform.localScale = Vector3.one * scale;
            }
        }

        /// <summary>
        /// Updates the rect transform screen position, using the world position data.
        /// Positions the object correctly in the parent <see cref="Rect"/>.
        /// </summary>
        /// <param name="worldPosition"></param>
        private void ApplyScreenPosition(Vector3 worldPosition)
        {
            if(_targetTracker.ActiveCamera == null || _parentRectTransform == null)
            {
                return;
            }

            Vector2 screenPosition = _targetTracker.ActiveCamera.WorldToScreenPoint(worldPosition) + _screenOffset;

            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRectTransform, screenPosition, null, out Vector2 localPosition))
            {
                _rectTransform.anchoredPosition = localPosition;
            }
        }

        public void FollowTransform(Transform target, Vector3 localOffset = default)
        {
            _targetTracker.SetTargetTransform(target, localOffset);
            HandleTargetChange(target != null);
        }

        public void HandleTargetChange(bool hasValidTarget)
        {
            if(hasValidTarget)
            {
                _disableTimeThreshold = float.MaxValue;
                enabled = true;
            }
            else
            {
                _disableTimeThreshold = Time.unscaledTime + _autoDisableDelay;
            }
        }

        public void StopTracking()
        {
            enabled = false;
            _targetTracker.StopTracking();
        }

        public void FollowWorldPosition(Vector3? worldPosition)
        {
            bool hasSet = _targetTracker.SetTargetWorldPosition(worldPosition);
            HandleTargetChange(hasSet);
        }

        /// <summary>
        /// Handles interpolation of transform components (position, rotation, scale).
        /// </summary>
        [Serializable]
        private sealed class InterpolationData
        {
            [Tooltip("Enables smooth interpolation between updates.")]
            [SerializeField]
            private bool _enableInterpolation = true;

            public Vector3 CurrentPosition { get; private set; } = Vector3.zero;
            public Vector3 TargetPosition { get; private set; } = Vector3.zero;

            public Quaternion CurrentRotation { get; private set; } = Quaternion.identity;
            public Quaternion TargetRotation { get; private set; } = Quaternion.identity;

            public float CurrentScale { get; private set; } = 1f;
            public float TargetScale { get; private set; } = 1f;

            public void UpdateTargets(Vector3 position, Quaternion rotation, float scale)
            {
                CurrentPosition = TargetPosition;
                CurrentRotation = TargetRotation;
                CurrentScale = TargetScale;

                TargetPosition = position;
                TargetRotation = rotation;
                TargetScale = scale;
            }

            /// <summary>
            /// Gets the interpolated values or instant target values depending on the interpolation enabled status.
            /// </summary>
            /// <returns></returns>
            public (Vector3 position, Quaternion rotation, float scale) GetUpdatedValues()
            {
                return _enableInterpolation
                    ? GetInterpolatedValues()
                    : (TargetPosition, TargetRotation, TargetScale);
            }

            /// <summary>
            /// Gets the interpolated values using current and target values. 
            /// </summary>
            /// <remarks>
            /// Ensures smoothing without any jitter using fixed time step delta.
            /// </remarks>
            private (Vector3 position, Quaternion rotation, float scale) GetInterpolatedValues()
            {
                float deltaTime = Time.time - Time.fixedTime;

                // We are still in the fixed time step
                if (deltaTime < Time.fixedDeltaTime)
                {
                    float interpolationFactor = deltaTime / Time.fixedDeltaTime;

                    return (
                        Vector3.Lerp(CurrentPosition, TargetPosition, interpolationFactor),
                        Quaternion.Lerp(CurrentRotation, TargetRotation, interpolationFactor),
                        Mathf.Lerp(CurrentScale, TargetScale, interpolationFactor));
                }

                return GetUpdatedValues();
            }
        }

        /// <summary>
        /// Handles tracking target <see cref="Transform"/> or <see cref="Vector3"/> position.
        /// Responsible for calculating current world position, rotation, and scale 
        /// </summary>
        [Serializable]
        private sealed class TargetTracker
        {
            [Tooltip("Determines whether to follow a transform or a specific world position.")]
            [SerializeField]
            private FollowMode _followMode;

            [Tooltip("The transform of the 3D world object to follow.")]
            [ShowIf(nameof(_followMode), FollowMode.FollowTransform)]
            [SerializeField]
            private Transform _targetTransform;

            [Tooltip("The specific world position to follow.")]
            [ShowIf(nameof(_followMode), FollowMode.FollowPosition)]
            [SerializeField]
            private Vector3 _targetWorldPosition;

            [Tooltip("Minimum scale of the UI element based on distance.")]
            [ShowIf(nameof(_transformUpdateFlags), TransformUpdateFlags.Scale, Comparison = UnityComparisonMethod.Equal)]
            [SerializeField]
            private float _minimumScale;

            [Tooltip("Maximum scale of the UI element based on distance.")]
            [ShowIf(nameof(_transformUpdateFlags), TransformUpdateFlags.Scale, Comparison = UnityComparisonMethod.Equal)]
            [SerializeField]
            private float _maximumScale;

            [Tooltip("Reference distance at which the UI element maintains its default scale (1f)")]
            [ShowIf(nameof(_transformUpdateFlags), TransformUpdateFlags.Scale, Comparison = UnityComparisonMethod.Equal)]
            [SerializeField]
            private float _referenceDistance;

            private Vector3 _localOffset = Vector3.zero;

            public Camera ActiveCamera => UnityUtils.CachedCamera;

            public void StopTracking() => _targetTransform = null;

            public bool HasValidTarget()
            {
                return _followMode switch
                {
                    FollowMode.FollowTransform => _targetTransform != null,
                    FollowMode.FollowPosition => true,
                    _ => false
                };
            }

            /// <summary>
            /// Sets the <paramref name="target"/> as a target transform to follow.
            /// </summary>
            /// <param name="target">Target transform to follow.</param>
            /// <param name="localOffset">Local offset from the transform.</param>
            public void SetTargetTransform(Transform target, Vector3 localOffset = default)
            {
                _followMode = FollowMode.FollowTransform;
                _targetTransform = target;
                _localOffset = localOffset;
            }

            /// <summary>
            /// Uses <paramref name="worldPosition"/> as the target world position to follow.
            /// </summary>
            /// <returns>If successfuly following a <b>valid</b> position.</returns>
            public bool SetTargetWorldPosition(Vector3? worldPosition)
            {
                _followMode = FollowMode.FollowPosition;

                if (worldPosition.HasValue)
                {
                    _targetWorldPosition = worldPosition.Value;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Calculates the current position in the world to follow the target.
            /// </summary>
            /// <returns></returns>
            public Vector3 CalculateWorldPosition()
            {
                return _followMode switch
                {
                    FollowMode.FollowTransform when _targetTransform != null => _targetTransform.TransformPoint(_localOffset),
                    FollowMode.FollowPosition => _targetWorldPosition,
                    _ => _targetWorldPosition
                };
            }

            /// <summary>
            /// Calculates the target rotation value to face the camera.
            /// </summary>
            public Quaternion CalculateWorldRotation(Vector3 worldPosition)
            {
                if (ActiveCamera == null)
                {
                    return Quaternion.identity;
                }

                Vector3 directionToCamera = (ActiveCamera.transform.position - worldPosition).normalized;
                return Quaternion.LookRotation(directionToCamera, Vector3.up);
            }

            /// <summary>
            /// Calculates the scale using the distance from the camera.
            /// </summary>
            public float CalculateTargetScale(Vector3 worldPosition)
            {
                if (ActiveCamera == null)
                {
                    return 1f;
                }

                float distanceToCamera = Vector3.Distance(ActiveCamera.transform.position, worldPosition);
                float normalizedDistance = distanceToCamera / _referenceDistance;

                return Mathf.Lerp(_maximumScale, _minimumScale, normalizedDistance);
            }
        }
    }
}