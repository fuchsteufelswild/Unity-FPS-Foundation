using System;
using System.Collections;
using UnityEngine;

namespace Nexora.Audio
{
    /// <summary>
    /// <inheritdoc cref="SequencePlayer" path="/summary"/>
    /// Audio cues must have <see cref="AudioClip"/>, no other resource.
    /// Makes sure that list elements are valid all the time.
    /// Provides randomness on <see cref="Volume"/> and <see cref="Pitch"/>,
    /// so every time they are acquired, value is a bit different.
    /// </summary>
    [Serializable]
    public sealed class AudioSequence :
        ISerializationCallbackReceiver
    {
        [SerializeField, HideLabel, IgnoreParent]
        private SequencePlayer _sequencePlayer;

        [Tooltip("Sequence of cues with own settings, may only contain AudioClips." +
            " Delays of the cues are in terms of total time, that is waiting for the previous ones count as well.")]
        [SerializeField, SpaceArea]
        [ReorderableList(ListStyle.Lined, HasLabels = false), IgnoreParent]
        private AudioCue[] _audioCues = Array.Empty<AudioCue>();

        public bool IsPlayable => _sequencePlayer.IsPlayable && _audioCues.Length > 0;
        public float Volume => _sequencePlayer.Volume;
        public float Pitch => _sequencePlayer.Pitch;

        public AudioCue[] AudioCues => _audioCues;

        public AudioSequence() { }

        public AudioSequence(float volume, float pitch, float randomness, AudioCue[] audioCues)
        {
            _sequencePlayer = new SequencePlayer(volume, pitch, randomness);
            _audioCues = audioCues;
        }

        /// <inheritdoc cref="SequencePlayer.PlaySequenceOn(AudioSource, MonoBehaviour, AudioCue[], float)"/>
        public void PlaySequenceOn(
            AudioSource audioSource,
            MonoBehaviour owner,
            float speed = 1f)
        {
            if(audioSource == null || owner == null
            || IsPlayable == false || UnityUtils.IsQuitting)
            {
                return;
            }

            _sequencePlayer.PlaySequenceOn(audioSource, owner, _audioCues, speed);
        }

        /// <inheritdoc cref="SequencePlayer.PlaySequenceOn(AudioSource, AudioCue[], float)"/>
        public IEnumerator PlaySequenceOn(AudioSource audioSource, float speed = 1f)
            => _sequencePlayer.PlaySequenceOn(audioSource, _audioCues, speed);

        public void OnAfterDeserialize() { }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                FixCues();
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Removes incompatible types from the array.
        /// <br></br>Makes sure that in the cue list for every cue
        /// comes after the previous one should have at least the same delay.
        /// <br></br>Increases the volumes of the cues that are silenced.
        /// </summary>
        private void FixCues()
        {
            if(_audioCues == null)
            {
                return;
            }

            RemoveIncompatibleTypes();
            FixDelays();
            FixVolumes();

            return;

            // Removes types that are incompatible (e.g RandomAudioContainer)
            void RemoveIncompatibleTypes()
            {
                for (int i = 0; i < _audioCues.Length; i++)
                {
                    if (_audioCues[i].Clip != null && _audioCues[i].Clip is not AudioClip)
                    {
                        UnityEditor.ArrayUtility.RemoveAt(ref _audioCues, i);
                    }
                }
            }

            // Clamp delay of the following cue to the previous one.
            void FixDelays()
            {
                float minDelay = 0.0f;
                for (int i = 0; i < _audioCues.Length; i++)
                {
                    _audioCues[i].Delay = Mathf.Max(minDelay, _audioCues[i].Delay);
                    minDelay = _audioCues[i].Delay;
                }
            }

            // Reopens the cues that are silenced.
            void FixVolumes()
            {
                for(int i = 0; i < _audioCues.Length; i++)
                {
                    if (_audioCues[i].Volume < AudioCue.SilenceVolumeLimit)
                    {
                        _audioCues[i].Volume = Volume;
                    }    
                }
            }
        }
#endif

        /// <summary>
        /// Class to play sequences one by one with their delays.
        /// Has fields for base scalers, and randomness values.
        /// </summary>
        [Serializable]
        private sealed class SequencePlayer
        {
            [BeginHorizontal]
            [Tooltip("Base volume of the audio cues the sequence has.")]
            [Header("Volume"), HideLabel]
            [SerializeField]
            [Clamp(0f, 1f)]
            private float _volume = 1f;

            [Tooltip("Base pitch of the audio cues the sequence has.")]
            [Header("Pitch"), HideLabel]
            [SerializeField]
            [Clamp(0f, 2f)]
            private float _pitch = 1f;

            [Header("Randomness"), HideLabel]
            [EndHorizontal, SerializeField]
            [Tooltip("Volume/pitch randomness (0 to 1).")]
            [Clamp(0f, 1f)]
            private float _randomness = 0.05f;

            [Tooltip("Scales randomness for pitch (0 to 1).")]
            [Range(0f, 1f)]
            private float _pitchRandomnessScale = 0.3f;

            public bool IsPlayable => _volume > AudioCue.SilenceVolumeLimit;
            public float Volume => _volume.Jitter(_randomness);
            public float Pitch => _pitch.Jitter(_randomness * _pitchRandomnessScale);

            public SequencePlayer() { }

            public SequencePlayer(float volume, float pitch, float randomness)
            {
                _volume = volume;
                _pitch = pitch;
                _randomness = randomness;
            }

            /// <summary>
            /// Starts a coroutine to play the cues one by one 
            /// if array contains more than one element or has delay.
            /// Else immediately plays one cue and return.
            /// </summary>
            public void PlaySequenceOn(
                AudioSource audioSource, 
                MonoBehaviour owner, 
                AudioCue[] audioCues,
                float speed = 1f)
            {
                audioSource.volume = Volume;
                audioSource.pitch = Pitch;

                if (audioCues.Length == 1 && audioCues.First().Delay < AudioCue.SilenceVolumeLimit)
                {
                    PlayImmediate();
                }
                else
                {
                    owner.StartCoroutine(PlaySequenceOn(audioSource, audioCues, speed));
                }

                return;

                void PlayImmediate()
                {
                    audioSource.PlayOneShot((AudioClip)audioCues.First().Clip, audioCues.First().Volume * _volume);
                }
            }

            /// <summary>
            /// Plays <see cref="AudioCue"/> one by one by respecting to their delays.
            /// </summary>
            /// <remarks>
            /// Uses <b>playOnAwake</b> as a hack to control the enabled status of the sequence.
            /// </remarks>
            public IEnumerator PlaySequenceOn(AudioSource audioSource, AudioCue[] audioCues, float speed = 1f)
            {
                audioSource.playOnAwake = true;

                float startTime = Time.time;
                float inverseSpeed = 1f / speed;
                foreach(AudioCue audioCue in  audioCues)
                {
                    // Waits for the remaining time to wait total for the required delay.
                    float passedTime = Time.time - startTime;
                    float remainingDelay = (audioCue.Delay - passedTime) * inverseSpeed;
                    yield return new Delay(remainingDelay);

                    audioSource.PlayOneShot((AudioClip)audioCue.Clip, audioCue.Volume * _volume);
                }

                audioSource.playOnAwake = false;
            }
        }
    }
}
