using System;

namespace Nexora.Motion
{
    /// <summary>
    /// Interface that controls motion datas for <see cref="IMotionDataReceiver"/>.
    /// </summary>
    public interface IMotionDataBroadcaster
    {
        /// <summary>
        /// Pushes new preset to the list of presets, and set it the active preset to be worked with.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="SetCurrentState{T}(T)" path="/remarks"/>
        /// This mechanism is used to double check for safety, when passing preset to the broadcaster
        /// you need to set the correct type. And in the broadcasters methods, it will again check
        /// if the preset's state enum matches the type of broadcaster's.
        /// </remarks>
        void AddPreset<T>(IMotionDataPreset preset) where T : Enum;

        /// <summary>
        /// Removes a preset from the list, if it was the active preset, effect is lost.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="AddPreset{T}(IMotionDataPreset)" path="/remarks"/>
        /// </remarks>
        void RemovePreset<T>(IMotionDataPreset preset) where T : Enum;

        /// <summary>
        /// Adds a receiver to listen to data override events.
        /// </summary>
        void AddDataReceiver(IMotionDataReceiver receiver);

        /// <summary>
        /// Remove a receiver from listening to data override events.
        /// </summary>
        void RemoveDataReceiver(IMotionDataReceiver receiver);

        /// <summary>
        /// Sets an override to be used for the specific type of motion data.
        /// </summary>
        /// <typeparam name="T">Type of the motion data.</typeparam>
        void SetDataOverride<T>(T motionData) where T : IMotionData;

        /// <summary>
        /// Sets the current state of the broadcaster. 
        /// </summary>
        /// <remarks>
        /// Make sure that passed
        /// enum type <typeparamref name="T"/> is of the same type the concrete
        /// broadcaster holds as in <see cref="MotionDataBroadcaster{StateType}"/>.
        /// </remarks>
        void SetCurrentState<T>(T state) where T : Enum;
    }
}