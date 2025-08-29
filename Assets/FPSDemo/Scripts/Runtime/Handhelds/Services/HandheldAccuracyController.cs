using Nexora.FPSDemo.Movement;
using System;
using System.Collections;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Controls the accuracy property using modifier and base accuracy.
    /// </summary>
    public interface IAccuracyController : ICharacterBehaviour
    {
        /// <returns>Accuracy modifier of the controller.</returns>
        float GetAccuracyModifier();

        /// <summary>
        /// Sets the base accuracy to be <paramref name="accuracy"/>.
        /// </summary>
        void SetBaseAccuracy(float accuracy);
    }

    public sealed class NoOpAccuracyController : IAccuracyController
    {
        public GameObject gameObject => null;
        public Transform transform => null;
        public bool enabled { get => true; set { } }
        public float GetAccuracyModifier() => 1f;
        public void SetBaseAccuracy(float accuracy) { }
        public Coroutine StartCoroutine(IEnumerator routine) => null;
        public void StopCoroutine(Coroutine routine) { }
    }

    [RequireCharacterBehaviour(typeof(ICharacterMotor), typeof(IMovementController))]
    public class HandheldAccuracyController :
        CharacterBehaviour,
        IAccuracyController
    {
        [SerializeField]
        private AccuracyCalculator _accuracyCalculator;

        [SerializeField]
        private SmoothedFloat _smoothedAccuracy;

        private float _baseAccuracy = 1f;

        private ICharacterMotor _characterMotor;
        private IMovementController _movementController;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _characterMotor = parent.GetCC<ICharacterMotor>();
            _movementController = parent.GetCC<IMovementController>();
            _smoothedAccuracy.SetInitialValue(1f);
        }

        public void SetBaseAccuracy(float accuracy) => _baseAccuracy = accuracy;

        public float GetAccuracyModifier()
        {
            float targetAccuracy = _accuracyCalculator.CalculateTargetAccuracy(_characterMotor, _movementController, _baseAccuracy);

            return _smoothedAccuracy.Update(targetAccuracy, Time.fixedDeltaTime);
        }
    }

    [Serializable]
    public sealed class AccuracyCalculator
    {
        [Tooltip("How does being airborne affect the accuracy?")]
        [SerializeField, Range(0f, 5f)]
        private float _airborneAccuracyModifier = 0.5f;

        [Tooltip("How does crouching affect the accuracy?")]
        [SerializeField, Range(0f, 5f)]
        private float _crouchAccuracyModifier = 1.15f;

        [Tooltip("How does proning affect the accuracy?")]
        [SerializeField, Range(0f, 5f)]
        private float _proneAccuracyModifier = 1.25f;

        [Tooltip("Animation curve that is to get accuracy modifier depending on the speed.")]
        [SerializeField]
        private AnimationCurve _speedAccuracyCurve;

        public float CalculateTargetAccuracy(ICharacterMotor characterMotor, IMovementController controller, float baseAccuracy)
        {
            if (characterMotor == null)
            {
                return baseAccuracy;
            }

            float targetAccuracy = _speedAccuracyCurve.Evaluate(characterMotor.Velocity.magnitude);

            if (characterMotor.IsGrounded == false)
            {
                targetAccuracy *= _airborneAccuracyModifier;
            }

            if (characterMotor.IsGrounded && controller.ActiveStateType == MovementStateType.Crouch)
            {
                targetAccuracy *= _crouchAccuracyModifier;
            }

            if (characterMotor.IsGrounded && controller.ActiveStateType == MovementStateType.Prone)
            {
                targetAccuracy *= _proneAccuracyModifier;
            }

            return targetAccuracy * baseAccuracy;
        }
    }
}