using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.ProceduralMotion
{
    public interface IHandheldMotionController
    {
        /// <summary>
        /// Target transform to apply motions on."
        /// </summary>
        Transform TargetTransform { get; }

        /// <summary>
        /// Current motion preset used by the motion controller.
        /// </summary>
        CharacterMotionDataPreset MotionPreset { get; }

        /// <summary>
        /// Base offset of the motion.
        /// </summary>
        Vector3 BaseOffset { get; }

        /// <summary>
        /// Position offset of the motion.
        /// </summary>
        Vector3 PositionOffset { get; }

        /// <summary>
        /// Rotation offset of the motion.
        /// </summary>
        Vector3 RotationOffset { get; }

        /// <summary>
        /// Invoked when the data preset changed.
        /// </summary>
        event UnityAction<CharacterMotionDataPreset, CharacterMotionDataPreset> PresetChanged;

        /// <summary>
        /// Invoked when any of the offsets are changed.
        /// </summary>
        event UnityAction<Vector3, Vector3> OffsetsChanged;

        /// <summary>
        /// Sets the motion data preset of this controller.
        /// </summary>
        /// <param name="preset">The data preset.</param>
        void SetMotionPreset(CharacterMotionDataPreset preset);

        /// <summary>
        /// Sets the new offsets for the controller.
        /// </summary>
        void SetOffsets(Vector3 positionOffset, Vector3 rotationOffset);
    }

    [DefaultExecutionOrder(ExecutionOrder.EarlyPresentation)]
    public sealed class HandheldMotionController : 
        MonoBehaviour, 
        IHandheldMotionController
    {
        [Tooltip("Target transform to apply motions on.")]
        [SerializeField, InLineEditor]
        private Transform _targetTransform;

        [Tooltip("Base offset of the motion, it is like changing the pivot of the motion.")]
        [SerializeField]
        private Vector3 _baseOffset;

        [Tooltip("Position offset of the motion.")]
        [SerializeField]
        private Vector3 _positionOffset;

        [Tooltip("Rotation offset of the motion.")]
        [SerializeField]
        private Vector3 _rotationOffset;

        [Tooltip("Motion preset used by the motion controller.")]
        [SerializeField]
        private CharacterMotionDataPreset _preset;

        public Transform TargetTransform => _targetTransform;
        public Vector3 BaseOffset => _baseOffset;
        public Vector3 PositionOffset => _positionOffset;
        public Vector3 RotationOffset => _rotationOffset;
        public CharacterMotionDataPreset MotionPreset => _preset;

        public event UnityAction<CharacterMotionDataPreset, CharacterMotionDataPreset> PresetChanged;
        public event UnityAction<Vector3, Vector3> OffsetsChanged;

        public void SetMotionPreset(CharacterMotionDataPreset preset)
        {
            if(_preset == preset)
            {
                return;
            }

            var oldPreset = _preset;
            _preset = preset;
            PresetChanged?.Invoke(oldPreset, preset);
        }

        public void SetOffsets(Vector3 positionOffset, Vector3 rotationOffset)
        {
            _positionOffset = positionOffset;
            _rotationOffset = rotationOffset;
            OffsetsChanged?.Invoke(positionOffset, rotationOffset);
        }
    }
}