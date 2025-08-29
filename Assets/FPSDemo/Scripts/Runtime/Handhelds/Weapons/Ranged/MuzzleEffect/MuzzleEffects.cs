using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.Rendering;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Abstract base class for any effect to be triggered by muzzle behaviour.
    /// </summary>
    [Serializable]
    public abstract class MuzzleEffect
    {
        /// <summary>
        /// Initialize with the <paramref name="gun"/>.
        /// </summary>
        /// <param name="gun">Gun that triggers the ejection.</param>
        public virtual void Initialize(IGun gun) { }

        /// <summary>
        /// Plays the effect.
        /// </summary>
        public abstract void Trigger();

#if UNITY_EDITOR
        public virtual void RefreshReferences(Transform behaviour)
        {

        }
#endif
    }

    [Serializable]
    public sealed class MuzzleAudioEffect : MuzzleEffect
    {
        [Tooltip("Audio played when triggered, (can be used as fire or tail fire as well).")]
        [SerializeField]
        private AudioCue _fireAudio = new(null);

        private IHandheld _handheld;

        public override void Initialize(IGun gun) => _handheld = gun as IHandheld;

        public override void Trigger() => _handheld.AudioPlayer.PlayClip(_fireAudio, BodyPart.Hands);
    }

    [Serializable]
    public sealed class MuzzleParticlesEffect : MuzzleEffect
    {
        [Tooltip("Cooldown between consecutive muzzle effect particle plays.")]
        [SerializeField, Range(0f, 1f)]
        private float _playCooldown = 0.2f;

        [Tooltip("Particle systems to play when muzzle effect is triggered.")]
        [ReorderableList(HasLabels = false, Foldable = true)]
        [SerializeField]
        private ParticleSystem[] _particles;

        private float _nextPlayTime;

        public override void Trigger()
        {
            if (Time.time < _nextPlayTime)
            {
                return;
            }

            foreach (var particle in _particles)
            {
                particle.Play(false);
            }

            _nextPlayTime = Time.time + _playCooldown;
        }

#if UNITY_EDITOR
        public override void RefreshReferences(Transform behaviour) => _particles = behaviour.GetComponentsInChildren<ParticleSystem>();
#endif
    }

    [Serializable]
    public sealed class MuzzleLightEffect : MuzzleEffect
    {
        [Tooltip("Light effect to play when muzzle effect triggered.")]
        [SerializeField]
        private DynamicLight _light;

        public override void Trigger() => _light.Play(false);

#if UNITY_EDITOR
        public override void RefreshReferences(Transform behaviour) => _light = behaviour.GetComponentInChildren<DynamicLight>();
#endif
    }

    /// <summary>
    /// Moves the center muzzle effect in the local space by some amount when it is aiming
    /// to adjust the position for effects or something else that might be needed.
    /// </summary>
    /// <remarks>
    /// Center of the muzzle effect is the 'local position' of the particle effect that 
    /// has the most children.
    /// </remarks>
    [Serializable]
    public sealed class MuzzleAimOffsetEffect : MuzzleEffect
    {
        [Tooltip("Additional offset used for muzzle effects when aiming the gun and firing.")]
        [SerializeField]
        private Vector3 _aimingOffset;

        [SerializeField]
        private Transform _targetRoot;

        private IGun _gun;
        private Vector3 _originalOffset;

        public override void Initialize(IGun gun)
        {
            _gun = gun;
            if (_targetRoot != null)
            {
                _originalOffset = _targetRoot.localPosition;
            }
        }

        public override void Trigger()
        {
            if (_targetRoot == null)
            {
                return;
            }

            _targetRoot.localPosition = _gun.AimBehaviour.IsAiming
                ? _originalOffset + _aimingOffset
                : _originalOffset;
        }

#if UNITY_EDITOR
        public override void RefreshReferences(Transform behaviour)
        {
            var particles = behaviour.GetComponentsInChildren<ParticleSystem>();

            if (particles != null && particles.IsEmpty() == false && _targetRoot == null)
            {
                int maxChildCount = 0;
                foreach (var particle in particles)
                {
                    var transform = particle.transform;

                    if (transform.childCount > maxChildCount)
                    {
                        maxChildCount = transform.childCount;
                        _targetRoot = transform;
                    }
                }

                if (_targetRoot == null)
                {
                    _targetRoot = particles[0].transform.parent;
                }
            }
        }
#endif
    }
}