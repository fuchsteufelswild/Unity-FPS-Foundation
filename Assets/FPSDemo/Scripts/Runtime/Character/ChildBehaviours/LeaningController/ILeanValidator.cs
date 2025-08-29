using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.ProceduralMotion;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    public readonly struct LeanValidationResult
    {
        public readonly bool CanLean;
        public readonly float MaxLeanPercent;
        public readonly bool IsObstructed;

        public LeanValidationResult(bool canLean, float maxLeanPercent, bool isObstructed = false)
        {
            CanLean = canLean;
            MaxLeanPercent = maxLeanPercent;
            IsObstructed = isObstructed;
        }

        public static readonly LeanValidationResult Allow = new(true, 1f);
        public static readonly LeanValidationResult Deny = new(false, 0f);
    }

    public interface ILeanValidator
    {
        /// <summary>
        /// Checks if the character can lean to the given state.
        /// </summary>
        /// <param name="leanState">Target lean state (left, center, or right).</param>
        /// <param name="characterMotor">Motor of the character that is to be leaning.</param>
        /// <returns>Result of the validation.</returns>
        LeanValidationResult ValidateLean(LeanState leanState, ICharacterMotor characterMotor, LeanMotion leanMotion);
    }

    /// <summary>
    /// Validates the lean depending on character's speed and grounding status.
    /// </summary>
    public sealed class SpeedBasedLeanValidator : ILeanValidator
    {
        private readonly float _maxAllowedSpeed;

        public SpeedBasedLeanValidator(float maxAllowedSpeed) => _maxAllowedSpeed = maxAllowedSpeed;

        public LeanValidationResult ValidateLean(LeanState leanState, ICharacterMotor characterMotor, LeanMotion leanMotion)
        {
            if(leanState == LeanState.Center)
            {
                return LeanValidationResult.Allow;
            }

            if(characterMotor == null)
            {
                return LeanValidationResult.Deny;
            }

            bool speedValid = characterMotor.Velocity.magnitude <= _maxAllowedSpeed;
            bool isGrounded = characterMotor.IsGrounded;

            return speedValid && isGrounded
                ? LeanValidationResult.Allow 
                : LeanValidationResult.Deny;
        }
    }

    /// <summary>
    /// Validates the lean depending on the obstructions on the leaning side.
    /// (e.g If there is something so close, then lean is rejected).
    /// </summary>
    public sealed class ObstructionBasedLeanValidator : ILeanValidator
    {
        /// <summary>
        /// Obstruction mask of what blocks leaning.
        /// </summary>
        private readonly LayerMask _obstructionMask;

        /// <summary>
        /// Extra distance in addition to lean offset when checking for obstructions.
        /// </summary>
        private readonly float _obstructionPadding;

        /// <summary>
        /// Maximum distance that an obstruction close to us, so that we can lean.
        /// </summary>
        private readonly float _maxLeanObstructionCutoff;

        /// <summary>
        /// Radius of the spherecast, that we will be using for detecting an obstruction.
        /// </summary>
        private readonly float _spherecastRadius;

        private RaycastHit _raycastHit;

        private Transform _characterRoot;

        public ObstructionBasedLeanValidator(LayerMask obstructionMask, float obstructionPadding, float maxLeanObstructionCutoff, Transform characterRoot, float spherecastRadius = 0.25f)
        {
            _obstructionMask = obstructionMask;
            _obstructionPadding = obstructionPadding;
            _maxLeanObstructionCutoff = maxLeanObstructionCutoff;
            _spherecastRadius = spherecastRadius;
            _characterRoot = characterRoot;
        }

        public LeanValidationResult ValidateLean(LeanState leanState, ICharacterMotor characterMotor, LeanMotion leanMotion)
        {
            if (leanState == LeanState.Center)
            {
                return LeanValidationResult.Allow;
            }

            if (leanMotion == null)
            {
                return LeanValidationResult.Deny;
            }

            return ValidateObstruction(leanState, leanMotion);
        }

        /// <summary>
        /// Checks if there is any obstruction in the direction that we are leaning to.
        /// <br></br>
        /// If obstruction is found then it checks how much we can lean towards there.
        /// </summary>
        /// <param name="leanState">Target lean state.</param>
        /// <param name="leanMotion"></param>
        /// <param name="root">Root transform of the character.</param>
        /// <returns>
        /// <br></br>-if it can lean, 
        /// <br></br>-how much it can lean (0 min, 1 max), 
        /// <br></br>-if there was any obstruction.</returns>
        private LeanValidationResult ValidateObstruction(LeanState leanState, LeanMotion leanMotion)
        {
            Vector3 startPosition = leanMotion.transform.position;
            Vector3 targetPosition = CalculateTargetPosition(leanState, leanMotion);

            var ray = new Ray(startPosition, targetPosition - startPosition);
            float distance = Vector3.Distance(startPosition, targetPosition) + _obstructionPadding;

            if(PhysicsUtils.SphereCastOptimized(ray, _spherecastRadius, distance, out _raycastHit, _obstructionMask, _characterRoot))
            {
                float obstructionPercent = _raycastHit.distance / distance;
                bool canLean = obstructionPercent > _maxLeanObstructionCutoff;

                return new LeanValidationResult(canLean, obstructionPercent, true);
            }

            return LeanValidationResult.Allow;
        }

        /// <summary>
        /// Calculates the target position where the leaning will be in world coordinates.
        /// </summary>
        /// <param name="leanState">Target lean state.</param>
        private Vector3 CalculateTargetPosition(LeanState leanState, LeanMotion leanMotion)
        {
            float sideOffset = leanState == LeanState.Left ? -leanMotion.Data.LeanSideOffset : leanMotion.Data.LeanSideOffset;
            Vector3 localTargetPosition = new Vector3(sideOffset, -leanMotion.Data.LeanHeightOffset, 0f);
            return leanMotion.transform.TransformPoint(localTargetPosition);
        }
    }

    public sealed class CompositeLeanValidator : ILeanValidator
    {
        private readonly IReadOnlyList<ILeanValidator> _validators;

        public CompositeLeanValidator(params ILeanValidator[] validators) => _validators = validators;

        public LeanValidationResult ValidateLean(LeanState leanState, ICharacterMotor characterMotor, LeanMotion leanMotion)
        {
            float minLeanPercent = 1f;
            bool hasObstruction = false;

            foreach (var validator in _validators)
            {
                LeanValidationResult result = validator.ValidateLean(leanState, characterMotor, leanMotion);

                if (result.CanLean == false)
                {
                    return LeanValidationResult.Deny;
                }

                minLeanPercent = Mathf.Min(minLeanPercent, result.MaxLeanPercent);
                hasObstruction = hasObstruction | result.IsObstructed;
            }

            return new LeanValidationResult(true, minLeanPercent, hasObstruction);
        }
    }
}