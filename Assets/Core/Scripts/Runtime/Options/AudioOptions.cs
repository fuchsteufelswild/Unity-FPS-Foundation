using Nexora.Audio;
using System;
using UnityEngine;

namespace Nexora.Options
{
    [CreateAssetMenu(menuName = CreateAssetMenuPath + "Audio Options", fileName = nameof(AudioOptions))]
    public sealed partial class AudioOptions : Options<AudioOptions>
    {
        [SerializeField]
        private Option<float> _masterVolume = new(1f);

        [SerializeField]
        private Option<float> _musicVolume = new(1f);

        [SerializeField]
        private Option<float> _sfxVolume = new(1f);

        [SerializeField]
        private Option<float> _uiVolume = new(1f);

        public Option<float> MasterVolume => _masterVolume;
        public Option<float> MusicVolume => _musicVolume;
        public Option<float> SFXVolume => _sfxVolume;
        public Option<float> UIVolume => _uiVolume;

        public Option<float> GetVolume(AudioChannel audioChannel) =>
            audioChannel switch
            {
                AudioChannel.Master => MasterVolume,
                AudioChannel.Music => MusicVolume,
                AudioChannel.SFX => SFXVolume,
                AudioChannel.UI => UIVolume,
                _ => throw new ArgumentOutOfRangeException(string.Format(
                    "The given enum value {0} is out of range for enum {1}", audioChannel.ToString(), nameof(AudioChannel)))
            };

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditorUtils.SafeOnValidate(this, () =>
            {
                _masterVolume.SetValue(Mathf.Clamp01(_masterVolume.Value));
                _musicVolume.SetValue(Mathf.Clamp01((_musicVolume.Value)));
                _sfxVolume.SetValue(Mathf.Clamp01((_sfxVolume.Value)));
                _uiVolume.SetValue(Mathf.Clamp01((_uiVolume.Value)));
            });
        }
#endif
    }
}