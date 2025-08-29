using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.InventorySystem
{
    public sealed class InventoryQueryService : IInventoryQueryService
    {
        private readonly IInventory _inventory;

        public InventoryQueryService(IInventory inventory) => _inventory = inventory;

        public bool ContainsItem(IItem item)
        {
            if(item == null)
            {
                return false;
            }

            return ContainsItem(filterItem => filterItem == item);
        }

        public bool ContainsItem(Func<IItem, bool> filter)
        {
            foreach(IContainer container in _inventory.Containers)
            {
                if(container.ContainsItem(filter))
                {
                    return true;
                }
            }

            return false;
        }

        public IContainer GetContainer(Func<IContainer, bool> filter)
        {
            foreach(IContainer container in _inventory.Containers)
            {
                if(filter == null || filter(container))
                {
                    return container;
                }
            }

            return null;
        }

        public List<IContainer> GetContainers(Func<IContainer, bool> filter)
        {
            var result = new List<IContainer>();
            foreach(IContainer container in _inventory.Containers)
            {
                if(filter == null || filter(container))
                {
                    result.Add(container);
                }
            }

            return result;
        }

        public int GetItemCount(Func<IItem, bool> filter)
        {
            int totalCount = 0;

            foreach(IContainer container in _inventory.Containers)
            {
                totalCount += container.GetItemCount(filter);
            }

            return totalCount;
        }
    }
}