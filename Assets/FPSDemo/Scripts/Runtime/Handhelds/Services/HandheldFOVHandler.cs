using Nexora.FPSDemo.CharacterBehaviours;
using System.Diagnostics;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Handles FOV change requests from <see cref="IHandheld"/> components, and delegates
    /// them to the <see cref="IFOVController"/> behaviour of the character.
    /// </summary>
    public interface IHandheldFOVHandler
    {
        /// <summary>
        /// Ease duration during changes on the values.
        /// </summary>
        public const float DefaultEaseAnimationDuration = 0.5f;

        /// <summary>
        /// Current FOV of the camera.
        /// </summary>
        public float CameraFOV { get; }

        /// <summary>
        /// Current FOV of the view model (e.g hands).
        /// </summary>
        public float ViewModelFOV { get; }

        /// <summary>
        /// Current size of the view model (e.g hands), (This is a uniform scale -> Vector3.one * scale).
        /// </summary>
        public float ViewModelSize { get; }

        /// <summary>
        /// Scales the default camera FOV to be <paramref name="fovScale"/>.
        /// </summary>
        /// <param name="fovScale">Scaler [0-1].</param>
        public void SetCameraFOV(float fovScale) => SetCameraFOV(fovScale, DefaultEaseAnimationDuration);

        /// <summary>
        /// Scales the default camera FOV to be <paramref name="fovScale"/> in <paramref name="duration"/> seconds
        /// with an optional <paramref name="delay"/> seconds.
        /// </summary>
        /// <param name="fovScale">Scaler [0-1].</param>
        /// <param name="duration">Duration of the set animation.</param>
        /// <param name="delay">Delay before starting the animation.</param>
        public void SetCameraFOV(float fovScale, float duration, float delay = 0f);

        /// <summary>
        /// Scales the view model's FOV to be <paramref name="fovScale"/>.
        /// </summary>
        /// <param name="fovScale">Scaler [0-1].</param>
        public void SetViewModelFOV(float fovScale) => SetViewModelFOV(fovScale, DefaultEaseAnimationDuration);


        /// <summary>
        /// Scales the view model's FOV to be <paramref name="fovScale"/> in <paramref name="duration"/> seconds
        /// with an optional <paramref name="delay"/> seconds.
        /// </summary>
        /// <param name="fovScale">Scaler [0-1].</param>
        /// <param name="duration">Duration of the set animation.</param>
        /// <param name="delay">Delay before starting the animation.</param>
        public void SetViewModelFOV(float fovScale, float duration, float delay = 0f);

        /// <summary>
        /// Sets the view model size to be <paramref name="size"/>.
        /// </summary>
        /// <param name="size">New view model size.</param>
        public void SetViewModelSize(float size);
    }

    /// <summary>
    /// <inheritdoc cref="IHandheldFOVHandler" path="/summary"/>
    /// </summary>
    /// <remarks>
    /// Has fields for configuring base values for the FOV's which might be required be unique for each item.
    /// </remarks>
    public sealed class HandheldFOVHandler : 
        MonoBehaviour,
        IHandheldFOVHandler
    {
        [Tooltip("Multiplier for the camera's FOV [0-1].")]
        [SerializeField, Range(0.1f, 1f), Delayed]
        private float _defaultCameraFOVScale = 1f;

        [Tooltip("Multiplier for the view model's size.")]
        [SerializeField, Range (0.1f, 1f), Delayed]
        private float _defaultViewModelSizeScale = 1f;

        [Tooltip("Value in degrees for the default FOV of the view model (this might unique for every handheld unlike camera FOV).")]
        [SerializeField, Range(10f, 120f), Delayed]
        private float _defaultViewModelFOV = 55f;

        private IFOVController _characterFOVController;

        public float CameraFOV => _characterFOVController.CurrentFOVState.CameraFOV;
        public float ViewModelFOV => _characterFOVController.CurrentFOVState.ViewModelFOV;
        public float ViewModelSize => _characterFOVController.CurrentFOVState.ViewModelSize;

        private void Awake()
        {
            _characterFOVController = GetComponentInParent<IHandheld>().Character.GetCC<IFOVController>();
        }

        private void OnEnable()
        {
            if(_characterFOVController != null)
            {
                SetViewModelFOV(1f, 0f);
                SetViewModelSize(1f);
                (this as IHandheldFOVHandler).SetCameraFOV(1f);
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            if(_characterFOVController == null)
            {
                return;
            }

            (this as IHandheldFOVHandler).SetViewModelFOV(1f);
        }

        public void SetCameraFOV(float fovScale, float duration, float delay = 0f)
            => _characterFOVController.SetCameraFOVBaseMultiplier(_defaultCameraFOVScale * fovScale, duration, delay);

        public void SetViewModelFOV(float fovScale, float duration, float delay = 0f)
            => _characterFOVController.SetViewModelFOV(_defaultViewModelFOV * fovScale, duration, delay);

        public void SetViewModelSize(float size) => _characterFOVController.SetViewModelSize(size * _defaultViewModelSizeScale);
    }
}