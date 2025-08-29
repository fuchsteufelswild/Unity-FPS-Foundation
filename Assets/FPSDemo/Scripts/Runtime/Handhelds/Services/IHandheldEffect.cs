using Nexora.Animation;
using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.Rendering;
using System;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Abstract base class for any effect to be triggered through a <see cref="IHandheld"/>.
    /// </summary>
    public interface IHandheldEffect
    {
        /// <summary>
        /// Executes the effect.
        /// </summary>
        /// <param name="handheld">The handheld item triggering the effect.</param>
        public abstract void Execute(IHandheld handheld);

        /// <summary>
        /// Stops the effect if it is possible.
        /// </summary>
        public void Stop() { }
    }

    [Serializable]
    public sealed class PlayAudioEffect : IHandheldEffect
    {
        [SerializeField]
        private AudioCue _audio;

        public void Execute(IHandheld handheld) => handheld.AudioPlayer.PlayClip(_audio, BodyPart.Hands);
    }

    [Serializable]
    public sealed class SetAnimatorParameterEffect : IHandheldEffect
    {
        [SerializeField, ReorderableList]
        private AnimatorParameter[] _parameters;

        public void Execute(IHandheld handheld)
        {
            foreach(var parameter in _parameters)
            {
                handheld.Animator.SetParameter(parameter.Type, parameter.Hash, parameter.Value);
            }
        }
    }

    [Serializable]
    public sealed class ToggleParticlesEffect : IHandheldEffect
    {
        [SerializeField, ReorderableList]
        private ParticleSystem[] _particles;

        public void Execute(IHandheld handheld)
        {
            foreach(var particle in _particles)
            {
                particle.Play(true);
            }
        }

        public void Stop()
        {
            foreach (var particle in _particles)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    [Serializable]
    public sealed class ToggleLightsEffect : IHandheldEffect
    {
        [SerializeField, ReorderableList]
        private DynamicLight[] _lights;

        public void Execute(IHandheld handheld)
        {
            foreach (var light in _lights)
            {
                light.Play();
            }
        }

        public void Stop()
        {
            foreach (var light in _lights)
            {
                light.Stop();
            }
        }
    }
}