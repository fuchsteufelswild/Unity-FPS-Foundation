using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nexora.Serialization
{
    /// <summary>
    /// Serializable version of <see cref="System.Guid"/> that works with Unity's serialization system.
    /// Uses memory overlapping to safely store the <see cref="Guid"/> into 4 different integer numbers 
    /// that are contiguously placed one after another to represent parts of the <see cref="Guid"/>.
    /// Depends on the <see cref="Guid"/> internal 16 bytes layout, little-endian form.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct UnityGuid : IEquatable<Guid>
    {
        // Full 128 bit Guid
        [FieldOffset(0)]
        public Guid Guid;

        // Parts of the guid represented as a 4 separate integers to work with Unity serialization system
        [SerializeField, FieldOffset(0)]
        private int _guidPart1; // Bytes 0-3
        [SerializeField, FieldOffset(4)]
        private int _guidPart2; // Bytes 4-7
        [SerializeField, FieldOffset(8)]
        private int _guidPart3; // Bytes 8-11
        [SerializeField, FieldOffset(12)]
        private int _guidPart4; // Bytes 12-15

        public static UnityGuid Empty => new(Guid.Empty);

        public UnityGuid(Guid guid)
        {
            _guidPart1 = _guidPart2 = _guidPart3 = _guidPart4 = 0;
            Guid = guid; // This overwrites the 16 byte memory set to 0 above 
        }

        public bool IsValid() => Guid != Guid.Empty;
        public void Clear() => Guid = Guid.Empty;

        // Conversions
        public static implicit operator Guid(UnityGuid guid) => guid.Guid;
        public static implicit operator UnityGuid(Guid guid) => new(guid);

        // Equality
        public override readonly bool Equals(object obj)
        {
            return obj switch
            {
                null => false,
                UnityGuid unityGuid => unityGuid == Guid,
                Guid guid => guid == Guid,
                _ => false
            };
        }

        public readonly bool Equals(Guid other) => Guid == other;

        public static bool operator ==(UnityGuid x, UnityGuid y)
            => x.Guid == y.Guid;
        public static bool operator !=(UnityGuid x, UnityGuid y)
            => !(x == y);

        public static bool operator ==(UnityGuid x, Guid y)
            => x.Guid == y;
        public static bool operator !=(UnityGuid x, Guid y)
            => !(x == y);
        
        public override int GetHashCode() => Guid.GetHashCode();
        public override string ToString() => Guid.ToString();
    }
}