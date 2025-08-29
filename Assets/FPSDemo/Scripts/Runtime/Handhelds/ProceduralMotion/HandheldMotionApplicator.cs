using Nexora.FPSDemo.Handhelds;
using Nexora.FPSDemo.Movement;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    public interface IHandheldMotionTargets
    {
        /// <summary>
        /// Motion handler of the head.
        /// </summary>
        CharacterMotionHandler HeadMotion { get; }

        /// <summary>
        /// Motion handler of the hands.
        /// </summary>
        CharacterMotionHandler HandsMotion { get; }
    }

    [DefaultExecutionOrder(ExecutionOrder.EarlyPresentation)]
    [RequireComponent(typeof(IHandheldMotionController))]
    public sealed class HandheldMotionApplicator : 
        MonoBehaviour,
        IHandheldMotionTargets
    {
        private IHandheldMotionController _motionController;
        private IFPSCharacter _fpsCharacter;

        public CharacterMotionHandler HeadMotion => _fpsCharacter?.HeadMotion;
        public CharacterMotionHandler HandsMotion => _fpsCharacter?.HandsMotion;


        private void Awake()
        {
            _motionController = GetComponent<IHandheldMotionController>();

            _fpsCharacter = GetComponentInParent<IHandheld>()?.Character as IFPSCharacter;
        }

        private void OnEnable()
        {
            _motionController.PresetChanged += OnPresetChanged;
            _motionController.OffsetsChanged += OnOffsetsChanged;

            ApplyOffsets(_motionController.PositionOffset, _motionController.RotationOffset);
            ApplyPreset(null, _motionController.MotionPreset);
        }

        private void OnDisable()
        {
            _motionController.PresetChanged -= OnPresetChanged;
            _motionController.OffsetsChanged -= OnOffsetsChanged;

            HandsMotion?.DataBroadcaster.RemovePreset<MovementStateType>(_motionController.MotionPreset);
        }

        private void OnOffsetsChanged(Vector3 position, Vector3 rotation) => ApplyOffsets(position, rotation);
        private void OnPresetChanged(CharacterMotionDataPreset oldPreset, CharacterMotionDataPreset newPreset) => ApplyPreset(oldPreset, newPreset);

        private void ApplyOffsets(Vector3 position, Vector3 rotation)
        {
            if(HandsMotion == null || gameObject.activeInHierarchy == false)
            {
                return;
            }

            HandsMotion.MotionMixer.ConfigureMixer(_motionController.TargetTransform, _motionController.BaseOffset, position, rotation);
        }

        private void ApplyPreset(CharacterMotionDataPreset oldPreset, CharacterMotionDataPreset newPreset)
        {
            if(HandsMotion == null || gameObject.activeInHierarchy == false)
            {
                return;
            }

            HandsMotion.DataBroadcaster.RemovePreset<MovementStateType>(oldPreset);
            HandsMotion.DataBroadcaster.AddPreset<MovementStateType>(newPreset);
        }
    }
}