using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Interface to denote that a class listens to event of type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <remarks>
    /// Event system works with this interface instead of plain <see cref="System.Action{T}"/>
    /// delegates. The reason is discoverability and the intention when inspecting class. It is not
    /// very easy to get the intention when type is not enforced, especially when the system is huge.
    /// </remarks>
    /// <typeparam name="TEvent">Type of the event.</typeparam>
    public interface IEventListener<TEvent>
        where TEvent : IReadonlyEvent
    {
        void OnEvent(in TEvent eventData);
    }
}