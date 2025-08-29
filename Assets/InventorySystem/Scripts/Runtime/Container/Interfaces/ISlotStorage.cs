using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Core interface for item-based <see cref="Slot"/> operations.
    /// </summary>
    /// <remarks>
    /// Can be used as an <see cref="IEnumerable{T}"/> of it.
    /// </remarks>
    public interface ISlotStorage : IEnumerable<ItemStack>
    {
        /// <summary>
        /// Inventory this slot storage is situated in.
        /// </summary>
        IInventory Inventory { get; }

        /// <summary>
        /// Name of the storage, this is used to identify them.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// How many slots does this storage has.
        /// </summary>
        int SlotsCount { get; }

        /// <summary>
        /// If this storage allows stacking items of the same type in a single <see cref="Slot"/>?
        /// </summary>
        bool AllowStacking { get; }

        /// <summary>
        /// Current weight of the storage.
        /// </summary>
        float CurrentWeight { get; }

        /// <summary>
        /// Maximum weight that can be carried in this storage.
        /// </summary>
        float MaxWeight { get; }

        /// <summary>
        /// Gets <see cref="ItemStack"/> from the <see cref="Slot"/> at <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="slotIndex">Slot index.</param>
        /// <returns>Item stored in the <see cref="Slot"/>.</returns>
        ItemStack GetItemAtIndex(int slotIndex);

        /// <summary>
        /// Sets the <see cref="ItemStack"/> of the <see cref="Slot"/> at <paramref name="slotIndex"/>
        /// as <paramref name="newStack"/>.
        /// </summary>
        /// <param name="slotIndex">Slot index.</param>
        /// <param name="newStack">New item to be put at slot.</param>
        /// <returns>Amount of items from the <paramref name="newStack"/> was added to the slot.</returns>
        int SetItemAtIndex(int slotIndex, ItemStack newStack);

        /// <summary>
        /// Adjust the stack amount of the <see cref="ItemStack"/> of the <see cref="Slot"/> at <paramref name="slotIndex"/>
        /// as <paramref name="adjustedQuatity"/>.
        /// </summary>
        /// <param name="slotIndex">Slot index.</param>
        /// <param name="adjustedQuatity">Adjusted stack amount.</param>
        /// <returns>How many of the <paramref name="addAmount"/> was successfuly added/removed?</returns>
        int AdjustStackAtIndex(int slotIndex, int addAmount);

        /// <summary>
        /// Sorts the storage using the <paramref name="comparison"/>.
        /// </summary>
        void SortItems(Comparison<ItemStack> comparison);

        /// <summary>
        /// Clears the storage of all items.
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds a listener to the <see langword="delegate"/> of a <see cref="Slot"/>.
        /// </summary>
        void AddSlotChangedListener(int slotIndex, SlotChangedDelegate listenerAction);

        /// <summary>
        /// Removes a listener from the <see langword="delegate"/> of a <see cref="Slot"/>.
        /// </summary>
        void RemoveSlotChangedListener(int slotIndex, SlotChangedDelegate listenerAction);

        /// <summary>
        /// Fired upon a change is done on the <see cref="ISlotStorage"/> itself.
        /// </summary>
        event UnityAction SlotStorageChanged;

        /// <summary>
        /// Fired upon a change done on a specific <see cref="Slot"/> of the <see cref="ISlotStorage"/>.
        /// </summary>
        event SlotChangedDelegate SlotChanged;
    }

    public struct SlotEnumerator : IEnumerator<Slot>
    {
        private readonly ISlotStorage _storage;
        private int _currentIndex;

        public SlotEnumerator(ISlotStorage storage)
        {
            _storage = storage;
            _currentIndex = -1;
        }

        public Slot Current => new(_storage, _currentIndex);
        object IEnumerator.Current => Current;
        public void Dispose() { }
        public bool MoveNext() => ++_currentIndex < _storage.SlotsCount;
        public void Reset() => _currentIndex = -1;
    }

    public readonly struct SlotEnumerable : IEnumerable<Slot>
    {
        private readonly ISlotStorage _storage;

        public SlotEnumerable(ISlotStorage storage) => _storage = storage;

        private readonly SlotEnumerator GetEnumerator() => new(_storage);

        readonly IEnumerator<Slot> IEnumerable<Slot>.GetEnumerator() => GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}