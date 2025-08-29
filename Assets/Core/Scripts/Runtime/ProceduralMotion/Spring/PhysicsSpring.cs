using System;
using UnityEngine;

namespace Nexora.Motion
{
    /// <summary>
    /// Type of the movement pattern.
    /// </summary>
    public enum SpringType
    {
        /// <summary>
        /// Slow, smooth movement to the target.
        /// </summary>
        Gentle,

        /// <summary>
        /// Quick, bouncy movement with oscillations.
        /// </summary>
        Bouncy,

        /// <summary>
        /// User-defined damping and spring strength.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Physically-based spring motion with a damped harmonic oscillation. 
    /// Think of it as you had a spring of length x(initially 0) and in a moment you make the spring's length y,
    /// naturally the spring in this state will be like pushed and will want to go to its resting state for its new length
    /// which is <see cref="_targetPosition"/>(The point the spring's tip would be if there would be no force applied at all).
    /// The spring goes on with the force and depending on its characteristics it can overshoot, gently or abruptly move etc..<br></br><br></br>
    /// Uses max step size and partition the movement into steps to make the movement smoother (check <see cref="Evaluate(float)"/>).
    /// Can be used to animate any type of movement that can oscillate, like movement of hands in an FP character, UI animations, camera movements etc.
    /// Uses <see cref="SpringSettings"/> to define the characteristics of the spring.
    /// </summary>
    /// <remarks>
    /// Takes the mass as 1. <br></br>
    /// May specifically define your own classes deriving
    /// from this class and handle arithmetic operations and equilibrium condition.
    /// Check <see cref="PhysicsSpring"/>, <see cref="PhysicsSpring2D"/>, <see cref="PhysicsSpring3D"/>.
    /// </remarks>
    /// <typeparam name="T">Type of the spring, like if it is one-dimensional then float, 2D it is Vector2 etc.</typeparam>
    public abstract class PhysicsSpringCore<T>
        where T : struct
    {
        /// <summary>
        /// Specifically 1/61f, so to not strictly match with typical screen
        /// refresh rate of 60Hz to avoid visual artifacts. <br></br>
        /// As screen refresh rate and physics refresh rate slowly desync from each other,
        /// this desync spreads the inconsistincies more evenly over time resulting in a more
        /// natural effect.
        /// </summary>
        private const float MaxStepSize = 1 / 61f;

        /// <summary>
        /// Helper used for math operations performed on type T of the value spring works with (float, Vector2, Vector3 etc.)
        /// </summary>
        private static ISpringMathOperations<T> mathOperations;

        private SpringSettings _settings;

        /// <summary>
        /// Target position mass is moving to.
        /// </summary>
        private T _targetPosition;

        /// <summary>
        /// Current position of the mass.
        /// </summary>
        private T _currentPosition;

        /// <summary>
        /// The rate at which the mass' position is changing.
        /// </summary>
        private T _currentVelocity;

        /// <summary>
        /// The rate at which the mass' velocity is changing.
        /// </summary>
        private T _currentAcceleration;

        /// <summary>
        /// Is mass is at rest, not moving by the spring?
        /// </summary>
        private bool _isAtRest;

        public SpringSettings Settings => _settings;
        public T CurrentPosition => _currentPosition;
        public T CurrentVelocity => _currentVelocity;
        public T CurrentAcceleration => _currentAcceleration;
        public bool IsAtRest => _isAtRest;

        /// <summary>
        /// The epsilon used to determine if the spring is reached the equilibrium state(infinitesimal moving).
        /// </summary>
        protected abstract float Precision { get; }
        protected abstract bool HasReachedEquilibrium { get; }
        
        /// <summary>
        /// Default constructor for a spring, initializes the spring with default spring settings.
        /// <br></br><see cref="SpringSettings.Default"/> => <inheritdoc cref="SpringSettings.Default" path="/summary"/>
        /// </summary>
        public PhysicsSpringCore() : this(SpringSettings.Default) { }

        /// <summary>
        /// Initializes the spring. Sets all values to default and attains the settings for usage.
        /// </summary>
        public PhysicsSpringCore(SpringSettings settings) 
        { 
            _settings = settings;
            Reset();
        }

        /// <summary>
        /// Resets the spring values.
        /// <br></br>! Spring settings is untouched !
        /// </summary>
        public void Reset()
        {
            _isAtRest = true;
            _targetPosition = default;
            _currentPosition = default;
            _currentVelocity = default;
            _currentAcceleration = default;
        }

        /// <summary>
        /// Changes the spring settings without interrupting the movement.
        /// After the changing the settings, spring will not be resting.
        /// </summary>
        public void ChangeSpringSettings(SpringSettings settings)
        {
            _settings = settings;
            _isAtRest = false;
        }

        /// <summary>
        /// Sets the target position of the spring and starts the movement.
        /// </summary>
        /// <param name="targetPosition">The new equilibrium position for the spring.</param>
        public void SetTargetPosition(T targetPosition)
        {
            _targetPosition = targetPosition;
            _isAtRest = false;
        }

        /// <summary>
        /// Moving the spring one frame further, simulating damped harmonic motion.
        /// Divides the movement into smaller steps for smoother motion and using spring
        /// equations calculates the new position of the spring.
        /// </summary>
        /// <remarks>
        /// Mass of the object is taken 1 for simplicity in all computations.
        /// </remarks>
        public T Evaluate(float deltaTime)
        {
            if(_isAtRest)
            {
                return default;
            }

            mathOperations ??= SpringMathOperationsFactory.Create<T>();

            T pos = _currentPosition;
            T vel = _currentVelocity;
            T acc = _currentAcceleration;

            float totalStepSize = deltaTime * _settings.Speed;
            float singleStepSize = Mathf.Min(MaxStepSize, totalStepSize);
            int stepCount = (int)Mathf.Round(totalStepSize / singleStepSize);

            for (int i = 0; i < stepCount; i++)
            {
                // Delta time is correctly required, so that we will not underuse the step.
                // If totalStepSize / singleStepSize are not evenly divided,
                // we use a slightly higher value to make up the missing part.
                // For instance, if the result is 6.5f, we take whole 5 steps one after another
                // and after that, take 1.5f steps for the last step.
                float dt = i == stepCount - 1 
                    ? totalStepSize - i * singleStepSize
                    : singleStepSize;

                // Standard motion equation,
                // How much a mass moves with the given velocity and acceleration
                // pos += vel * dt + acc * dt^2 / 2
                pos = Add(pos, 
                          Add(
                              Mul(vel, dt), 
                              Mul(acc, dt * dt * 0.5f)
                              )
                          );

                // Calculating new acceleration using spring forces
                // Hookes Law(F = -kx) + Damping Force(F = -cv)
                // x = pos - _targetPosition, k = springStrength
                // c = dampingRatio, v = vel
                T calculatedAcc = Add(
                    Mul(
                        Sub(pos, _targetPosition), 
                        -_settings.SpringStrength), 
                    Mul(vel, -_settings.DampingRatio));

                // Calculating new velocity using both start and end acceleration. v += a * dt
                // a = (acceleration beginning of the step + acceleration end of the step) / 2
                vel = Add(vel, Mul(Add(acc, calculatedAcc), dt * 0.5f));
                // Updating acceleration to the end of the time step
                acc = calculatedAcc;
            }

            UpdateValues();

            return _currentPosition;

            void UpdateValues()
            {
                _currentPosition = pos;
                _currentVelocity = vel;
                _currentAcceleration = acc;
                if (HasReachedEquilibrium)
                {
                    _isAtRest = true;
                }
            }
        }

        #region Arithmetics

        private T Mul(T left,  float right)
            => mathOperations.Multiply(left, right);

        private T Add(T left, T right)
            => mathOperations.Add(left, right);

        private T Sub(T left, T right)
            => mathOperations.Add(left, mathOperations.Negate(right));
        #endregion
    }

