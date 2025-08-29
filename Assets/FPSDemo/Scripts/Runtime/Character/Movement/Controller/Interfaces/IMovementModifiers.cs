namespace Nexora.FPSDemo.Movement
{
    public interface IMovementModifiers
    {
        /// <summary>
        /// Value for the speed multiplier affected by all the factors of environment, status,
        /// inventory etc.
        /// </summary>
        CompositeValue SpeedModifier { get; }

        /// <summary>
        /// Value for the acceleration multiplier affected by all the factors of environment, status,
        /// inventory etc.
        /// </summary>
        CompositeValue AccelerationModifier { get; }

        /// <summary>
        /// Value for the decelereation multiplier affected by all the factors of environment, status,
        /// inventory etc.
        /// </summary>
        CompositeValue DecelerationModifier { get; }
    }
}