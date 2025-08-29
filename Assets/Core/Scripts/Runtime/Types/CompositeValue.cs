using System.Collections.Generic;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Evaluates a base value modified by multiple modifiers. Useful for movement properties
    /// like speed, acceleration etc.
    /// </summary>
    public sealed class CompositeValue
    {
        public delegate float Modifier();

        private readonly List<Modifier> _modifiers;
        private readonly float _baseValue;

        public CompositeValue(float baseValue = 1f)
        {
            _baseValue = baseValue;
            _modifiers = new List<Modifier>();
        }

        public CompositeValue(float baseValue, CompositeValue source)
        {
            _baseValue = baseValue;
            _modifiers = source?._modifiers ?? new List<Modifier>();
        }

        /// <returns>Current evaluated value, when factored in all the modifiers in effect.</returns>
        public float Evaluate()
        {
            float result = _baseValue;
            foreach (var modifier in _modifiers)
            {
                result *= modifier();
            }

            return result;
        }

        public void AddModifier(Modifier modifier)
        {
            if (modifier != null && _modifiers.Contains(modifier) == false)
            {
                _modifiers.Add(modifier);
            }
        }

        public void RemoveModifier(Modifier modifier)
        {
            if (modifier != null)
            {
                _modifiers.Remove(modifier);
            }
        }
    }
}