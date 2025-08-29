using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Describes the change done on <see cref="Slot"/>.
    /// </summary>
    public enum SlotChangeType
    {
        /// <summary>
        /// Item contained 
        /// </summary>
        ItemChanged = 0,

        StackChanged = 1
    }

    public delegate void SlotChangedDelegate(in Slot itemSlot, SlotChangeType slotChangeType);

    /// <summary>
    /// Represents a slot contained in a <see cref="ISlotStorage"/>. It is merely a just a reference 
    /// to a concrete slot stored at a specific index. Propagates all calls to its owner <see cref="ISlotStorage"/>
    /// and through that gets its value.
    /// </summary>
    /// <remarks>
    /// It is better to have data stored in an upper class, as this way makes saving much easier and controllable.
    /// And this class is the proxy, callers would get what they want irrespective of the data's location.
    /// </remarks>
    public readonly struct Slot
    {
        public static readonly Slot None = default;

        public readonly ISlotStorage Storage;

        public readonly int SlotIndex;

        public Slot(ISlotStorage container, int slotIndex)
        {
            Storage = container ?? throw new ArgumentNullException(nameof(container));
            SlotIndex = slotIndex >= 0 && slotIndex < container.SlotsCount
                ? slotIndex
                : throw new ArgumentOutOfRangeException(nameof(slotIndex));
        }

        /// <summary>
        /// Just a propagator of events to the *parent*, that is, the <see cref="ISlotStorage"/>
        /// so that it can store events.
        /// </summary>
        public event SlotChangedDelegate SlotChanged
        {
            add => Storage?.AddSlotChangedListener(SlotIndex, value);
            remove => Storage?.RemoveSlotChangedListener(SlotIndex, value);
        }

        /// <summary>
        /// Is the slot currently locked? Locked slots is not interactable and cannot contain any content.
        /// </summary>
        public bool IsLocked => false;

        public bool IsValid => Storage != null;
        public bool HasItem => Item != null;
        public bool IsEmpty => HasItem == false;
        
        /// <summary>
        /// Gets the <see cref="InventorySystem.ItemStack"/> currently stored in the slot.
        /// </summary>
        public ItemStack ItemStack => Storage?.GetItemAtIndex(SlotIndex) ?? ItemStack.Empty;

        /// <summary>
        /// Gets the <see cref="IItem"/> currently stored in the slot.
        /// </summary>
        public IItem Item => ItemStack.Item;

        /// <summary>
        /// Gets how many item of same type is currently stored in the slot.
        /// </summary>
        public int ItemQuantity => ItemStack.Quantity;
        
        public bool TryGetItem(out IItem item)
        {
            item = Item;
            return item != null;
        }

        public bool TryGetStack(out ItemStack stack)
        {
            stack = ItemStack;
            return stack.Item != null;
        }

        /// <returns>Number of items from <paramref name="itemStack"/> successfuly added to the slot.</returns>
        public int SetItem(ItemStack itemStack) => Storage?.SetItemAtIndex(SlotIndex, itemStack) ?? 0;

        /// <returns>How many of the <paramref name="addAmount"/> was successfuly added/removed?</returns>
        public int AdjustItemQuantity(int addAmount) => Storage?.AdjustStackAtIndex(SlotIndex, addAmount) ?? 0;

        public void Clear() => SetItem(ItemStack.Empty);

        public readonly override bool Equals(object obj) => obj is Slot other && Equals(other);
        public readonly bool Equals(Slot other) => this == other;

        public static bool operator ==(Slot left, Slot right)
            => left.Storage == right.Storage && left.SlotIndex == right.SlotIndex;

        public static bool operator !=(Slot left, Slot right)
            => (left == right) == false;

        public readonly override int GetHashCode() => HashCode.Combine(Storage, SlotIndex);

        public readonly override string ToString()
        {
            if(IsValid == false)
            {
                return "Invalid Slot";
            }

            ItemStack itemStack = ItemStack;
            string itemString = itemStack.IsValid ? ItemStack.ToString() : "Empty";
            return $"{Storage.Name}[{SlotIndex}]: {itemString}";
        }
    }
}