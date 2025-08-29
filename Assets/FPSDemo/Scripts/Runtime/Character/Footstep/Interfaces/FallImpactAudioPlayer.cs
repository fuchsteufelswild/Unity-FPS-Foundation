using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.SurfaceSystem;
using UnityEngine;

namespace Nexora.FPSDemo.Footsteps
{
    /// <summary>
    /// Information about the fall data.
    /// </summary>
    public readonly struct FallImpactData
    {
        public readonly float Speed;
        public FallImpactData(float speed) => Speed = speed;
    }

    public interface IFallImpactAudioPlayer
    {
        /// <summary>
        /// Plays an impact audio using the obtained data of <paramref name="groundData"/> and <paramref name="fallImpactData"/>
        /// </summary>
        /// <param name="groundData">Information about the ground controller standing on.</param>
        /// <param name="fallImpactData">Information about the fall impact.</param>
        /// <returns>Information about the played audio.</returns>
        AudioEffectResult PlayImpactAudio(in GroundDetectionData groundData, in FallImpactData fallImpactData);
    }

    /// <summary>
    /// Class to play audio of fall impacts.
    /// </summary>
    public sealed class FallImpactAudioPlayer : IFallImpactAudioPlayer
    {
        private readonly FootstepConfig _footstepConfig;
        private float _nextPossibleImpactTime;

        public FallImpactAudioPlayer(FootstepConfig config) => _footstepConfig = config;

        public AudioEffectResult PlayImpactAudio(in GroundDetectionData groundData, in FallImpactData fallImpactData)
        {
            if(groundData.IsValid == false || ShouldPlayImpact(fallImpactData.Speed) == false)
            {
                return default;
            }

            float impactIntensity = CalculateImpactIntensity(fallImpactData.Speed);
            SurfaceEffectType fallImpactType = GetImpactEffectType(impactIntensity);

            _nextPossibleImpactTime = Time.time + _footstepConfig.ImpactCooldown;

            SurfaceDefinition surface = SurfaceSystemModule.Instance.PlayEffectFromHit(
                in groundData.Hit,
                fallImpactType,
                SurfaceEffectFlags.Audio,
                impactIntensity);

            return new AudioEffectResult(surface, impactIntensity, fallImpactType);
        }

        private bool ShouldPlayImpact(float impactSpeed)
        {
            return Mathf.Abs(impactSpeed) > _footstepConfig.MinImpactSpeed 
                && Time.time > _nextPossibleImpactTime;
        }

        private float CalculateImpactIntensity(float impactSpeed)
        {
            return Mathf.InverseLerp(_footstepConfig.MinImpactSpeed, _footstepConfig.MaxImpactSpeed, Mathf.Abs(impactSpeed));
        }

        private SurfaceEffectType GetImpactEffectType(float impactIntensity)
        {
            return impactIntensity switch
            {
                float intensity when intensity > 0.75f => SurfaceEffectType.HardFallImpact,
                float intensity when intensity > 0.15f => SurfaceEffectType.MediumFallImpact,
                _ => SurfaceEffectType.LightFallImpact
            };
        }
    }
}