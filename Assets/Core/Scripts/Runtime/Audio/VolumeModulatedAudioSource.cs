using UnityEngine;

namespace Nexora.Audio
{
    /// <summary>
    /// Class to fades in/out audio source volume, while making it a little fluctuate 
    /// with noise to give it a natural vibe. 
    /// </summary>
    /// <remarks>
    /// <see cref="ModulatedAudioSource.ExternalParameterMultiplier"/> is used for extra control.
    /// </remarks>
    [AddComponentMenu(ComponentMenuPaths.Audio + nameof(VolumeModulatedAudioSource))]
    public class VolumeModulatedAudioSource :
        ModulatedAudioSource
    {
        [SerializeField]
        [Range(0f, 10f)]
        private float _volume;

        /// <summary>
        /// Updates the volume using the defined noise, external multiplier and
        /// the current fade progress.
        /// </summary>
        protected override void UpdateAudioParameters(float deltaTime)
        {
            float noise = _noise.Enabled
                ? Mathf.PerlinNoise(Time.time * _noise.Speed, 0f) * _noise.Intensity
                : 0f;

            float modulatedVolume = (_volume + noise * _volume) * ExternalParameterMultiplier * _fadeProgress;

            _audioSource.volume = modulatedVolume;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditorUtils.SafeOnValidate(this, () =>
            {
                Validate();
            });
        }

        protected override void Validate()
        {
            base.Validate();
            _audioSource.volume = _volume;
        }
#endif
    }
}
