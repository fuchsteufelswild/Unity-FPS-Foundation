using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    public interface IStaminaAudioHandler
    {
        /// <summary>
        /// Checks the stamina and plays heavy breathing if needed.
        /// </summary>
        /// <param name="currentStamina">Current stamina amount.</param>
        /// <param name="heavyBreathThreshold">The amount, under which the character starts to heavy breath.</param>
        void HandleHeavyBreathing(float currentStamina, float heavyBreathThreshold);
    }

    public sealed class StaminaAudioHandler : IStaminaAudioHandler
    {
        private readonly ICharacter _character;
        private readonly float _breathingHeavyDuration;
        private readonly AudioCue _breathingHeavyAudio;

        private float _heavyBreathTimer;

        public StaminaAudioHandler(ICharacter character, float breathingHeavyDuration, AudioCue breathingHeavyAudio)
        {
            _character = character;
            _breathingHeavyDuration = breathingHeavyDuration;
            _breathingHeavyAudio = breathingHeavyAudio;
        }

        public void HandleHeavyBreathing(float currentStamina, float threshold)
        {
            // Stamina should be less than threshold
            // Heavy breathing exists (character heavy breaths more than 0 seconds)
            // Last heavy breathing loop has not finished
            if(currentStamina < threshold
            && _breathingHeavyDuration > 0f
            && _heavyBreathTimer < Time.time)
            {
                _character.AudioPlayer.StartLoop(_breathingHeavyAudio, BodyPart.Head, _breathingHeavyDuration);

                _heavyBreathTimer = Time.time + _breathingHeavyDuration;
            }
        }
    }
}