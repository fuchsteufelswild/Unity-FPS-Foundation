using Nexora.FPSDemo.Movement;
using Nexora.Motion;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    /// <summary>
    /// Applies bob motion. Has a continuous motion that is applied when character is on the move 
    /// or turning, it is changed with each step-cycle. And at each step, applies a sudden force
    /// for a step feeling.
    /// </summary>
    /// <remarks>
    /// Creates a something like a figure-eight path, an infinite symbol kind of movement.
    /// </remarks>
    [RequireCharacterBehaviour(typeof(ICharacterMotor), typeof(IMovementController))]
    [RequireComponent(typeof(AdditiveMotionApplier))]
    public sealed class BobMotion : CharacterDataMotion<BobData>
    {
        [Range(0.1f, 10f)]
        [SerializeField]
        private float _resetSpeed = 2f;

        [Range(0f, Mathf.PI)]
        [SerializeField]
        private float _rotationDelay = 0.25f;

        private BobMotionConfig _config = new();
        private BobParameterCalculator _bobParameterCalculator = new();
        private BobStateController _bobStateController;
        private StepForceProcessor _stepForceProcessor;

        private AdditiveMotionApplier _additiveMotionApplier;
        private IMovementController _movementController;
        private ICharacterMotor _characterMotor;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _characterMotor = parent.GetCC<ICharacterMotor>();
            _movementController = parent.GetCC<IMovementController>();

            _bobStateController = new BobStateController(in _config);
            _stepForceProcessor = new StepForceProcessor(in _config);
        }

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            _movementController.StepCycleEnded += OnStepped;
            _additiveMotionApplier = Mixer.GetMotion<AdditiveMotionApplier>();
        }

        protected override void OnBehaviourDisable(ICharacter parent)
        {
            _movementController.StepCycleEnded -= OnStepped;
        }

        private void OnStepped()
        {
            if(_bobStateController.ShouldUpdateBob(CurrentMotionData, _characterMotor) == false)
            {
                return;
            }

            _stepForceProcessor.ProcessStep(CurrentMotionData, _characterMotor, _additiveMotionApplier, CombinedBlendWeight);
            _bobParameterCalculator.ToggleInversion();
        }

        protected override void OnMotionDataChanged(BobData motionData)
        {
            if(motionData == null)
            {
                return;
            }

            _positionSpring.ChangeSpringSettings(motionData.PositionSpring);
            _rotationSpring.ChangeSpringSettings(motionData.RotationSpring);
        }

        public override void Tick(float deltaTime)
        {
            bool shouldUpdateBob = _bobStateController.ShouldUpdateBob(CurrentMotionData, _characterMotor);

            if(_rotationSpring.IsAtRest == false || shouldUpdateBob)
            {
                ProcessBobMotion(shouldUpdateBob, deltaTime);
                ApplyBobResults();
            }
        }

        private void ProcessBobMotion(bool shouldUpdateBob, float deltaTime)
        {
            if(shouldUpdateBob)
            {
                float bobParameter = _bobParameterCalculator.CalculateBobParameter
                    (CurrentMotionData.CalculationMethod, _movementController.StepCycle, CurrentMotionData.TimeBasedBobSpeed);

                _bobStateController.UpdateActiveBob(bobParameter, CurrentMotionData, _rotationDelay);
            }
            else
            {
                _bobStateController.UpdateInactiveBob(deltaTime, _resetSpeed, CurrentMotionData);
            }
        }

        private void ApplyBobResults()
        {
            SetTargetPosition(_bobStateController.PositionBob);
            SetTargetRotation(_bobStateController.RotationBob);
        }

        /// <summary>
        /// Keeps the state of bob motion, and responsible for updating the state forward
        /// or back to zero slowly depending on the active state.
        /// </summary>
        private sealed class BobStateController
        {
            private readonly BobMotionConfig _config;

            private float _bobParameter;
            private Vector3 _positionBob;
            private Vector3 _rotationBob;

            public float BobParameter => _bobParameter;
            public Vector3 PositionBob => _positionBob;
            public Vector3 RotationBob => _rotationBob;

            public BobStateController(in BobMotionConfig config) => _config = config;

            /// <summary>
            /// Determines if bob motion should be active given the movement conditions.
            /// </summary>
            public bool ShouldUpdateBob(BobData data, ICharacterMotor characterMotor)
            {
                if(data == null)
                {
                    return false;
                }

                return data.CalculationMethod == BobCalculationMethod.TimeBased ||
                    (characterMotor.IsGrounded &&
                    (characterMotor.Velocity.sqrMagnitude > _config.VelocityThreshold ||
                     characterMotor.TurnSpeed > _config.TurnSpeedThreshold));
            }

            /// <summary>
            /// Updates the bob motion with given parameters.
            /// </summary>
            public void UpdateActiveBob(float bobParameter, BobData data, float rotationDelay)
            {
                _bobParameter = bobParameter;
                _positionBob = BobMathCalculator.CalculatePositionBob(bobParameter, data.PositionAmplitude);
                _rotationBob = BobMathCalculator.CalculateRotationBob(bobParameter, data.RotationAmplitude, rotationDelay);
            }

            /// <summary>
            /// Updates bob motion to reset (to zero), slowly with each call.
            /// </summary>
            public void UpdateInactiveBob(float deltaTime, float dataResetSpeed, BobData data)
            {
                float resetSpeed = data == null
                    ? _config.DefaultResetSpeed
                    : dataResetSpeed;

                _bobParameter = Mathf.MoveTowards(_bobParameter, 0f, resetSpeed);
                _positionBob = BobMathCalculator.ResetTowardsZero(_positionBob, resetSpeed, _config.ResetThreshold);
                _rotationBob = BobMathCalculator.ResetTowardsZero(_rotationBob, resetSpeed, _config.ResetThreshold);
            }
        }

        /// <summary>
        /// Handles step-based bob force application, applies forces in each step (left-right-left-right..).
        /// </summary>
        private sealed class StepForceProcessor
        {
            private readonly BobMotionConfig _config;

            public StepForceProcessor(in BobMotionConfig config) => _config = config;

            public void ProcessStep(BobData data, ICharacterMotor characterMotor, AdditiveMotionApplier motionApplier, float combinedBlendWeight)
            {
                if(data == null || motionApplier == null)
                {
                    return;
                }

                float stepFactor = characterMotor.Velocity.sqrMagnitude < _config.VelocityThreshold ? 0f : 1f;
                float totalMultiplier = stepFactor * combinedBlendWeight;

                motionApplier.AddPositionForce(data.PositionStepImpulseForce, totalMultiplier);
                motionApplier.AddRotationForce(data.RotationStepImpulseForce, totalMultiplier);
            }
        }

        /// <summary>
        /// Calculates bob parameter depending on calculation methods.
        /// </summary>
        private sealed class BobParameterCalculator
        {
            private bool _shouldInvertBob;

            public void ToggleInversion() => _shouldInvertBob = !_shouldInvertBob;
            public void ResetInversion() => _shouldInvertBob = false;

            /// <summary>
            /// Calculates the bob parameter based on <paramref name="calculationMethod"/>.
            /// </summary>
            /// <param name="calculationMethod">Calculation method for the bob parameter.</param>
            /// <param name="stepCycle">Current step cycle (0 or 1).</param>
            /// <param name="bobSpeed">Bobbing speed.</param>
            /// <returns>Calculated bob parameter value.</returns>
            /// <exception cref="NotImplementedException">In the case of different <see cref="BobCalculationMethod"/> passed.</exception>
            public float CalculateBobParameter(BobCalculationMethod calculationMethod, float stepCycle, float bobSpeed)
            {
                return calculationMethod switch
                {
                    BobCalculationMethod.StepCycleBased => CalculateStepBasedParameter(stepCycle),
                    BobCalculationMethod.TimeBased => CalculateTimeBasedParameter(bobSpeed),
                    _ => throw new NotImplementedException()
                };
            }

            /// <summary>
            /// Has return value like this as character is making movement.
            /// 0 - PI
            /// PI - 2*PI
            /// 0 - PI
            /// PI - 2*PI
            /// ...
            /// </summary>
            /// <param name="stepCycle">How much of a step is completed [0,1]</param>
            /// <returns></returns>
            private float CalculateStepBasedParameter(float stepCycle)
            {
                float parameter = stepCycle * Mathf.PI;
                return parameter + (_shouldInvertBob ? 0f : Mathf.PI);
            }

            private float CalculateTimeBasedParameter(float bobSpeed) => Time.time * bobSpeed;

        }

        private readonly struct BobMotionConfig
        {
            public readonly float VelocityThreshold;
            public readonly float TurnSpeedThreshold;
            public readonly float ResetThreshold;
            public readonly float DefaultResetSpeed;

            public BobMotionConfig(
                float velocityThreshold = 0.1f, float turnSpeedThreshold = 0.1f, 
                float resetThreshold = 0.001f, float defaultResetSpeed = 50f)
            {
                VelocityThreshold = velocityThreshold;
                TurnSpeedThreshold = turnSpeedThreshold;
                ResetThreshold = resetThreshold;
                DefaultResetSpeed = defaultResetSpeed;
            }
        }
    }

    public static class BobMathCalculator
    {
        /// <summary>
        /// Calculates position bob using cosine waves with different frequencies.
        /// </summary>
        public static Vector3 CalculatePositionBob(float bobParameter, Vector3 amplitude)
        {
            return new Vector3
            {
                x = Mathf.Cos(bobParameter) * amplitude.x,
                y = Mathf.Cos(bobParameter * 2) * amplitude.y,
                z = Mathf.Cos(bobParameter) * amplitude.z
            };
        }

        /// <summary>
        /// Calculates rotation bob using cosine waves with different frequencies along with a phase delay.
        /// </summary>
        public static Vector3 CalculateRotationBob(float bobParameter, Vector3 amplitude, float rotationDelay)
        {
            return new Vector3
            {
                x = Mathf.Cos(bobParameter * 2f + rotationDelay) * amplitude.x,
                y = Mathf.Cos(bobParameter + rotationDelay) * amplitude.y,
                z = Mathf.Cos(bobParameter + rotationDelay) * amplitude.z
            };
        }

        /// <summary>
        /// Resets the <paramref name="current"/> towards zero slowly with each call.
        /// </summary>
        /// <param name="current">Current value.</param>
        /// <param name="resetSpeed">Speed of the reset process.</param>
        /// <param name="resetThreshold">Under which value it is considered reset.</param>
        /// <returns>Value that is moved towards zero, if it was already below threshold then directly zero.</returns>
        public static Vector3 ResetTowardsZero(Vector3 current, float resetSpeed, float resetThreshold)
        {
            if(current.MagnitudeSum() <= resetThreshold)
            {
                return Vector3.zero;
            }

            return Vector3.MoveTowards(current, Vector3.zero, resetSpeed);
        }
    }
}