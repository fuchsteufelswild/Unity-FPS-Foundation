using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.InventorySystem
{
    public sealed class InventoryOperationsService : IInventoryOperationsService
    {
        private readonly IInventory _inventory;

        public InventoryOperationsService(IInventory inventory) => _inventory = inventory;

        public (int addedAmount, string rejectionMessage) AddItemsWithID(int itemID, int amount)
        {
            if (DefinitionRegistry<ItemDefinition>.TryGetByID(itemID, out ItemDefinition itemDefinition) == false)
            {
                return (0, ContainerAddConstraint.InvalidItem);
            }

            // Duplication is justified, it must be this way, check ContainerOperationsService.AddItemsWithID
            // for the details.
            int remainingToAdd = amount;

            IReadOnlyList<IContainer> containers = _inventory.Containers;
            string lastRejectionMessage = string.Empty;
            int totalAdded = 0;

            for (int i = 0; i < containers.Count && remainingToAdd > 0; i++)
            {
                var (added, rejectionMessage) = containers[i].AddItemsWithID(itemID, remainingToAdd);
                lastRejectionMessage = rejectionMessage;

                remainingToAdd -= added;
                totalAdded += added;
            }

            return (totalAdded, totalAdded > 0 ? string.Empty : lastRejectionMessage);
        }

        public (int addedAmount, string rejectionMessage) AddItem(ItemStack itemStack)
        {
            int remainingToAdd = itemStack.Quantity;

            IReadOnlyList<IContainer> containers = _inventory.Containers;
            string lastRejectionMessage = string.Empty;
            int totalAdded = 0;

            for(int i = 0; i <  containers.Count && remainingToAdd > 0; i++)
            {
                itemStack.Quantity = remainingToAdd;

                var (added, rejectionMessage) = containers[i].AddItem(itemStack);
                lastRejectionMessage = rejectionMessage;

                remainingToAdd -= added;
                totalAdded += added;
            }

            return (totalAdded, totalAdded > 0 ? string.Empty : lastRejectionMessage);
        }

        public int RemoveItems(Func<IItem, bool> filter, int amount)
        {
            int remainingToRemove = amount;
            int totalRemoved = 0;

            IReadOnlyList<IContainer> containers = _inventory.Containers;

            for(int i = 0; i < containers.Count && remainingToRemove > 0; i++)
            {
                int removedAmount = containers[i].RemoveItems(filter, remainingToRemove);
                totalRemoved += removedAmount;
                remainingToRemove -= removedAmount;
            }

            return totalRemoved;
        }
    }
}