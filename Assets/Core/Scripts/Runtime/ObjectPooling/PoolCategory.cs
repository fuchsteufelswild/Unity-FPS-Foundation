using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Nexora.ObjectPooling
{
    /// <summary>
    /// Represents a category for object pooling. 
    /// This class should be extended by more specific projects to add new categories.
    /// </summary>
    public class PoolCategory : IEquatable<PoolCategory>
    {
        public static readonly PoolCategory None = new PoolCategory("None", HashCode.Combine("None"));

        public string Name { get; }

        public int Hash { get; }

        /// <summary>
        /// Cannot be directly instantiated outside of this <see langword="class"/> hierarchy, allowed for extension.
        /// </summary>
        protected PoolCategory(string name, int hash)
        {
            Name = name;
            Hash = hash;
        }

        public override string ToString() => Name;
        public override int GetHashCode() => HashCode.Combine(Name);

        public override bool Equals(object obj)
        {
            if (obj is PoolCategory other)
            {
                return Equals(other);
            }

            return false;
        }

        public bool Equals(PoolCategory other) => Hash == other.Hash;

        public static bool operator ==(PoolCategory left, PoolCategory right) 
            => left.Equals(right);
        public static bool operator !=(PoolCategory left, PoolCategory right) 
            => !left.Equals(right);
    }
}