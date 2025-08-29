using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Constraint to either require any or exclude all specific list of <see cref="ItemBehaviour"/>.
    /// </summary>
    [CreateAssetMenu(menuName = AssetMenuPath + nameof(ItemBehaviourConstraint), fileName = nameof(ItemBehaviourConstraint))]
    public sealed class ItemBehaviourConstraint :
        ClassTypeFilterConstraint<ItemBehaviourConstraint,  ItemBehaviour>
    {
        [SerializeField, ClassImplements(typeof(ItemAction), AllowAbstract = false)]
        private SerializedType[] _itemBehaviourTypes;

        private ItemBehaviourConstraint() { }

        public static ItemBehaviourConstraint Create(FilterPolicy filterPolicy, Type[] itemBehaviourTypes)
        {
            var instance = CreateNew(filterPolicy, itemBehaviourTypes);
            instance._itemBehaviourTypes = new SerializedType[itemBehaviourTypes.Length];
            for (int i = 0; i < instance._itemBehaviourTypes.Length; i++)
            {
                instance._itemBehaviourTypes[i] = new SerializedType(itemBehaviourTypes[i]);
            }

            return instance;
        }

        public override int GetAllowedCount(IContainer container, IItem item, int requestedAmount)
        {
            return FilteringPolicy == FilterPolicy.Require
                ? HandleRequireFilter(item, requestedAmount)
                : HandleExcludeFilter(item, requestedAmount);
        }

        private int HandleRequireFilter(IItem item, int requestedAmount)
        {
            foreach (SerializedType itemBehaviourType in _itemBehaviourTypes)
            {
                if (item.ItemDefinition.HasBehaviourOfType(itemBehaviourType))
                {
                    return requestedAmount;
                }
            }

            return 0;
        }

        private int HandleExcludeFilter(IItem item, int requestedAmount)
        {
            foreach (SerializedType itemBehaviourType in _itemBehaviourTypes)
            {
                if (item.ItemDefinition.HasBehaviourOfType(itemBehaviourType))
                {
                    return 0;
                }
            }

            return requestedAmount;
        }
    }
}