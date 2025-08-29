using OdinSerializer;
using System;
using System.IO;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Interface to provide serialization functionality for saving the game.
    /// </summary>
    public interface ISerializer
    {
        T Deserialize<T>(byte[] data) where T : class;
        byte[] Serialize<T>(T obj) where T : class;
        void Serialize<T>(T obj, Stream stream) where T : class;
    }

    [Serializable]
    public class OdinSerializer : ISerializer
    {
        public T Deserialize<T>(byte[] data)
            where T : class
        {
            return SerializationUtility.DeserializeValue<T>(data, DataFormat.Binary);
        }

        public byte[] Serialize<T>(T obj)
            where T : class
        {
            return SerializationUtility.SerializeValue<T>(obj, DataFormat.Binary);
        }

        public void Serialize<T>(T obj, Stream stream)
            where T : class
        {
            var context = new SerializationContext();
            var writer = new BinaryDataWriter(stream, context);
            SerializationUtility.SerializeValue(obj, writer);
        }
    }
}