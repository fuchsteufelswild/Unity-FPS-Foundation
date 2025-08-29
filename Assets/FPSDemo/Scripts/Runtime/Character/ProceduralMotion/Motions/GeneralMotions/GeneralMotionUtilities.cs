using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    /// <summary>
    /// Controls the state of a simple motion, responsible for starting/ticking/stopping motion
    /// and calculating the position/rotation.
    /// </summary>
    public class GeneralMotionStateController
    {
        protected readonly GeneralMotionPlayer _animationPlayer;
        protected readonly GeneralMotionCurveEvaluator _curveEvaluator;

        public bool IsMotionActive => _animationPlayer.IsPlaying;
        public float ElapsedAnimationTime => _animationPlayer.ElapsedTime;

        public GeneralMotionStateController(GeneralMotionPlayer animationPlayer, GeneralMotionCurveEvaluator curveEvaluator)
        {
            _animationPlayer = animationPlayer;
            _curveEvaluator = curveEvaluator;
        }

        public void StartAnimation(float speedFactor) => _animationPlayer.StartAnimation(speedFactor);

        /// <summary>
        /// Ticks position and rotation animations one frame further. Stops the animation if completed.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="motionData"></param>
        /// <returns>Position in local space, rotation, and motion states.</returns>
        public (Vector3 position, Vector3 rotation, bool positionContinue, bool rotationContinue) Tick(float deltaTime, Transform targetTransform, CurveData motionData)
        {
            if (_animationPlayer.IsPlaying == false)
            {
                return (Vector3.zero, Vector3.zero, false, false);
            }

            float speedFactor = _animationPlayer.SpeedFactor;

            var (position, positionContinue) = _curveEvaluator.EvaluatePosition(motionData, targetTransform, _animationPlayer.ElapsedTime, speedFactor);
            var (rotation, rotationContinue) = _curveEvaluator.EvaluateRotation(motionData, _animationPlayer.ElapsedTime, speedFactor);

            _animationPlayer.Tick(deltaTime);

            bool isComplete = positionContinue == false && rotationContinue == false;

            if (isComplete)
            {
                _animationPlayer.StopAnimation();
            }

            return (position, rotation, positionContinue, rotationContinue);
        }
    }

    public class GeneralMotionPlayer
    {
        private float _elapsedTime;
        private float _speedFactor;
        private bool _isPlaying;

        public float ElapsedTime => _elapsedTime;
        public float SpeedFactor => _speedFactor;
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Starts an animation, resets the time and sets the speed factor.
        /// </summary>
        public void StartAnimation(float speedFactor)
        {
            _elapsedTime = 0f;
            _speedFactor = Mathf.Max(0f, speedFactor);
            _isPlaying = _speedFactor > 0f;
        }

        /// <summary>
        /// Forwards animation one frame further.
        /// </summary>
        /// <returns>Current elapsed time.</returns>
        public float Tick(float deltaTime)
        {
            if (_isPlaying == false)
            {
                return _elapsedTime;
            }

            _elapsedTime += deltaTime;
            return _elapsedTime;
        }

        /// <summary>
        /// Stops the animation, but keep elapsed time as it is.
        /// </summary>
        public void StopAnimation()
        {
            _isPlaying = false;
            _speedFactor = 0f;
        }

        /// <summary>
        /// Resets to inactive state with a very high time value.
        /// </summary>
        /// <param name="resetTime">Very high time value.</param>
        public void ResetToInactive(float resetTime)
        {
            _elapsedTime = resetTime;
            _isPlaying = false;
            _speedFactor = 0f;
        }

        /// <summary>
        /// Checks if any of the of the animations active (position or rotation).
        /// </summary>
        /// <param name="positionAnimationDuration">Length of the position animation.</param>
        /// <param name="rotationAnimationDuration">Length of the rotation animation.</param>
        /// <returns>If animation should continue.</returns>
        public bool ShouldContinueAnimation(float positionAnimationDuration, float rotationAnimationDuration)
            => (ElapsedTime < positionAnimationDuration) || (ElapsedTime < rotationAnimationDuration);
    }

    public class GeneralMotionCurveEvaluator
    {
        private readonly GeneralMotionRandomizationManager _randomizationManager;

        public GeneralMotionCurveEvaluator(GeneralMotionRandomizationManager randomizationManager = null)
        {
            _randomizationManager = randomizationManager;
        }

        public (Vector3 position, bool shouldContinue) EvaluatePosition(CurveData motionData, Transform targetTransform, float elapsedTime, float speedFactor = 1f)
        {
            bool shouldContinue = motionData.PositionCurves.Duration > elapsedTime;

            if(shouldContinue == false)
            {
                return (Vector3.zero, false);
            }

            Vector3 position = motionData.PositionCurves.Evaluate(elapsedTime) * speedFactor;
            position = targetTransform.InverseTransformVector(position);
            position = _randomizationManager?.ApplyToPosition(position) ?? position;

            return (position, true);
        }

        public (Vector3 rotation, bool shouldContinue) EvaluateRotation(CurveData motionData, float elapsedTime, float speedFactor = 1f)
        {
            bool shouldContinue = motionData.RotationCurves.Duration > elapsedTime;

            if (shouldContinue == false)
            {
                return (Vector3.zero, false);
            }

            Vector3 rotation = motionData.RotationCurves.Evaluate(elapsedTime) * speedFactor;
            rotation = _randomizationManager?.ApplyToRotation(rotation) ?? rotation;

            return (rotation, true);
        }
    }

    public class GeneralMotionRandomizationManager
    {
        [Flags]
        public enum ApplyTransformComponentParts
        {
            None = 0,
            X = 1 << 0,
            Y = 1 << 1,
            Z = 1 << 2,
            All = X | Y | Z
        }

        private IRandomizationStrategy _randomizationStrategy;
        private ApplyTransformComponentParts _positionPartsToApply;
        private ApplyTransformComponentParts _rotationPartsToApply;

        public GeneralMotionRandomizationManager(
            IRandomizationStrategy randomizationStrategy, 
            ApplyTransformComponentParts positionPartsToApply,
            ApplyTransformComponentParts rotationPartsToApply)
        {
            _randomizationStrategy = randomizationStrategy;
            _positionPartsToApply = positionPartsToApply;
            _rotationPartsToApply = rotationPartsToApply;
        }

        /// <summary>
        /// Generates the next random factor.
        /// </summary>
        public float GenerateNextFactor() => _randomizationStrategy.GenerateNextFactor();
        public void Reset() => _randomizationStrategy.Reset();

        public Vector3 ApplyToPosition(Vector3 position)
        {
            float randomFactor = _randomizationStrategy.CurrentRandomFactor;

            if (EnumFlagsComparer.HasFlag(_positionPartsToApply, ApplyTransformComponentParts.X))
            {
                position.x *= randomFactor;
            }

            if (EnumFlagsComparer.HasFlag(_positionPartsToApply, ApplyTransformComponentParts.Y))
            {
                position.y *= randomFactor;
            }

            if (EnumFlagsComparer.HasFlag(_positionPartsToApply, ApplyTransformComponentParts.Z))
            {
                position.z *= randomFactor;
            }

            return position;
        }

        public Vector3 ApplyToRotation(Vector3 rotation)
        {
            float randomFactor = _randomizationStrategy.CurrentRandomFactor;

            if (EnumFlagsComparer.HasFlag(_rotationPartsToApply, ApplyTransformComponentParts.X))
            {
                rotation.x *= randomFactor;
            }

            if (EnumFlagsComparer.HasFlag(_rotationPartsToApply, ApplyTransformComponentParts.Y))
            {
                rotation.y *= randomFactor;
            }

            if (EnumFlagsComparer.HasFlag(_rotationPartsToApply, ApplyTransformComponentParts.Z))
            {
                rotation.z *= randomFactor;
            }

            return rotation;
        }
    }

    public interface IRandomizationStrategy
    {
        /// <summary>
        /// Gets the current random factor.
        /// </summary>
        float CurrentRandomFactor { get; }

        /// <summary>
        /// Generates the next randomization factor.
        /// </summary>
        float GenerateNextFactor();

        /// <summary>
        /// Resets the randomization back to the initial state.
        /// </summary>
        void Reset();
    }

    public sealed class AlternatingRandomizationStrategy : IRandomizationStrategy
    {
        private readonly float _initialRandomFactor;
        private readonly float _positiveRandomFactor;
        private readonly float _negativeRandomFactor;

        private float _currentRandomFactor;

        public float CurrentRandomFactor => _currentRandomFactor;

        public AlternatingRandomizationStrategy(
            float initialRandomFactor = -1f, float positiveRandomFactor = 1f, float negativeRandomFactor = -1f)
        {
            _initialRandomFactor = initialRandomFactor;
            _positiveRandomFactor = positiveRandomFactor;
            _negativeRandomFactor = negativeRandomFactor;
        }

        public float GenerateNextFactor()
        {
            _currentRandomFactor = _currentRandomFactor > 0f
                ? _negativeRandomFactor
                : _positiveRandomFactor;

            return _currentRandomFactor;
        }

        public void Reset() => _currentRandomFactor = _initialRandomFactor;
    }

}