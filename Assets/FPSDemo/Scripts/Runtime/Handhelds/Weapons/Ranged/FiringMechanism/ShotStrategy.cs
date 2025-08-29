using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.SurfaceSystem;
using Nexora.ObjectPooling;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Structure use information about the shoot result.
    /// </summary>
    public readonly struct ShotResult
    {
        public readonly RaycastHit? HitInfo;

        public ShotResult(RaycastHit? hitInfo)
        {
            HitInfo = hitInfo;
        }
    }

    /// <summary>
    /// Interface for implementing different shoot methods like HitScan, Projectile etc.
    /// </summary>
    public abstract class ShotStrategy
    {
        [Tooltip("How many shots should be fired.")]
        [SerializeField, Range(1, 50)]
        protected int _shotCount = 1;

        [Tooltip("Range for minimum-maximum spread for shots.")]
        [SerializeField, MinMaxSlider(0f, 100f)]
        protected Vector2 _spreadRange;

        protected Transform _headTransform;

        public virtual void Enable(IGun gun)
        {
            IHandheld handheld = gun as IHandheld;
            _headTransform = handheld.Character.GetTransformOfBodyPart(BodyPart.Head);
        }

        /// <summary>
        /// Executes the firing logic.
        /// </summary>
        /// <returns>Enumerable of shot results, one <see cref="ShotResult"/> for each projectile/ray fired.</returns>
        public abstract IEnumerable<ShotResult> Execute(IGun gun, IGunImpactEffectBehaviour impactEffector, float accuracy);

        /// <summary>
        /// Calculates spread of the shots.
        /// </summary>
        /// <param name="accuracy">Accuracy of the shots.</param>
        /// <returns>Spread amount.</returns>
        protected float CalculateSpread(float accuracy) => Mathf.Lerp(_spreadRange.x, _spreadRange.y, 1f - accuracy);

        /// <summary>
        /// Calculates speed multiplier that might be applied to the shots.
        /// </summary>
        /// <param name="gun">Gun that is firing the shot.</param>
        /// <returns>Speed multiplier.</returns>
        protected float CalculateSpeedMultiplier(IGun gun)
        {
            // It might be modified when adding [PowerUps]
            float speedMultiplier = 1f;
            if (gun.TriggerMechanism is IChargedTriggerBehaviour chargedTrigger)
            {
                speedMultiplier *= chargedTrigger.TriggerCharge;
            }

            return speedMultiplier;
        }

        /// <returns>Current launch context, used for path visualization.</returns>
        public virtual LaunchContext GetLaunchContext(IGun gun) 
            => new LaunchContext(_headTransform.position, _headTransform.forward * 3000f, Vector3.zero, 0f);
    }

    /// <summary>
    /// Handles firing logic by shooting rays from the character's view.
    /// </summary>
    [Serializable]
    public sealed class HitScanShootStrategy : ShotStrategy
    {
        [Tooltip("Maximum distance to check for rays.")]
        [SerializeField, Range(1f, 1000f)]
        private float _maxRayDistance = 300;

        [Title("Tracer")]
        [Tooltip("Prefab to be spawned for the tracer of the projectile.")]
        [SerializeField]
        private Tracer _tracerPrefab;

        [Tooltip("Speed of the spawned tracer.")]
        [SerializeField, Range(1f, 1000f)]
        private float _tracerSpeed = 100f;

        public override void Enable(IGun gun)
        {
            base.Enable(gun);

            if (_tracerPrefab != null && ObjectPoolingModule.Instance.HasPool(_tracerPrefab) == false)
            {
                ObjectPoolingModule.Instance.RegisterPool(_tracerPrefab,
                    new ManagedSceneObjectPool<Tracer>(_tracerPrefab, gun.gameObject.scene, FPSDemoPoolCategory.Projectiles, 8, 32));
            }
        }

        /// <summary>
        /// Fires number of rays from the head view of the character, delegate information to <b>ImpactEffector</b>
        /// in the case of successful hit and spawns tracers.
        /// </summary>
        /// <inheritdoc path="/returns"/>
        public override IEnumerable<ShotResult> Execute(IGun gun, IGunImpactEffectBehaviour impactEffector, float accuracy)
        {
            IHandheld handheld = gun as IHandheld;
            ICharacter character = handheld.Character;

            float spread = Mathf.Lerp(_spreadRange.x, _spreadRange.y, 1f - accuracy);

            for(int i = 0; i < _shotCount; i++)
            {
                Ray ray = PhysicsUtils.GenerateRay(_headTransform, spread);

                var tracer = _tracerPrefab != null
                    ? ObjectPoolingModule.Instance.Get(_tracerPrefab, ray.origin, Quaternion.LookRotation(ray.direction))
                    : null;

                if (PhysicsUtils.RaycastOptimized(ray, _maxRayDistance, out RaycastHit hit, Layers.SolidObjectsMask, character.transform))
                {
                    impactEffector.TriggerEffect(hit, ray.direction, float.PositiveInfinity, hit.distance);

                    if (tracer != null)
                    {
                        tracer.Initialize(ray.origin, hit.point, _tracerSpeed);
                    }

                    yield return new ShotResult(hit);
                }
                else if (tracer != null)
                {
                    tracer.Initialize(ray.origin, ray.GetPoint(_maxRayDistance), _tracerSpeed);
                    yield return new ShotResult(null);
                }
            }
        }
    }

    [Serializable]
    public sealed class ProjectileShotStrategy : ShotStrategy
    {
        [Title("Shoot")]
        [Tooltip("Projectile to be spawned when shooting.")]
        [SerializeField, NotNull]
        private Projectile _projectilePrefab;

        [Title("Physics")]
        [Tooltip("Position offset when spawning projectile.")]
        [SerializeField]
        private Vector3 _spawnPositionOffset = Vector3.zero;

        [Tooltip("Rotation offset when spawning projectile.")]
        [SerializeField]
        private Vector3 _spawnRotationOffset = Vector3.zero;

        [Tooltip("Weight of the character's speed taken into account when firing " +
            "(when character is moving, projectile's initial speed should take it into account as well for correct physics).")]
        [SerializeField, Range(0f, 5f)]
        private float _characterSpeedWeight = 0.85f;

        [Tooltip("Speed the projectile is spawned with.")]
        [SerializeField, Range(1f, 1000f)]
        private float _projectileSpeed = 150f;

        [Tooltip("The torque projectile is spawned with.")]
        [SerializeField]
        private Vector3 _projectileTorque = Vector3.zero;

        [Tooltip("Gravity effecting on the projectile.")]
        [SerializeField, Range(0f, 100f)]
        private float _gravity = 10f;

        public override void Enable(IGun gun)
        {
            base.Enable(gun);

            if(_projectilePrefab != null && ObjectPoolingModule.Instance.HasPool(_projectilePrefab) == false)
            {
                ObjectPoolingModule.Instance.RegisterPool(_projectilePrefab,
                    new ManagedSceneObjectPool<Projectile>(_projectilePrefab, gun.gameObject.scene, FPSDemoPoolCategory.Projectiles, 8, 32));
            }
        }

        public override IEnumerable<ShotResult> Execute(IGun gun, IGunImpactEffectBehaviour impactEffector, float accuracy)
        {
            IHandheld handheld = gun as IHandheld;
            ICharacter character = handheld.Character;

            Vector3 characterVelocity = character.TryGetCC(out ICharacterMotor characterMotor)
                ? characterMotor.Velocity * _characterSpeedWeight
                : Vector3.zero;

            float speedMultiplier = CalculateSpeedMultiplier(gun);
            float spread = Mathf.Lerp(_spreadRange.x, _spreadRange.y, 1f - accuracy);

            for(int i = 0; i < _shotCount; i++)
            {
                Ray ray = PhysicsUtils.GenerateRay(_headTransform, spread);

                Vector3 spawnPosition = ray.origin + _headTransform.TransformVector(_spawnPositionOffset);
                Quaternion spawnRotation = Quaternion.LookRotation(ray.direction) * Quaternion.Euler(_spawnRotationOffset);

                Projectile projectile = ObjectPoolingModule.Instance.Get(_projectilePrefab, spawnPosition, spawnRotation);

                var context = new LaunchContext(spawnPosition,
                    ray.direction * speedMultiplier * _projectileSpeed + characterVelocity,
                    _projectileTorque,
                    _gravity);

                projectile.Launch(character, gun.ImpactEffector, context, null);

                yield return new ShotResult();
            }
        }

        public override LaunchContext GetLaunchContext(IGun gun)
            => new LaunchContext(_headTransform.position, _headTransform.forward * CalculateSpeedMultiplier(gun) * _projectileSpeed, Vector3.zero, 0f);
    }
}