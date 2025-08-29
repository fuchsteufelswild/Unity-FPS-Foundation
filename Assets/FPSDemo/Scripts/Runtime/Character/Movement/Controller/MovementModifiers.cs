using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    public sealed class MovementModifiers : IMovementModifiers
    {
        public CompositeValue SpeedModifier { get; private set; }
        public CompositeValue AccelerationModifier { get; private set; }
        public CompositeValue DecelerationModifier { get; private set; }

        public MovementModifiers(MovementControllerConfig controllerConfig)
        {
            SpeedModifier = new CompositeValue(controllerConfig.BaseSpeedMultiplier, null);
            AccelerationModifier = new CompositeValue(controllerConfig.BaseAcceleration, null);
            DecelerationModifier = new CompositeValue(controllerConfig.BaseDeceleration, null);
        }
    }
}