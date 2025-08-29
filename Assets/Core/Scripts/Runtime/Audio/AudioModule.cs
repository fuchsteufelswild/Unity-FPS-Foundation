using Nexora.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

namespace Nexora.Audio
{
    /// <summary>
    /// Big module that controls <see cref="AudioSource"/> pooling, Playing <see cref="AudioResource"/>
    /// as loops or single shots on various <see cref="AudioChannel"/>, Handling main <see cref="AudioMixer"/> values 
    /// depending on <see cref="AudioOptions"/>.
    /// </summary>
    /// <remarks>
    /// Uses subclasses <see cref="AudioSourcePool"/>, <see cref="AudioPlayer"/>, <see cref="OptionsEventHandler"/>,
    /// <see cref="AudioMixerGroups"/> to divide responsiblities into SRP.
    /// </remarks>
    [DefaultExecutionOrder(ExecutionOrder.GameModule)]
    [CreateAssetMenu(menuName = CreateMenuPath + "Audio Module", fileName = nameof(AudioModule))]
    public sealed partial class AudioModule : GameModule<AudioModule>
    {
        [SerializeField]
        [Tooltip("Main mixer that controls all audio.")]
        private AudioMixer _mainMixer;

        [Title("Snapshots")]
        [Tooltip("Default mixer to revert to.")]
        [SerializeField]
        private AudioMixerSnapshot _defaultMixerSnaphot;

        [SerializeField]
        [HideLabel, NewLabel("Mixer Groups")]
        private AudioMixerGroups _audioMixerGroups = new();

        [Tooltip("2D range representing, (initial, max) 2D audio source count.")]
        [SerializeField]
        [MinMaxSlider(8, 16)]
        private Vector2Int _audioSourceCountRange2D = new Vector2Int(8, 16);

        [Tooltip("2D range representing, (initial, max) 3D audio source count.")]
        [SerializeField]
        [MinMaxSlider(16, 64)]
        private Vector2Int _audioSourceCountRange3D = new Vector2Int(16, 64);

        private AudioSourcePool _audioSourcePool2D;
        private AudioSourcePool _audioSourcePool3D;

        private Transform _audioSourcesRoot;

        private AudioPlayer _audioPlayer2D;
        private AudioPlayer _audioPlayer3D;

        private OptionsEventHandler _optionsEventHandler;
        private ModuleCoroutineRunner _coroutineRunner;

        public AudioMixerSnapshot DefaultMixerSnaphot => _defaultMixerSnaphot;

        /// <summary>
        /// Dummy <see cref="MonoBehaviour"/> to play coroutines for looped audios.
        /// </summary>
        private sealed class ModuleCoroutineRunner : MonoBehaviour { }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init() => LoadOrCreateInstance();

        protected override void OnInitialized()
        {
            _audioSourcesRoot = CreateChildUnderModulesRoot("[AudioModuleRuntime]");
            _coroutineRunner = _audioSourcesRoot.AddComponent<ModuleCoroutineRunner>();

            CreateAudioSourcePools();
            CreateAudioPlayers();
            
            // _optionsEventHandler = new OptionsEventHandler(_mainMixer);

            return;

            void CreateAudioSourcePools()
            {
                _audioSourcePool2D = new AudioSourcePool2D(_audioMixerGroups.GetMixerGroup(AudioChannel.SFX), _audioSourcesRoot, _audioSourceCountRange2D);
                _audioSourcePool3D = new AudioSourcePool3D(_audioMixerGroups.GetMixerGroup(AudioChannel.SFX), _audioSourcesRoot, _audioSourceCountRange3D);
            }

            void CreateAudioPlayers()
            {
                _audioPlayer2D = new AudioPlayer(_audioSourcePool2D, _audioMixerGroups, _coroutineRunner);
                _audioPlayer3D = new AudioPlayer(_audioSourcePool3D, _audioMixerGroups, _coroutineRunner);
            }
        }

        /// <summary>
        /// Gets <see cref="AudioMixer"/> DB value using volume setting value (0-1).
        /// </summary>
        private static float GetDBForVolume(float volume) => Mathf.Log(Mathf.Clamp(volume, 0.001f, 1f)) * 20;

        /// <inheritdoc cref="AudioPlayer.PlayClipOneShot(AudioResource, AudioSource, float, float, AudioChannel)"/>
        public AudioSource PlayClipOneShot(
            AudioResource clip,
            float volume = 1f,
            float delay = 0f,
            AudioChannel audioChannel = AudioChannel.SFX)
        {
            return _audioPlayer2D.PlayClipOneShot(clip, audioSource:null, volume, delay, audioChannel);
        }

