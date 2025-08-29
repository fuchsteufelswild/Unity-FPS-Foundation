using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.Options
{
    /// <summary>
    /// Main interface for an option, provides an interface to access boxed underlying option value.
    /// Because, an option may have values such as <see langword="bool"/>, <see langword="float"/>, <see langword="int"/> etc.
    /// </summary>
    /// <remarks>
    /// It is designed this way as performance is not main concern in the options, and we want to be able to store options having different underlying value
    /// in the same list. If we were to design the interface with generics, we couldn't do it.
    /// </remarks>
    public interface IOption
    {
        object Value { get; set; }
    }

    /// <summary>
    /// Concrete class for the options. Provides access to underlying value as well as boxed value type through <see cref="IOption"/> interface.
    /// </summary>
    [Serializable]
    public sealed class Option<T> : 
        IOption
        where T : struct, 
                  IEquatable<T>
    {
        [SerializeField]
        private T _value;

        public event UnityAction<T> OnValueChanged;

        public T Value => _value;

        public Option(T value = default)
        {
            _value = value;
        }

        object IOption.Value 
        { 
            get => _value; 
            set => SetValue(value as T? ?? default); 
        }

        public void SetValue(T value)
        {
            if(_value.Equals(value))
            {
                return;
            }

            _value = value;
            OnValueChanged?.Invoke(value);
        }

        public static implicit operator T(Option<T> option) => option.Value;
    }
}