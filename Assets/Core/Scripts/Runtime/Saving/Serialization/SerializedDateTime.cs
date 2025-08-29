using System;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nexora.Serialization
{
    /// <summary>
    /// Serialized wrapper for <see cref="System.DateTime"/>, uses <see cref="StructLayoutAttribute"/> and memory trick to serialize the component.
    /// Uses integer value <see cref="_ticks"/> to let Unity serialize the field into inspector and load back,
    /// in that way <see cref="DateTime"/> field is automatically serialized as well.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct SerializedDateTime
    {
        [FieldOffset(0)]
        public DateTime DateTime;

        [FieldOffset(0)]
        [SerializeField]
        private ulong _ticks;

        public SerializedDateTime(DateTime time)
        {
            _ticks = 0;
            DateTime = time;
        }

        public static implicit operator SerializedDateTime(DateTime time) => new(time);
        public static implicit operator DateTime(SerializedDateTime time) => time.DateTime;

        public override bool Equals(object obj)
        {
            return obj switch
            {
                null => false,
                DateTime dateTime => dateTime == DateTime,
                SerializedDateTime serializedDateTime => serializedDateTime == DateTime,
                _ => false
            };
        }

        public readonly bool Equals(SerializedDateTime other) => DateTime == other.DateTime;
        public readonly bool Equals(DateTime other) => DateTime == other;

        public static bool operator ==(SerializedDateTime x, SerializedDateTime y)
            => x.DateTime == y.DateTime;

        public static bool operator !=(SerializedDateTime x, SerializedDateTime y)
            => !(x == y);

        public static bool operator ==(SerializedDateTime x, DateTime y)
            => x.DateTime == y;
        public static bool operator !=(SerializedDateTime x, DateTime y)
            => x.DateTime != y;

        public override int GetHashCode() => DateTime.GetHashCode();
        public override string ToString() => DateTime.ToString(CultureInfo.InvariantCulture);
    }
}