        /// <inheritdoc cref="AudioPlayer.PlayClipOneShot(AudioResource, Vector3, float, float, AudioChannel)"/>
        public AudioSource PlayClipOneShot(
            AudioResource clip, 
            Vector3 position, 
            float volume = 1f, 
            float delay = 0f,
            AudioChannel audioChannel = AudioChannel.SFX)
        {
            return _audioPlayer3D.PlayClipOneShot(clip, position, volume, delay, audioChannel);
        }

        /// <inheritdoc cref="AudioPlayer.PlayClipOneShot(AudioResource, Transform, float, float, AudioChannel)"/>
        public AudioSource PlayClipOneShot(
            AudioResource clip,
            Transform parent,
            float volume = 1f,
            float delay = 0f,
            AudioChannel audioChannel = AudioChannel.SFX)
        {
            return _audioPlayer3D.PlayClipOneShot(clip, parent, volume, delay, audioChannel);
        }

        /// <inheritdoc cref="AudioPlayer.StartLoop(AudioResource, float, float, AudioChannel)"/>
        public AudioSource StartLoop(
            AudioResource clip,
            float volume = 1f,
            float duration = 1f,
            AudioChannel audioChannel = AudioChannel.SFX)
        {
            return _audioPlayer2D.StartLoop(clip, volume, duration, audioChannel);
        }

        /// <inheritdoc cref="AudioPlayer.StartLoop(AudioResource, Transform, float, float, AudioChannel)"/>
        public AudioSource StartLoop(
            AudioResource clip,
            Transform parent,
            float volume = 1f,
            float duration = 1f,
            AudioChannel audioChannel = AudioChannel.SFX)
        {
            return _audioPlayer3D.StartLoop(clip, parent, volume, duration, audioChannel);
        }

        /// <summary>
        /// Stops the loop on the <paramref name="audioSource"/>.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="AudioSource.playOnAwake"/> is a hack that is mentioned in <see cref="AudioPlayer"/>.
        /// </remarks>
        public void StopLoop(AudioSource audioSource)
        {
            audioSource.playOnAwake = false;
        }

        /// <summary>
        /// Pool that uses <see cref="List{T}"/> as underlying container to hold <see cref="AudioSource"/>.
        /// Defines initial and maximum amount of <see cref="AudioSource"/> available in the pool. 
        /// Uses circular search technique to iterate through the pool and gets the first available
        /// object in the pool, may create a new object. If no object is found frees one object and returns it.
        /// </summary>
        private abstract class AudioSourcePool
        {
            private const int MaxAcquireRetryCount = 5;

            private List<AudioSource> _audioSources;

            private Vector2Int _audioSourceCountRange = new Vector2Int(16, 64);

            private int InitialAudioSourceCount => _audioSourceCountRange.x;
            private int MaxAudioSourceCount => _audioSourceCountRange.y;

            /// <summary>
            /// Current index in the pool list.
            /// </summary>
            private int _currentListIndex = -1;

            /// <summary>
            /// Audio mixer group all <see cref="AudioSource"/> belongs to this pool uses.
            /// </summary>
            protected AudioMixerGroup _mixerGroup;

            /// <summary>
            /// Parent transform under which newly created <see cref="AudioSource"/> objects will go to.
            /// </summary>
            protected Transform _poolParentTransform;

            public AudioSourcePool(AudioMixerGroup mixerGroup, Transform poolParentTransform, Vector2Int audioSourceCountRange)
            {
                _mixerGroup = mixerGroup;
                _poolParentTransform = poolParentTransform;
                _audioSourceCountRange = audioSourceCountRange;

                CreateInitialAudioSources(mixerGroup);
            }

            /// <summary>
            /// Creates initial number audio sources and adds them to the pool.
            /// </summary>
            public void CreateInitialAudioSources(AudioMixerGroup mixerGroup)
            {
                _audioSources = new List<AudioSource>(MaxAudioSourceCount);
                for (int i = 0; i < InitialAudioSourceCount; i++)
                {
                    _audioSources.Add(CreateAudioSource());
                }
            }

            protected abstract AudioSource CreateAudioSource();

