using Nexora.FPSDemo.Movement;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    /// <summary>
    /// Calculates dynamic FOV multipliers depending on character state.
    /// </summary>
    public interface IFOVMultiplierCalculator
    {
        /// <summary>
        /// Calculates multiplier that will be used to scale FOV value.
        /// </summary>
        float CalculateMultiplier(ICharacterMotor characterMotor);
    }

    public sealed class SpeedBasedFOVCalculator : IFOVMultiplierCalculator
    {
        private readonly AnimationCurve _speedCurve;

        public SpeedBasedFOVCalculator(AnimationCurve speedCurve) => _speedCurve = speedCurve;

        public float CalculateMultiplier(ICharacterMotor characterMotor)
        {
            float horizontalSpeed = characterMotor.Velocity.Horizontal().magnitude;
            return _speedCurve.Evaluate(horizontalSpeed);
        }
    }

    public sealed class HeightBasedFOVCalculator : IFOVMultiplierCalculator
    {
        private readonly AnimationCurve _heightCurve;

        public HeightBasedFOVCalculator(AnimationCurve heightCurve) => _heightCurve = heightCurve;

        public float CalculateMultiplier(ICharacterMotor characterMotor) => _heightCurve.Evaluate(characterMotor.Height);
    }

    public sealed class AirborneFOVCalculator : IFOVMultiplierCalculator
    {
        private readonly float _airborneMultiplier;

        public AirborneFOVCalculator(float airborneMultiplier) => _airborneMultiplier = airborneMultiplier;

        public float CalculateMultiplier(ICharacterMotor characterMotor) => characterMotor.IsGrounded ? 1f : _airborneMultiplier;
    }

    public sealed class CompositeFOVMultiplierCalculator : IFOVMultiplierCalculator
    {
        private readonly IReadOnlyList<IFOVMultiplierCalculator> _calculators;

        public CompositeFOVMultiplierCalculator(params IFOVMultiplierCalculator[] calculators) 
            => _calculators = calculators;

        public float CalculateMultiplier(ICharacterMotor characterMotor)
        {
            float result = 1f;
            foreach(var calculator in _calculators)
            {
                result *= calculator.CalculateMultiplier(characterMotor);
            }

            return result;
        }
    }
}