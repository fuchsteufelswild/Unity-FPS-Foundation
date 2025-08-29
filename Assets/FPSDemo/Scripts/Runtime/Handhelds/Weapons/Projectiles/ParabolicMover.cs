using Nexora.FPSDemo.Handhelds.RangedWeapon;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    public sealed class ParabolicMover : ProjectileMoveStrategy
    {
        [Tooltip("Matches projectile's rotation with its velocity (speed).")]
        [SerializeField]
        private bool _matchRotationToVelocity = true;

        private Transform _projectileTransform;
        private ICharacter _owner;
        private int _collisionMask;
        private float _gravity;

        private float _launchTime;
        private Vector3 _launchPosition;
        private Vector3 _launchDirection;
        private float _launchSpeed;
        private Vector3 _launchTorque;
        private bool _isActive;

        private Vector3 _lerpStartPosition;
        private Vector3 _lerpTargetPosition;

        // Used repetitively to not allocate
        private RaycastHit _cachedHit;

        private void Awake() => _projectileTransform = transform;

        public override void Launch(ICharacter character, IGunImpactEffectBehaviour impactEffector,
            in LaunchContext context, UnityAction hitCallback = null)
        {
            base.Launch(character, impactEffector, in context, hitCallback);

            _owner = character;
            _launchPosition = context.Origin;
            _launchDirection = context.Velocity.normalized;
            _gravity = context.Gravity;
            _collisionMask = context.LayerMask;
            _launchSpeed = context.Velocity.magnitude;
            _launchTorque = context.Torque;
            _hitCallback = hitCallback;

            _isActive = true;
            _launchTime = Time.time;

            // Ensure positions are correct on start
            _projectileTransform.position = _launchPosition;
            _lerpStartPosition = _launchPosition;
            _lerpTargetPosition = _launchPosition;

            RaiseLaunchEvent(character, in context);
            FixedUpdate();
        }

        private void Update()
        {
            if (_isActive == false || _launchTime < 0f)
            {
                return;
            }

            FixedTimeStepInterpolation();
        }

        /// <summary>
        /// Performs fixed-timestep interpolation for smooth movement between physics frames.
        /// Sets object's local position to: 
        /// <br></br>->If we are in the same frame, then interpolates from current to target, if not just snaps to target.
        /// Smooths out the values for the upcoming update in <see cref="FixedUpdate"/>.
        /// </summary>
        /// <remarks>
        /// Used to avoid stuttering when visual FPS is higher than physics FPS.
        /// </remarks>
        private void FixedTimeStepInterpolation()
        {
            float delta = Time.time - Time.fixedTime;
            // If t >= 1, we have already processed this frame
            float t = delta / Time.fixedDeltaTime;
            _projectileTransform.localPosition = Vector3.Lerp(_lerpStartPosition, _lerpTargetPosition, t);

            if (_matchRotationToVelocity)
            {
                Vector3 velocity = _lerpTargetPosition - _lerpStartPosition;

                if (velocity.sqrMagnitude > 0.001f)
                {
                    Vector3 torque = EvaluateTorque(Time.time - _launchTime);

                    _projectileTransform.rotation = Quaternion.LookRotation(velocity) * Quaternion.LookRotation(torque);
                }
            }
        }

        private Vector3 EvaluateTorque(float elapsedTime)
        {
            return elapsedTime < 0f
                ? Vector3.zero
                : _launchTorque * (_launchSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Guesses the next point of the projectile and makes calculations of potential hit and fires event if it occurs.
        /// </summary>
        /// <remarks>
        /// Updates lerp start/target positions according to the guesses point.
        /// </remarks>
        private void FixedUpdate()
        {
            if (_isActive == false)
            {
                return;
            }

            float elapsedTime = Time.time - _launchTime;

            Vector3 currentPoint = EvaluePointInParabola(_launchPosition, _launchDirection * _launchSpeed, _gravity, elapsedTime);
            Vector3 nextPoint = EvaluePointInParabola(_launchPosition, _launchDirection * _launchSpeed, _gravity, elapsedTime + Time.fixedDeltaTime);

            Vector3 direction = nextPoint - currentPoint;
            float distance = direction.magnitude;

            var ray = new Ray(currentPoint, direction);

            if (PhysicsUtils.RaycastOptimized(ray, distance, out _cachedHit, _collisionMask, _owner?.transform, QueryTriggerInteraction.UseGlobal))
            {
                _isActive = false;
                _hitCallback?.Invoke();
                _impactEffector?.TriggerEffect(_cachedHit, direction, distance, (_launchPosition - _cachedHit.point).magnitude);
                RaiseOnHitEvent(_cachedHit);
            }
            else
            {
                _lerpStartPosition = currentPoint;
                _lerpTargetPosition = nextPoint;
            }
        }

        /// <summary>
        /// Evaluates the point in parabola given <paramref name="t"/> seconds has passed.
        /// Simple parabolic formula: x = vt + 1/2 (at^2)
        /// </summary>
        /// <param name="t">Time has passed since projectile launched.</param>
        /// <returns>Point in the parabola trajectory of the projectile given <paramref name="t"/>.</returns>
        private Vector3 EvaluePointInParabola(Vector3 launchPosition, Vector3 launchVelocity, float gravity, float t)
        {
            Vector3 trajectory = launchPosition + launchVelocity * t;
            Vector3 gravityEffect = Vector3.down * (0.5f * gravity * t * t);

            return trajectory + gravityEffect;
        }

        public override bool TryPredictPath(in LaunchContext context, float duration, int stepCount, out Vector3[] path, out RaycastHit? hit)
        {
            path = new Vector3[stepCount];
            hit = null;

            Vector3 previousPoint = context.Origin;
            path[0] = previousPoint;

            for(int i = 1; i < stepCount; i++)
            {
                float t = (i / (float)(stepCount - 1)) * duration;
                Vector3 currentPoint = EvaluePointInParabola(context.Origin, transform.forward * context.Velocity.magnitude, 
                    context.Gravity, t);

                Vector3 direction = currentPoint - previousPoint;

                var ray = new Ray(previousPoint, direction.normalized);

                if(PhysicsUtils.RaycastOptimized(ray, direction.magnitude, out RaycastHit raycastHit, context.LayerMask))
                {
                    path[i] = raycastHit.point;

                    hit = raycastHit;
                    return true;
                }

                path[i] = currentPoint;
                previousPoint = currentPoint;
            }

            return false;
        }
    }
}