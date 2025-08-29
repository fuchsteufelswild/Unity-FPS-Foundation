using Nexora.Audio;
using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.ProceduralMotion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    public interface IBodyLeanController : ICharacterBehaviour
    {
        /// <summary>
        /// Current leaning of the character.
        /// </summary>
        LeanState CurrentLeanState { get; }

        /// <summary>
        /// Current max leaning of the character [0, 1].
        /// </summary>
        float CurrentMaxLeanPercent { get; }

        /// <summary>
        /// Leans to <paramref name="leanState"/>.
        /// </summary>
        void SetLeanState(LeanState leanState);

        /// <returns>Can lean to <paramref name="leanState"/>.</returns>
        bool CanLeanTo(LeanState leanState);
    }

    public sealed class BodyLeanController : 
        CharacterBehaviour,
        IBodyLeanController
    {
        [Tooltip("Lean motion for the body.")]
        [SerializeField, NotNull]
        private LeanMotion _bodyLeanMotion;

        [Tooltip("Lean motion for the handheld items.")]
        [SerializeField, NotNull]
        private LeanMotion _handheldLeanMotion;

        [SerializeField]
        private LeanTimingController _leanTimingController;

        [Title("Obstruction Detection")]
        [Tooltip("Obstruction mask of what blocks leaning.")]
        [SerializeField]
        private LayerMask _obstructionMask;

        [Tooltip("Extra distance in addition to lean offset when checking for obstructions.")]
        [SerializeField]
        private float _obstructionPadding = 0.2f;

        [Tooltip("Maximum distance that an obstruction close to us, so that we can lean.")]
        [SerializeField]
        private float _maxLeanObstructionCutoff = 0.4f;

        [Title("Character Settings")]
        [Tooltip("Maximum speed allowed to lean to sides, above this value you cannot lean.")]
        [SerializeField]
        private float _maxAllowedCharacterSpeed = 4f;

        [SerializeField]
        private LeanAudioController _leanAudioController;

        private ILeanValidator _leanValidator;
        private ICharacterMotor _characterMotor;
        private LeanMotionGroup _leanMotionGroup;

        private LeanState _currentLeanState;
        private float _currentMaxLeanPercent;
        private bool _isInitialized;

        public LeanState CurrentLeanState => _currentLeanState;
        public float CurrentMaxLeanPercent => _currentMaxLeanPercent;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _characterMotor = parent.GetCC<ICharacterMotor>();
            _leanAudioController.Initialize(parent);
            InitializeValidator();
            InitializeMotionGroup();
            _isInitialized = true;
        }

        private void InitializeValidator()
        {
            var speedValidator = new SpeedBasedLeanValidator(_maxAllowedCharacterSpeed);
            var obstructionValidator = new ObstructionBasedLeanValidator(_obstructionMask, _obstructionPadding, _maxLeanObstructionCutoff, Parent.transform);
            
            _leanValidator = new CompositeLeanValidator(speedValidator,  obstructionValidator);
        }

        private void InitializeMotionGroup()
        {
            var controllers = new List<ILeanMotionController>();

            if(_bodyLeanMotion != null)
            {
                controllers.Add(new LeanMotionController(_bodyLeanMotion));
            }

            if(_handheldLeanMotion != null)
            {
                controllers.Add(new LeanMotionController(_handheldLeanMotion));
            }

            _leanMotionGroup = new LeanMotionGroup(controllers.ToArray());
        }

        private void SetMaxLeanPercent(float percent)
        {
            _currentMaxLeanPercent = percent;
            _leanMotionGroup.SetMaxLeanPercent(_currentMaxLeanPercent);
        }

        private void Update()
        {
            if(_isInitialized == false || _currentLeanState == LeanState.Center)
            {
                return;
            }

            ValidateCurrentLeanState();
        }

        private void ValidateCurrentLeanState()
        {
            if(CanLeanTo(_currentLeanState) == false)
            {
                ApplyLeanState(LeanState.Center);
            }
        }

        public bool CanLeanTo(LeanState leanState)
        {
            if(_leanValidator == null)
            {
                return false;
            }

            LeanValidationResult result = _leanValidator.ValidateLean(leanState, _characterMotor, _bodyLeanMotion);

            if(result.IsObstructed)
            {
                SetMaxLeanPercent(result.MaxLeanPercent);
            }
            else
            {
                SetMaxLeanPercent(1f);
            }

            return result.CanLean;
        }

        private void ApplyLeanState(LeanState leanState)
        {
            if(leanState == _currentLeanState)
            {
                return;
            }

            _currentLeanState = leanState;

            _leanAudioController?.PlayLeanAudio(_currentMaxLeanPercent);

            if(leanState == LeanState.Center)
            {
                SetMaxLeanPercent(1f);
            }

            _leanMotionGroup?.SetLeanState(leanState);
        }

        public void SetLeanState(LeanState leanState)
        {
            if(CanSetLeanState(leanState) == false)
            {
                return;
            }

            if(CanLeanTo(leanState))
            {
                ApplyLeanState(leanState);
                _leanTimingController?.RegisterLeanStateChange();
            }
        }

        private bool CanSetLeanState(LeanState leanState)
        {
            return leanState != _currentLeanState
                && (_leanTimingController?.CanChangeLeanState() ?? true);
        }

        [Serializable]
        private sealed class LeanTimingController
        {
            [Tooltip("Time to wait between consecutive leans.")]
            [SerializeField, Range(0f, 3f)]
            private float _leanCooldown = 0.2f;

            private float _nextAllowedTime;

            public bool CanChangeLeanState() => Time.time >= _nextAllowedTime;

            public void RegisterLeanStateChange() => _nextAllowedTime = Time.time + _leanCooldown;
        }

        [Serializable]
        private sealed class LeanAudioController
        {
            private const float DefaultAudioCooldown = 0.35f;
            private const float minAudioIntensity = 0.3f;

            [SerializeField]
            private AudioCue _leanAudio = new(null);

            private float _audioCooldown;
            private ICharacter _character;

            private float _nextAudioTime;

            public void Initialize(ICharacter character, float audioCooldown = DefaultAudioCooldown)
            {
                _character = character;
                _audioCooldown = audioCooldown;
            }

            public void PlayLeanAudio(float intensity)
            {
                if(CanPlayAudio() == false || _leanAudio.Clip == null)
                {
                    return;
                }

                _nextAudioTime = Time.time + _audioCooldown;

                float volume = Mathf.Max(intensity, minAudioIntensity);

                _character.AudioPlayer.PlayClip(_leanAudio, BodyPart.Chest, volume);
            }

            public bool CanPlayAudio() => Time.time >= _nextAudioTime;
        }
    }
}