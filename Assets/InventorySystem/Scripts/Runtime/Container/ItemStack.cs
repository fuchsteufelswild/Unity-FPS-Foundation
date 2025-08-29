using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Struct for placing/using a item in inventory, or in a pickup, or in any place.
    /// It combines <see cref="IItem"/> with a quantity of <see langword="int"/> to provide
    /// robust way to represent any kind of item, from weapons to miscellanous stackable building materials.
    /// </summary>
    /// <remarks>
    /// We don't mark it as a <see langword="readonly struct"/> as it is very small <see langword="struct"/>
    /// and we might want to use it like a <see langword="anonymous class"/> as well; when we pass <see cref="ItemStack"/>
    /// into a <see langword="method"/> where we might add/remove quantity from it.
    /// </remarks>
    public struct ItemStack : IEquatable<ItemStack>
    {
        public IItem Item;

        public int Quantity;

        public static readonly ItemStack Empty = default;

        public ItemStack(IItem item, int quantity = 1)
        {
            Item = item;
            Quantity = quantity;
        }

        public readonly bool HasItem => Item != null;
        public readonly bool IsValid => Item != null && Quantity > 0;

        /// <summary>
        /// Total weight of the stack.
        /// </summary>
        public readonly float TotalWeight => IsValid ? Item.Weight * Quantity : 0f;

        /// <summary>
        /// Returns maximum amount of stack size the underlying item allows.
        /// </summary>
        public readonly int MaxStackSize => (IsValid && Item.IsStackable) ? Item.MaxStackSize : 0;

        public readonly bool CanMergeWith(ItemStack other) => Item?.ID == other.Item?.ID;
        public readonly bool CanMergeWith(int itemID) => Item?.ID == itemID;

        public ItemStack MergeWith(ItemStack other)
        {
            if(CanMergeWith(other) == false)
            {
                return this;
            }

            return new ItemStack(Item, Math.Min(Quantity + other.Quantity, Item.MaxStackSize));
        }

        public readonly override string ToString()
            => IsValid ? $"{Item} x {Quantity}" : "Empty";

        public readonly override bool Equals(object obj) => obj is ItemStack other && Equals(other);
        public readonly bool Equals(ItemStack other) => Item == other.Item && Quantity == other.Quantity;

        public override readonly int GetHashCode() => HashCode.Combine(Item, Quantity);

        public static bool operator ==(ItemStack left, ItemStack right) => left.Equals(right);
        public static bool operator !=(ItemStack left, ItemStack right) => !left.Equals(right);
    }
}