            /// <summary>
            /// Acquires a <see cref="AudioSource"/> from pool that is not in use.
            /// Tries for maximum of <see cref="MaxAcquireRetryCount"/>.
            /// If no available <see cref="AudioSource"/> found, stops the one at
            /// the end of the list and returns.
            /// </summary>
            /// <returns>Available <see cref="AudioSource"/>.</returns>
            public AudioSource Acquire()
            {
                int attempCount = 0;

                while(true)
                {
                    _currentListIndex = (_currentListIndex + 1) % _audioSources.Count;

                    var audioSource = _audioSources[_currentListIndex];
                    if(audioSource == null)
                    {
                        _audioSources[_currentListIndex] = CreateAudioSource();
                    }
                    else if(audioSource.isPlaying == false)
                    {
                        return audioSource;
                    }
                    else if(_audioSources.Count < MaxAudioSourceCount)
                    {
                        audioSource = CreateAudioSource();
                        _currentListIndex = _audioSources.Count;
                        _audioSources.Add(audioSource);
                        return audioSource;
                    }
                    else if(attempCount < MaxAcquireRetryCount)
                    {
                        attempCount++;
                    }
                    else
                    {
                        audioSource.Stop();
                        return audioSource;
                    }
                }
            }
        }

        private sealed class AudioSourcePool2D : AudioSourcePool
        {
            public AudioSourcePool2D(AudioMixerGroup mixerGroup, Transform poolParentTransform, Vector2Int audioSourceCountRange) 
                : base(mixerGroup, poolParentTransform, audioSourceCountRange)
            { 
            }

            protected override AudioSource CreateAudioSource()
            {
                var audioSource = _poolParentTransform.AddComponent<AudioSource>();

                audioSource.outputAudioMixerGroup = _mixerGroup;
                audioSource.volume = 1f;
                audioSource.playOnAwake = false;
                audioSource.spatialize = false;
                audioSource.spatialBlend = 0f;
                audioSource.minDistance = AudioSourceDefaults.Min3DRange;

                return audioSource;
            }
        }

        private sealed class AudioSourcePool3D : AudioSourcePool
        {
            private const string NewObjectName = "3D Audio Source";

            public AudioSourcePool3D(AudioMixerGroup mixerGroup, Transform poolParentTransform, Vector2Int audioSourceCountRange) 
                : base(mixerGroup, poolParentTransform, audioSourceCountRange)
            {
            }

            protected override AudioSource CreateAudioSource()
            {
                var audioObject = new GameObject(NewObjectName);
                var audioSource = audioObject.AddComponent<AudioSource>();

                audioObject.transform.SetParent(_poolParentTransform);

                audioSource.outputAudioMixerGroup = _mixerGroup;
                audioSource.volume = 1f;
                audioSource.playOnAwake = false;
                audioSource.spatialize = true;
                audioSource.spatialBlend = 1f;
                audioSource.minDistance = AudioSourceDefaults.Min3DRange;

                return audioSource;
            }
        }

        /// <summary>
        /// Container to represent all <see cref="AudioMixerGroup"/> available.
        /// </summary>
        [Serializable]
        private sealed class AudioMixerGroups
        {
            [SerializeField]
            private AudioMixerGroup _masterMixerGroup;

            [SerializeField]
            private AudioMixerGroup _musicMixerGroup;

            [SerializeField]
            private AudioMixerGroup _sfxMixerGroup;

            [SerializeField]
            private AudioMixerGroup _uiMixerGroup;

            public AudioMixerGroup GetMixerGroup(AudioChannel audioChannel) =>
                audioChannel switch
                {
                    AudioChannel.Master => _masterMixerGroup,
                    AudioChannel.Music => _musicMixerGroup,
                    AudioChannel.SFX => _sfxMixerGroup,
                    AudioChannel.UI => _uiMixerGroup,
                    _ => throw new ArgumentOutOfRangeException()
                };
        }
        
        /// <summary>
        /// Handles the connection between <see cref="AudioOptions"/> and 
        /// and <see cref="AudioMixer"/>. Updates the mixer's values when
        /// any value is changed in the options (e.g in settings window).
        /// </summary>
        private sealed class OptionsEventHandler
        {
            public OptionsEventHandler(AudioMixer mainMixer)
            {
                RegisterEvents(mainMixer);
                InitializeMixerValues(mainMixer);
            }

