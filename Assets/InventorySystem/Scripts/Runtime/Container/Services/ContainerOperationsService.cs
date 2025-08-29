using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    public class ContainerOperationsService : IContainerOperationsService
    {
        private readonly ISlotStorage _storage;
        private readonly IContainerAddConstraintService _addConstraints;

        public ContainerOperationsService(ISlotStorage storage, IContainerAddConstraintService addConstraints)
        {
            _storage = storage;
            _addConstraints = addConstraints;
        }

        public (int addedAmount, string rejectionMessage) AddItemsWithID(int itemID, int amount)
        {
            if (DefinitionRegistry<ItemDefinition>.TryGetByID(itemID, out ItemDefinition itemDefinition) == false)
            {
                return (0, ContainerAddConstraint.InvalidItem);
            }

            var (allowedCount, rejectionMessage) = 
                FilterThroughConstraints(new ItemStack(Item.CreateSharedDummyItem(itemDefinition), amount));

            int remainingToAdd = Math.Min(allowedCount, amount);
            int totalAdded = AddToExistingStacks(new ItemStack(new Item(itemDefinition), remainingToAdd), ref remainingToAdd);

            FillEmptySlots();

            return (totalAdded,
                totalAdded > 0
                ? string.Empty
                : ContainerAddConstraint.InventoryFull);

            // This has to be done in this way, cannot call AddItem
            // Because, think of a case in which we want to add 5 Swords to the inventory
            // If we don't generate a new item every time we add a sword all will share same
            // <DynamicProperties>
            void FillEmptySlots()
            {
                for (int i = 0; i < _storage.SlotsCount && remainingToAdd > 0; i++)
                {
                    ItemStack existingStack = _storage.GetItemAtIndex(i);
                    // Is Empty?
                    if (existingStack.HasItem == false)
                    {
                        int addedAmount = _storage.SetItemAtIndex(i, new ItemStack(new Item(itemDefinition), remainingToAdd));
                        totalAdded += addedAmount;
                        remainingToAdd -= addedAmount;
                    }
                }
            }
        }

        public (int addedAmount, string rejectionMessage) AddItem(ItemStack itemStack)
        {
            var (allowedCount, rejectionMessage) = FilterThroughConstraints(itemStack);

            int remainingToAdd = Math.Min(allowedCount, itemStack.Quantity);

            int totalAdded = AddToExistingStacks(itemStack, ref remainingToAdd);
            totalAdded += AddToEmptySlots(itemStack, ref remainingToAdd);

            return (totalAdded, 
                totalAdded > 0 
                ? string.Empty 
                : ContainerAddConstraint.InventoryFull);
        }

        /// <summary>
        /// Filters the given <paramref name="itemStack"/> through all of the constraints described
        /// for the container.
        /// </summary>
        private (int allowedCount, string rejectionMessage) FilterThroughConstraints(ItemStack itemStack)
        {
            if (itemStack.IsValid == false)
            {
                return (0, ContainerAddConstraint.InvalidItem);
            }

            var (allowedCount, rejectionMessage) = _addConstraints.GetAllowedCount(itemStack);
            if (allowedCount == 0)
            {
                return (0, rejectionMessage);
            }

            return (allowedCount, string.Empty);
        }

        /// <summary>
        /// Adds the <paramref name="itemStack"/> to existing slots that already has
        /// the same item type contained in them and has at least one quantity.
        /// </summary>
        /// <param name="itemStack">Item to add.</param>
        /// <param name="remainingToAdd">How many another we can add?</param>
        /// <returns>Added total amount.</returns>
        private int AddToExistingStacks(ItemStack itemStack, ref int remainingToAdd)
        {
            if(_storage.AllowStacking == false || itemStack.Item.IsStackable == false)
            {
                return 0;
            }

            int totalAdded = 0;
            for(int i = 0; i < _storage.SlotsCount && remainingToAdd > 0; i++)
            {
                ItemStack existingItem = _storage.GetItemAtIndex(i);
                if (existingItem.IsValid && existingItem.CanMergeWith(itemStack))
                {
                    int added = _storage.AdjustStackAtIndex(i, remainingToAdd);
                    totalAdded += added;
                    remainingToAdd -= added;
                }
            }

            return totalAdded;
        }

        /// <summary>
        /// Adds the <paramref name="itemStack"/> to slots that are empty, that is has no item in it.
        /// </summary>
        /// <param name="itemStack">Item to add.</param>
        /// <param name="remainingToAdd">How many another we can add?</param>
        /// <returns>Added total amount.</returns>
        private int AddToEmptySlots(ItemStack itemStack, ref int remainingToAdd)
        {
            int totalAdded = 0;

            for(int i = 0; i < _storage.SlotsCount && remainingToAdd > 0; i++)
            {
                ItemStack existingItem = _storage.GetItemAtIndex(i);
                if(existingItem.HasItem == false)
                {
                    int addedAmount = _storage.SetItemAtIndex(i, new ItemStack(itemStack.Item, remainingToAdd));
                    totalAdded += addedAmount;
                    remainingToAdd -= addedAmount;
                }
            }

            return totalAdded;
        }

        public int RemoveItems(Func<IItem, bool> filter, int amount)
        {
            int remainingToRemove = amount;
            int totalRemoved = 0;
            for (int i = 0; i < _storage.SlotsCount && remainingToRemove > 0; i++)
            {
                ItemStack existingItem = _storage.GetItemAtIndex(i);
                if (existingItem.IsValid && filter(existingItem.Item))
                {
                    int removedAmount = _storage.AdjustStackAtIndex(i, -remainingToRemove);
                    totalRemoved += removedAmount;
                    remainingToRemove -= removedAmount;
                }
            }

            return totalRemoved;
        }
    }
}