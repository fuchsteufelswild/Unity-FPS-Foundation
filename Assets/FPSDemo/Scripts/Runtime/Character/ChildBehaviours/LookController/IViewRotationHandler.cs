using Nexora.FPSDemo.Options;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    /// <summary>
    /// Handles pitch/yaw rotation using provided input.
    /// </summary>
    public interface IViewRotationHandler
    {
        /// <summary>
        /// Last calculated view angles.
        /// </summary>
        Vector2 ViewAngles { get; }

        /// <summary>
        /// Delta in the 'ViewAngles' in the <b>last</b> frame.
        /// </summary>
        Vector2 LastDelta { get; }

        /// <summary>
        /// Apply rotation using <paramref name="input"/>.
        /// </summary>
        /// <param name="input"></param>
        void ApplyRotation(Vector2 input);

        /// <summary>
        /// Resets angles to <paramref name="initialAngles"/>. If not provided then zero.
        /// </summary>
        /// <param name="initialAngles">Optional initial (default) angles.</param>
        void ResetAngles(Vector2? initialAngles = null);
        
        /// <summary>
        /// Sets the limits of rotation, denotes the pitch angle limit.
        /// </summary>
        /// <param name="pitchLimit">Pitch angle limit.</param>
        void SetPitchLimit(Vector2 pitchLimit);

        /// <summary>
        /// Adds <paramref name="addition"/> to view angles manually.
        /// </summary>
        void AddToViewAngles(Vector2 addition);
    }

    public sealed class ViewRotationHandler : IViewRotationHandler
    {
        private readonly Transform _pitchTransform;
        private readonly Transform _yawTransform;

        private Vector2 _viewAngles;
        private Vector2 _lastDelta;
        private Vector2 _pitchLimit;

        public Vector2 ViewAngles => _viewAngles;
        public Vector2 LastDelta => _lastDelta;

        public ViewRotationHandler(Transform pitchTransform, Transform yawTransform, Vector2 pitchLimit)
        {
            _pitchTransform = pitchTransform ?? throw new ArgumentNullException(nameof(pitchTransform));
            _yawTransform = yawTransform ?? throw new ArgumentNullException(nameof(yawTransform));
            SetPitchLimit(pitchLimit);
        }

        public void AddToViewAngles(Vector2 addition) => _viewAngles += addition;
        public void SetPitchLimit(Vector2 pitchLimit) => _pitchLimit = pitchLimit;

        public void ApplyRotation(Vector2 input)
        {
            Vector2 previousAngles = _viewAngles;

            float pitchInput = input.x * (InputOptions.Instance.InvertMouse ? 1f : -1f);
            _viewAngles.x = Mathf.Clamp(_viewAngles.x + pitchInput, _pitchLimit.x, _pitchLimit.y);

            _viewAngles.y += input.y;

            _lastDelta = _viewAngles - previousAngles;
        }

        public void UpdateTransforms(Vector2 additionalInput = default)
        {
            float pitchInput = Mathf.Clamp(additionalInput.x + _viewAngles.x, _pitchLimit.x, _pitchLimit.y);
            float yawInput = _viewAngles.y + additionalInput.y;

            _yawTransform.localRotation = Quaternion.Euler(0f, yawInput, 0f);
            _pitchTransform.localRotation = Quaternion.Euler(pitchInput, 0f, 0f);
        }

        public void ResetAngles(Vector2? initialAngles = null)
        {
            if(initialAngles.HasValue)
            {
                _viewAngles = initialAngles.Value;
            }
            else
            {
                _viewAngles = new Vector2(0f, _yawTransform.localEulerAngles.y);
            }

            _lastDelta = Vector2.zero;
            UpdateTransforms();
        }
    }

    /// <summary>
    /// Keeps track of additional input value which is used in combination with input provided.
    /// </summary>
    /// <remarks>
    /// Additional input value is independent from the input provided, it is freely set without any
    /// relation to mouse input.
    /// </remarks>
    public sealed class AdditiveViewRotationHandler : IViewRotationHandler
    {
        private readonly ViewRotationHandler _baseHandler;

        private Vector2 _additiveInput;

        public Vector2 AdditiveInput => _additiveInput;

        public Vector2 ViewAngles => _baseHandler.ViewAngles;
        public Vector2 LastDelta => _baseHandler.LastDelta;

        public AdditiveViewRotationHandler(Transform pitchTransform, Transform yawTransform, Vector2 pitchLimit)
            => _baseHandler = new ViewRotationHandler(pitchTransform, yawTransform, pitchLimit);

        public void AddToViewAngles(Vector2 addition) => _baseHandler.AddToViewAngles(addition);
        public void SetAdditiveInput(Vector2 additiveInput) => _additiveInput = additiveInput;
        public void SetPitchLimit(Vector2 pitchLimit) => _baseHandler.SetPitchLimit(pitchLimit);

        public void ApplyRotation(Vector2 input)
        {
            _baseHandler.ApplyRotation(input);
            _baseHandler.UpdateTransforms(_additiveInput);
        }

        public void ResetAngles(Vector2? initialAngles = null)
        {
            _baseHandler.ResetAngles(initialAngles);
            _additiveInput = Vector2.zero;
        }
    }
}