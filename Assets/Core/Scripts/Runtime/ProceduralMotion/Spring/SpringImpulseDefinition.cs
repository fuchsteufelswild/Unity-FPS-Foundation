using System;
using UnityEngine;

namespace Nexora.Motion
{
    /// <summary>
    /// Represents a spring impulse that applies <see cref="Impulse"/> for <see cref="Duration"/> seconds.
    /// Has operator overloads to work with <see cref="Vector3"/> and <see langword="float"/>.
    /// </summary>
    /// <remarks>
    /// It is a data class, it just represents an impulse. Applying the force is not its responsibility.
    /// </remarks>
    [Serializable]
    public struct SpringImpulseDefinition
    {
        public const float DefaultImpulseDuration = 0.125f;
        public const float MinEffectiveDuration = 0.01f;
        
        [Tooltip("Impulse vector in 3D space.")]
        public Vector3 Impulse;

        [Tooltip("How long the impulse will be applied. (in seconds)")]
        [Range(0f, 1f)]
        public float Duration;

        /// <summary>
        /// Default spring impulse, has zero impulse.
        /// </summary>
        public static readonly SpringImpulseDefinition Default = new(Vector3.zero, DefaultImpulseDuration);

        public SpringImpulseDefinition(Vector3 impulse, float duration)
        {
            Impulse = impulse;
            Duration = Mathf.Max(duration, 0); // Can't be negative
        }

        public bool IsNegligible() => Duration < MinEffectiveDuration || Impulse.ApproximatelyEquals(Vector3.zero);

        /// <returns>A new <see cref="SpringImpulseDefinition"/> whose impulse is added by <paramref name="impulse"/>.</returns>
        public static SpringImpulseDefinition operator +(SpringImpulseDefinition springImpulse, Vector3 impulse)
        {
            springImpulse.Impulse += impulse;
            return springImpulse;
        }

        /// <returns>A new <see cref="SpringImpulseDefinition"/> whose impulse is scaled by <paramref name="scaler"/>.</returns>
        public static SpringImpulseDefinition operator *(SpringImpulseDefinition springImpulse, float scaler)
        {
            springImpulse.Impulse *= scaler;
            return springImpulse;
        }
    }

    /// <summary>
    /// Defines a <see cref="SpringImpulse"/> with specified delay.
    /// </summary>
    [Serializable]
    public struct DelayedSpringImpulseDefinition
    {
        [Tooltip("Spring impulse to be applied after delay.")]
        public SpringImpulseDefinition SpringImpulse;

        [Tooltip("Delay(in seconds) after the spring impulse is applied.")]
        [Range(0f, 5f)]
        public float Delay;

        /// <summary>
        /// Default spring impulse, zero impulse and no delay.
        /// </summary>
        public static readonly DelayedSpringImpulseDefinition Default = new(SpringImpulseDefinition.Default, 0f);

        public DelayedSpringImpulseDefinition(SpringImpulseDefinition springImpulse, float delay)
        {
            SpringImpulse = springImpulse;
            Delay = delay;
        }
    }

    /// <summary>
    /// Definition we can generate <see cref="SpringImpulseDefinition"/> from. 
    /// Has min/max impulse interval and duration, and possibly randomize all signs of the generated impulse.
    /// </summary>
    /// <remarks>
    /// Each time it generates an <see cref="SpringImpulseDefinition"/> or casted to <see cref="SpringImpulseDefinition"/>,
    /// it generates a whole new value. That is, not the same value it is always used. Think of it as a 
    /// generator class.
    /// </remarks>
    [Serializable]
    public struct RandomSpringImpulseDefinitionGenerator
    {
        [Tooltip("Minimum possible impulse.")]
        public Vector3 MinImpulse;
        [Tooltip("Maximum possible impulse.")]
        public Vector3 MaxImpulse;
        [Tooltip("Duration impulse is applied. (in seconds)")]
        public float Duration;
        [Tooltip("Should randomly invert each component sign(x,y,z).")]
        public bool InvertRandomly;

        public static readonly RandomSpringImpulseDefinitionGenerator Default = new(Vector3.zero, Vector3.zero);

        public RandomSpringImpulseDefinitionGenerator(
            Vector3 minImpulse, 
            Vector3 maxImpulse, 
            float duration = SpringImpulseDefinition.DefaultImpulseDuration, 
            bool invertRandomly = false)
        {
            MinImpulse = minImpulse;
            MaxImpulse = maxImpulse;
            Duration = duration;
            InvertRandomly = invertRandomly;
        }

        /// <summary>
        /// Casts impulse definition to impulse. 
        /// <inheritdoc cref="GenerateImpulse" path="/summary"/>
        /// </summary>
        /// <param name="randomImpulseDefinition"></param>
        public static implicit operator SpringImpulseDefinition(RandomSpringImpulseDefinitionGenerator randomImpulseDefinition)
            => randomImpulseDefinition.GenerateImpulse();

        /// <summary>
        /// Generates random impulse using the definition values.
        /// </summary>
        public SpringImpulseDefinition GenerateImpulse()
        {
            Vector3 randomImpulse = Nexora.MathUtils.GetRandomPoint(MinImpulse, MaxImpulse);
            
            if(InvertRandomly)
            {
                randomImpulse = randomImpulse.RandomizeSigns();
            }

            return new SpringImpulseDefinition(randomImpulse, Duration);
        }
    }
}
