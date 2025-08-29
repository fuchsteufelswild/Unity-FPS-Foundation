using Nexora.Audio;
using Nexora.FPSDemo.SurfaceSystem;
using System;
using UnityEngine;

namespace Nexora.FPSDemo
{
    public sealed class SurfaceImpactHandler : MonoBehaviour
    {
        [Tooltip("Cooldown between consecutive effect plays.")]
        [SerializeField, Range(0f, 2f)]
        private float _effectPlayCooldown = 0.5f;

        [ReorderableList]
        [ReferencePicker(typeof(SurfaceImpactEffect))]
        [SerializeReference]
        private SurfaceImpactEffect[] _effects = Array.Empty<SurfaceImpactEffect>();

        private float _nextEffectPlayTime;

        private void Awake()
        {
            foreach(var effect in _effects)
            {
                effect.Initialize(GetComponentInChildren<Collider>());
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(Time.time < _nextEffectPlayTime)
            {
                return;
            }

            foreach (var effect in _effects)
            {
                effect.Play(collision);
            }

            _nextEffectPlayTime = Time.time + _effectPlayCooldown;
        }
    }

    [Serializable]
    public abstract class SurfaceImpactEffect
    {
        public virtual void Initialize(Collider collider)
        {

        }

        public abstract void Play(Collision other);
    }

    [Serializable]
    public sealed class SurfaceAudioEffect : SurfaceImpactEffect
    {
        [SerializeField]
        private AudioCue _impactAudio = new(null);

        [SerializeField, Range(0f, 2f)]
        private float _volumeMultiplier = 1f;

        public override void Play(Collision other) => Play(other.contacts[0].point, 1f);

        public void Play(Vector3 position, float volumeMultiplier) => AudioModule.Instance.PlayCueOneShot(_impactAudio, position, volumeMultiplier * _volumeMultiplier);
    }

    [Serializable]
    public sealed class SurfaceDynamicEffect : SurfaceImpactEffect
    {
        private const float MinImpactSpeedThreshold = 2.5f;

        private SurfaceEffects _surfaceEffectData;

        public override void Initialize(Collider collider)
        {
            SurfaceDefinition surface = SurfaceSystemModule.Instance.GetSurfaceFromCollider(collider);
            if(surface != null && surface.TryGetEffect(SurfaceEffectType.LightFallImpact, out _surfaceEffectData))
            {
                return;
            }

            Debug.LogError("Surface visual effect needs to have a valid surface effect data.");
        }

        public override void Play(Collision other)
        {
            float relativeVelocityMagnitude = other.relativeVelocity.magnitude;
            if(relativeVelocityMagnitude < MinImpactSpeedThreshold)
            {
                return;
            }

            float audioVolume = Mathf.Clamp(relativeVelocityMagnitude / MinImpactSpeedThreshold / 10f, 0.2f, 1f);

            ContactPoint contact = other.contacts[0];
            Vector3 spawnPosition = contact.point;
            Quaternion spawnRotation = Quaternion.LookRotation(contact.normal);

            SurfaceSystemModule.Instance.PlayEffect(_surfaceEffectData, spawnPosition, spawnRotation, SurfaceEffectFlags.AudioVisual, 1f, null);
        }
    }
}