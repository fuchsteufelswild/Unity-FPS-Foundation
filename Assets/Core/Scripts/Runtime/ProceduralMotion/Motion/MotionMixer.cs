using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.Motion
{
    /// <summary>
    /// Mixer for collection of <see cref="IMotion"/>. Uses enum to define its update
    /// mode of various types <see cref="BlendUpdateMode"/> which determines what time should mixing take place.
    /// Keeps track of the motions it owns and updates their respective parent blend weights.
    /// </summary>
    /// <remarks>
    /// Some of the update modes given provide a smooth motion update, using linear interpolation. These are
    /// provided specifically with fixed time updates, by using a clever technique there are no artifacts visible
    /// when main engine updates faster than the physics engine.
    /// </remarks>
    [DefaultExecutionOrder(ExecutionOrder.LatePresentation)]
    public sealed class MotionMixer :
        MonoBehaviour,
        IMotionMixer
    {
        /// <summary>
        /// Determines when the mixing should occur. And the type of it (Immediate or Lerp)
        /// </summary>
        private enum BlendUpdateMode
        {
            /// <summary>
            /// Immediately mixes motions without any smoothing in Unity's <b>Update</b>.
            /// </summary>
            Update = 0,

            /// <summary>
            /// Immediately mixes motions without any smoothing in Unity's <b>LateUpdate</b>.
            /// </summary>
            LateUpdate = 1,

            /// <summary>
            /// Immediately mixes motions without any smoothing in Unity's <b>FixedUpdate</b>.
            /// </summary>
            FixedUpdate = 2,

            /// <summary>
            /// Interpolates from current values to target values in <b>Update</b> using <see cref="FixedTimeStepInterpolation"/>.
            /// Then in <b>FixedUpdate</b> calls <see cref="SmoothBlendMotions(float)"/>.
            /// </summary>
            /// <remarks>
            /// Check <see cref="FixedTimeStepInterpolation"/> and <see cref="SmoothBlendMotions(float)"/> for the main idea.
            /// </remarks>
            FixedLerpUpdate = 3,

            /// <summary>
            /// Interpolates from current values to target values in <b>Update</b> using <see cref="FixedTimeStepInterpolation"/>.
            /// Then in <b>FixedUpdate</b> calls <see cref="SmoothBlendMotions(float)"/>.
            /// </summary>
            /// <remarks>
            /// Check <see cref="FixedTimeStepInterpolation"/> and <see cref="SmoothBlendMotions(float)"/> for the main idea.
            /// </remarks>
            FixedLerpLateUpdate = 4,
        }

        [Tooltip("At what frame call the motions are applied (Update, LateUpdate, FixedUpdate etc.).")]
        [SerializeField]
        private BlendUpdateMode _updateMode;

        [Tooltip("Transform mixed motions are applied to.")]
        [SerializeField, InLineEditor]
        private Transform _target;

        [Title("Offset")]
        [Tooltip("Pivot offset applied when mixing motions.")]
        [SerializeField]
        private Vector3 _baseOffset;

        [Tooltip("Position offset applied when mixing motions.")]
        [SerializeField]
        private Vector3 _positionOffset;

        [Tooltip("Rotation offset applied when mixing motions.")]
        [SerializeField]
        private Vector3 _rotationOffset;

        private Vector3 _currentPosition = Vector3.zero;
        private Quaternion _currentRotation = Quaternion.identity;
        private Vector3 _targetPosition = Vector3.zero;
        private Quaternion _targetRotation = Quaternion.identity;

        private float _blendWeightMultiplier = 1f;

        private bool _isPlaying;

        private MotionContainer _motions = new();
        private MotionBlender _motionBlender;

        private void Awake()
        {
            _isPlaying = _target != null;
            _motionBlender = new MotionBlender(_motions);
        }

        public float BlendWeight
        {
            get => _blendWeightMultiplier;
            set
            {
                value = Mathf.Clamp01(value);
                _blendWeightMultiplier = value;

                _motions.UpdateMixerBlendWeightMultiplier(_blendWeightMultiplier);
            }
        }

        public Transform Target => _target;

        /// <summary>
        /// Offset applied before mixing motions. This is like modifying the base (0, 0, 0)
        /// </summary>
        public Vector3 BaseOffset => _baseOffset;

        /// <summary>
        /// Offset added to the target position after motions are mixed.
        /// </summary>
        public Vector3 PositionOffset => _positionOffset;

        /// <summary>
        /// Offset added to the target rotation after motions are mixed.
        /// </summary>
        public Vector3 RotationOffset => _rotationOffset;

        /// <summary>
        /// <inheritdoc cref="IMotionMixer.AddMotion(IMotion)" path="/summary"/>
        /// Sets the mixer blend weight for the <paramref name="motion"/>.
        /// </summary>
        public void AddMotion(IMotion motion)
        {
            if (_motions.AddMotion(motion))
            {
                motion.MixerBlendWeight = _blendWeightMultiplier;
            }
        }

        public void RemoveMotion(IMotion motion)
            => _motions.RemoveMotion(motion);

        T IMotionMixer.GetMotion<T>() => _motions.GetMotion<T>();

        bool IMotionMixer.TryGetMotion<T>(out T motion) => _motions.TryGetMotion(out motion);

        public Vector3 InverseTransformVector(Vector3 worldVector)
            => Target.InverseTransformVector(worldVector);

        public void ConfigureMixer(Transform target, Vector3 baseOffset, Vector3 positionOffset, Vector3 rotationOffset)
        {
            _target = target;
            _baseOffset = baseOffset;
            _positionOffset = positionOffset;
            _rotationOffset = rotationOffset;
            _isPlaying = target != null;
        }

        private void Update()
        {
            if (_isPlaying == false || _target == null)
            {
                return;
            }

            switch (_updateMode)
            {
                case BlendUpdateMode.Update:
                    ImmediateBlendMotions(Time.deltaTime);
                    break;
                case BlendUpdateMode.FixedLerpUpdate:
                    FixedTimeStepInterpolation();
                    return;
            }
        }

        private void FixedUpdate()
        {
            if (_isPlaying == false || _target == null)
            {
                return;
            }

            switch (_updateMode)
            {
                case BlendUpdateMode.FixedUpdate:
                    ImmediateBlendMotions(Time.fixedDeltaTime);
                    break;
                case BlendUpdateMode.FixedLerpUpdate:
                case BlendUpdateMode.FixedLerpLateUpdate:
                    SmoothBlendMotions(Time.fixedDeltaTime);
                    break;
            }
        }

        private void LateUpdate()
        {
            if (_isPlaying == false || _target == null)
            {
                return;
            }

            switch (_updateMode)
            {
                case BlendUpdateMode.LateUpdate:
                    ImmediateBlendMotions(Time.deltaTime);
                    break;
                case BlendUpdateMode.FixedLerpLateUpdate:
                    FixedTimeStepInterpolation();
                    break;
            }
        }

        /// <summary>
        /// Performs fixed-timestep interpolation for smooth movement between physics frames.
        /// Sets object's local position/rotation to: 
        /// <br></br>->If we are in the same frame, then interpolates from current to target, if not just snaps to target.
        /// Smooths out the values for the upcoming update in <see cref="FixedUpdate"/>.
        /// </summary>
        /// <remarks>
        /// Used to avoid stuttering when visual FPS is higher than physics FPS.
        /// </remarks>
        private void FixedTimeStepInterpolation()
        {
            float delta = Time.time - Time.fixedTime;
            bool stillInSameFrame = delta < Time.fixedDeltaTime;

            if (stillInSameFrame)
            {
                float t = delta / Time.fixedDeltaTime; // 0-1 interpolation factor
                Vector3 targetPosition = Vector3.Lerp(_currentPosition, _targetPosition, t);
                Quaternion targetRotation = Quaternion.Lerp(_currentRotation, _targetRotation, t);
                _target.SetLocalPositionAndRotation(targetPosition, targetRotation);
            }
            else
            {
                _target.SetLocalPositionAndRotation(_targetPosition, _targetRotation);
            }
        }

        private void ImmediateBlendMotions(float deltaTime)
        {
            var (targetPosition, targetRotation) =
                _motionBlender.BlendMotions(deltaTime, _baseOffset, _positionOffset, _rotationOffset);

            _target.SetLocalPositionAndRotation(targetPosition, targetRotation);
        }

        private void SmoothBlendMotions(float deltaTime)
        {
            var (newTargetPosition, newTargetRotation) =
                _motionBlender.BlendMotions(deltaTime, _baseOffset, _positionOffset, _rotationOffset);

            _currentPosition = _targetPosition;
            _currentRotation = _targetRotation;
            _targetPosition = newTargetPosition;
            _targetRotation = newTargetRotation;
        }

        /// <summary>
        /// Container to keep track of motions the <see cref="MotionMixer"/> controls,
        /// and also the type-motion cache to prevent same motion type added twice.
        /// </summary>
        /// <remarks>
        /// Can be used as a <see cref="IEnumerable"/> in <see langword="foreach"/> loops.
        /// </remarks>
        private sealed class MotionContainer
            : IEnumerable<IMotion>
        {
            private readonly List<IMotion> _motions = new();
            private readonly Dictionary<Type, IMotion> _motionTypeCache = new();

            public void UpdateMixerBlendWeightMultiplier(float newBlendWeightMultiplier)
            {
                foreach (var motion in _motions)
                {
                    motion.MixerBlendWeight = newBlendWeightMultiplier;
                }
            }

            /// <summary>
            /// Adds motion type to the cache and if it wasn't exist earlier, adds it to the list.
            /// </summary>
            /// <returns>If add operation succesful.</returns>
            public bool AddMotion(IMotion motion)
            {
                if (motion == null)
                {
                    return false;
                }

                var motionType = motion.GetType();
                if (_motionTypeCache.ContainsKey(motionType) == false)
                {
                    _motionTypeCache.Add(motionType, motion);
                    _motions.Add(motion);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Removes the motion from the list and from the cache.
            /// </summary>
            public void RemoveMotion(IMotion motion)
            {
                if (motion == null)
                {
                    return;
                }

                if (_motionTypeCache.Remove(motion.GetType()))
                {
                    _motions.Remove(motion);
                }
            }

            /// <summary>
            /// Gets motion of the type <typeparamref name="T"/>.
            /// </summary>
            /// <typeparam name="T">Derived type of the motion.</typeparam>
            public T GetMotion<T>()
                where T : class, IMotion
            {
                if (_motionTypeCache.TryGetValue(typeof(T), out var motion))
                {
                    return (T)motion;
                }

                return null;
            }

            /// <summary>
            /// Gets motion of the type <typeparamref name="T"/>.
            /// </summary>
            /// <typeparam name="T">Derived type of the motion.</typeparam>
            public bool TryGetMotion<T>(out T motion)
                where T : class, IMotion
            {
                if (_motionTypeCache.TryGetValue(typeof(T), out var mixedMotion) == false)
                {
                    motion = null;
                    return false;
                }

                motion = (T)mixedMotion;
                return true;
            }

            public IEnumerator<IMotion> GetEnumerator() => _motions.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _motions.GetEnumerator();
        }

        /// <summary>
        /// Motion blend helper, blends all the motions attached to the mixer.
        /// </summary>
        private class MotionBlender
        {
            private MotionContainer _motions;

            public MotionBlender(MotionContainer motions) => _motions = motions;

            /// <summary>
            /// Blends all the motions registered in the mixer and returns the target position,rotation pair.
            /// For each motion, adds the position offset to the resulting motion space.
            /// Note that rotation is changed when adding a motion due to rotation offset, and the motion
            /// added subsequently will be added using the already changed space. In that way, every motion
            /// is added correctly together.
            /// </summary>
            /// <remarks>
            /// Advances all motions a frame further.
            /// </remarks>
            public (Vector3 targetPosition, Quaternion targetRotation) BlendMotions(
                float deltaTime,
                Vector3 baseOffset,
                Vector3 positionOffset,
                Vector3 rotationOffset)
            {
                Vector3 targetPosition = baseOffset;
                Quaternion targetRotation = Quaternion.identity;

                foreach (var motion in _motions)
                {
                    motion.Tick(deltaTime);
                    targetPosition += targetRotation * motion.CalculatePositionOffset(deltaTime);
                    targetRotation *= motion.CalculateRotationOffset(deltaTime);
                }

                // Apply offsets
                // First removes the baseOffset rotation, and then adds position offset
                // Adds rotation offset
                targetPosition = targetPosition - targetRotation * baseOffset + positionOffset;
                targetRotation *= Quaternion.Euler(rotationOffset);

                return (targetPosition, targetRotation);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditorUtils.SafeOnValidate(this, () =>
            {
                if (Application.isPlaying == false && _target != null)
                {
                    _target.SetLocalPositionAndRotation(_positionOffset, Quaternion.Euler(_rotationOffset));
                }
            });
        }

        private void Reset() => _target = transform;

        private void OnDrawGizmos()
        {
            // Shows a handle at world position where _baseOffset is applied to the mixer
            // It is the pivot of the object as before applying any motion, we apply _baseOffset
            Color handleColor = new(0.1f, 1f, 0.1f, 0.5f);
            const float HandleRadius = 0.08f;

            var prevColor = UnityEditor.Handles.color;
            UnityEditor.Handles.color = handleColor;
            UnityEditor.Handles.SphereHandleCap(
                0, 
                transform.TransformPoint(_baseOffset), 
                Quaternion.identity, 
                HandleRadius, 
                EventType.Repaint);
            UnityEditor.Handles.color = prevColor;
        }
#endif
    }
}