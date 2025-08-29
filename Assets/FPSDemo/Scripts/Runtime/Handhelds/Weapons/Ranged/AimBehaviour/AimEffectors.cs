using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.ProceduralMotion;
using Nexora.Motion;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Base class for defining an effector that might control parameters of many different sections
    /// when aiming (e.g movement speed, FOV, recoil, and many more).
    /// </summary>
    [Serializable]
    public abstract class AimEffector
    {
        protected IGunAimBehaviour _aimBehaviour;

        public void Initialize(IGunAimBehaviour aimBehaviour) 
        { 
            _aimBehaviour = aimBehaviour;

            aimBehaviour.OnAimingStarted += OnAimingStarted;
            aimBehaviour.OnAimingStopped += OnAimingStopped;
        }

        public virtual void Enable(IGun gun) { }
        public virtual void Disable(IGun gun) { }

        protected abstract void OnAimingStarted();
        protected abstract void OnAimingStopped();
    }

    /// <summary>
    /// Effector for adding multiplier to recoil while aiming.
    /// </summary>
    [Serializable]
    public sealed class AimRecoilEffector : AimEffector
    {
        [Tooltip("Multiplier for changing the recoil while aiming.")]
        [SerializeField, Range(0f, 5f)]
        private float _aimRecoilMultiplier;

        private IGun _gun;

        public override void Enable(IGun gun)
        {
            _gun = gun;
            _gun.AddComponentChangedListener(GunBehaviourType.RecoilSystem, OnRecoilBehaviourChanged);
        }

        private void OnRecoilBehaviourChanged() => _gun.RecoilMechanism.SetRecoilMultiplier(_aimRecoilMultiplier);

        public override void Disable(IGun gun) => _gun.RemoveComponentChangedListener(GunBehaviourType.RecoilSystem, OnRecoilBehaviourChanged);

        protected override void OnAimingStarted() => _gun.RecoilMechanism.SetRecoilMultiplier(_aimRecoilMultiplier);

        protected override void OnAimingStopped() => _gun.RecoilMechanism.SetRecoilMultiplier(1);
    }

    /// <summary>
    /// Effector for modifying movement speed while aiming.
    /// </summary>
    [Serializable]
    public sealed class AimMovementSpeedEffector : AimEffector
    {
        [Tooltip("Multiplier for changing movement speed while aiming.")]
        [SerializeField, Range(0f, 1f)]
        private float _aimMovementSpeedMultiplier;

        private IMovementSpeedAdjuster _movementSpeedAdjuster;

        public override void Enable(IGun gun) => _movementSpeedAdjuster = gun as IMovementSpeedAdjuster;

        protected override void OnAimingStarted() => _movementSpeedAdjuster?.SpeedModifier.AddModifier(GetMovementSpeedMultiplier);
        protected override void OnAimingStopped() => _movementSpeedAdjuster?.SpeedModifier.RemoveModifier(GetMovementSpeedMultiplier);

        private float GetMovementSpeedMultiplier() => _aimMovementSpeedMultiplier;
    }

    /// <summary>
    /// Effector for changing crosshair when aiming.
    /// </summary>
    [Serializable]
    public sealed class AimCrosshairEffector : AimEffector
    {
        [Tooltip("Which crosshair should be selected while aiming?")]
        [SerializeField, Range(-1, 100)]
        private int _aimCrosshairID;

        private ICrosshairHandler _crosshairHandler;

        public override void Enable(IGun gun) => _crosshairHandler = gun as ICrosshairHandler;

        protected override void OnAimingStarted()
        {
            if (_crosshairHandler != null)
            {
                _crosshairHandler.CrosshairID = _aimCrosshairID;
            }
        }

        protected override void OnAimingStopped() => _crosshairHandler?.ResetCrosshair();
    }

    /// <summary>
    /// Effector for changing camera and view model FOV by a scaler when aiming.
    /// </summary>
    [Serializable]
    public sealed class AimFOVEffector : AimEffector
    {
        [Tooltip("FOV scale of the camera while aiming (Multiplies with base FOV).")]
        [SerializeField, Range(0.1f, 3f)]
        private float _cameraFOVScale = 0.8f;

        [Tooltip("FOV scale of the view model (e.g hands) while aiming.")]
        [SerializeField, Range(0.1f, 3f)]
        private float _viewModelFOVScale = 0.8f;

        [Tooltip("Duration for lerping to the new FOV.")]
        [SerializeField, Range(0f, 2f)]
        private float _fovEaseDuration = 0.3f;

        private HandheldFOVHandler _fovHandler;

        public override void Enable(IGun gun)
        {
            var handheld = gun as IHandheld;
            _fovHandler = handheld.gameObject.GetComponentInDirectChildren<HandheldFOVHandler>();
        }

        protected override void OnAimingStarted() => ApplyFOV(true);

        protected override void OnAimingStopped() => ApplyFOV(false);

        private void ApplyFOV(bool enable)
        {
            float cameraScale = enable ? _cameraFOVScale : 1f;
            float viewModelScale = enable ? _viewModelFOVScale : 1f;

            _fovHandler?.SetCameraFOV(cameraScale, _fovEaseDuration);
            _fovHandler?.SetViewModelFOV(viewModelScale, _fovEaseDuration);
        }
    }

    /// <summary>
    /// Effector for setting animator parameter upon aiming start/end.
    /// </summary>
    [Serializable]
    public sealed class AimAnimatorEffector : AimEffector
    {
        private IHandheld _handheld;

        public override void Enable(IGun gun) => _handheld = gun as IHandheld;

        protected override void OnAimingStarted() => _handheld.Animator.SetBool(HandheldAnimationConstants.IsAiming, true);
        protected override void OnAimingStopped() => _handheld.Animator.SetBool(HandheldAnimationConstants.IsAiming, false);
    }

    /// <summary>
    /// Effector for playing audio when aim start/end.
    /// </summary>
    [Serializable]
    public sealed class AimAudioEffector : AimEffector
    {
        [SerializeField]
        private AudioCue _aimStartAudio = new(null);

        [SerializeField]
        private AudioCue _aimStopAudio = new(null);

        private IHandheld _handheld;

        public override void Enable(IGun gun) => _handheld = gun as IHandheld;

        protected override void OnAimingStarted() => _handheld.AudioPlayer.PlayClip(_aimStartAudio, BodyPart.Hands);
        protected override void OnAimingStopped() => _handheld.AudioPlayer.PlayClip(_aimStopAudio, BodyPart.Hands);
    }

    #region Procedural Motion Effectors
    /// <summary>
    /// Effector for adding <see cref="MotionDataPreset{StateType}"/> while aiming.
    /// </summary>
    [Serializable]
    public sealed class AimMotionPresetEffector : AimEffector
    {
        [SerializeField]
        private CharacterMotionDataPreset _motionPreset;

        private IHandheld _handheld;

        public override void Enable(IGun gun) => _handheld = gun as IHandheld;

        protected override void OnAimingStarted()
        {
            _handheld.MotionTargets.HeadMotion.DataBroadcaster.AddPreset<MovementStateType>(_motionPreset);
        }

        protected override void OnAimingStopped()
        {
            _handheld.MotionTargets.HeadMotion.DataBroadcaster.RemovePreset<MovementStateType>(_motionPreset);
        }
    }

    /// <summary>
    /// Effector for modifying <see cref="HeadTiltMotion"/> while aiming.
    /// </summary>
    [Serializable]
    public sealed class AimHeadTiltMotionEffector : AimEffector
    {
        private IHandheld _handheld;
        private float _previousBlendWeight;

        public override void Enable(IGun gun)
            => _handheld = gun as IHandheld;

        protected override void OnAimingStarted()
        {
            var motion = _handheld.MotionTargets.HandsMotion.MotionMixer.GetMotion<HeadTiltMotion>();

            _previousBlendWeight = motion.SelfBlendWeight;
            motion.SelfBlendWeight = 0f;
        }

        protected override void OnAimingStopped()
        {
            var motion = _handheld.MotionTargets.HandsMotion.MotionMixer.GetMotion<HeadTiltMotion>();

            motion.SelfBlendWeight = _previousBlendWeight;
        }
    }

    /// <summary>
    /// Effector for modifying <see cref="OffsetMotion"/> while aiming.
    /// </summary>
    [Serializable]
    public sealed class AimOffsetMotionEffector : AimEffector
    {
        private IHandheld _handheld;

        public override void Enable(IGun gun) => _handheld = gun as IHandheld;

        protected override void OnAimingStarted()
        {
            var motion = _handheld.MotionTargets.HandsMotion.MotionMixer.GetMotion<OffsetMotion>();

            motion.IgnoreMixerBlendWeight = true;
        }

        protected override void OnAimingStopped()
        {
            var motion = _handheld.MotionTargets.HandsMotion.MotionMixer.GetMotion<OffsetMotion>();

            motion.IgnoreMixerBlendWeight = false;
        }
    }

    /// <summary>
    /// Effector for setting <see cref="OffsetData"/> when aiming.
    /// </summary>
    [Serializable]
    public sealed class AimHandOffsetEffector : AimEffector
    {
        [SerializeField]
        private OffsetData _offsetMotionData;

        private IHandheld _handheld;

        public override void Enable(IGun gun) => _handheld = gun as IHandheld;

        protected override void OnAimingStarted() => ApplyOffset(true);
        protected override void OnAimingStopped() => ApplyOffset(false);

        private void ApplyOffset(bool enable)
        {
            _handheld.MotionTargets.HandsMotion.DataBroadcaster.SetDataOverride<OffsetData>(enable ? _offsetMotionData : null);
        }
    }

    /// <summary>
    /// Effector used for adding damp effect to the head and hans procedural motion when aiming.
    /// </summary>
    [Serializable]
    public sealed class AimMotionDampenerEffector : AimEffector
    {
        [SerializeField, Range(0f, 1f)]
        private float _headDampening = 0.3f;

        [SerializeField, Range(0f, 1f)]
        private float _handsDampening = 0.3f;

        private IHandheld _handheld;

        public override void Enable(IGun gun) => _handheld = gun as IHandheld;

        protected override void OnAimingStarted() => ApplyDampening(true);
        protected override void OnAimingStopped() => ApplyDampening(false);

        private void ApplyDampening(bool enable)
        {
            IMotionMixer headMixer = _handheld.MotionTargets.HeadMotion.MotionMixer;
            IMotionMixer handsMixer = _handheld.MotionTargets.HandsMotion.MotionMixer;

            headMixer.BlendWeight = enable ? _headDampening : 1f;
            handsMixer.BlendWeight = enable ? _handsDampening : 1f;
        }
    }
    #endregion

    /// <summary>
    /// Effector used for enabling UI-based scope effect when aiming.
    /// </summary>
    [Serializable]
    public sealed class ScopeUIEffector : AimEffector
    {
        [SerializeField, Range(0f, 1f)]
        private float _scopeEnableDelay = 0.3f;

        public event UnityAction<bool> ScopeStateChanged;

        private IHandheld _handheld;
        private MonoBehaviour _coroutineRunner;
        private Coroutine _scopeCoroutine;

        public override void Enable(IGun gun)
        {
            _handheld = gun as IHandheld;
            _coroutineRunner = gun as MonoBehaviour;
        }

        protected override void OnAimingStarted()
        {
            _scopeCoroutine = _coroutineRunner.InvokeDelayed(() =>
            {
                _handheld.IsGeometryVisible = false;
                ScopeStateChanged?.Invoke(true);
            }, _scopeEnableDelay);
        }

        protected override void OnAimingStopped()
        {
            CoroutineUtils.StopCoroutine(_coroutineRunner, ref _scopeCoroutine);
            _handheld.IsGeometryVisible = true;
            ScopeStateChanged?.Invoke(false);
        }
    }
}