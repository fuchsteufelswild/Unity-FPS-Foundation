using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Nexora.Audio
{
    /// <summary>
    /// Struct that represents audio to be played. 
    /// Has fields for audio clip, volume, and an optional delay before playing the audio.
    /// </summary>
    [Serializable]
    public struct AudioCue
    {
        public const float SilenceVolumeLimit = 0.001f;

        [Tooltip("The audio resource containing the clip, it can be Audio Random Container or an Audio Clip.")]
        public AudioResource Clip;

        [Tooltip("Volume multiplier, (0 to 1).")]
        public float Volume;

        [Tooltip("Delay before playing the clip (seconds).")]
        [Min(0f)]
        public float Delay;

        public AudioCue(AudioClip clip, float volume = 1f, float delay = 0f)
        {
            Clip = clip;
            Volume = volume;
            Delay = delay;
        }

        public readonly bool IsPlayable => Volume > SilenceVolumeLimit && Clip != null;
    }
}