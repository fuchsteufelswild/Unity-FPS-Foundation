using Nexora.FPSDemo.Movement;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    /// <summary>
    /// Config used for denoting animation configuration when start/restart FOV animations on Camera or ViewModel.
    /// </summary>
    public readonly struct FOVAnimationConfig
    {
        public readonly float Duration;
        public readonly float Delay;
        public readonly Ease EaseType;

        public FOVAnimationConfig(float duration, float delay, Ease easeType)
        {
            Duration = duration;
            Delay = delay;
            EaseType = easeType;
        }

        public static readonly FOVAnimationConfig Default = new(0.3f, 0f, Ease.QuadInOut);
        public static readonly FOVAnimationConfig Instant = new(0.01f, 0f, Ease.QuadInOut);
    }

    /// <summary>
    /// Denotes a snapshot of the full FOV state.
    /// </summary>
    public readonly struct FOVState
    {
        public readonly float CameraFOV;
        public readonly float ViewModelFOV;
        public readonly float ViewModelSize;
        public readonly float CameraMultiplier;

        public FOVState(float cameraFOV, float viewModelFOV, float viewModelSize, float cameraMultiplier)
        {
            CameraFOV = cameraFOV;
            ViewModelFOV = viewModelFOV;
            ViewModelSize = viewModelSize;
            CameraMultiplier = cameraMultiplier;
        }
    }

    public interface IFOVController : ICharacterBehaviour
    {
        /// <summary>
        /// Current state of the controller, keeps the information about the 
        /// current state of the Camera and View Model.
        /// </summary>
        FOVState CurrentFOVState { get; }

        /// <summary>
        /// Sets the base multiplier of camera FOV to <paramref name="multiplier"/> and starts
        /// an animation using the provided values.
        /// </summary>
        /// <param name="multiplier">New base multiplier.</param>
        /// <param name="duration">Duration of the lerp animation.</param>
        /// <param name="delay">Delay before starting to animate.</param>
        void SetCameraFOVBaseMultiplier(float multiplier, float duration = 0.3f, float delay = 0f);

        /// <summary>
        /// Sets the FOV of View Model to <paramref name="fov"/> and starts
        /// an animation using the provided values.
        /// </summary>
        /// <param name="fov">New fov value.</param>
        /// <param name="duration">Duration of the lerp animation.</param>
        /// <param name="delay">Delay before starting to animate.</param>
        void SetViewModelFOV(float fov, float duration = 0.3f, float delay = 0f);

        /// <summary>
        /// Sets the View Model's uniform size to <paramref name="size"/>.
        /// </summary>
        /// <param name="size">New uniform size.</param>
        void SetViewModelSize(float size);
    }


    /// <summary>
    /// Controls both Camera and View Model's FOV values. Using smooth animations for a better
    /// experience.
    /// </summary>
    /// <remarks>
    /// Camera FOV is the FOV in the Graphics Settings, but view model FOV is unique to every weapon
    /// and can be changed (the reason is models sometimes need tweaking to match the needed look).
    /// </remarks>
    [RequireCharacterBehaviour(typeof(ICharacterMotor))]
    public sealed class FOVController : 
        CharacterBehaviour, 
        IFOVController
    {
        [Title("View Model Settings")]
        [Tooltip("Denotes the FOV of the view model (e.g hands).")]
        [SerializeField, Range(20f, 90f)]
        private float _baseViewModelFOV = 60f;

        [Tooltip("Ease that will be used for animating the View Model's FOV setting.")]
        [SerializeField]
        private Ease _viewModelEaseType = Ease.QuadInOut;

        [Tooltip("Shader property name for enabled state of the view model FOV.")]
        [SerializeField]
        private string _viewModelFOVEnabledProperty = "_ViewModelFOVEnabled";

        [Tooltip("Shader property name for FOV value of the view model.")]
        [SerializeField]
        private string _viewModelFOVProperty = "_ViewModelFOV";

        [Title("Camera Settings")]
        [SerializeField, NotNull]
        private Camera _camera;

        [Tooltip("Ease that will be used for animating the Camera's FOV setting.")]
        [SerializeField]
        private Ease _cameraEaseType = Ease.QuadInOut;

        [Title("Dynamic FOV Modifiers")]
        [SerializeField]
        private float _airborneFOVMultiplier = 1.1f;

        [Tooltip("Curve used for determining dynamic FOV multiplier caused by the speed of the character.")]
        [SerializeField]
        private AnimationCurve _speedFOVCurve = new(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

        [Tooltip("Curve used for determining dynamic FOV multiplier caused by the height of the character.")]
        [SerializeField]
        private AnimationCurve _heightFOVCurve = new(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

        private ICameraFOVController _cameraFOVController;
        private IViewModelController _viewModelController;
        private IFOVMultiplierCalculator _fovMultiplierCalculator;
        private ICharacterMotor _characterMotor;
        
        public FOVState CurrentFOVState => new FOVState(
            _cameraFOVController?.CurrentFOV ?? 0f,
            _viewModelController?.CurrentFOV ?? 0f,
            _viewModelController?.CurrentSize ?? 1f,
            _cameraFOVController?.BaseMultiplier ?? 1f);


        private void Awake()
        {
            InitializeControllers();
        }

        protected override void OnBehaviourStart(ICharacter parent) => _characterMotor = parent.GetCC<ICharacterMotor>();

        private void InitializeControllers()
        {
            _cameraFOVController = new CameraFOVController(_camera, _cameraEaseType);
            
            var shaderConfig = new ViewModelShaderConfig(_viewModelFOVEnabledProperty, _viewModelFOVProperty);
            _viewModelController = new ViewModelController(transform, in shaderConfig, _baseViewModelFOV, _viewModelEaseType);

            _fovMultiplierCalculator = new CompositeFOVMultiplierCalculator(
                new SpeedBasedFOVCalculator(_speedFOVCurve),
                new HeightBasedFOVCalculator(_heightFOVCurve),
                new AirborneFOVCalculator(_airborneFOVMultiplier));
        }

        protected override void OnBehaviourDestroy(ICharacter parent)
        {
            (_cameraFOVController as IDisposable)?.Dispose();
            (_viewModelController as IDisposable)?.Dispose();
        }

        public void SetCameraFOVBaseMultiplier(float multiplier, float duration = 0.3f, float delay = 0f)
        {
            var config = new FOVAnimationConfig(duration, delay, _cameraEaseType);
            _cameraFOVController?.SetBaseMultiplier(multiplier, in config);
        }

        public void SetViewModelFOV(float fov, float duration = 0.3f, float delay = 0f)
        {
            var config = new FOVAnimationConfig(duration, delay, _viewModelEaseType);
            _viewModelController?.SetFOV(fov, in config);
        }

        public void SetViewModelSize(float size) => _viewModelController?.SetSize(size);

        public void Update()
        {
            UpdateDynamicFOV();
            UpdateControllers();
        }

        private void UpdateDynamicFOV()
        {
            if(_characterMotor == null || _fovMultiplierCalculator == null)
            {
                return;
            }

            float dynamicMultiplier = _fovMultiplierCalculator.CalculateMultiplier(_characterMotor);
            _cameraFOVController?.SetDynamicMultiplier(dynamicMultiplier);
        }

        private void UpdateControllers()
        {
            _cameraFOVController?.UpdateFOV();
            _viewModelController?.UpdateShaderProperties();
        }
    }
}