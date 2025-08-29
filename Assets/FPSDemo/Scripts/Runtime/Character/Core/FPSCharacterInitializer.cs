using Nexora.FPSDemo.ProceduralMotion;
using Nexora.Motion;
using UnityEngine;

namespace Nexora.FPSDemo
{
    [DefaultExecutionOrder(ExecutionOrder.LateGameLogic)]
    internal sealed class FPSCharacterInitializer : MonoBehaviour
    {
        [SerializeField, NotNull]
        private FPSCharacter _fpsCharacter;

        [SerializeField, NotNull]
        private MotionMixer _headMotionMixer;

        [SerializeField, NotNull]
        private MotionMixer _handsMotionMixer;

        private void Start()
        {
            CharacterMotionHandler headMotionHandler = new CharacterMotionHandler(
                _headMotionMixer, 
                _headMotionMixer.GetComponent<IMotionDataBroadcaster>(), 
                (_headMotionMixer as IMotionMixer).GetMotion<AdditiveShakeMotion>());

            CharacterMotionHandler handsMotionHandler = new CharacterMotionHandler(
                _handsMotionMixer,
                _handsMotionMixer.GetComponent<IMotionDataBroadcaster>(),
                (_handsMotionMixer as IMotionMixer).GetMotion<AdditiveShakeMotion>());

            _fpsCharacter.Initialize(headMotionHandler, handsMotionHandler);
        }
    }
}