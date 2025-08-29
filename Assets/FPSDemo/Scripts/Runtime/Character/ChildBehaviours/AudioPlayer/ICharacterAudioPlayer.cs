using Nexora.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    /// <summary>
    /// Interface for playing audios that are on/attached to the character.
    /// </summary>
    public interface ICharacterAudioPlayer
    {
        /// <summary>
        /// Play <paramref name="clip"/> at <paramref name="bodyPart"/> with given parameters.
        /// </summary>
        /// <param name="clip">Clip to play.</param>
        /// <param name="bodyPart">Body part where the audio will be played.</param>
        /// <param name="volume">Volume multiplier of the audio.</param>
        /// <param name="delay">Delay before playing the clip.</param>
        /// <returns><see cref="AudioSource"/> on which the clip is to be played.</returns>
        AudioSource PlayClip(AudioResource clip, BodyPart bodyPart, float volume = 1f, float delay = 0f);

        /// <summary>
        /// Plays <paramref name="sequence"/> at <paramref name="bodyPart"/> with given parameters.
        /// </summary>
        /// <param name="sequence"><see cref="AudioSequence"/> to play.</param>
        /// <param name="bodyPart">Body part where the audio will be played.</param>
        /// <param name="speed">Speed of playing the sequence.</param>
        /// <returns><see cref="AudioSource"/> on which the clip is to be played.</returns>
        AudioSource PlaySequence(AudioSequence sequence, BodyPart bodyPart, float speed = 1f);

        /// <summary>
        /// Plays <paramref name="clip"/> as loop at <paramref name="bodyPart"/> with given parameters.
        /// </summary>
        /// <param name="clip">Clip to loop.</param>
        /// <param name="bodyPart">Body part where the audio will be played.</param>
        /// <param name="volume">Volume multiplier of the audio.</param>
        /// <param name="duration">Duration fo the loop, default is infinity.</param>
        /// <returns><see cref="AudioSource"/> on which the clip is to be played.</returns>
        AudioSource StartLoop(AudioResource clip, BodyPart bodyPart, float volume = 1f, float duration = float.PositiveInfinity);

        /// <summary>
        /// Stops the <paramref name="audioSource"/> loop that is playing.
        /// </summary>
        void StopLoop(AudioSource audioSource);
    }

    /// <summary>
    /// Default audio player if no other is existent, just uses global <see cref="AudioSource"/> irrespective
    /// of the character.
    /// </summary>
    public class DefaultCharacterAudioPlayer : ICharacterAudioPlayer
    {
        public AudioSource PlayClip(AudioResource clip, BodyPart bodyPart, float volume = 1, float delay = 0)
            => AudioModule.Instance.PlayClipOneShot(clip, volume, delay);

        public AudioSource PlaySequence(AudioSequence sequence, BodyPart bodyPart, float speed = 1)
            => AudioModule.Instance.PlaySequenceOneShot(sequence, speed);

        public AudioSource StartLoop(AudioResource clip, BodyPart bodyPart, float volume = 1, float duration = float.PositiveInfinity)
            => AudioModule.Instance.StartLoop(clip, volume, duration);

        public void StopLoop(AudioSource audioSource)
            => AudioModule.Instance.StopLoop(audioSource);
    }

    public static partial class CharacterAudioPlayerExtensions
    {
        public static AudioSource PlayClip(this ICharacterAudioPlayer audioPlayer, AudioCue audioData, BodyPart bodyPart)
        {
            return audioData.IsPlayable
                ? audioPlayer.PlayClip(audioData.Clip, bodyPart, audioData.Volume, audioData.Delay)
                : null;
        }

        public static AudioSource PlayClip(this ICharacterAudioPlayer audioPlayer, AudioCue audioData, BodyPart bodyPart, float volumeScale)
        {
            return audioData.IsPlayable
                ? audioPlayer.PlayClip(audioData.Clip, bodyPart, audioData.Volume * volumeScale, audioData.Delay)
                : null;
        }

        public static AudioSource StartLoop(this ICharacterAudioPlayer audioPlayer, AudioCue audioData, BodyPart bodyPart, float duration = float.PositiveInfinity)
        {
            return audioData.IsPlayable
                ? audioPlayer.StartLoop(audioData.Clip, bodyPart, audioData.Volume, duration)
                : null;
        }
    }
}