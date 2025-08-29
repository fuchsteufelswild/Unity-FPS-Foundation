using Nexora.Audio;
using Nexora.FPSDemo.Handhelds.RangedWeapon;
using Nexora.ObjectPooling;
using System;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.GridLayoutGroup;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Abstract class for playing effects depending on projectile's phase.
    /// </summary>
    [Serializable]
    public abstract class ProjectileEffector
    {
        public virtual void Initialize(IMonoBehaviour projectile, ProjectileMoveStrategy mover)
        {
            mover.Launched += OnProjectileLaunched;
            mover.Hit += OnProjectileHit;
            mover.Collided += OnProjectileCollided;
        }

        public virtual void OnUpdate(float deltaTime) { }
        protected virtual void OnProjectileCollided(Collision collision) { }
        protected virtual void OnProjectileHit(RaycastHit hit) { }
        protected virtual void OnProjectileLaunched(ICharacter character, in LaunchContext context) { }
    }

    /// <summary>
    /// Plays audio effects for the projectile.
    /// </summary>
    [Serializable]
    public sealed class ProjectileAudioEffector : ProjectileEffector
    {
        [SerializeField]
        private AudioCue _impactAudio = new(null);

        protected override void OnProjectileHit(RaycastHit hit) => PlayAudio(hit.point);
        protected override void OnProjectileCollided(Collision collision) => PlayAudio(collision.contacts[0].point);
        private void PlayAudio(Vector3 point) => AudioModule.Instance.PlayCueOneShot(_impactAudio, point);
    }

    /// <summary>
    /// Controls projectile's lifetime with pooling.
    /// </summary>
    [Serializable]
    public sealed class ProjectileLifetimeEffector : ProjectileEffector
    {
        [Tooltip("How long this projectile can be in the trajectory.")]
        [SerializeField, Range(0.05f, 100f)]
        private float _lifeTime = 10f;

        /// <summary>
        /// Object pool component.
        /// </summary>
        private PooledSceneObject _pooledObject;

        public override void Initialize(IMonoBehaviour projectile, ProjectileMoveStrategy mover)
        {
            base.Initialize(projectile, mover);

            mover.Lifetime = _lifeTime;

            _pooledObject = projectile.GetComponent<PooledSceneObject>();
        }

        protected override void OnProjectileLaunched(ICharacter character, in LaunchContext context) => _pooledObject?.Release(_lifeTime);
        protected override void OnProjectileHit(RaycastHit hit) => ReleaseProjectile();
        protected override void OnProjectileCollided(Collision collision) => ReleaseProjectile();
        private void ReleaseProjectile() => _pooledObject?.Release(_pooledObject.AutoReleaseDelay);
    }

    /// <summary>
    /// Effector that renders a trail for the projectile.
    /// </summary>
    [Serializable]
    public sealed class ProjectileTrailEffector : ProjectileEffector
    {
        [Tooltip("Trail of the projectile.")]
        [SerializeField, NotNull]
        private TrailRenderer _trailRenderer;

        public override void Initialize(IMonoBehaviour projectile, ProjectileMoveStrategy mover)
        {
            base.Initialize(projectile, mover);
            if(_trailRenderer == null)
            {
                _trailRenderer = projectile.GetComponent<TrailRenderer>();
                _trailRenderer.emitting = false;
            }
        }

        protected override void OnProjectileLaunched(ICharacter character, in LaunchContext context)
        {
            _trailRenderer.Clear();
            _trailRenderer.emitting = true;
        }

        protected override void OnProjectileHit(RaycastHit hit)
        {
            _trailRenderer.emitting = false;
            _trailRenderer.time = 0f;
        }
    }

    /// <summary>
    /// Plays particles on the projectile.
    /// </summary>
    [Serializable]
    public sealed class ProjectileParticlesEffector : ProjectileEffector
    {
        [Tooltip("Particle system used for the projectile.")]
        [SerializeField, NotNull]
        private ParticleSystem _particleSystem;

        protected override void OnProjectileLaunched(ICharacter character, in LaunchContext context) => _particleSystem.Play(false);
        protected override void OnProjectileHit(RaycastHit hit) => StopProjectiles();
        protected override void OnProjectileCollided(Collision collision) => StopProjectiles();
        private void StopProjectiles() => _particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    /// <summary>
    /// Scales projectiles size over time using <see cref="AnimationCurve"/>.
    /// </summary>
    [Serializable]
    public sealed class ScaleOverTimeEffect : ProjectileEffector
    {
        [Tooltip("Animation curve used for modifying the scale of the projectile over time.")]
        [SerializeField]
        private AnimationCurve _sizeOverTime = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Transform _projectileTransform;
        private float _launchTime;
        private Vector3 _initialScale;

        private float _lifetime;

        public override void Initialize(IMonoBehaviour projectile, ProjectileMoveStrategy mover)
        {
            base.Initialize(projectile, mover);

            _projectileTransform = projectile.transform;
            _lifetime = mover.Lifetime;
            _initialScale = projectile.transform.localScale;
        }

        protected override void OnProjectileLaunched(ICharacter character, in LaunchContext context)
        {
            _launchTime = Time.time;
        }

        public override void OnUpdate(float deltaTime)
        {
            float progress = (Time.time - _launchTime) / _lifetime;
            float scaleMultiplier = _sizeOverTime.Evaluate(progress);
            _projectileTransform.localScale = _initialScale * scaleMultiplier;
        }
    }

    /// <summary>
    /// Effector for invoking events.
    /// </summary>
    [Serializable]
    public sealed class ProjectileEventEffector : ProjectileEffector
    {
        [SerializeField]
        private UnityEvent<ICharacter> _onLaunched;

        [SerializeField]
        private UnityEvent<ICharacter> _onDetonate;

        private ICharacter _owner;

        protected override void OnProjectileLaunched(ICharacter character, in LaunchContext context)
        {
            _owner = character;
            _onLaunched?.Invoke(character);
        }

        protected override void OnProjectileCollided(Collision collision) => _onDetonate?.Invoke(_owner);
        protected override void OnProjectileHit(RaycastHit hit) => _onDetonate?.Invoke(_owner);
    }

    /// <summary>
    /// Applies modifier to <see cref="Rigidbody"/> velocity upon impact.
    /// </summary>
    [Serializable]
    public sealed class RigidbodyImpactVelocityScaler : ProjectileEffector
    {
        [Tooltip("Modifies the velocity of the rigidbody upon impact.")]
        [SerializeField, Range(0f, 5f)]
        private float _velocityScale = 0.6f;

        [SerializeField, NotNull]
        private Rigidbody _projectileRigidbody;

        public override void Initialize(IMonoBehaviour projectile, ProjectileMoveStrategy mover)
        {
            base.Initialize(projectile, mover);

            _projectileRigidbody = projectile.GetComponent<Rigidbody>();
        }

        protected override void OnProjectileCollided(Collision collision)
        {
            if (_projectileRigidbody != null)
            {
                _projectileRigidbody.linearVelocity *= _velocityScale;
                _projectileRigidbody.angularVelocity *= _velocityScale;
            }
        }
    }

    #region Detonation
    [Serializable]
    public sealed class DetonationEffector : ProjectileEffector
    {
        private enum DetonationMode
        {
            AfterDelay = 0,
            OnImpact = 1,
            OnImpactAfterDelay = 2
        }

        [Flags]
        private enum DetonationFlags
        {
            None = 0,
            FreezeRigidbody = 1 << 0,
            DisableCollider = 1 << 1
        }

        [Tooltip("Controls when the denotation should be initiated.")]
        [SerializeField]
        private DetonationMode _detonationMode;

        [Tooltip("Delay of the detonation after launch.")]
        [HideIf(nameof(_detonationMode), DetonationMode.OnImpact)]
        [SerializeField, Range(0f, 100f)]
        private float _detonationDelay = 5f;

        [Tooltip("How should detonation affect the bodies.")]
        [SerializeField]
        private DetonationFlags _detonationFlags;

        public event UnityAction Detonated;

        private MonoBehaviour _coroutineRunner;
        private Rigidbody _projectileRigidbody;
        private Collider _projectileCollider;

        public override void Initialize(IMonoBehaviour projectile, ProjectileMoveStrategy mover)
        {
            base.Initialize(projectile, mover);

            _coroutineRunner = projectile as MonoBehaviour;
            _projectileRigidbody = projectile.GetComponent<Rigidbody>();
            _projectileCollider = projectile.GetComponent<Collider>();
        }

        protected override void OnProjectileLaunched(ICharacter character, in LaunchContext context)
        {
            if(_detonationMode == DetonationMode.AfterDelay)
            {
                _coroutineRunner.InvokeDelayed(Detonate, _detonationDelay);
            }

            if (EnumFlagsComparer.HasFlag(_detonationFlags, DetonationFlags.DisableCollider) && _projectileCollider != null)
            {
                _projectileCollider.enabled = true;
            }
        }

        protected override void OnProjectileCollided(Collision collision) => HandleImpactDetonation();
        protected override void OnProjectileHit(RaycastHit hit) => HandleImpactDetonation();

        private void HandleImpactDetonation()
        {
            if(_detonationMode == DetonationMode.OnImpact)
            {
                Detonate();
            }
            else if(_detonationMode == DetonationMode.OnImpactAfterDelay)
            {
                _coroutineRunner.InvokeDelayed(Detonate, _detonationDelay);
            }
        }

        private void Detonate()
        {
            if (EnumFlagsComparer.HasFlag(_detonationFlags, DetonationFlags.FreezeRigidbody) && _projectileRigidbody != null)
            {
                _projectileRigidbody.isKinematic = true;
            }

            if (EnumFlagsComparer.HasFlag(_detonationFlags, DetonationFlags.DisableCollider) && _projectileCollider != null)
            {
                _projectileCollider.enabled = false;
            }

            Detonated?.Invoke();
        }
    }
    #endregion
}