using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IAttachmentFeedbackPlayer
    {
        /// <summary>
        /// Plays effects on changing the mode of the attachment.
        /// </summary>
        void PlayModeChangeEffects();
    }

    [Serializable]
    public sealed class AttachmentFeedbackPlayer : IAttachmentFeedbackPlayer
    {
        [Tooltip("Audio played when changing mode.")]
        [SerializeField]
        private AudioCue _changeModeAudio = new(null);

        [Tooltip("Controls if animation will be played upon changing the mode.")]
        [SerializeField]
        private bool _playChangeModeAnimation;

        private IHandheld _handheld;

        public void Initialize(IHandheld handheld) => _handheld = handheld;

        public void PlayModeChangeEffects()
        {
            _handheld.AudioPlayer.PlayClip(_changeModeAudio, BodyPart.Hands);

            if(_playChangeModeAnimation)
            {
                _handheld.Animator.SetTrigger(HandheldAnimationConstants.ChangeMode);
            }
        }
    }
}