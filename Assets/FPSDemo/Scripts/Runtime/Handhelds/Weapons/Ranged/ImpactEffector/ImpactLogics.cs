using Nexora.FPSDemo.SurfaceSystem;
using Nexora.ObjectPooling;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public class ImpactContext
    {
        public float Damage;
        public float Force;

        // Read-only context
        public readonly Vector3 HitPoint;
        public readonly Vector3 HitDirection;
        public readonly Collider HitCollider;
        public readonly Rigidbody HitRigidbody;
        public readonly ICharacter DamageSource;
        public readonly DamageType DamageType;
        public readonly float DistanceTravelled;

        public readonly RaycastHit? RaycastHit;
        public readonly Collision Collision;

        public ImpactContext(float baseDamage, float baseForce, Vector3 hitPoint, Vector3 hitDirection, 
            Collider hitCollider, Rigidbody hitRigidbody, ICharacter damageSource, 
            DamageType damageType, float distanceTravelled, RaycastHit? raycastHit = null, Collision collision = null)
        {
            Damage = baseDamage;
            Force = baseForce;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            HitCollider = hitCollider;
            HitRigidbody = hitRigidbody;
            DamageSource = damageSource;
            DamageType = damageType;
            DistanceTravelled = distanceTravelled;
            RaycastHit = raycastHit;
            Collision = collision;
        }
    }

    /// <summary>
    /// Applies logic to the passed in context data(modifies it), and then it will be handed over to another <see cref="ImpactLogic"/>.
    /// </summary>
    [Serializable]
    public abstract class ImpactLogic
    {
        public virtual void Initialize(IGun gun) { }

        /// <summary>
        /// Processes the <paramref name="context"/>. Override to define specific rules.
        /// </summary>
        public abstract void Process(ImpactContext context);
    }

    /// <summary>
    /// Applies falloff to the impact context depending on the speed or distance travelled.
    /// </summary>
    [Serializable]
    public sealed class FalloffLogic : ImpactLogic
    {
        private enum CalculationMethod
        {
            [Tooltip("Falloff is calculated by the distance travelled from muzzle to the surface.")]
            Distance = 0,

            [Tooltip("Falloff is calculated by the speed of the projectile at the moment of impact.")]
            Speed = 1
        }

        [Tooltip("Method for calculating the falloff.")]
        [SerializeField]
        private CalculationMethod _falloffType;

        [MinMaxSlider(0f, 1000f)]
        [SerializeField]
        private Vector2 _thresholdRange;

        public override void Process(ImpactContext context)
        {
            // [Revisit] Would there be a case when character is dead but projectile is still flying and might hit another character?
            float value = _falloffType == CalculationMethod.Distance
                ? Vector3.Distance(context.DamageSource.transform.position, context.HitPoint)
                : context.HitRigidbody?.linearVelocity.magnitude ?? 0f;

            float range = Mathf.Max(0.01f, _thresholdRange.y - _thresholdRange.x);
            float progress = (value - _thresholdRange.x) / range;

            float falloffMod = 1f - Mathf.Clamp01(progress);

            context.Damage *= falloffMod;
            context.Force *= falloffMod;
        }
    }

    /// <summary>
    /// Applies damage to the hit target if it has a <see cref="IDamageReceiver"/>.
    /// </summary>
    [Serializable]
    public sealed class DamageLogic : ImpactLogic
    {
        public override void Process(ImpactContext context)
        {
            if(context.HitCollider.TryGetComponent(out IDamageReceiver damageReceiver))
            {
                Vector3 hitForce = context.HitDirection * context.Force;
                var args = new DamageContext(context.DamageSource, context.DamageType, context.HitPoint, hitForce);
                damageReceiver.ReceiveDamage(context.Damage, in args);
            }
        }
    }

    /// <summary>
    /// Applies physics force to the hit body.
    /// </summary>
    [Serializable]
    public sealed class PhysicsForceLogic : ImpactLogic
    {
        public override void Process(ImpactContext context)
        {
            Vector3 hitForce = context.HitDirection * context.Force;

            if(context.HitCollider.TryGetComponent(out IPhysicsImpactHandler impactHandler))
            {
                impactHandler.HandleImpact(context.HitPoint, hitForce);
            }
            else if(context.HitRigidbody != null && context.HitRigidbody.isKinematic == false)
            {
                context.HitRigidbody.AddForceAtPosition(hitForce, context.HitPoint, ForceMode.Impulse);
            }
        }
    }

    /// <summary>
    /// Plays surface effect upon impact.
    /// </summary>
    [Serializable]
    public sealed class SurfaceVisualsLogic : ImpactLogic
    {
        public override void Process(ImpactContext context)
        {
            if(context.RaycastHit.HasValue)
            {
                SurfaceSystemModule.Instance.PlayEffectFromHit(
                    context.RaycastHit.Value,
                    context.DamageType.GetSurfaceEffectType(),
                    parentEffects: context.HitRigidbody != null);
            }
            else if(context.Collision != null)
            {
                SurfaceSystemModule.Instance.PlayEffectFromCollision(
                    context.Collision,
                    context.DamageType.GetSurfaceEffectType(),
                    parentEffects: context.HitRigidbody != null);
            }
        }
    }

    /// <summary>
    /// Spawns explosion upon impact.
    /// </summary>
    [Serializable]
    public sealed class ExplosionLogic : ImpactLogic
    {
        [SerializeField]
        private Component _explosionPrefab; // [Revisit] ExplosionEffect script if needed

        [SerializeField, Range(1, 64)]
        private int _poolCapacity = 8;

        public override void Initialize(IGun gun)
        {
            if(ObjectPoolingModule.Instance.HasPool(_explosionPrefab) == false)
            {
                ObjectPoolingModule.Instance.RegisterPool(_explosionPrefab,
                    new ManagedSceneObjectPool<Component>(_explosionPrefab, gun.gameObject.scene, FPSDemoPoolCategory.Projectiles, 2, _poolCapacity));
            }
        }

        public override void Process(ImpactContext context)
        {
            var explosion = ObjectPoolingModule.Instance.Get(_explosionPrefab, context.HitPoint, Quaternion.identity);
            // explosion.Detonate(context.DamageSource);
        }
    }
}