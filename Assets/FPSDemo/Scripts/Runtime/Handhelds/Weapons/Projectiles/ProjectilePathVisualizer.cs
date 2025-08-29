using Nexora.FPSDemo.Handhelds.RangedWeapon;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public sealed class ProjectilePathVisualizer : MonoBehaviour
    {
        private const float NormalInterpolationSpeed = 8f;

        [Title("References")]
        [Tooltip("Line renderer used for drawing the path.")]
        [SerializeField, NotNull]
        private LineRenderer _lineRenderer;

        [Tooltip("Indicator showing the hit.")]
        [SerializeField, NotNull]
        private Transform _hitRenderer;

        [Tooltip("The mover component whose path will be visualized.")]
        [SerializeField, NotNull]
        private ProjectileMoveStrategy _targetMover;

        [Title("Prediction")]
        [Tooltip("How long into the future is predicted for the path drawing.")]
        [SerializeField, Range(0f, 20f)]
        private float _predictedSeconds = 2f;

        [Tooltip("How many points will be in the line (precision).")]
        [SerializeField, Range(1, 100)]
        private int _stepCount = 16;

        [Title("Hit Marker")]
        [SerializeField, MinMaxSlider(0f, 20f)]
        private Vector2 _hitMarkerSizeRange = new(0.15f, 0.25f);

        [SerializeField, Range(0f, 10f)]
        private float _hitMarkerPositionOffset = 0.1f;

        private Vector3[] _pathPositions;
        private Vector3 _lastHitNormal;

        private LaunchContext _lastReceivedContext;

        private void Awake()
        {
            _pathPositions = new Vector3[_stepCount];
            Disable();
        }

        public void Enable()
        {
            _lineRenderer.enabled = true;
            _hitRenderer.gameObject.SetActive(true);
            enabled = true;
        }

        public void Disable()
        {
            _lineRenderer.enabled = false;
            _hitRenderer.gameObject.SetActive(false);
            enabled = false;
        }

        private void LateUpdate()
        {
            if (_targetMover.TryPredictPath(in _lastReceivedContext, _predictedSeconds, _stepCount, out _pathPositions, out RaycastHit? hit))
            {
                UpdateLineRenderer(_pathPositions);
                UpdateHitMarker(hit.Value, _pathPositions);
            }
            else
            {
                UpdateLineRenderer(_pathPositions);
                _hitRenderer.gameObject.SetActive(false);
            }
        }

        public void UpdateVisualization(IGunFiringMechanismBehaviour fireSystem)
        {
            if(enabled == false || _targetMover == null)
            {
                return;
            }
            _lastReceivedContext = fireSystem.GetLaunchContext();
        }

        private void UpdateLineRenderer(Vector3[] path)
        {
            _lineRenderer.positionCount = path.Length;
            _lineRenderer.SetPositions(path);
        }

        private void UpdateHitMarker(RaycastHit hit, Vector3[] path)
        {
            _hitRenderer.gameObject.SetActive(false);

            _lastHitNormal = Vector3.Lerp(_lastHitNormal, hit.normal, Time.deltaTime * NormalInterpolationSpeed);

            _hitRenderer.position = hit.point + _lastHitNormal * _hitMarkerPositionOffset;
            _hitRenderer.rotation = Quaternion.LookRotation(_lastHitNormal);

            float travelDistance = Vector3.Distance(path[0], hit.point);
            float maxDistance = Vector3.Distance(path[0], path.Last());

            float sizeLerp = Mathf.Clamp01(travelDistance / maxDistance);
            float hitSize = Mathf.Lerp(_hitMarkerSizeRange.x, _hitMarkerSizeRange.y, sizeLerp);

            _hitRenderer.localScale = hitSize * Vector3.one;
        }
    }
}