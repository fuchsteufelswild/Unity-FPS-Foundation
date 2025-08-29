using Nexora.FPSDemo.Handhelds.RangedWeapon;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    public sealed class LinearMover : ProjectileMoveStrategy
    {
        private Vector3 _launchPosition;
        private Vector3 _targetPosition;
        private float _speed;
        private bool _isActive;

        public override void Launch(ICharacter character, IGunImpactEffectBehaviour impactEffector, in LaunchContext context, UnityAction hitCallback = null)
        {
            base.Launch(character, impactEffector, in context, hitCallback);

            _launchPosition = context.Origin;
            _targetPosition = context.Origin + context.Velocity;
            _speed = context.Speed ?? 2000f;

            transform.position = _launchPosition;
            _isActive = true;

            RaiseLaunchEvent(character, in context);
        }

        public override bool TryPredictPath(in LaunchContext context, float duration, int stepCount, out Vector3[] path, out RaycastHit? hit)
        {
            throw new System.NotImplementedException();
        }

        private void Update()
        {
            if (_isActive == false)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);

            if(Vector3.Distance(_launchPosition, _targetPosition) < 0.1f)
            {
                _isActive = false;

                RaiseOnHitEvent(new RaycastHit { point = _targetPosition });
            }
        }
    }
}
