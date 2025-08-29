using UnityEngine;
using System;
using System.Runtime.Serialization;

namespace Nexora.InventorySystem
{ 
    public class Item : 
        IItem,
        IDeserializationCallback
    {
        [SerializeField]
        private int _id;

        /// <summary>
        /// Properties that can changed at runtime.
        /// </summary>
        [SerializeField]
        private DynamicItemProperty[] _dynamicProperties;

        [NonSerialized]
        private ItemDefinition _itemDefinition;

        private static Item _sharedDummyItem = new();

        public int ID => _id;
        public ItemDefinition ItemDefinition => _itemDefinition;

        public string Name => IsNull ? string.Empty : _itemDefinition.Name;

        public float Weight => IsNull ? 0 : _itemDefinition.Weight;

        public bool IsStackable => IsNull ? false : _itemDefinition.MaxStackSize > 1;

        public int MaxStackSize => IsNull ? 0 : _itemDefinition.MaxStackSize;

        public bool IsNull => _id == DataConstants.NullID;

        /// <summary>
        /// A null item.
        /// </summary>
        public Item()
        {
            _id = 0;
            _dynamicProperties = Array.Empty<DynamicItemProperty>();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Item(Item other)
        {
            _id = other._id;
            _itemDefinition = other._itemDefinition;
            _dynamicProperties = DynamicItemProperty.Clone(other._dynamicProperties);
        }

        /// <summary>
        /// Constructing a new item from a data. Main creation method for an item.
        /// </summary>
        public Item(ItemDefinition itemDefiniton)
        {
            _id = itemDefiniton.ID;
            _itemDefinition = itemDefiniton;
            _dynamicProperties = ItemDefinition.GenerateProperties();
        }

        /// <summary>
        /// Creates a dummy item that doesn't have any dynamic properties,
        /// just temporarily used and disposed.
        /// </summary>
        public static Item CreateDummyItem(ItemDefinition itemDefinition)
        {
            Item dummyItem = new();
            dummyItem._id = itemDefinition?.ID ?? DataConstants.NullID;
            dummyItem._itemDefinition = itemDefinition;
            return dummyItem;
        }

        /// <summary>
        /// Creates a dummy item that doesn't have any dynamic properties.
        /// </summary>
        /// <remarks>
        /// Note! It is used by all of the <see cref="Item"/> instances, it 
        /// is to be used instead of <see cref="CreateDummyItem(ItemDefinition)"/>
        /// for performance-critical places to avoid memory allocation. But be cautious
        /// of race conditions.
        /// </remarks>
        public static Item CreateSharedDummyItem(ItemDefinition itemDefinition)
        {
            _sharedDummyItem._id = itemDefinition?.ID ?? DataConstants.NullID;
            _sharedDummyItem._itemDefinition = itemDefinition;
            return _sharedDummyItem;
        }

        public override string ToString() => Name;

        public bool TryGetDynamicProperty(DefinitionReference<DynamicItemPropertyDefinition> propertyID, out DynamicItemProperty property)
        {
            property = GetDynamicProperty(propertyID);
            return property != null;
        }

        public DynamicItemProperty GetDynamicProperty(DefinitionReference<DynamicItemPropertyDefinition> propertyID)
        {
            foreach(DynamicItemProperty property in _dynamicProperties)
            {
                if(property.PropertyID == propertyID)
                {
                    return property;
                }
            }

            return null;
        }

        public void OnDeserialization(object sender) 
            => _itemDefinition = DefinitionRegistry<ItemDefinition>.GetByID(_id);
    }
}