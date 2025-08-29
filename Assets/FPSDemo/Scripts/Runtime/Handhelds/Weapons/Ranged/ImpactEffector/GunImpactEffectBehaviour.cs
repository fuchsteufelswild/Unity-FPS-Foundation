using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunImpactEffectBehaviour : IGunBehaviour
    {
        /// <summary>
        /// Triggers the impact effect based on <see cref="RaycastHit"/>.
        /// </summary>
        /// <param name="hit">Hit surface information.</param>
        /// <param name="hitDirection">Direction of the hit when impacted the surface.</param>
        /// <param name="projectileSpeed">Speed of the projectile when impacted the surface.</param>
        /// <param name="distanceTravelled">Distance travelled by the projectile from muzzle to the surface.</param>
        void TriggerEffect(RaycastHit hit, Vector3 hitDirection, float projectileSpeed, float distanceTravelled);

        /// <summary>
        /// Triggers the impact effect based on <see cref="Collision"/>.
        /// </summary>
        /// <param name="distanceTravelled">Distance travelled by the projectile from muzzle to the surface.</param>
        void TriggerEffect(Collision collision, float distanceTravelled);
    }

    public sealed class NullImpactEffect : IGunImpactEffectBehaviour
    {
        public static readonly NullImpactEffect Instance = new();

        public void Attach() { }
        public void Detach() { }
        public void TriggerEffect(RaycastHit hit, Vector3 hitDirection, float projectileSpeed, float distanceTravelled) { }
        public void TriggerEffect(Collision collision, float distanceTravelled) { }
    }

    /// <summary>
    /// Processes impact when projectile impacts a surface, uses <see cref="ImpactLogic"/> as part of modular
    /// design to play effects, deal damage, spawn explosion etc.
    /// </summary>
    public sealed class GunImpactEffectBehaviour :
        GunBehaviour,
        IGunImpactEffectBehaviour
    {
        [Tooltip("Damage type that is inflicted to the 'IDamageReceiver'.")]
        [SerializeField]
        private DamageType _damageType = DamageType.Ballistic;

        [Tooltip("The damage inflicted to the 'IDamageReceiver' in the evenet of impact.")]
        [SerializeField]
        private float _baseDamage = 30f;

        [Tooltip("How much force is applied to the impacted surface in the event of impact.")]
        [SerializeField]
        private float _baseForce = 30f;

        /// <summary>
        /// The array of logic components that will be processed <b>in order</b> in the event of impact.
        /// </summary>
        /// <remarks>
        /// ! The order is important, as the modified <see cref="ImpactContext"/> is passed down in the pipeline. !
        /// </remarks>
        [ReorderableList(ElementLabel = "Logic")]
        [ReferencePicker(typeof(ImpactLogic), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private ImpactLogic[] _logicPipeline = Array.Empty<ImpactLogic>();

        private void OnEnable()
        {
            if(Gun != null)
            {
                Gun.ImpactEffector = this;

                foreach(var logic in _logicPipeline)
                {
                    logic.Initialize(Gun);
                }
            }
        }

        public void TriggerEffect(RaycastHit hit, Vector3 hitDirection, float projectileSpeed, float distanceTravelled)
        {
            var context = new ImpactContext(_baseDamage, _baseForce, hit.point, hitDirection, 
                hit.collider, hit.rigidbody, Handheld.Character, _damageType, distanceTravelled, hit, null);
            
            foreach(var logic in _logicPipeline)
            {
                logic.Process(context);
            }
        }

        public void TriggerEffect(Collision collision, float distanceTravelled)
        {
            ContactPoint contact = collision.contacts[0];
            var context = new ImpactContext(_baseDamage, _baseForce, contact.point, -contact.normal, collision.collider, 
                collision.rigidbody, Handheld.Character, _damageType, distanceTravelled, null, collision);

            foreach (var logic in _logicPipeline)
            {
                logic.Process(context);
            }
        }
    }
}