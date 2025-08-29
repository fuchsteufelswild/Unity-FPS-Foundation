using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Constraint to either require any or exclude all specific list of <see cref="ItemAction"/>.
    /// </summary>
    [CreateAssetMenu(menuName = AssetMenuPath + nameof(ItemActionConstraint), fileName = nameof(ItemActionConstraint))]
    public sealed class ItemActionConstraint :
        ClassTypeFilterConstraint<ItemActionConstraint, ItemAction>
    {
        [SerializeField, ClassImplements(typeof(ItemAction), AllowAbstract = false)]
        private SerializedType[] _itemActionTypes;

        private ItemActionConstraint() { }

        public static ItemActionConstraint Create(FilterPolicy filterPolicy, Type[] itemActionTypes)
        {
            var instance = CreateNew(filterPolicy, itemActionTypes);
            instance._itemActionTypes = new SerializedType[itemActionTypes.Length];
            for(int i = 0; i < instance._itemActionTypes.Length; i++)
            {
                instance._itemActionTypes[i] = new SerializedType(itemActionTypes[i]);
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
            foreach (SerializedType itemActionType in _itemActionTypes)
            {
                if (item.ItemDefinition.HasActionOfType(itemActionType))
                {
                    return requestedAmount;
                }
            }

            return 0;
        }

        private int HandleExcludeFilter(IItem item, int requestedAmount)
        {
            foreach (SerializedType itemActionType in _itemActionTypes)
            {
                if (item.ItemDefinition.HasActionOfType(itemActionType))
                {
                    return 0;
                }
            }

            return requestedAmount;
        }
    }
}