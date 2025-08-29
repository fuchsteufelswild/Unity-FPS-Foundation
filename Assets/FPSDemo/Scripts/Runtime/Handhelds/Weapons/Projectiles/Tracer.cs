using Nexora.FPSDemo.Handhelds.RangedWeapon;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Represents tracers of very fast projectiles (like hitscan).
    /// </summary>
    /// <remarks>
    /// Uses the same structure as <see cref="IProjectile"/> to add effects through <see cref="ProjectileEffector"/>.
    /// </remarks>
    public sealed class Tracer : 
        MonoBehaviour,
        IMonoBehaviour
    {
        [Tooltip("Movement strategy for the tracer.")]
        [SerializeField, NotNull]
        private ProjectileMoveStrategy _mover;

        [ReorderableList(ElementLabel = "Effector")]
        [ReferencePicker(typeof(ProjectileEffector), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private ProjectileEffector[] _effectors = Array.Empty<ProjectileEffector>();

        private void Awake()
        {
            if (_mover == null)
            {
                Debug.LogError("Tracer cannot function without a mover");
                enabled = false;
                return;
            }

            foreach (var effector in _effectors)
            {
                effector.Initialize(this, _mover);
            }
        }

        public void Initialize(Vector3 startPosition, Vector3 targetPosition, float speed)
        {
            var context = new LaunchContext(
                origin: startPosition,
                velocity: (targetPosition - startPosition).normalized * Vector3.Distance(targetPosition, startPosition),
                speed: speed,
                torque: Vector3.zero,
                gravity: 0f);

            _mover.Launch(null, null, in context, null);
        }
    }
}