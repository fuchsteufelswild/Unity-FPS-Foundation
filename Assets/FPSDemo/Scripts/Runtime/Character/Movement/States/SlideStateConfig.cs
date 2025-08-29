using Nexora.Audio;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    [CreateAssetMenu(menuName = "Nexora/Movement/States/" + nameof(SlideStateConfig), fileName = "SlideConfig_")]
    public sealed class SlideStateConfig : ScriptableObject
    {
        [Title("Slide Speed Settings")]
        [Tooltip("Slide speed curve over time.")]
        [SerializeField]
        private AnimationCurve _speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Tooltip("Initial speed multiplier based on incoming velocity.")]
        [SerializeField]
        private AnimationCurve _enterSpeedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Tooltip("Initial slide boost.")]
        [SerializeField, Range(0f, 50f)]
        private float _impulse = 12f;

        [Tooltip("Minimum required speed to start sliding.")]
        [SerializeField, Range(0f, 30f)]
        private float _minRequiredSpeed = 5f;

        [Tooltip("Speed, under which sliding stops.")]
        [SerializeField, Range(0.1f, 15f)]
        private float _stopSpeed = 1.5f;

        [Title("Slide Control Settings")]
        [Tooltip("Character height while sliding.")]
        [SerializeField, Range(0f, 3f)]
        private float _height = 1f;

        [Tooltip("How fast the speed changes while sliding.")]
        [SerializeField, Range(0f, 50f)]
        private float _acceleration = 8f;

        [Tooltip("How much player input affects direction while sliding.")]
        [SerializeField, Range(0f, 10f)]
        private float _inputInfluence = 2f;

        [Tooltip("Slope speed multiplier.")]
        [SerializeField, Range(0f, 50f)]
        private float _slopeFactor = 3f;

        [Title("Slide Audio Settings")]
        [Tooltip("Looping slide sound.")]
        [SerializeField]
        private AudioCue _slideAudio;

        [Tooltip("Audio fade duration.")]
        [SerializeField, Range(0f, 3f)]
        private float _slideAudioDuration = 0.4f;

        /// <summary>
        /// Slide speed curve over time.
        /// </summary>
        public AnimationCurve SpeedCurve => _speedCurve;

        /// <summary>
        /// Initial slide boost.
        /// </summary>
        public float Impulse => _impulse;

        /// <summary>
        /// Initial speed multiplier based on incoming velocity.
        /// </summary>
        public AnimationCurve EnterSpeedCurve => _enterSpeedCurve;

        /// <summary>
        /// Minimum required speed to start sliding.
        /// </summary>
        public float MinRequiredSpeed => _minRequiredSpeed;

        /// <summary>
        /// Speed, under which sliding stops.
        /// </summary>
        public float StopSpeed => _stopSpeed;

        /// <summary>
        /// Character height while sliding.
        /// </summary>
        public float Height => _height;

        /// <summary>
        /// How fast the speed changes when sliding.
        /// </summary>
        public float Acceleration => _acceleration;

        /// <summary>
        /// How much player input affects direction while sliding.
        /// </summary>
        public float Inputnfluence => _inputInfluence;

        /// <summary>
        /// Slope speed multiplier.
        /// </summary>
        public float SlopeFactor => _slopeFactor;

        /// <summary>
        /// Looping slide sound.
        /// </summary>
        public AudioCue SlideAudio => _slideAudio;

        /// <summary>
        /// Audio fade duration.
        /// </summary>
        public float SlideAudioDuration => _slideAudioDuration;
    }
}