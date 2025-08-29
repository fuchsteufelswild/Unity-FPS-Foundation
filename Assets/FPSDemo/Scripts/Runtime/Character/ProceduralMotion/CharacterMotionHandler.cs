using Nexora.Motion;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    public sealed class CharacterMotionHandler
    {
        public IMotionMixer MotionMixer { get; }
        public IMotionDataBroadcaster DataBroadcaster { get; }
        public IShakeHandler ShakeHandler { get; }

        public CharacterMotionHandler(IMotionMixer motionMixer, IMotionDataBroadcaster dataBroadcaster, IShakeHandler shakeHandler)
        {
            MotionMixer = motionMixer;
            DataBroadcaster = dataBroadcaster;
            ShakeHandler = shakeHandler;
        }
    }
}