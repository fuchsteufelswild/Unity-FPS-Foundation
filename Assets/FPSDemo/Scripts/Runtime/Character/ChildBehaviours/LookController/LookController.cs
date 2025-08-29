using Nexora.FPSDemo.Movement;
using Nexora.SaveSystem;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    [OptionalCharacterBehaviour(typeof(ICharacterMotor))]
    public class LookController : 
        CharacterBehaviour, 
        ILookController,
        ISaveableComponent
    {
        [Tooltip("Camera for fov based sensitivity compensation (optional).")]
        [SerializeField]
        private Camera _camera;

        [Tooltip("Transform, which is affected by the pitch rotation (up/down).")]
        [SerializeField, NotNull]
        private Transform _pitchTransform;

        [Tooltip("Transform, which is affected by the yaw rotation (left/right).")]
        [SerializeField, NotNull]
        private Transform _yawTransform;

        [Tooltip("Smoothing steps used for smoothing the input.")]
        [SerializeField, Range(1, 30)]
        private int _smoothingStepCount = 10;

        [Tooltip("Limit imposed on pitch angle.")]
        [SerializeField]
        private Vector2 _pitchLimit = new Vector2(-60f, 90f);

        private ILookInputProvider _primaryInputProvider;
        private ILookInputProvider _additiveInputProvider;

        private IInputSmoother _inputSmoother;
        private ISensitivityCalculator _sensitivityCalculator;
        private AdditiveViewRotationHandler _rotationHandler;

        private Vector2 _currentLookInput;
        private bool _isInitialized;

        private Vector2 _savedViewAngles;

        public Vector2 ViewAngles => _rotationHandler?.ViewAngles ?? Vector2.zero;
        public Vector2 LookInput => _currentLookInput;
        public Vector2 LookDelta => _rotationHandler?.LastDelta ?? Vector2.zero;

        public bool IsActive { get; private set; }

        private void Awake() => enabled = false;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            InitializeDependencies();
            _isInitialized = true;
        }

        private void InitializeDependencies()
        {
            _inputSmoother = new ExponentialInputSmoother(_smoothingStepCount);
            _sensitivityCalculator = new FOVCompensatedSensitivityCalculator(_camera);
            _rotationHandler = new AdditiveViewRotationHandler(_pitchTransform, _yawTransform, _pitchLimit);
            _rotationHandler.AddToViewAngles(_savedViewAngles);
        }

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            if (parent.TryGetCC<ICharacterMotor>(out var characterMotor))
            {
                characterMotor.Teleported += HandleCharacterTeleport;
            }
        }

        protected override void OnBehaviourDisable(ICharacter parent)
        {
            if(parent.TryGetCC<ICharacterMotor>(out var characterMotor))
            {
                characterMotor.Teleported -= HandleCharacterTeleport;
            }
        }

        private void HandleCharacterTeleport()
        {
            _rotationHandler?.ResetAngles();
            _inputSmoother?.Reset();
        }

        public void SetAdditiveInputProvider(ILookInputProvider inputProvider)
        {
            if(_additiveInputProvider != null)
            {
                // _rotationHandler?.AddToViewAngles(_additiveInputProvider.GetLookInput());
                _rotationHandler?.AddToViewAngles(_rotationHandler?.AdditiveInput ?? Vector2.zero);
            }    

            _additiveInputProvider = inputProvider;
        }
        
        public void SetPrimaryInputProvider(ILookInputProvider inputProvider)
        {
            _primaryInputProvider = inputProvider;
            SetActive(inputProvider != null);
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            enabled = active;
        }

        private void LateUpdate()
        {
            if(_isInitialized == false || IsActive == false || _primaryInputProvider == null)
            {
                return;
            }

            ProcessLookInput();
        }

        /// <summary>
        /// Takes a new input, smoothes it using previous ones, combines it with sensitivity to get final value.
        /// Then adds additive input if had and applies rotation.
        /// </summary>
        private void ProcessLookInput()
        {
            float deltaTime = Time.deltaTime;

            Vector2 rawInput = _primaryInputProvider.GetLookInput();
            Vector2 smoothedInput = _inputSmoother.SmoothInput(rawInput);
            float sensitivity = _sensitivityCalculator.CalculateSensitivity(deltaTime);

            _currentLookInput = smoothedInput * sensitivity;

            Vector2 additiveInput = _additiveInputProvider?.GetLookInput() ?? Vector2.zero;
            _rotationHandler.SetAdditiveInput(additiveInput);

            _rotationHandler.ApplyRotation(_currentLookInput);
        }

        #region Saving
        void ISaveableComponent.Load(object data) => _savedViewAngles = (Vector2)data;
        object ISaveableComponent.Save() => _rotationHandler.ViewAngles;
        #endregion
    }
}