using System;

namespace Nexora.Motion
{
    /// <summary>
    /// Interface to be able receive a new motion data.
    /// </summary>
    public interface IMotionDataReceiver
    {
        /// <summary>
        /// Type of the motion data this interface receive.
        /// </summary>
        Type MotionDataType { get; }

        /// <summary>
        /// Receives new motion data.
        /// </summary>
        void SetMotionData(IMotionData motionData);
    }
}