using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Provides an interface for general Add/Remove operations on the <see cref="IContainer"/>
    /// irrespective of a specific slot.
    /// </summary>
    public interface IContainerOperationsService
    {
        /// <summary>
        /// Adds the given <paramref name="itemStack"/> into the container.
        /// </summary>
        /// <param name="itemStack">Item stack to be added (item, quantity).</param>
        /// <returns>(How many items added, a message if it failed)</returns>
        (int addedAmount, string rejectionMessage) AddItem(ItemStack itemStack);

        /// <summary>
        /// Adds <paramref name="amount"/> items with the <paramref name="itemID"/> into the container.
        /// </summary>
        /// <param name="itemID">ID of the item type that is to be added.</param>
        /// <param name="amount">How many items do we want to add?</param>
        /// <returns>(How many items added, a message if it failed)</returns>
        (int addedAmount, string rejectionMessage) AddItemsWithID(int itemID, int amount);

        /// <summary>
        /// Removes 'n' 'item' from the container. 
        /// Where 'n' is the quantity of the stack, and
        /// 'item' is the underlying 'item' in the stack.
        /// </summary>
        /// <returns>Amount of items removed.</returns>
        int RemoveItem(ItemStack itemStack) => RemoveItems(item => item == itemStack.Item, itemStack.Quantity);

        /// <summary>
        /// Removes <paramref name="amount"/> items that passed the <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">Used to filter items.</param>
        /// <param name="amount">How many items we want to remove?</param>
        /// <returns>Amount of items removed.</returns>
        int RemoveItems(Func<IItem, bool> filter, int amount);

        /// <summary>
        /// Removes <paramref name="amount"/> items that has the <paramref name="itemID"/>.
        /// </summary>
        /// <param name="itemID">ID of the item type that will be removed.</param>
        /// <param name="amount">How many items we want to remove?</param>
        /// <returns>Amount of items removed.</returns>
        int RemoveItemsWithID(int itemID, int amount) => RemoveItems(ItemFilters.WithID(itemID), amount);
    }
}