using Nexora.FPSDemo.SurfaceSystem;
using UnityEngine;

namespace Nexora.FPSDemo
{
    public enum DamageOutcomeType
    {
        /// <summary>
        /// A normal damage.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Critical hit.
        /// </summary>
        Critical = 1,

        /// <summary>
        /// Damage results in fatal hit, like killing/destroying the target.
        /// </summary>
        Fatal = 2,

        /// <summary>
        /// Damage is missed.
        /// </summary>
        Missed = 3,

        /// <summary>
        /// Specific head shot.
        /// </summary>
        Headshot = 4,

        /// <summary>
        /// Damage on the weak spot of the target.
        /// </summary>
        Weakspot = 5
    }

    /// <summary>
    /// Possible damage types.
    /// </summary>
    public enum DamageType
    {
        Undefined = 0,

        Ballistic = 1,

        Explosive = 2,

        Blunt = 3
    }

    /// <summary>
    /// Used for propagating information about the damage.
    /// </summary>
    public readonly struct DamageContext
    {
        /// <summary>
        /// Damage is applied by?
        /// </summary>
        public readonly IDamageSource Source;

        /// <summary>
        /// Type of the applied damage.
        /// </summary>
        public readonly DamageType DamageType;

        /// <summary>
        /// The world-space point where damage was applied.
        /// </summary>
        public readonly Vector3 ImpactPoint;

        /// <summary>
        /// The force applied at the place where the damage was applied.
        /// </summary>
        public readonly Vector3 ImpactForce;

        public static readonly DamageContext Empty = new();

        public DamageContext(IDamageSource source, DamageType damageType, Vector3 impactPoint, Vector3 impactForce)
        {
            Source = source;
            DamageType = damageType;
            ImpactPoint = impactPoint;
            ImpactForce = impactForce;
        }
    }

    public interface IDamageSource : IMonoBehaviour
    {
        /// <summary>
        /// Damage source identifier.
        /// </summary>
        string DamageSourceName { get; }
    }

    public interface IDamageReceiver : IMonoBehaviour
    {
        /// <summary>
        /// Character that is receiving the damage (owner).
        /// </summary>
        ICharacter Character { get; }

        /// <summary>
        /// Processes the receives damage and does what is required.
        /// </summary>
        /// <param name="damage">Damage amount received.</param>
        /// <param name="context">Additional context about the damage.</param>
        /// <returns>Outcome of the damage received.</returns>
        DamageOutcomeType ReceiveDamage(float damage, in DamageContext context = default);
    }

    /// <summary>
    /// Interface for enabling handling physics impacts.
    /// </summary>
    public interface IPhysicsImpactHandler
    {
        /// <summary>
        /// Handles the physics impact that might be due to something falling, explosion, hit from melee or projectile etc.
        /// </summary>
        /// <param name="hitPoint">Where the impact landed.</param>
        /// <param name="hitForce">Force of the impact in 3D coordinates.</param>
        void HandleImpact(Vector3 hitPoint, Vector3 hitForce);
    }

    public static class DamageTypeExtensions
    {
        /// <summary>
        /// Gets corresponding <see cref="SurfaceEffectType"/> for the given <see cref="DamageType"/>.
        /// </summary>
        /// <returns>Corresponding <see cref="SurfaceEffectType"/> for the given <paramref name="damageType"/>.</returns>
        public static SurfaceEffectType GetSurfaceEffectType(this DamageType damageType)
        {
            // This list should be full normally, with corresponding assets and logic it can be programmed as wished.
            return damageType switch
            {
                DamageType.Ballistic => SurfaceEffectType.BulletImpact,
                _ => SurfaceEffectType.GenericImpact
            };
        }
    }
}