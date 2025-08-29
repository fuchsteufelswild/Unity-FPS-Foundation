using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Event bus for communication between systems and components.
    /// <see cref="IEventListener{TEvent}"/> can subscribe to the bus and listens to
    /// data of <see cref="IReadonlyEvent"/> published. 
    /// </summary>
    /// <remarks>
    /// For optimization it works with <b><see langword="readonly struct"/></b> derived from <see cref="IReadonlyEvent"/>.
    /// </remarks>
    /// <typeparam name="TEvent">Type of the event.</typeparam>
    public sealed class EventBus<TEvent> 
        where TEvent : struct, IReadonlyEvent
    {
        private static EventBus<TEvent> _instance = new();

        private readonly List<IEventListener<TEvent>> _listeners = new();
        private readonly Queue<TEvent> _eventQueue = new();

        private bool _isProcessingEvents = false;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void EditorReload()
        {
            _instance._listeners.Clear();
            _instance._eventQueue.Clear();
        }
#endif

        public static void Subscribe(IEventListener<TEvent> listener) => _instance._listeners.AddUnique(listener);

        public static void Unsubscribe(IEventListener<TEvent> listener) => _instance._listeners.Remove(listener);

        /// <summary>
        /// Adds the event into queue, and processes all of the current events depending on the
        /// optional <paramref name="process"/> parameter.
        /// </summary>
        /// <param name="process">Should immediately invoke processing of all events after adding this one?</param>
        public static void Publish(in TEvent eventData, bool process = false)
        {
            _instance._eventQueue.Enqueue(eventData);
            if (process)
            {
                ProcessEvents();
            }
        }

        /// <inheritdoc cref="ProcessEventsInternal"/>
        public static void ProcessEvents() => _instance.ProcessEventsInternal();

        /// <summary>
        /// Processes all events currently published to the listeners one by one.
        /// </summary>
        /// <remarks>
        /// In the FIFO(first in, first out) manner.
        /// </remarks>
        private void ProcessEventsInternal()
        {
            if (_isProcessingEvents || _eventQueue.IsEmpty())
            {
                return;
            }

            _isProcessingEvents = true;
            while(_eventQueue.TryDequeue(out TEvent eventData))
            {
                ProcessEvent(eventData);
            }
            _isProcessingEvents = false;
        }

        /// <summary>
        /// Processes event to all of the listeners.
        /// </summary>
        private void ProcessEvent(in TEvent eventData)
        {
            foreach (var listener in _listeners)
            {
                try
                {
                    listener.OnEvent(in eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("Error in event listener: {0}", ex.Message);
                }
            }
        }
    }
}