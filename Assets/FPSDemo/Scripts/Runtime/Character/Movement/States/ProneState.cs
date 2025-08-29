using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    [Serializable]
    public class ProneState : GroundedMovementState
    {
        [Title("Prone Settings")]
        [Tooltip("The controller's height when proning.")]
        [SerializeField, Range(0f, 2f)]
        private float _proneHeight = 0.7f;

        [Tooltip("Cooldown duration between prones.")]
        [SerializeField, Range(0f, 2f)]
        private float _proneCooldown = 0.4f;

        [Title("Audio")]
        [SerializeField]
        private AudioCue _proneAudio = new(null);

        [SerializeField]
        private AudioCue _standUpAudio = new(null);

        private float _nextAllowedProneTime;

        public override MovementStateType StateType => MovementStateType.Prone;

        public override bool CanTransitionTo()
        {
            return Time.time > _nextAllowedProneTime
                && CharacterMotor.IsGrounded
                && CharacterMotor.CanSetHeight(_proneHeight);
        }

        public override void OnEnter(MovementStateType previousStateType)
        {
            _nextAllowedProneTime = Time.time + _proneCooldown;
            CharacterMotor.SetHeight(_proneHeight);
            Character.AudioPlayer.PlayClip(_proneAudio, BodyPart.Chest);
        }

        public override void OnExit()
        {
            Character.AudioPlayer.PlayClip(_standUpAudio, BodyPart.Chest);
        }

        public override void UpdateLogic()
        {
            if (MovementController.TrySetState(MovementStateType.Airborne)) return;

            bool cooldownFinished = Time.time >= _nextAllowedProneTime;
            if (cooldownFinished == false)
            {
                return;
            }

            if (ShouldStandUp())
            {
                MovementInput.ConsumeProneInput();
                MovementInput.ConsumeCrouchInput();
                MovementInput.ConsumeJumpInput();

                MovementController.TrySetState(CharacterMotor.SimulatedVelocity.sqrMagnitude > 0.1f
                    ? MovementStateType.Walk
                    : MovementStateType.Idle);
            }
            else if (ShouldCrouch())
            {
                // [Revisit]
                MovementInput.ConsumeProneInput();
                MovementInput.ConsumeJumpInput();

                MovementController.TrySetState(MovementStateType.Crouch);
            }
        }

        private bool ShouldStandUp()
        {
            return MovementInput.IsProningHeld == false
                || MovementInput.IsJumpPressed
                || MovementInput.IsRunningHeld;
        }

        private bool ShouldCrouch() => MovementInput.IsCrouchingHeld;
    }
}