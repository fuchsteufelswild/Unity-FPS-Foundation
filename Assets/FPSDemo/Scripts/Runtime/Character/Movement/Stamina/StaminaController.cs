using Nexora.Audio;
using Nexora.SaveSystem;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Class that controls character's stamina, increase/decrease when changing states 
    /// and the regeneration aspect of it.
    /// </summary>
    [RequireComponent(typeof(IMovementController))]
    public sealed class StaminaController : 
        CharacterBehaviour, 
        IStaminaController,
        ISaveableComponent
    {
        [Tooltip("Initial stamina, used when it is initialized.")]
#if UNITY_EDITOR
        [DynamicRange(nameof(GetMinStamina), nameof(GetMaxStamina))]
#endif
        [SerializeField]
        private float _stamina = 1f;

        [Tooltip("Initial max stamina value, it can be changed in runtime to be higher or less.")]
#if UNITY_EDITOR
        [OnValueChanged(nameof(ValidateStamina))]
#endif
        [SerializeField, Range(1f, 1000f)]
        private float _maxStamina = 5f;

        [Title("Settings")]
        [Tooltip("Amount of time in seconds stamina regeneration stops when it is lowered.")]
        [SerializeField, Range(0f, 5f)]
        private float _regenerationPauseDuration;

        [Tooltip("How long character should breathe heavily when its stamina gets below a certain level.")]
        [SerializeField, Range(-0.5f, 30f)]
        private float _breathingHeavyDuration = 6f;

        [ShowIf(nameof(_breathingHeavyDuration), 0f, Comparison = UnityComparisonMethod.Greater)]
        [SerializeField]
        private AudioCue _breathingHeavyAudio = new(null);

        [Tooltip("Profiles that are used to denote information about the stamina settings of " +
            "the movement states. If there is no profile for a movement state, then default will be used.")]
        [ReorderableList(ListStyle.Lined), LabelByChild("StateType")]
        [SerializeField]
        private StaminaProfile[] _staminaProfiles = Array.Empty<StaminaProfile>();

        private static readonly StaminaProfile DefaultProfile = new(MovementStateType.Idle, 0f, 0f, 2f);
        private static readonly StaminaThresholds Thresholds = new(null, null, null);

        private IHealthController _healthController;
        private IMovementController _movementController;

        private IStaminaAudioHandler _audioHandler;
        private IStaminaMovementHandler _movementHandler;
        private IStaminaRegenerator _staminaRegenerator;

        private StaminaProfile _currentStaminaProfile;

        public event UnityAction<float> StaminaChanged;

        public float CurrentStamina
        {
            get => _stamina;
            private set
            {
                float clampedStamina = Mathf.Clamp(value, 0, _maxStamina);
                // No change
                if(Mathf.Approximately(_stamina, clampedStamina))
                {
                    return;
                }

                bool wasDecreased = clampedStamina < _stamina;
                _stamina = clampedStamina;

                if(wasDecreased)
                {
                    _staminaRegenerator?.PauseRegeneration(_regenerationPauseDuration);
                }

                StaminaChanged?.Invoke(_stamina);
                _audioHandler?.HandleHeavyBreathing(_stamina, Thresholds.HeavyBreathingThreshold);
                _movementHandler?.HandleMovementBlocking(_stamina, in Thresholds);
            }
        }

        public float MaxStamina
        {
            get => _maxStamina;
            private set => _maxStamina = value;
        }

        public void ModifyStamina(float amount) => CurrentStamina += amount;
        public void SetMaxStamina(float maxStamina) => MaxStamina = maxStamina;
        public void RestoreToMax() => CurrentStamina = MaxStamina;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _audioHandler = new StaminaAudioHandler(parent, _breathingHeavyDuration, _breathingHeavyAudio);
            _staminaRegenerator = new StaminaRegenerator();

            _healthController = parent.HealthController;
            _healthController.Respawned += OnRespawn;
            _healthController.Death += OnDeath;

            RestoreToMax();
            enabled = _healthController.IsAlive();
        }

        private void EnsureMovementHandlerInitialized() => _movementHandler ??= new StaminaMovementHandler(_movementController, this);

        protected override void OnBehaviourDestroy(ICharacter parent)
        {
            _healthController.Respawned -= OnRespawn;
            _healthController.Death -= OnDeath;
        }

        private void OnRespawn()
        {
            RestoreToMax();
            enabled = true;
        }

        private void OnDeath(in DamageContext context)
        {

        }

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            _movementController = parent.GetCC<IMovementController>();
            _movementController.AddStateTransitionListener(MovementStateType.None, OnMovementStateChanged);
            _currentStaminaProfile = GetProfileOfType(_movementController.ActiveStateType);
        }

        protected override void OnBehaviourDisable(ICharacter parent) => _movementController.RemoveStateTransitionListener(MovementStateType.None, OnMovementStateChanged);

        private void OnMovementStateChanged(MovementStateType stateType)
        {
            EnsureMovementHandlerInitialized();

            ModifyStamina(_currentStaminaProfile.ExitChange);
            _currentStaminaProfile = GetProfileOfType(_movementController.ActiveStateType);
            ModifyStamina(_currentStaminaProfile.EnterChange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StaminaProfile GetProfileOfType(MovementStateType stateType)
        {
            foreach(var profile in _staminaProfiles)
            {
                if(profile.StateType == stateType)
                {
                    return profile;
                }
            }

            return DefaultProfile;
        }

        private void Update() => UpdateStamina();

        private void UpdateStamina()
        {
            if (_currentStaminaProfile.ChangeRatePerSec < 0f)
            {
                CurrentStamina += _currentStaminaProfile.ChangeRatePerSec * Time.deltaTime;
            }
            else if (_staminaRegenerator?.CanRegenerate == true)
            {
                CurrentStamina += _currentStaminaProfile.ChangeRatePerSec * Time.deltaTime;
            }
        }

        #region Saving
        private sealed class SaveData
        {
            public float Stamina;
            public float MaxStamina;
        }

        public void Load(object data)
        {
            SaveData save = (SaveData)data;
            _maxStamina = save.MaxStamina;
            _stamina = save.Stamina;
        }

        public object Save()
        {
            return new SaveData 
            { 
                MaxStamina = _maxStamina, 
                Stamina = _stamina 
            };
        }
        #endregion

#if UNITY_EDITOR
        protected void ValidateStamina() => CurrentStamina = _stamina;
        protected float GetMaxStamina() => _maxStamina;
        protected float GetMinStamina() => 0f;
#endif

        /// <summary>
        /// Denotes stamina profile for a specific <see cref="MovementStateType"/>.
        /// Has values for entering/exiting state and the usage of stamina in that state.
        /// </summary>
        [Serializable]
        private sealed class StaminaProfile
        {
            /// <summary>
            /// Which <see cref="MovementStateType"/> this state is for.
            /// </summary>
            public MovementStateType StateType;

            /// <summary>
            /// What is the change of stamina when entered this state.
            /// </summary>
            [Range(-2f, 2f)]
            public float EnterChange;

            /// <summary>
            /// What is the change of stamina when exiting this state.
            /// </summary>
            [Range(-2f, 2f)]
            public float ExitChange;

            /// <summary>
            /// How much stamina is used/regenerated per sec when in this state.
            /// </summary>
            [Range(-2f, 2f)]
            public float ChangeRatePerSec;

            public StaminaProfile(MovementStateType stateType, float enterChange, float exitChange, float changeRatePerSec)
            {
                StateType = stateType;
                EnterChange = enterChange;
                ExitChange = exitChange;
                ChangeRatePerSec = changeRatePerSec;
            }
        }
    }
}