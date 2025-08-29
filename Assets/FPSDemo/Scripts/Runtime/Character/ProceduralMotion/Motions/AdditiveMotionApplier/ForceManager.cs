using Nexora.Motion;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    /// <summary>
    /// Represents a force that can be evaluated at a given time.
    /// </summary>
    public interface IForceEvaluator
    {
        /// <param name="currentTime">Current game time.</param>
        /// <returns>Evaluated force for <paramref name="currentTime"/>.</returns>
        Vector3 Evaluate(float currentTime);

        /// <param name="currentTime">Current game time.</param>
        /// <returns>If the force is expired and should be removed.</returns>
        bool IsExpired(float currentTime);
    }

    /// <summary>
    /// Represents a constant force that is applied for a fixed amount of time.
    /// </summary>
    public sealed class ConstantForce : IForceEvaluator
    {
        private readonly Vector3 _force;
        private readonly float _expireTime;

        public ConstantForce(Vector3 force, float expireTime)
        {
            _force = force;
            _expireTime = expireTime;
        }

        public Vector3 Evaluate(float currentTime) => _force;
        public bool IsExpired(float currentTime) => currentTime > _expireTime;
    }

    /// <summary>
    /// Represents a force that is driven by an <see cref="AnimationCurve"/> in all 3 directions of x,y,z.
    /// </summary>
    public sealed class AnimationCurveForce : IForceEvaluator
    {
        private readonly AnimationCurve3D _animationCurve;
        private readonly float _startTime;
        private readonly float _endTime;

        public AnimationCurveForce(AnimationCurve3D animationCurve, float startTime)
        {
            _animationCurve = animationCurve;
            _startTime = startTime;
            _endTime = _startTime + animationCurve.Duration;
        }

        public Vector3 Evaluate(float currentTime) => _animationCurve.Evaluate(currentTime - _startTime);
        public bool IsExpired(float currentTime) => currentTime > _endTime;
    }

    public sealed class ForceManager
    {
        public const float MinimumAnimationCurveDuration = 0.01f;

        private readonly List<IForceEvaluator> _forces;

        public int ActiveForceCount => _forces.Count;
        public bool HasActiveForces => ActiveForceCount > 0;

        public ForceManager(int initialCapacity = 2) => _forces = new List<IForceEvaluator>(initialCapacity);

        public void Clear() => _forces.Clear();

        /// <summary>
        /// Adds a constant force that is applied for <paramref name="duration"/>.
        /// </summary>
        /// <param name="force">Force to apply.</param>
        /// <param name="duration">Duration the force is applied for.</param>
        /// <param name="currentTime">Current game time.</param>
        public void AddConstantForce(Vector3 force, float duration, float currentTime)
        {
            if(force == Vector3.zero)
            {
                return;
            }

            var constantForce = new ConstantForce(force, currentTime + duration);
            _forces.Add(constantForce);
        }

        /// <summary>
        /// Adds a force that is driven by <see cref="AnimationCurve"/> in all 3 directions.
        /// </summary>
        public void AddAnimationCurveForce(AnimationCurve3D animationCurve, float currentTime)
        {
            if(animationCurve.Duration < MinimumAnimationCurveDuration)
            {
                return;
            }

            var animationCurveForce = new AnimationCurveForce(animationCurve, currentTime);
            _forces.Add(animationCurveForce);
        }

        /// <summary>
        /// Evaluates all forces and returns the accumulated result and removes the forces that are expired.
        /// </summary>
        /// <param name="currentTime">Current game time.</param>
        /// <returns>Accumulated force.</returns>
        public Vector3 EvaluateAndCleanup(float currentTime)
        {
            if(_forces.IsEmpty())
            {
                return Vector3.zero;
            }

            Vector3 totalForce = Vector3.zero;

            for(int i = _forces.Count - 1; i >= 0; i--)
            {
                IForceEvaluator force = _forces[i];

                if(force.IsExpired(currentTime))
                {
                    _forces.RemoveAt(i);
                }
                else
                {
                    totalForce += force.Evaluate(currentTime);
                }
            }

            return totalForce;
        }
    }

    public static class ForceManagerExtensions
    {
        public static void AddSpringImpulse(
            this ForceManager forceManager, 
            SpringImpulseDefinition springImpulse, 
            float scale, 
            float currentTime)
        {
            if(springImpulse.IsNegligible())
            {
                return;
            }

            Vector3 scaledForce = springImpulse.Impulse * scale;
            forceManager.AddConstantForce(scaledForce, springImpulse.Duration, currentTime);
        }
    }
}