using System;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Serializable, Equatable, Comparable enum wrapper that gives functionality to underlying enum type as if it is a value.
    /// Can be used like a regular enum.
    /// Unlike regular enums, prevents boxing on comparisons and when used in <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>
    /// </summary>
    /// <typeparam name="T">The enum type being wrapped.</typeparam>
    [Serializable]
    public struct EnumValue<T> : 
        IEquatable<EnumValue<T>>, 
        IComparable<EnumValue<T>>,
        IComparable
        where T : struct, Enum
    {
        [SerializeField] 
        private T _value;

        public T Value => _value;

        public EnumValue(T value)
        {
            _value = value;
        }

        public override readonly bool Equals(object obj)
        {
            if (obj is EnumValue<T> enumValue)
            {
                return Equals(enumValue);
            }

            return false;
        }

        public readonly bool Equals(EnumValue<T> other)
            => _value.Equals(other._value);

        public static bool operator ==(EnumValue<T> left, EnumValue<T> right)
            => left.Equals(right);

        public static bool operator !=(EnumValue<T> left, EnumValue<T> right)
            => !(left == right);

        public readonly int CompareTo(object obj)
        {
            return obj switch
            {
                EnumValue<T> other => CompareTo(other),
                null => 1,
                _ => throw new ArgumentException($"Object must be of type {nameof(EnumValue<T>)}")
            };
        }

        /// <summary>
        /// <see cref="Convert.ToInt32"/> may result in boxing, so we use slightly complex implementation.
        /// </summary>
        public readonly int CompareTo(EnumValue<T> other)
        {
            return ((IConvertible)_value).ToInt32(null).CompareTo(
               ((IConvertible)other.Value).ToInt32(null));
        }

        public static bool operator <(EnumValue<T> left, EnumValue<T> right)
            => left.CompareTo(right) < 0;

        public static bool operator >(EnumValue<T> left, EnumValue<T> right)
            => left.CompareTo(right) > 0;

        public static bool operator <=(EnumValue<T> left, EnumValue<T> right)
            => left.CompareTo(right) <= 0;

        public static bool operator >=(EnumValue<T> left, EnumValue<T> right)
            => left.CompareTo(right) >= 0;

        public override readonly int GetHashCode() => _value.GetHashCode();

        public static implicit operator T(EnumValue<T> value) => value.Value;
        public static implicit operator EnumValue<T>(T value) => new(value);
    }
}
