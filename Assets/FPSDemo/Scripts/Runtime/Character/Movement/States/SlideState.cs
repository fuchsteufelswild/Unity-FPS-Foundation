using Nexora.FPSDemo.CharacterBehaviours;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    public static class SlideConstants
    {
        public const float AirborneImpulseReduction = 0.33f;
        public const float JumpDelayAfterSlideStart = 0.2f;
        public const float MinVelocityForTransition = 2f;
        public const float SlopeAngleThreshold = 3f;
        public const float SideCollisionSpeedReduction = 0.5f;
        public const float SlopeAccelerationMultiplier = 10f;

        public const float MinInputThreshold = 0.1f;
    }

    [Serializable]
    public class SlideState : CharacterMovementState
    {
        [Tooltip("Contains the variable settings for slide state.")]
        [SerializeField]
        private SlideStateConfig _config;

        private SlideStateData _slideState;
        private AudioSource _slideLoopSource;

        public override MovementStateType StateType => MovementStateType.Slide;
        public override float StepCycleLength => float.PositiveInfinity;
        public override bool ApplyGravity => false;
        public override bool SnapToGround => true;

        public override bool CanTransitionTo()
        {
            return CharacterMotor.Velocity.Horizontal().magnitude > _config.MinRequiredSpeed
                && CharacterMotor.IsGrounded
                && CharacterMotor.CanSetHeight(_config.Height);
        }

        protected override void OnInitialized() => _slideState.Reset();

        public override void OnEnter(MovementStateType previousStateType)
        {
            InitializeSlideState(previousStateType);
            CharacterMotor.SetHeight(_config.Height);
            _slideLoopSource = Character.AudioPlayer.StartLoop(_config.SlideAudio, BodyPart.Feet, _config.SlideAudioDuration);
            MovementInput.ConsumeRunInput();
            MovementInput.ConsumeRunInput();
        }

        private void InitializeSlideState(MovementStateType previousStateType)
        {
            _slideState.Direction = Vector3.ClampMagnitude(CharacterMotor.SimulatedVelocity, 1f);
            _slideState.StartTime = Time.time;
            _slideState.IsJumpPressed = false;

            float baseImpulseModifier = _config.EnterSpeedCurve.Evaluate(CharacterMotor.Velocity.magnitude) * _config.Impulse;

            _slideState.InitialImpulseModifier = previousStateType == MovementStateType.Airborne
                ? baseImpulseModifier * SlideConstants.AirborneImpulseReduction 
                : baseImpulseModifier;
        }

        public override void OnExit()
        {
            Character.AudioPlayer.StopLoop(_slideLoopSource);
            // _slideLoopSource = null;
        }

        public override void UpdateLogic()
        {
            HandleStateTransitions();
            if(MovementInput.IsJumpPressed)
            {
                _slideState.IsJumpPressed = true;
            }
        }

        private void HandleStateTransitions()
        {
            if (MovementController.TrySetState(MovementStateType.Airborne)) return;

            if (TryTransitionBasedOnSpeed()) return;

            if (ShouldTransitionToJump() && MovementController.TrySetState(MovementStateType.Jump)) return;
        }

        private bool TryTransitionBasedOnSpeed()
        {
            if(CharacterMotor.Velocity.magnitude >= _config.StopSpeed)
            {
                return false;
            }

            if (MovementInput.IsRunningHeld && MovementController.TrySetState(MovementStateType.Run)) return true;

            if (ShouldTransitionToCrouch() && MovementController.TrySetState(MovementStateType.Crouch)) return true;

            return false;
        }

        private bool ShouldTransitionToCrouch()
            => CharacterMotor.Velocity.magnitude < SlideConstants.MinVelocityForTransition
            && MovementInput.RawMovementDirection.sqrMagnitude > 0.1f;

        private bool ShouldTransitionToJump() 
            => _slideState.IsJumpPressed && _slideState.ElapsedTime > SlideConstants.JumpDelayAfterSlideStart;

        public override Vector3 UpdateVelocity(Vector3 currentVelocity, float deltaTime)
        {
            return SlidePhysics.CalculateSlideVelocity(
                currentVelocity,
                _slideState.Direction,
                _slideState.ElapsedTime,
                _slideState.InitialImpulseModifier,
                _config,
                CharacterMotor,
                MovementController,
                MovementInput,
                deltaTime);
        }

        private struct SlideStateData
        {
            public Vector3 Direction;
            public float InitialImpulseModifier;
            public float StartTime;
            public bool IsJumpPressed;

            public void Reset()
            {
                Direction = Vector3.zero;
                InitialImpulseModifier = 0f;
                StartTime = 0f;
                IsJumpPressed = false;
            }

            public float ElapsedTime => Time.time - StartTime;
        }
    }

    public static class SlidePhysics
    {
        public static Vector3 CalculateSlideVelocity(
            Vector3 currentVelocity,
            Vector3 slideDirection,
            float slideTime,
            float initialImpulse,
            SlideStateConfig slideConfig,
            ICharacterMotor motor,
            IMovementController controller,
            IMovementInputController input,
            float deltaTime)
        {
            Vector3 targetVelocity = slideDirection * (slideConfig.SpeedCurve.Evaluate(slideTime) * initialImpulse);
            targetVelocity = ApplySidewaysMovement(targetVelocity, input, motor.transform, slideConfig.Inputnfluence);

            // We use 'currentVelocity' here as it is a natural effect, like gravity or wind
            currentVelocity = ApplySlopeEffect(currentVelocity, motor, slideConfig.SlopeFactor, deltaTime);

            targetVelocity *= GetVelocityModifier(controller, motor);

            float accelerationModifier = targetVelocity.sqrMagnitude > 0.1f
                ? controller.AccelerationModifier.Evaluate()
                : controller.DecelerationModifier.Evaluate();

            return Vector3.Lerp(currentVelocity, targetVelocity,
                deltaTime * slideConfig.Acceleration * accelerationModifier);
        }

        private static Vector3 ApplySidewaysMovement(Vector3 targetVelocity, IMovementInputController input, Transform motorTransform, float inputInfluence)
        {
            // Give a slight nudge in the raw input direction for feel of control
            if(Mathf.Abs(input.RawMovementDirection.x) > SlideConstants.MinInputThreshold)
            {
                Vector3 sidewaysMovement = motorTransform.TransformVector(Vector3.right * input.RawMovementDirection.x);
                targetVelocity += Vector3.ClampMagnitude(sidewaysMovement, 1f);
            }

            // Don't change magnitude, but move in the direction of input
            float movementMagnitude = targetVelocity.magnitude;
            return Vector3.ClampMagnitude(input.WorldMovementDirection * inputInfluence + targetVelocity, movementMagnitude);
        }

        /// <summary>
        /// Speeds up the character if its sliding down a slope.
        /// </summary>
        private static Vector3 ApplySlopeEffect(Vector3 currentVelocity, ICharacterMotor motor, float slopeFactor, float deltaTime)
        {
            float surfaceAngle = motor.GroundSurfaceAngle;
            if(surfaceAngle <= SlideConstants.SlopeAngleThreshold)
            {
                return currentVelocity;
            }

            bool isDescendingSlope = Vector3.Dot(motor.GroundNormal, motor.SimulatedVelocity) > 0f;
            if(isDescendingSlope == false)
            {
                return currentVelocity;
            }

            float slopeSteepness = Mathf.Min(surfaceAngle, motor.SlopeLimit) / motor.SlopeLimit;
            Vector3 slopeDirection = motor.GroundNormal.Horizontal();

            return currentVelocity + 
                slopeDirection * (slopeSteepness * slopeFactor * deltaTime * SlideConstants.SlopeAccelerationMultiplier);
        }

        private static float GetVelocityModifier(IMovementController controller, ICharacterMotor motor)
        {
            float velocityMod = controller.SpeedModifier.Evaluate() * motor.GetSlopeSpeedMultiplier();

            if(motor.CollisionFlags.HasFlag(CollisionFlags.Sides))
            {
                velocityMod *= SlideConstants.SideCollisionSpeedReduction;
            }

            return velocityMod;
        }
    }

    
}