using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.SurfaceSystem;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexora.FPSDemo.Footsteps
{
    /// <summary>
    /// Denotes the result of an audio effect played.
    /// </summary>
    public readonly struct AudioEffectResult
    {
        public readonly SurfaceDefinition Surface;
        public readonly float Volume;
        public readonly SurfaceEffectType EffectType;

        public AudioEffectResult(SurfaceDefinition surface, float volume, SurfaceEffectType effectType)
        {
            Surface = surface;
            Volume = volume;
            EffectType = effectType;
        }
    }

    /// <summary>
    /// Class that controls audio for footsteps of a character along with impact foot audios.
    /// </summary>
    [RequireCharacterBehaviour(typeof(ICharacterMotor), typeof(IMovementController))]
    public sealed class FootstepsController : CharacterBehaviour
    {
        [Tooltip("Configuration settings for footsteps.")]
        [SerializeField]
        private FootstepConfig _configuration;

        private IMovementController _movementController;
        private ICharacterMotor _characterMotor;

        private IGroundDetection _groundDetection;
        private IFootstepAudioPlayer _footstepAudioPlayer;
        private IFallImpactAudioPlayer _fallImpactAudioPlayer;
        private ISurfaceModifiers _surfaceModifiers;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _movementController = parent.GetCC<IMovementController>();
            _characterMotor = parent.GetCC<ICharacterMotor>();

            _groundDetection = new GroundDetection(_configuration, transform);
            _footstepAudioPlayer = new FootstepAudioPlayer(_configuration);
            _fallImpactAudioPlayer = new FallImpactAudioPlayer(_configuration);
            _surfaceModifiers = new SurfaceModifiers(_movementController);
        }

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            _movementController.StepCycleEnded += OnStepCycleEnded;
            _movementController.AddStateTransitionListener(MovementStateType.Jump, OnJumpStart);
            _characterMotor.FallImpact += OnFallImpact;
        }

        protected override void OnBehaviourDisable(ICharacter parent)
        {
            _movementController.StepCycleEnded -= OnStepCycleEnded;
            _movementController.RemoveStateTransitionListener(MovementStateType.Jump, OnJumpStart);
            _characterMotor.FallImpact -= OnFallImpact;
        }

        private void OnStepCycleEnded()
        {
            GroundDetectionData groundData = _groundDetection.DetectGround();
            if(groundData.IsValid == false)
            {
                return;
            }

            var movementData = new MovementData(
                _characterMotor.Velocity.magnitude, _characterMotor.TurnSpeed, _movementController.ActiveStateType);

            AudioEffectResult result = _footstepAudioPlayer.PlayFootstepAudio(in groundData, in movementData);

            _surfaceModifiers.SetSurface(result.Surface);
        }

        private void OnFallImpact(float impactSpeed)
        {
            GroundDetectionData groundData = _groundDetection.DetectGround();
            if (groundData.IsValid == false)
            {
                return;
            }

            var fallImpactData = new FallImpactData(impactSpeed);
            AudioEffectResult result = _fallImpactAudioPlayer.PlayImpactAudio(in groundData, in fallImpactData);

            _surfaceModifiers.SetSurface(result.Surface);
        }

        private void OnJumpStart(MovementStateType previousState)
        {
            GroundDetectionData groundData = _groundDetection.DetectGround();
            if (groundData.IsValid == false)
            {
                return;
            }

            SurfaceDefinition surface = SurfaceSystemModule.Instance.PlayEffectFromHit(
                in groundData.Hit,
                SurfaceEffectType.JumpStart,
                SurfaceEffectFlags.Audio);

            _surfaceModifiers.SetSurface(null);
        }

        [Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            _configuration.Validate();            
        }
    }
}