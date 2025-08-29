using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.ProceduralMotion;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Base effector for listening to <see cref="ChargedTrigger"/> events and then execute
    /// effects if needed.
    /// </summary>
    [Serializable]
    public abstract class ChargeTriggerEffector
    {
        public virtual void Initialize(IChargedTriggerBehaviour chargedTriggerBehaviour)
        {
            chargedTriggerBehaviour.ChargeStarted += OnChargeStarted;
            chargedTriggerBehaviour.ChargeUpdated += OnChargeUpdated;
            chargedTriggerBehaviour.ChargeMaxed += OnChargeMaxed;
            chargedTriggerBehaviour.ChargeCanceled += OnChargeCanceled;
            chargedTriggerBehaviour.OnFired += OnFired;
        }

        public virtual void Tick(float triggerCharge, float deltaTime) { }
        public virtual void LateUpdate() { }
        public virtual void Enable(IGun gun) { }
        public virtual void Disable(IGun gun) { }

        protected virtual void OnChargeStarted() { }
        protected virtual void OnChargeUpdated(float triggerCharge) { }
        protected virtual void OnChargeMaxed() { }
        protected virtual void OnChargeCanceled() { }
        protected virtual void OnFired(float triggerCharge) { }
    }

    /// <summary>
    /// Effector that plays for different charge phases.
    /// </summary>
    [Serializable]
    public sealed class ChargeAudioEffector : ChargeTriggerEffector
    {
        [Tooltip("Cooldown between charge audios to prevent spamming.")]
        [SerializeField, Range(0f, 1f)]
        private float _chargeAudioCooldown = 0.3f;

        [SerializeField]
        private AudioCue _chargeStartAudio = new(null);

        [Tooltip("Audio played when the charge reaches maximum value.")]
        [SerializeField]
        private AudioCue _chargeMaxAudio = new(null);

        [Tooltip("Audio played when charge was released but it didn't meet minimum charge value.")]
        [SerializeField]
        private AudioCue _chargeCancelAudio = new(null);

        private IHandheld _handheld;
        private float _nextPlayTime;

        public override void Enable(IGun gun) => _handheld = gun as IHandheld;

        protected override void OnChargeStarted() => PlaySound(_chargeStartAudio);
        protected override void OnChargeMaxed() => PlaySound(_chargeMaxAudio);
        protected override void OnChargeCanceled() => PlaySound(_chargeCancelAudio);

        private void PlaySound(AudioCue sound)
        {
            if (Time.time > _nextPlayTime)
            {
                _handheld.AudioPlayer.PlayClip(sound, BodyPart.Hands);
                _nextPlayTime = Time.time + _chargeAudioCooldown;
            }
        }
    }

    /// <summary>
    /// Triggers animator parameters depending on charge phase.
    /// </summary>
    [Serializable]
    public sealed class ChargeAnimatorEffector : ChargeTriggerEffector
    {
        private IHandheld _handheld;

        public override void Enable(IGun gun) => _handheld = gun as IHandheld;

        protected override void OnChargeStarted()
        {
            _handheld.Animator.SetTrigger(HandheldAnimationConstants.IsCharging);
        }

        protected override void OnChargeMaxed()
        {
            _handheld.Animator.SetTrigger(HandheldAnimationConstants.FullCharge);
        }

        protected override void OnFired(float triggerCharge)
        {
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsEmpty, false);
            _handheld.Animator.SetTrigger(HandheldAnimationConstants.Shoot);
        }
    }

    /// <summary>
    /// Applies procedural motion effects when trigger is being charged.
    /// </summary>
    [Serializable]
    public sealed class ChargeMotionEffector : ChargeTriggerEffector
    {
        [Tooltip("Motion damp amount.")]
        [SerializeField, Range(0f, 1f)]
        private float _motionDampening = 0.5f;

        [Tooltip("Offset data used for override.")]
        [SerializeField]
        private OffsetData _handsOffset;

        [Tooltip("Motion preset to add for the head.")]
        [SerializeField]
        private CharacterMotionDataPreset _headMotionPreset;

        private IHandheld _handheld;
        private float _previousHeadTiltWeight;

        protected override void OnChargeStarted() => ApplyEffects(true);
        protected override void OnChargeCanceled() => ApplyEffects(false);
        protected override void OnFired(float triggerCharge) => ApplyEffects(false);

        public void ApplyEffects(bool enable)
        {
            HandleHeadMotion(enable);
            HandleHandsMotion(enable);
        }

        private void HandleHeadMotion(bool enable)
        {
            CharacterMotionHandler headMotion = _handheld.MotionTargets.HeadMotion;

            if (enable)
            {
                headMotion.DataBroadcaster.AddPreset<MovementStateType>(_headMotionPreset);
                headMotion.MotionMixer.BlendWeight = _motionDampening;
            }
            else
            {
                headMotion.DataBroadcaster.RemovePreset<MovementStateType>(_headMotionPreset);
                headMotion.MotionMixer.BlendWeight = 1f;
            }
        }

        /// <summary>
        /// Sets data override for <see cref="OffsetData"/>.
        /// <br></br> Changes motion mixer blend weight for its dampening effect.
        /// <br></br> Updates <see cref="HeadTiltMotion"/> blend weight.
        /// <br></br> Changes <see cref="OffsetMotion"/> behaviour to mixer blend weight.
        /// </summary>
        /// <param name="enable"></param>
        private void HandleHandsMotion(bool enable)
        {
            CharacterMotionHandler handsMotion = _handheld.MotionTargets.HandsMotion;

            handsMotion.MotionMixer.BlendWeight = enable ? _motionDampening : 1f;
            handsMotion.DataBroadcaster.SetDataOverride<OffsetData>(enable ? _handsOffset : null);

            if(handsMotion.MotionMixer.TryGetMotion(out HeadTiltMotion headTilt))
            {
                if(enable)
                {
                    _previousHeadTiltWeight = headTilt.SelfBlendWeight;
                    headTilt.SelfBlendWeight = 0f;
                }
                else
                {
                    headTilt.SelfBlendWeight = _previousHeadTiltWeight;
                }
            }

            handsMotion.MotionMixer.GetMotion<OffsetMotion>().IgnoreMixerBlendWeight = enable;
        }
    }

    /// <summary>
    /// Handles FOV changes while charging.
    /// </summary>
    [Serializable]
    public sealed class ChargeFOVHandler : ChargeTriggerEffector
    {
        [Tooltip("FOV scale of the camera while charging (Multiplies with base FOV).")]
        [SerializeField, Range(0.1f, 3f)]
        private float _cameraFOVScale = 0.8f;

        [Tooltip("FOV scale of the view model (e.g hands) while charhing.")]
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

        protected override void OnChargeStarted() => ApplyFOV(true);
        protected override void OnChargeCanceled() => ApplyFOV(false);
        protected override void OnFired(float triggerCharge) => ApplyFOV(true);

        private void ApplyFOV(bool enable)
        {
            float cameraScale = enable ? _cameraFOVScale : 1f;
            float viewModelScale = enable ? _viewModelFOVScale : 1f;

            float fovEaseDuration = _fovEaseDuration * (enable ? 1.1f : 0.9f);

            _fovHandler?.SetCameraFOV(cameraScale, fovEaseDuration);
            _fovHandler?.SetViewModelFOV(viewModelScale, fovEaseDuration);
        }
    }

    /// <summary>
    /// Visualizes the path upon charging the trigger.
    /// </summary>
    [Serializable]
    public sealed class ChargeProjectilePathVisualizer : ChargeTriggerEffector
    {
        [SerializeField, NotNull]
        private ProjectilePathVisualizer _pathVisualizer;

        [Tooltip("Minimum charge amount required for path visualization to activate.")]
        [SerializeField, Range(0f, 1f)]
        private float _minChargeAmountToVisualize = 0.3f;

        private IGun _gun;

        public override void Enable(IGun gun) => _gun = gun;

        protected override void OnFired(float triggerCharge) => _pathVisualizer.Disable();
        public override void Tick(float triggerCharge, float deltaTime)
        {
            if(triggerCharge > _minChargeAmountToVisualize) // [Revisit] Min Charge Time
            {
                if(_pathVisualizer.enabled == false)
                {
                    _pathVisualizer.Enable();
                }

                _pathVisualizer.UpdateVisualization(_gun.FiringMechanism);
            }
        }
        protected override void OnChargeCanceled() => _pathVisualizer.Disable();
    }

    /// <summary>
    /// Updates the crosshair UI based on the charge level.
    /// </summary>
    [Serializable]
    public sealed class ChargeCrosshairUIEffector : ChargeTriggerEffector
    {
        private ICrosshairHandler _crosshairHandler;

        public override void Enable(IGun gun) => _crosshairHandler = gun as ICrosshairHandler;

        // protected override void OnChargeUpdated(float triggerCharge) => 
        public override void Tick(float triggerCharge, float deltaTime) => _crosshairHandler.SetCharge(triggerCharge);

        public override void Disable(IGun gun) => _crosshairHandler.SetCharge(0f);
    }

    /// <summary>
    /// Controls <see cref="HandheldPartAnimator"/> dependign on charge phase.
    /// </summary>
    [Serializable]
    public sealed class ChargeMovingPartsEffector : ChargeTriggerEffector
    {
        [SerializeField, NotNull]
        private HandheldPartAnimator _movingParts;

        public override void Initialize(IChargedTriggerBehaviour chargedTriggerBehaviour)
        {
            _movingParts.Initialize();
            base.Initialize(chargedTriggerBehaviour);
        }

        protected override void OnChargeStarted() => _movingParts.BeginMovement();
        public override void LateUpdate() => _movingParts.UpdateMovement();
        protected override void OnChargeCanceled() => _movingParts.StopMovement();
        protected override void OnFired(float triggerCharge) => _movingParts.StopMovement();
    }
}