            /// <summary>
            /// Registers to <see cref="AudioOptions"/> event callbacks.
            /// Updates <see cref="AudioMixer"/> values using <see cref="GetDBForVolume(float)"/>.
            /// </summary>
            private void RegisterEvents(AudioMixer mainMixer)
            {
                var audioOptions = AudioOptions.Instance;
                audioOptions.MasterVolume.OnValueChanged += volume =>
                    mainMixer.SetFloat(AudioMixerVolumeParameters.Master, GetDBForVolume(audioOptions.MasterVolume));

                audioOptions.MusicVolume.OnValueChanged += volume =>
                    mainMixer.SetFloat(AudioMixerVolumeParameters.Music, GetDBForVolume(audioOptions.MusicVolume));

                audioOptions.SFXVolume.OnValueChanged += volume =>
                    mainMixer.SetFloat(AudioMixerVolumeParameters.SFX, GetDBForVolume(audioOptions.SFXVolume));

                audioOptions.UIVolume.OnValueChanged += volume =>
                    mainMixer.SetFloat(AudioMixerVolumeParameters.UI, GetDBForVolume(audioOptions.UIVolume));
            }

            /// <summary>
            /// Initialize <see cref="AudioMixer"/> values using the last <see cref="AudioOptions"/>.
            /// (It can be a save file or default settings created in the editor if save is not present).
            /// </summary>
            private void InitializeMixerValues(AudioMixer mainMixer)
            {
                var audioOptions = AudioOptions.Instance;
                mainMixer.SetFloat(AudioMixerVolumeParameters.Master, GetDBForVolume(audioOptions.MasterVolume));
                mainMixer.SetFloat(AudioMixerVolumeParameters.Music, GetDBForVolume(audioOptions.MusicVolume));
                mainMixer.SetFloat(AudioMixerVolumeParameters.SFX, GetDBForVolume(audioOptions.SFXVolume));
                mainMixer.SetFloat(AudioMixerVolumeParameters.UI, GetDBForVolume(audioOptions.UIVolume));
            }
        }

        /// <summary>
        /// Class that allows playing single and loop audio clips on specific channels passed in.
        /// Utilizes <see cref="AudioSourcePool"/> to retrieve correct type of <see cref="AudioSource"/> object.
        /// </summary>
        /// <remarks>
        /// Loops are played with ease in/out to increase natural feel.
        /// <br></br> <b>There is a small hack used to differentiate between loop state of <see cref="AudioSource"/>
        /// , using <see cref="AudioSource.playOnAwake"/> value. 
        /// <br></br><see cref="AudioSource.isPlaying"/> is not used as it is used by <see cref="AudioSourcePool"/>
        /// and <see cref="AudioSource.loop"/> is not used as it is a metric to differentiate between single shot
        /// and looped audio sources.</b>
        /// </remarks>
        private sealed class AudioPlayer
        {
            private const float MinVolumeEaseDuration = 0.25f;
            private const float MaxVolumeEaseDuration = 0.5f;

            private const float DelayEpsilon = 0.01f;

            private AudioSourcePool _audioSourcePool;
            private AudioMixerGroups _audioMixerGroups;
            private Transform _rootTransform;

            private ModuleCoroutineRunner _coroutineRunner;

            public AudioPlayer(AudioSourcePool audioSourcePool, AudioMixerGroups audioMixerGroups, ModuleCoroutineRunner coroutineRunner)
            {
                _audioSourcePool = audioSourcePool;
                _audioMixerGroups = audioMixerGroups;
                _coroutineRunner = coroutineRunner;
                _rootTransform = _coroutineRunner.transform;
            }

            /// <summary>
            /// Plays the <paramref name="clip"/> on an <see cref="AudioSource"/> placed at <paramref name="position"/>.
            /// </summary>
            /// <remarks>
            /// Has option delay parameter to control timing.
            /// </remarks>
            /// <returns><see cref="AudioSource"/> on which the clip is played.</returns>
            public AudioSource PlayClipOneShot(
                AudioResource clip, 
                Vector3 position, 
                float volume = 1f, 
                float delay = 0f, 
                AudioChannel audioChannel = AudioChannel.SFX)
            {
                var audioSource = _audioSourcePool.Acquire();
                audioSource.transform.SetParent(_rootTransform);
                audioSource.transform.position = position;
                return PlayClipOneShot(clip, audioSource, volume, delay, audioChannel);
            }

