using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Keeps main configuration seettings for the movement controller.
    /// <br></br><br></br>
    /// Base values are for the normal state movement controller, they are later can be scaled up/down
    /// with other modifiers, like carrying a lot weight might reduce them or enchanting potion might surge them,
    /// or the terrain effectors etc..
    /// <br></br><br></br>
    /// Values regarding 'Step Cycle' are for calculating the footsteps of the character. To calculate when a real
    /// step is made, factoring in both movement and turning.
    /// </summary>
    [CreateAssetMenu(menuName = "Nexora/Movement/Movement Controller Config", fileName = "ControllerConfig_")]
    public sealed class MovementControllerConfig : ScriptableObject
    {
        [Title("Movement")]
        [Help("Base multiplier for the controller's main movement speed.")]
        [SerializeField, Range(0f, 10f)]
        private float _baseSpeedMultiplier = 1f;

        [Help("Base acceleration value for the controller's movement.")]
        [SerializeField, Range(1f, 100f)]
        private float _baseAcceleration = 8f;

        [Help("Base deceleration value for the controller's movement.")]
        [SerializeField, Range(1f, 100f)]
        private float _baseDeceleration = 10f;

        [Title("Step Cycle")]
        [Help("How fast is the transition between different state step lengths?" +
            " (Run state and Walk state can have different step lengths, this is the transition speed)")]
        [SerializeField, Range(0.1f, 10f)]
        private float _stepLerpSpeed = 1.5f;

        [Help("Length of the step length when turning.")]
        [SerializeField, Range(0f, 10f)]
        private float _turnStepLength = 0.8f;

        [Help("Maximum speed of step for turning around.")]
        [SerializeField, Range(0f, 10f)]
        private float _maxTurnStepSpeed = 1.75f;

        /// <summary>
        /// Base multiplier for the controller's main movement speed.
        /// </summary>
        public float BaseSpeedMultiplier => _baseSpeedMultiplier;

        /// <summary>
        /// Base acceleration value for the controller's movement.
        /// </summary>
        public float BaseAcceleration => _baseAcceleration;

        /// <summary>
        /// Base deceleration value for the controller's movement.
        /// </summary>
        public float BaseDeceleration => _baseDeceleration;

        /// <summary>
        /// How fast is the transition between different state step lengths?
        /// </summary>
        public float StepLerpSpeed => _stepLerpSpeed;

        /// <summary>
        /// Length of the step length when turning.
        /// </summary>
        public float TurnStepLength => _turnStepLength;

        /// <summary>
        /// Maximum velocity of step for turning around.
        /// </summary>
        public float MaxTurnStepSpeed => _maxTurnStepSpeed;
    }
}