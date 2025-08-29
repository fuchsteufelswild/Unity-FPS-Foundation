using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Provides query operations on container contents.
    /// </summary>
    public interface IContainerQueryService
    {
        /// <returns>Does container contain the <paramref name="item"/>?</returns>
        bool ContainsItem(IItem item);

        /// <returns>Does container contain an item that passes the <paramref name="filter"/>?</returns>
        bool ContainsItem(Func<IItem, bool> filter);

        /// <returns>Amount of items that passes the <paramref name="filter"/> are stored in the container.</returns>
        int GetItemCount(Func<IItem, bool> filter);

        /// <returns>Does container contain an item with the given <paramref name="itemID"/>.</returns>
        bool ContainsItemWithID(int itemID) => ContainsItem(ItemFilters.WithID(itemID));

        /// <returns>Amount of items that has the <paramref name="itemID"/> are stored in the container.</returns>
        int GetItemCountWithID(int itemID) => GetItemCount(ItemFilters.WithID(itemID)); 
    }
}