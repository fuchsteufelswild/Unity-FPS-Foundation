using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.SurfaceSystem;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nexora.FPSDemo.Footsteps
{
    /// <summary>
    /// Information about the current movement of the controller.
    /// </summary>
    public readonly struct MovementData
    {
        public readonly float Speed;
        public readonly float TurnSpeed;
        public readonly MovementStateType StateType;

        public MovementData(float speed, float turnSpeed, MovementStateType stateType)
        {
            Speed = speed;
            TurnSpeed = turnSpeed;
            StateType = stateType;
        }
    }

    public interface IFootstepAudioPlayer
    {
        /// <summary>
        /// Plays an footstep audio using the obtained data of <paramref name="groundData"/> and <paramref name="movementData"/>
        /// </summary>
        /// <param name="groundData">Information about the ground controller standing on.</param>
        /// <param name="movementData">Information about the movement of the controller.</param>
        /// <returns>Information about the played audio.</returns>
        AudioEffectResult PlayFootstepAudio(in GroundDetectionData groundData, in MovementData movementData);
    }

    public sealed class FootstepAudioPlayer : IFootstepAudioPlayer
    {
        private enum FootstepType
        {
            Left = 0,
            Right = 1,
        }

        private readonly FootstepConfig _footstepConfig;
        private FootstepType _currentFootstepType;

        public FootstepAudioPlayer(FootstepConfig config) => _footstepConfig = config;

        public AudioEffectResult PlayFootstepAudio(in GroundDetectionData groundData, in MovementData movementData)
        {
            if(groundData.IsValid == false)
            {
                return default;
            }

            float volume = CalculateFootstepVolume(in movementData);
            if(volume <= 0)
            {
                return default;
            }

            SurfaceEffectType effectType = GetFootstepEffectType(movementData.StateType);
            AlternateFootstep();

            SurfaceDefinition surface = SurfaceSystemModule.Instance.PlayEffectFromHit(
                in groundData.Hit,
                effectType,
                SurfaceEffectFlags.Audio,
                volume);

            return new AudioEffectResult(surface, volume, effectType);
        }

        private float CalculateFootstepVolume(in MovementData movementData)
        {
            float totalSpeed = movementData.Speed + movementData.TurnSpeed;

            if(totalSpeed < _footstepConfig.MinAudibleSpeed)
            {
                return 0f;
            }

            float normalizedSpeed = Mathf.InverseLerp(_footstepConfig.MinAudibleSpeed, _footstepConfig.MaxVolumeSpeed, totalSpeed);
            return Mathf.Lerp(_footstepConfig.MinFootstepVolume, _footstepConfig.MaxFootstepVolume, normalizedSpeed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SurfaceEffectType GetFootstepEffectType(MovementStateType stateType)
        {
            return stateType == MovementStateType.Run ? SurfaceEffectType.RunFootstep : SurfaceEffectType.WalkFootstep;
        }

        private void AlternateFootstep()
        {
            _currentFootstepType = _currentFootstepType == FootstepType.Left
                ? FootstepType.Right
                : FootstepType.Left;
        }
    }
}