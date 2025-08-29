using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora
{
    /// <summary>
    /// Scriptable object to define a event channel that is not widely available as much as <see cref="EventBus{TEvent}"/>.
    /// But this is for more like a local and more controlled, designer-friendly events. As the <see cref="ScriptableObject"/>
    /// asset will be put into the field in the inspector, and for the objects that are not instantiated but already existing
    /// in the scene will be shown when we request to see all references.
    /// </summary>
    /// <remarks>
    /// Technically, whole <see cref="EventBus{TEvent}"/> could be implemented as a <see cref="ScriptableObject"/> following a 
    /// similar architecture that is of <see cref="GameModule{T}"/>, which has an only one instance per type T and it could be
    /// instantiated as a <see cref="ScriptableObject"/> asset and passed to the classes as well; a combined event system.
    /// <br></br>
    /// But tradeoff would be ambiguity between objects that solely access the event bus via code and some via asset,
    /// this might cause confusion. And we couldn't achieve what we achieve now, as the <see cref="EventChannel{TEvent}"/> are independent from
    /// <see cref="EventBus{TEvent}"/>; <see cref="EventBus{TEvent}"/> stacks publishes one after another and with a single call(like a thread dispatcher)
    /// it processes all events, whereas <see cref="EventChannel{TEvent}"/> just invokes event immediately.
    /// </remarks>
    /// <typeparam name="TEvent"></typeparam>
    [DefaultExecutionOrder(ExecutionOrder.GameModule)]
    public abstract class EventChannel<TEvent> :
        ScriptableObject
        where TEvent : struct, IReadonlyEvent
    {
        protected const string AssetMenuPath = "Nexora/Event Channels/";

        [Tooltip("This events will be called before any other. Use cautiously and very sparingly" +
            " as the objects should be alive when making the call.")]
        [SerializeField, ReorderableList(ElementLabel = "Event")]
        private List<UnityEvent<TEvent>> _highPriorityEvents;

        private readonly List<IEventListener<TEvent>> _listeners = new();

        private void OnEnable() => _listeners.Clear();

        [Conditional("UNITY_EDITOR")]
        private void OnValidate() => _listeners.Clear();

        public void Raise(in TEvent eventData)
        {
            foreach(var evt in _highPriorityEvents)
            {
                evt?.Invoke(eventData);
            }

            foreach(var listener in _listeners)
            {
                listener?.OnEvent(in eventData);
            }
        }

        public void Subscribe(IEventListener<TEvent> listener) => _listeners.Add(listener);
        public void Unsubscribe(IEventListener<TEvent> listener) => _listeners.Remove(listener);
    }

    /* Can be used like following
    public readonly struct TestEvent : IReadonlyEvent
    {
        public readonly int Stat;

        public TestEvent(int Stat)
        {
            this.Stat = Stat;
        }
    }

    [CreateAssetMenu(menuName = CreateMenuPath + nameof(TestEventChannel), fileName = "EventChannel_")]
    public sealed class TestEventChannel : EventChannel<TestEvent>
    {
    }
    */
}