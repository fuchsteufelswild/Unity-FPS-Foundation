using UnityEngine;

namespace Nexora.Audio
{
    public enum AudioChannel
    {
        Master,
        Music,
        SFX,
        UI
    }

    public static partial class AudioMixerVolumeParameters
    {
        public const string Master = "MasterVolume";
        public const string Music = "MusicVolume";
        public const string SFX = "SFXVolume";
        public const string UI = "UIVolume";
    }

    public static partial class AudioSourceDefaults
    {
        public const float Min3DRange = 2f;
    }
}