    /// <inheritdoc/>
    public sealed class PhysicsSpring
        : PhysicsSpringCore<float>
    {
        protected override float Precision => Mathf.Epsilon;
        protected override bool HasReachedEquilibrium => Mathf.Abs(CurrentAcceleration) < Precision;

        public PhysicsSpring() : base() { }
        public PhysicsSpring(SpringSettings settings) : base(settings) { }
    }

    /// <inheritdoc/>
    public sealed class PhysicsSpring2D
        : PhysicsSpringCore<Vector2>
    {
        protected override float Precision => 0.01f;
        protected override bool HasReachedEquilibrium 
            => (Mathf.Abs(CurrentAcceleration.x) < Precision
             && Mathf.Abs(CurrentAcceleration.y) < Precision);

        public PhysicsSpring2D() : base() { }
        public PhysicsSpring2D(SpringSettings settings) : base(settings) { }
    }

    /// <inheritdoc/>
    public sealed class PhysicsSpring3D
        : PhysicsSpringCore<Vector3>
    {
        protected override float Precision => 0.00001f;
        protected override bool HasReachedEquilibrium 
            => (Mathf.Abs(CurrentAcceleration.x) < Precision
             && Mathf.Abs(CurrentAcceleration.y) < Precision
             && Mathf.Abs(CurrentAcceleration.z) < Precision);

        public PhysicsSpring3D() : base() { }
        public PhysicsSpring3D(SpringSettings settings) : base(settings) { }
    }

    /// <summary>
    /// Settings for <see cref="PhysicsSpring{T}"/>, defines damping, strength of the spring, mass, and response speed.
    /// Provides a <see langword="static readonly"/> default spring to be used as a general purpose spring (<see cref="Default"/>).
    /// </summary>
    [Serializable]
    public struct SpringSettings
    {
        [Tooltip("(Resistance) How quickly oscillations fade (0 = bouncy, 100 = stiff).")]
        [Range(0f, 100f)]
        public float DampingRatio;

        [Tooltip("How strongly the spring moves toward the target.")]
        [Range(0f, 1000f)]
        public float SpringStrength;

        [Tooltip("Speed multiplier for the spring motion.")]
        [Range(0f, 10f)]
        public float Speed;

        /// <summary>
        /// Default spring settings for a general spring.
        /// </summary>
        public static readonly SpringSettings Default = new(10f, 120f, 1f);

        public SpringSettings(float dampingRatio, float springStrength, float speed)
        {
            DampingRatio = dampingRatio;
            SpringStrength = springStrength;
            Speed = speed;
        }
    }
}
