using System.Collections.Generic;
using UnityEngine;

namespace Nexora
{
    public enum MessageType : byte
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    /// <summary>
    /// Manages the dispatching of messages to registered listeners for a generic sender type.
    /// In projects may create a class named MessageDispatchers and predefine dispatchers for specific classes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender (e.g., IPlayer, GameObject, etc.).</typeparam>
    public sealed class MessageDispatcher<TSender>
        where TSender : class
    {
        private static readonly MessageDispatcher<TSender> _instance = new();
        private readonly List<IMessageListener<TSender>> _listeners = new();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            if (_instance != null)
            {
                _instance._listeners.Clear();
            }
        }
#endif

        public static MessageDispatcher<TSender> Instance => _instance;

        public void AddListener(IMessageListener<TSender> listener) => _listeners.Add(listener);
        public void RemoveListener(IMessageListener<TSender> listener) => _listeners.Remove(listener);

        public void Dispatch(TSender sender, MessageType messageType, string message, Sprite sprite = null)
        {
            var args = new MessageArgs(messageType, message, sprite);
            Dispatch(sender, args);
        }

        public void Dispatch(TSender sender, in MessageArgs args)
        {
            foreach (var listener in _listeners)
            {
                listener.OnMessageReceived(sender, args);
            }
        }
    }

    public interface IMessageListener<in TSender>
        where TSender : class
    {
        void OnMessageReceived(TSender sender, in MessageArgs args);
    }


    public readonly struct MessageArgs
    {
        public readonly MessageType MessageType;

        public readonly string Message;

        public readonly Sprite Sprite;

        public MessageArgs(MessageType messageType, string message, Sprite sprite)
        {
            MessageType = messageType;
            Message = message;
            Sprite = sprite;
        }
    }

    public static class DefaultMessageDispatchers
    {
        public static readonly MessageDispatcher<UnityEngine.Object> GeneralMessageDispatcher = new();
    }
}
