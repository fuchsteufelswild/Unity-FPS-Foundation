using Nexora.FPSDemo.Handhelds.RangedWeapon;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    public interface IProjectile : IMonoBehaviour
    {
        /// <summary>
        /// Launches the projectile.
        /// </summary>
        /// <param name="character">Owner character.</param>
        /// <param name="impactEffector">Impact effector on contact with a surface.</param>
        /// <param name="context">Information about the launch state.</param>
        /// <param name="hitCallback">Callback to invoke when contact with a surface.</param>
        void Launch(ICharacter character, IGunImpactEffectBehaviour impactEffector, in LaunchContext context, UnityAction hitCallback = null);
    }

    public sealed class Projectile : 
        MonoBehaviour,
        IProjectile
    {
        [Tooltip("Object responsible for moving the projectile.")]
        [SerializeField, NotNull]
        private ProjectileMoveStrategy _mover;

        [ReorderableList(ElementLabel = "Effector")]
        [ReferencePicker(typeof(ProjectileEffector), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private ProjectileEffector[] _effectors = Array.Empty<ProjectileEffector>();

        private void Awake()
        {
            if(_mover == null)
            {
                Debug.LogError("Projectile cannot function without a mover");
                enabled = false;
                return;
            }

            foreach(var effector in _effectors)
            {
                effector.Initialize(this, _mover);
            }
        }

        public void Launch(ICharacter character, IGunImpactEffectBehaviour impactEffector, in LaunchContext context, UnityAction hitCallback)
        {
            _mover.Launch(character, impactEffector, context, hitCallback);
        }

        private void Update()
        {
            foreach (var effector in _effectors)
            {
                effector.OnUpdate(Time.deltaTime);
            }
        }
    }
}