            /// <summary>
            /// Plays the <paramref name="clip"/> on an <see cref="AudioSource"/> that is placed under the transform <paramref name="parent"/>.
            /// </summary>
            /// <remarks><inheritdoc cref="PlayClipOneShot(AudioResource, Vector3, float, float, AudioChannel)"/></remarks>
            /// <returns><inheritdoc cref="PlayClipOneShot(AudioResource, Vector3, float, float, AudioChannel)"/></returns>
            public AudioSource PlayClipOneShot(
                AudioResource clip,
                Transform parent,
                float volume = 1f,
                float delay = 0f,
                AudioChannel audioChannel = AudioChannel.SFX)
            {
                var audioSource = _audioSourcePool.Acquire();
                audioSource.transform.SetParent(parent);
                audioSource.transform.localPosition = Vector3.zero;
                return PlayClipOneShot(clip, audioSource, volume, delay, audioChannel);
            }

            /// <summary>
            /// Plays the passed <paramref name="clip"/> on an <see cref="AudioSource"/> that is taken from the pool.
            /// </summary>
            /// <remarks><inheritdoc cref="PlayClipOneShot(AudioResource, Vector3, float, float, AudioChannel)"/></remarks>
            /// <returns><inheritdoc cref="PlayClipOneShot(AudioResource, Vector3, float, float, AudioChannel)"/></returns>
            public AudioSource PlayClipOneShot(
                AudioResource clip,
                AudioSource audioSource = null,
                float volume = 1f,
                float delay = 0f,
                AudioChannel audioChannel = AudioChannel.SFX)
            {
                audioSource ??= _audioSourcePool.Acquire();
                SetAudioSourceParameters();
                Play();

                return audioSource;

                void SetAudioSourceParameters()
                {
                    audioSource.resource = clip;
                    audioSource.volume = volume;
                    audioSource.pitch = 1f;
                    audioSource.loop = false;
                    audioSource.minDistance = AudioSourceDefaults.Min3DRange;
                    audioSource.outputAudioMixerGroup = _audioMixerGroups.GetMixerGroup(audioChannel);
                }

                void Play()
                {
                    if(delay < DelayEpsilon)
                    {
                        audioSource.Play();
                    }
                    else
                    {
                        audioSource.PlayDelayed(delay);
                    }
                }
            }

            public AudioSource StartLoop(
                AudioResource clip,
                float volume = 1f,
                float duration = float.PositiveInfinity,
                AudioChannel audioChannel = AudioChannel.SFX)
            {
                var audioSource = _audioSourcePool.Acquire();
                SetLoopedAudioSourceParameters(audioSource, clip, audioChannel);

                _coroutineRunner.StartCoroutine(PlayLoop(audioSource, volume, duration, true));

                return audioSource;
            }

            public AudioSource StartLoop(
                AudioResource clip,
                Transform parent,
                float volume = 1f,
                float duration = float.PositiveInfinity,
                AudioChannel audioChannel = AudioChannel.SFX)
            {
                var audioSource = _audioSourcePool.Acquire();
                audioSource.transform.SetParent(parent);
                audioSource.transform.localPosition = Vector3.zero;

                SetLoopedAudioSourceParameters(audioSource, clip, audioChannel);

                _coroutineRunner.StartCoroutine(PlayLoop(audioSource, volume, duration, false));
                return audioSource;
            }

            private void SetLoopedAudioSourceParameters(
                AudioSource audioSource,
                AudioResource clip,
                AudioChannel audioChannel)
            {
                audioSource.loop = true;
                audioSource.resource = clip;
                audioSource.pitch = 1f;
                audioSource.playOnAwake = true;
                audioSource.outputAudioMixerGroup = _audioMixerGroups.GetMixerGroup(audioChannel);
            }

            /// <summary>
            /// Plays the <paramref name="audioSource"/> for the <paramref name="duration"/>.\
            /// </summary>
            /// <remarks>
            /// Eases in/out volume before loop is started and after loop duration is completed.
            /// </remarks>
            /// <param name="reparentToRoot">Should put <paramref name="audioSource"/> object under root transform.</param>
            /// <returns></returns>
            private IEnumerator PlayLoop(AudioSource audioSource, float volume, float duration, bool reparentToRoot = false)
            {
                audioSource.Play();

                float volumeEaseDuration = Mathf.Clamp(duration * 0.25f, MinVolumeEaseDuration, MaxVolumeEaseDuration);
                yield return audioSource.EaseVolume(0f, volume, volumeEaseDuration);
                yield return LoopAudio(audioSource, duration);
                yield return audioSource.EaseVolume(volume, 0f, volumeEaseDuration);

                audioSource.Stop();

                if (reparentToRoot)
                {
                    audioSource.transform.SetParent(_rootTransform);
                }
            }

