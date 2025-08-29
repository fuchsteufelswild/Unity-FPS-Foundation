using Nexora.FPSDemo.Handhelds.RangedWeapon;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    public delegate void ProjectileHitDelegate(RaycastHit hit);
    public delegate void ProjectileCollisionDelegate(Collision collision);
    public delegate void ProjectileLaunchDelegate(ICharacter character, in LaunchContext context);

    /// <summary>
    /// Abstract class for implementing movement logic of a projectile.
    /// </summary>
    public abstract class ProjectileMoveStrategy : MonoBehaviour
    {
        protected UnityAction _hitCallback;
        protected IGunImpactEffectBehaviour _impactEffector;

        /// <summary>
        /// Lifetime of the projectile.
        /// </summary>
        public float Lifetime { get; set; }

        /// <summary>
        /// Called when projectile hits a surface with raycast (manual physics based).
        /// </summary>
        public event ProjectileHitDelegate Hit;

        /// <summary>
        /// Called when projectile collides with an object (pure physics based).
        /// </summary>
        public event ProjectileCollisionDelegate Collided;

        /// <summary>
        /// Called when the projectile is launched.
        /// </summary>
        public event ProjectileLaunchDelegate Launched;

        /// <summary>
        /// Launches the projectile with given <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The initial launch conditions.</param>
        public virtual void Launch(ICharacter character, IGunImpactEffectBehaviour impactEffector, in LaunchContext context, UnityAction hitCallback = null)
        {
            _hitCallback = hitCallback;
            _impactEffector = impactEffector;
        }

        /// <summary>
        /// Predicts the projectile's trajectory over a set duration.
        /// </summary>
        /// <param name="context">The initial launch conditions.</param>
        /// <param name="duration">How many seconds into the future to predict?</param>
        /// <param name="stepCount">The number of points to calculate for the path.</param>
        /// <param name="path">An array of points for the predicted path.</param>
        /// <param name="hit">Information on the first predicted collision, if any.</param>
        /// <returns><see langword="true"/> if collision was predicted, <see langword="false"/> otherwise</returns>
        public abstract bool TryPredictPath(in LaunchContext context, float duration, int stepCount, out Vector3[] path, out RaycastHit? hit);

        protected void RaiseOnHitEvent(RaycastHit hit) => Hit?.Invoke(hit);
        protected void RaiseOnCollisionEvent(Collision collision) => Collided?.Invoke(collision);
        protected void RaiseLaunchEvent(ICharacter character, in LaunchContext context)  => Launched?.Invoke(character, in context);
    }   
}