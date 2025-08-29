using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    [Serializable]
    public class CrouchState : GroundedMovementState
    {
        [Title("Crouch Settings")]
        [Tooltip("The controller's height when crouching.")]
        [SerializeField, Range(0f, 2f)]
        private float _crouchHeight = 0.7f;

        [Tooltip("Cooldown duration between crouchs.")]
        [SerializeField, Range(0f, 2f)]
        private float _crouchCooldown = 0.4f;

        [Title("Audio")]
        [SerializeField]
        private AudioCue _crouchAudio = new(null);

        [SerializeField]
        private AudioCue _standUpAudio = new(null);

        private float _nextAllowedCrouchTime;

        public override MovementStateType StateType => MovementStateType.Crouch;

        public override bool CanTransitionTo()
        {
            return Time.time > _nextAllowedCrouchTime
                && CharacterMotor.IsGrounded
                && CharacterMotor.CanSetHeight(_crouchHeight);
        }

        public override void OnEnter(MovementStateType previousStateType)
        {
            _nextAllowedCrouchTime = Time.time + _crouchCooldown;
            CharacterMotor.SetHeight(_crouchHeight);
            Character.AudioPlayer.PlayClip(_crouchAudio, BodyPart.Chest);
        }

        public override void OnExit()
        {
            Character.AudioPlayer.PlayClip(_standUpAudio, BodyPart.Chest);
        }

        public override void UpdateLogic()
        {
            if (MovementController.TrySetState(MovementStateType.Airborne)) return;

            bool cooldownFinished = Time.time >= _nextAllowedCrouchTime;
            if (cooldownFinished == false)
            {
                return;
            }

            if (ShouldStandUp())
            {
                MovementInput.ConsumeCrouchInput();
                MovementInput.ConsumeJumpInput();

                MovementController.TrySetState(CharacterMotor.SimulatedVelocity.sqrMagnitude > 0.1f
                    ? MovementStateType.Walk
                    : MovementStateType.Idle);
            }
            else if (ShouldProne() && MovementController.TrySetState(MovementStateType.Prone)) return;
        }

        private bool ShouldStandUp()
        {
            return MovementInput.IsCrouchingHeld == false
                || MovementInput.IsJumpPressed
                || MovementInput.IsRunningHeld;
        }

        private bool ShouldProne() => MovementInput.IsProningHeld;
    }
}