            /// <summary>
            /// Loops the audio for the given duration. If audio source is stopped externally
            /// then loop automatically stops as well.
            /// </summary>
            private IEnumerator LoopAudio(AudioSource audioSource, float duration)
            {
                while(audioSource.playOnAwake)
                {
                    if(duration <= 0f)
                    {
                        audioSource.playOnAwake = false;
                    }
                    duration -= Time.deltaTime;

                    yield return null;
                }
            }
        }
    }

    #region Cue/Sequence Player
    public sealed partial class AudioModule
    {
        public AudioSource PlayCueOneShot(
            AudioCue audioCue, 
            float volumeMultiplier = 1f, 
            AudioChannel audioChannel = AudioChannel.SFX)
        {
            return audioCue.IsPlayable
                ? PlayClipOneShot(audioCue.Clip, audioCue.Volume * volumeMultiplier, 0f, audioChannel)
                : null;
        }

        public AudioSource PlayCueOneShot(
            AudioCue audioCue, 
            Vector3 position, 
            float volumeMultiplier = 1f, 
            AudioChannel audioChannel = AudioChannel.SFX)
        {
            return audioCue.IsPlayable
                ? PlayClipOneShot(audioCue.Clip, position, audioCue.Volume * volumeMultiplier, 0f, audioChannel)
                : null;
        }

        public AudioSource PlayCueOneShot(
            AudioCue audioCue, 
            Transform parent, 
            float volumeMultiplier = 1f, 
            AudioChannel audioChannel = AudioChannel.SFX)
        {
            return audioCue.IsPlayable
                ? PlayClipOneShot(audioCue.Clip, parent, audioCue.Volume * volumeMultiplier, 0f, audioChannel)
                : null;
        }

        /// <summary>
        /// Plays the <paramref name="sequence"/> on an <see cref="AudioSource"/>.
        /// </summary>
        /// <returns><see cref="AudioSource"/> on which the sequence is played.</returns>
        public AudioSource PlaySequenceOneShot(
            AudioSequence sequence, 
            float speed = 1f, 
            AudioChannel audioChannel = AudioChannel.SFX)
        {
            if(sequence.IsPlayable == false)
            {
                return null;
            }

            var audioSource = _audioSourcePool2D.Acquire();
            audioSource.outputAudioMixerGroup = _audioMixerGroups.GetMixerGroup(audioChannel);
            sequence.PlaySequenceOn(audioSource, _coroutineRunner, speed);
            return audioSource;
        }

        /// <summary>
        /// Plays the <paramref name="sequence"/> on an <see cref="AudioSource"/> at <paramref name="position"/>. 
        /// </summary>
        /// <returns><inheritdoc cref="PlaySequenceOneShot(AudioSequence, float, AudioChannel)"/></returns>
        public AudioSource PlaySequenceOneShot(
            AudioSequence sequence,
            Vector3 position,
            float speed = 1f,
            AudioChannel audioChannel = AudioChannel.SFX)
        {
            if (sequence.IsPlayable == false)
            {
                return null;
            }

            var audioSource = _audioSourcePool2D.Acquire();
            audioSource.transform.position = position;
            audioSource.transform.SetParent(_audioSourcesRoot);
            audioSource.outputAudioMixerGroup = _audioMixerGroups.GetMixerGroup(audioChannel);
            sequence.PlaySequenceOn(audioSource, _coroutineRunner, speed);
            return audioSource;
        }

        /// <summary>
        /// Plays the <paramref name="sequence"/> on an <see cref="AudioSource"/> that is attached to <paramref name="parent"/>.
        /// </summary>
        /// <returns><inheritdoc cref="PlaySequenceOneShot(AudioSequence, float, AudioChannel)"/></returns>
        public AudioSource PlaySequenceOneShot(
            AudioSequence sequence,
            Transform parent,
            float speed = 1f,
            AudioChannel audioChannel = AudioChannel.SFX)
        {
            if (sequence.IsPlayable == false)
            {
                return null;
            }

            var audioSource = _audioSourcePool2D.Acquire();
            audioSource.transform.SetParent(parent);
            audioSource.transform.localPosition = Vector3.zero;
            audioSource.outputAudioMixerGroup = _audioMixerGroups.GetMixerGroup(audioChannel);
            sequence.PlaySequenceOn(audioSource, _coroutineRunner, speed);
            return audioSource;
        }
    }
    #endregion
}
