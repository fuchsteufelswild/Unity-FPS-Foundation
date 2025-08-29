using System;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Reference to a <see cref="Definition{T}"/>. Stores unique ID of definition
    /// to later access. <b>Can be used as a <see cref="Definition{T}"/></b>.
    /// </summary>
    /// <remarks>
    /// Can be initialized with a <see cref="Definition{T}"/>, <see langword="string"/>(definition name),
    /// or <see langword="int"/>(definition ID). And can be implicity converted to these types back and forth.
    /// <br></br>
    /// </remarks>
    /// <typeparam name="T">Type of the <see cref="Definition{T}"/> reference.</typeparam>
    [Serializable]
    public struct DefinitionReference<T> :
        IEquatable<DefinitionReference<T>>
        where T : Definition<T>
    {
        [SerializeField]
        private int _definitionID;

        public static DefinitionReference<T> NullRef = new(DataConstants.NullID);

        public readonly T Definition => DefinitionRegistry<T>.GetByID(_definitionID);
        public readonly string Name => IsNull ? string.Empty : Definition.Name;
        public readonly string Description => IsNull ? string.Empty : Definition.Description;
        public readonly Sprite Icon => IsNull ? null : Definition.Icon;
        public readonly bool IsNull => _definitionID == NullRef._definitionID;

        public DefinitionReference(string definitionName)
            : this(DefinitionRegistry<T>.GetByName(definitionName))
        {
        }

        public DefinitionReference(Definition<T> definition)
            : this(definition?.ID ?? NullRef._definitionID)
        {
        }

        public DefinitionReference(int definitionID) => _definitionID = definitionID;

        public static implicit operator int(DefinitionReference<T> definitionReference)
            => definitionReference._definitionID;

        public static implicit operator T(DefinitionReference<T> definitionReference)
            => definitionReference.Definition;

        public static implicit operator string(DefinitionReference<T> definitionReference)
            => definitionReference.Name;

        public static implicit operator DefinitionReference<T>(T definition)
            => new DefinitionReference<T>(definition);

        public static implicit operator DefinitionReference<T>(int definitionID)
            => new DefinitionReference<T>(definitionID);

        public static implicit operator DefinitionReference<T>(string definitionName)
            => new DefinitionReference<T>(definitionName);

        public static bool operator ==(DefinitionReference<T> left, DefinitionReference<T> right)
            => left._definitionID == right._definitionID;

        public static bool operator ==(DefinitionReference<T> left, T right)
            => left._definitionID == right.ID;

        public static bool operator ==(DefinitionReference<T> left, int right)
            => left._definitionID == right;

        public static bool operator ==(DefinitionReference<T> left, string right)
            => left.Name == right;

        public static bool operator !=(DefinitionReference<T> left, DefinitionReference<T> right)
            => !(left == right);

        public static bool operator !=(DefinitionReference<T> left, T right)
            => !left.Equals(right);

        public static bool operator !=(DefinitionReference<T> left, int right)
            => !left.Equals(right);

        public static bool operator !=(DefinitionReference<T> left, string right)
            => !left.Equals(right);

        public override readonly bool Equals(object obj)
        {
            return obj switch
            {
                DefinitionReference<T> definitionReference => Equals(definitionReference),
                T definition => _definitionID == definition.ID,
                int definitionID => _definitionID == definitionID,
                string definitionName => Name == definitionName,
                _ => false
            };
        }

        public readonly bool Equals(DefinitionReference<T> other)
            => this._definitionID == other._definitionID;

        public override readonly int GetHashCode() => _definitionID.GetHashCode();
    }
}
