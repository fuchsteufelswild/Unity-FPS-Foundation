namespace Nexora
{
    /// <summary>
    /// Interface to denote a class that can publish <see cref="TEvent"/>.
    /// This is different from <see cref="EventBus{TEvent}"/> that it is for more 
    /// localized, in-system communication, or a communication between 2-3 classes.
    /// Whereas, <see cref="EventBus{TEvent}"/> is for communication between systems,
    /// or when an event need to have many listeners that are not related. And for 
    /// <see cref="EventBus{TEvent}"/>, any class can listen for a specific event,
    /// but for <see cref="IEventPublisher{TEvent}"/>, only classes that know about
    /// it can listen to its events.
    /// </summary>
    /// <remarks>
    /// This is like a <see cref="EventChannel{TEvent}"/>, but without <b>ScriptableObject</b>
    /// approach. More like a code-based and more controllable, rather than designer friendly 
    /// system.
    /// </remarks>
    /// <typeparam name="TEvent">Type of the event.</typeparam>
    public interface IEventPublisher<TEvent>
        where TEvent : IReadonlyEvent
    {
        void Register(IEventListener<TEvent> listener);
        void Unregister(IEventListener<TEvent> listener);
        void Publish(in TEvent eventData);
    }
}