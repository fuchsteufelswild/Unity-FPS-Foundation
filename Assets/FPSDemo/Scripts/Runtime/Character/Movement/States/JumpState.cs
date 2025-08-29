using Nexora.Audio;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    [Serializable]
    public class JumpState : CharacterMovementState
    {
        [Tooltip("The vertical height that character can jump.")]
        [SerializeField, Range(0.1f, 10f)]
        private float _jumpHeight = 1f;

        [Tooltip("Maximum jumps available.")]
        [SerializeField, Range(1, 10)]
        private int _maxJumps = 1;

        [Tooltip("Cooldown between jumps (in seconds).")]
        [SerializeField, Range(0.1f, 2f)]
        private float _jumpCooldown = 0.3f;

        [Tooltip("Time window after leaving ground when jump is still allowed. " +
            "Allows jumping for a short time after walking off ledge.")]
        [SerializeField, Range(0f, 1f)]
        private float _bunnyTime;

        [SerializeField]
        private AudioCue _jumpSound = new(null);

        private int _remainingJumps;
        private float _nextJumpTime;
        private bool _wasGroundedLastFrame;

        public override MovementStateType StateType => MovementStateType.Jump;
        public override float StepCycleLength => 1f;
        public override bool ApplyGravity => false;
        public override bool SnapToGround => false;

        public int MaxJumps
        {
            get => _maxJumps;
            set
            {
                _maxJumps = value;
                if(CharacterMotor.IsGrounded)
                {
                    ResetJumpCount();
                }
            }
        }
        private void ResetJumpCount() => _remainingJumps = _maxJumps;

        protected override void OnInitialized()
        {
            CharacterMotor.GroundedChanged += HandleGroundedChanged;

            _nextJumpTime = -1f;
            MaxJumps = _maxJumps;
        }

        private void HandleGroundedChanged(bool isGrounded)
        {
            if(isGrounded)
            {
                _nextJumpTime = Time.time + _jumpCooldown;
                ResetJumpCount();
            }
        }

        public override bool CanTransitionTo()
        {
            if(CharacterMotor.CanSetHeight(CharacterMotor.DefaultHeight) == false)
            {
                return false;
            }

            // If just walked off ledge and falls, it is taken as a jump too, this for this edge case.
            // So if you click jump when fell of ledge, it will consume 2 jumps.
            if(_remainingJumps == _maxJumps && 
             (CharacterMotor.IsGrounded || CharacterMotor.LastGroundedChangeTime + _bunnyTime > Time.time) == false)
            {
                _remainingJumps--;
            }

            return _remainingJumps > 0 && Time.time >= _nextJumpTime;
        }

        public override void OnEnter(MovementStateType previousStateType) => ExecuteJump();

        private void ExecuteJump()
        {
            CharacterMotor.SetHeight(CharacterMotor.DefaultHeight);
            _remainingJumps--;
            _nextJumpTime = Time.time + _jumpCooldown;

            MovementInput.ConsumeJumpInput();
            MovementInput.ConsumeCrouchInput();
            MovementInput.ConsumeProneInput();
        }

        public override void UpdateLogic()
        {
            // Always try to be airborne when in jump state.
            MovementController.TrySetState(MovementStateType.Airborne);
        }

        public override Vector3 UpdateVelocity(Vector3 currentVelocity, float deltaTime)
        {
            if(_jumpHeight > 0.1f)
            {
                // v^2 = u^2 + 2as
                float jumpVelocity = Mathf.Sqrt(2 * CharacterMotor.Gravity * _jumpHeight);
                currentVelocity.y = jumpVelocity;
            }

            return currentVelocity;
        }
    }
}