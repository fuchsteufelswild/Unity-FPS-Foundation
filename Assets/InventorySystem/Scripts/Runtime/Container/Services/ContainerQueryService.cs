using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    public sealed class ContainerQueryService : IContainerQueryService
    {
        private readonly ISlotStorage _storage;

        public ContainerQueryService(ISlotStorage storage) => _storage = storage;

        public bool ContainsItem(IItem item)
        {
            if(item == null)
            {
                return false;
            }

            return ContainsItem(containerItem => containerItem == item);
        }

        public bool ContainsItem(Func<IItem, bool> filter)
        {
            for(int i = 0; i < _storage.SlotsCount; i++)
            {
                IItem item = _storage.GetItemAtIndex(i).Item;
                if(item != null && filter(item))
                {
                    return true;
                }
            }

            return false;
        }

        public int GetItemCount(Func<IItem, bool> filter)
        {
            int count = 0;
            for(int i = 0; i < _storage.SlotsCount; i++)
            {
                ItemStack itemStack = _storage.GetItemAtIndex(i);
                if(itemStack.IsValid && filter(itemStack.Item))
                {
                    count += itemStack.Quantity;
                }
            }

            return count;
        }
    }
}