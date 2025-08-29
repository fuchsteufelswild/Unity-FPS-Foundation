using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// What <see langword="type"/> of value a <see cref="DynamicItemProperty"/> stores?
    /// </summary>
    public enum ItemPropertyValueType
    {
        Double = 0,
        Float = 1,
        Integer = 2,
        Boolean = 3,
        Item = 4
    }


    /// <summary>
    /// Holds a value that can be changed at <b>Runtime</b> for <see cref="IItem"/>. Think of it like
    /// <b>Durability</b>, <b>ExtraDamge</b> etc.; 
    /// every item has it but it might be different for different <see langword="object"/> instance.
    /// Unlike other properties, which might be <b>Damage</b>, <b>MaxDurability</b> etc.
    /// </summary>
    /// <remarks>
    /// It is bit like a hack to have a single <see langword="double"/> value to control
    /// of the different value types, but having a class hierarchy makes serialization a problem
    /// and class hierarchy is an overkill for this simple case, especially in a performance manner.
    /// It might not worth to have a virtual call every time we request for a dynamic property. It might be called,
    /// many times in a frame.
    /// </remarks>
    [Serializable]
    public sealed class DynamicItemProperty
    {
        /// <summary>
        /// Used to detect if a value has been changed.
        /// </summary>
        private const double ComparisonEpsilon = 0.001;

        [SerializeField]
        private DefinitionReference<DynamicItemPropertyDefinition> _propertyID;

        [SerializeField]
        private double _propertyValue;

        public event Action<DynamicItemProperty> ValueChanged;

        public int PropertyID => _propertyID;

        public DynamicItemProperty(DynamicItemPropertyDefinition propertyDefinition, double propertyValue)
        {
            _propertyID = new DefinitionReference<DynamicItemPropertyDefinition>(propertyDefinition);
            _propertyValue = propertyValue;
        }

        /// <returns>Shallow copy of the property.</returns>
        public DynamicItemProperty Clone() => (DynamicItemProperty)MemberwiseClone();

        /// <summary>
        /// Treats the <see cref="DynamicItemProperty"/> as a <see langword="double"/>.
        /// </summary>
        public double DoubleValue
        {
            get => _propertyValue;
            set => SetValueInterval(value);
        }

        /// <summary>
        /// Treats the <see cref="DynamicItemProperty"/> as a <see langword="float"/>.
        /// </summary>
        public float FloatValue
        {
            get => (float)_propertyValue;
            set => SetValueInterval(value);
        }

        /// <summary>
        /// Treats the <see cref="DynamicItemProperty"/> as an <see langword="int"/>.
        /// </summary>
        public int IntegerValue
        {
            get => (int)_propertyValue;
            set => SetValueInterval(value);
        }

        /// <summary>
        /// Treats the <see cref="DynamicItemProperty"/> as a <see langword="bool"/>.
        /// </summary>
        /// <remarks>
        /// Values are converted to either 0 or 1.
        /// </remarks>
        public bool BooleanValue
        {
            get => _propertyValue > 0f;
            set => SetValueInterval(value ? 1f : 0f);
        }

        /// <summary>
        /// Treats the <see cref="DynamicItemProperty"/> as a <see cref="IItem"/>.
        /// Returns the <see cref="Definition.ID"/> of <see cref="IItem"/> which is
        /// of type <see langword="int"/>.
        /// </summary>
        /// <remarks>
        /// Usage can be thought as Weapon attachments for iron sight, scope, trigger, magazine
        /// any kind of <see cref="IItem"/> that can be changed in gameplay on the weapon.
        /// <br></br>
        /// Or an enchanment object that is placed onto a magical weapon, that enchanment later
        /// can be another object.
        /// </remarks>
        public int LinkedItemID
        {
            get => (int)_propertyValue;
            set => SetValueInterval(value);
        }

        private void SetValueInterval(double value)
        {
            double previousValue = _propertyValue;
            _propertyValue = value;
            if(Math.Abs(previousValue - value) < ComparisonEpsilon)
            {
                ValueChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Clones the given item properties into a new array.
        /// </summary>
        public static DynamicItemProperty[] Clone(DynamicItemProperty[] properties)
        {
            if(properties == null || properties.IsEmpty())
            {
                return Array.Empty<DynamicItemProperty>();
            }

            var clonedProperties = new DynamicItemProperty[properties.Length];
            for(int i = 0; i < properties.Length; i++)
            {
                clonedProperties[i] = properties[i].Clone();
            }

            return clonedProperties;
        }
    }

    /// <summary>
    /// Generator to generate new <see cref="DynamicItemProperty"/> using a <see cref="DynamicItemPropertyDefinition"/>
    /// and a value. 
    /// </summary>
    /// <remarks>
    /// Provides functionality to have a random value generated for the property. It is useful when having
    /// properties that are randomly generated. Like for pickups, items with fire damage from a range (x,y)
    /// might be generated. Whereas, for an attachment the value could be just the ID of the item.
    /// </remarks>
    [Serializable]
    public struct DynamicItemPropertyGenerator : IEquatable<DynamicItemPropertyGenerator>
    {
        [SerializeField]
        private int _itemPropertyDefinition;

        [Tooltip("Value range for the property, used if '_generateRandomValue' is used. " +
            "\n!For 'LinkedItemID' and 'bool' only 'x' field is used.!")]
        [SerializeField]
        private Vector2 _valueRange;

        [SerializeField]
        private bool _generateRandomValue;

        public readonly DefinitionReference<DynamicItemPropertyDefinition> PropertyDefinition 
            => _itemPropertyDefinition;

        //public DynamicItemPropertyGenerator(DynamicItemPropertyDefinition propertyDefinition)
        //{
        //    _itemPropertyDefinition = new DefinitionReference<DynamicItemPropertyDefinition>(propertyDefinition);
        //    _valueRange = Vector2.zero;
        //    _generateRandomValue = false;
        //}

        public readonly DynamicItemProperty GenerateProperty()
            => new DynamicItemProperty(PropertyDefinition, GeneratePropertyValue());

        public readonly double GeneratePropertyValue()
        {
            return DefinitionRegistry<DynamicItemPropertyDefinition>.GetByID(_itemPropertyDefinition).PropertyValueType switch
            {
                ItemPropertyValueType.Double => GetValue(_valueRange, _generateRandomValue),
                ItemPropertyValueType.Float => (float)GetValue(_valueRange, _generateRandomValue),
                ItemPropertyValueType.Integer => (int)GetValue(_valueRange, _generateRandomValue),
                ItemPropertyValueType.Boolean => _valueRange.x > 0f ? 1f : 0f,
                ItemPropertyValueType.Item => _valueRange.x,
                _ => throw new ArgumentOutOfRangeException("Enum is invalid value")
            };

            double GetValue(Vector2 valueRange, bool generateRandomValue) 
                => generateRandomValue ? valueRange.GetRandomFromRange() : valueRange.x;
        }

        public readonly override int GetHashCode() => _itemPropertyDefinition.GetHashCode();

        public readonly override bool Equals(object obj)
            => obj is DynamicItemPropertyDefinition other && Equals(other);

        public readonly bool Equals(DynamicItemPropertyGenerator other) 
            => _itemPropertyDefinition.Equals(other._itemPropertyDefinition);
    }
}