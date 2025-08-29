using Nexora.FPSDemo.Handhelds.RangedWeapon;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyMover : ProjectileMoveStrategy
    {
        private Rigidbody _rigidbody;
        private bool _isActive;

        private Vector3 _launchPosition;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        public override void Launch(ICharacter character, IGunImpactEffectBehaviour impactEffector, in LaunchContext context, UnityAction hitCallback = null)
        {
            base.Launch(character, impactEffector, in context, hitCallback);

            _launchPosition = context.Origin;

            _rigidbody.isKinematic = false;
            _rigidbody.position = context.Origin;
            _rigidbody.rotation = Quaternion.LookRotation(context.Velocity.normalized);
            _rigidbody.linearVelocity = context.Velocity;
            _rigidbody.angularVelocity = context.Torque;

            enabled = true;
            _isActive = true;

            RaiseLaunchEvent(character, in context);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isActive == false)
            {
                return;
            }

            float travelledDistance = Vector3.Distance(_launchPosition, transform.position);

            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.None;

            _isActive = false;

            _hitCallback?.Invoke();
            _impactEffector.TriggerEffect(collision, travelledDistance);
            RaiseOnCollisionEvent(collision);

            enabled = false;
        }

        public override bool TryPredictPath(in LaunchContext context, float duration, int stepCount, out Vector3[] path, out RaycastHit? hit)
        {
            throw new System.NotImplementedException();
        }
    }
}