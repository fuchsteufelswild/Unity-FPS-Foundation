using Nexora.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace Nexora.FPSDemo
{
    public enum AudioPlaybackMode
    {
        Static2D = 0,

        /// <summary>
        /// Audio played in 3D space at a position (not moving).
        /// </summary>
        Static3D = 1,

        /// <summary>
        /// Audio played in 3D space and attached to the <see cref="Transform"/>.
        /// </summary>
        Follow3D = 2
    }

    public interface IAudioPlaybackStrategy
    {
        /// <summary>
        /// Play <paramref name="clip"/> at <paramref name="transform"/> with given parameters.
        /// </summary>
        /// <param name="clip">Clip to play.</param>
        /// <param name="transform">Transform where the audio will be played.</param>
        /// <param name="volume">Volume multiplier of the audio.</param>
        /// <param name="delay">Delay before playing the clip.</param>
        /// <returns><see cref="AudioSource"/> on which the clip is to be played.</returns>
        AudioSource PlayClip(AudioResource clip, Transform transform, float volume, float delay);

        /// <summary>
        /// Plays <paramref name="sequence"/> at <paramref name="transform"/> with given parameters.
        /// </summary>
        /// <param name="sequence"><see cref="AudioSequence"/> to play.</param>
        /// <param name="transform">Transform where the audio will be played.</param>
        /// <param name="speed">Speed of playing the sequence.</param>
        /// <returns><see cref="AudioSource"/> on which the clip is to be played.</returns>
        AudioSource PlaySequence(AudioSequence sequence, Transform transform, float speed);

        /// <summary>
        /// Plays <paramref name="clip"/> as loop at <paramref name="transform"/> with given parameters.
        /// </summary>
        /// <param name="clip">Clip to loop.</param>
        /// <param name="transform">Transform where the audio will be played.</param>
        /// <param name="volume">Volume multiplier of the audio.</param>
        /// <param name="duration">Duration fo the loop, default is infinity.</param>
        /// <returns><see cref="AudioSource"/> on which the clip is to be played.</returns>
        AudioSource StartLoop(AudioResource clip, Transform transform, float volume, float duration);
    }

    /// <summary>
    /// Plays the all given audios as <b>2D</b>.
    /// </summary>
    public sealed class PlaybackStrategy2D : IAudioPlaybackStrategy
    {
        public AudioSource PlayClip(AudioResource clip, Transform transform, float volume, float delay)
        {
            return AudioModule.Instance.PlayClipOneShot(clip, volume, delay);
        }

        public AudioSource PlaySequence(AudioSequence sequence, Transform transform, float speed)
        {
            return AudioModule.Instance.PlaySequenceOneShot(sequence, speed);
        }

        public AudioSource StartLoop(AudioResource clip, Transform transform, float volume, float duration)
        {
            return AudioModule.Instance.StartLoop(clip, volume, duration);
        }
    }

    /// <summary>
    /// Plays all given audio in <b>3D static</b> space, that is at the position of the given <see cref="Transform"/>.
    /// </summary>
    public sealed class PlaybackStrategy3DStatic : IAudioPlaybackStrategy
    {
        public AudioSource PlayClip(AudioResource clip, Transform transform, float volume, float delay)
        {
            return AudioModule.Instance.PlayClipOneShot(clip, transform.position, volume, delay);
        }

        public AudioSource PlaySequence(AudioSequence sequence, Transform transform, float speed)
        {
            return AudioModule.Instance.PlaySequenceOneShot(sequence, transform.position, speed);
        }

        public AudioSource StartLoop(AudioResource clip, Transform transform, float volume, float duration)
        {
            return AudioModule.Instance.StartLoop(clip, transform, volume, duration);
        }
    }

    /// <summary>
    /// Plays all given audio as an object attached to the <see cref="Transform"/> provided.
    /// </summary>
    public sealed class PlaybackStrategy3DFollow : IAudioPlaybackStrategy
    {
        public AudioSource PlayClip(AudioResource clip, Transform transform, float volume, float delay)
        {
            return AudioModule.Instance.PlayClipOneShot(clip, transform, volume, delay);
        }

        public AudioSource PlaySequence(AudioSequence sequence, Transform transform, float speed)
        {
            return AudioModule.Instance.PlaySequenceOneShot(sequence, transform, speed);
        }

        public AudioSource StartLoop(AudioResource clip, Transform transform, float volume, float duration)
        {
            return AudioModule.Instance.StartLoop(clip, transform, volume, duration);
        }
    }
}