using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Abstract base class for any effect to be triggered by dry fire.
    /// </summary>
    [Serializable]
    public abstract class CartridgeVisualizationEffect
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
    }

    [Serializable]
    public sealed class CartridgeVisualizationAudioEffect : MuzzleEffect
    {
        [Tooltip("Audio played when triggered cartridge visulization change.")]
        [SerializeField]
        private AudioCue _audio = new(null);

        private IHandheld _handheld;

        public override void Initialize(IGun gun) => _handheld = gun as IHandheld;

        public override void Trigger() => _handheld.AudioPlayer.PlayClip(_audio, BodyPart.Hands);
    }
}