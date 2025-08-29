using System;
using System.Collections.Generic;

using static Nexora.InventorySystem.ItemPickupBase.PickupResult;

namespace Nexora.InventorySystem
{
    public static class ItemStackSorters
    {
        public static readonly Comparison<ItemStack> ByDisplayName = (a, b) 
            => CompareByName(a.Item?.ItemDefinition.DisplayName, b.Item?.ItemDefinition.DisplayName);

        public static readonly Comparison<ItemStack> ByRarity = (a, b)
            => CompareByRarity(a.Item?.ItemDefinition.ItemRarity, b.Item?.ItemDefinition.ItemRarity);

        public static int CompareByName(string left,  string right)
        {
            if(string.IsNullOrEmpty(left))
            {
                return 1;
            }

            if (string.IsNullOrEmpty(right))
            {
                return -1;
            }

            return string.Compare(left, right, StringComparison.InvariantCultureIgnoreCase);
        }

        public static int CompareByRarity(ItemRarityDefinition left, ItemRarityDefinition right)
        {
            if(left == null)
            {
                return 1;
            }

            if(right == null)
            {
                return -1;
            }

            return left.RarityValue.CompareTo(right.RarityValue);
        }
    }

    public static class TransferUtility
    {
        /// <summary>
        /// Transfers the <see cref="ItemStack"/> stored in <paramref name="slot"/>
        /// to <paramref name="targetInventory"/>.
        /// </summary>
        /// <returns>How many items successfuly transfered.</returns>
        public static int TransferToInventory(this Slot slot, IInventory targetInventory)
        {
            if(targetInventory == null || slot.IsEmpty)
            {
                return 0;
            }

            int addedAmount = targetInventory.AddItem(slot.ItemStack).addedAmount;
            slot.AdjustItemQuantity(-addedAmount);

            return addedAmount;
        }

        /// <summary>
        /// Transfers the contents stored in the <paramref name="slot"/> to <paramref name="targetInventory"/>.
        /// </summary>
        /// <returns>If operation was successful.</returns>
        public static bool TransferOrSwapToSameTagInventory(this Slot slot, IInventory targetInventory)
        {
            if(targetInventory == null)
            {
                return false;
            }

            return slot.TransferOrSwapToSameTagContainers(targetInventory.Containers);
        }

        /// <summary>
        /// Transfers the contents stored in the <paramref name="slot"/> to any of the <paramref name="containers"/>.
        /// </summary>
        /// <returns>If operation was successful.</returns>
        public static bool TransferOrSwapToSameTagContainers(this Slot slot, IReadOnlyList<IContainer> containers)
        {
            if(containers.IsEmpty())
            {
                return false;
            }

            var itemTag = slot.Item.ItemDefinition.ItemTag;
            if (itemTag.IsNull)
            {
                return false;
            }

            Func<IContainer, bool> filter = ContainerFilters.RequiringTag(itemTag);
            var sourceContainer = slot.Storage as IContainer;

            foreach (IContainer container in containers)
            {
                if(container == sourceContainer)
                {
                    continue;
                }

                if(filter(container))
                {
                    if(slot.TransferOrSwapWithContainer(container))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Transfers the contents stored in the <paramref name="slot"/> to the <paramref name="targetContainer"/>.
        /// </summary>
        /// <returns>If operation was successful.</returns>
        public static bool TransferOrSwapWithContainer(this Slot slot, IContainer targetContainer)
        {
            var sourceContainer = slot.Storage as IContainer;
            if(sourceContainer == null || sourceContainer == targetContainer)
            {
                return false;
            }

            int addedAmount = targetContainer.AddItem(slot.ItemStack).addedAmount;
            if(addedAmount > 0)
            {
                slot.AdjustItemQuantity(-addedAmount);
                return true;
            }

            Slot targetSlot = targetContainer.GetSlot(SlotFilters.EmptyOrLastSlot);
            return TransferOrSwapWithSlot(slot, in targetSlot);
        }

        /// <summary>
        /// Swaps the contents of <paramref name="slot"/> and <paramref name="targetSlot"/>.
        /// </summary>
        /// <remarks>
        /// If <paramref name="targetSlot"/> is empty, it is a transfer.
        /// </remarks>
        /// <returns>If operation was successful.</returns>
        public static bool TransferOrSwapWithSlot(this Slot slot, in Slot targetSlot)
        {
            if(CanSwapWithSlot(slot, targetSlot) == false)
            {
                return false;
            }

            ItemStack sourceSlotStack = slot.ItemStack;
            ItemStack targetSlotStack = targetSlot.ItemStack;

            slot.SetItem(targetSlotStack);
            targetSlot.SetItem(sourceSlotStack);

            return true;
        }

        /// <returns>If the slots can swap contents.</returns>
        private static bool CanSwapWithSlot(this Slot slot, in Slot targetSlot)
        {
            // Slots must be valid
            if (slot.IsValid == false || targetSlot.IsValid == false)
            {
                return false;
            }

            // Containers must be valid
            IContainer sourceContainer = slot.Storage as IContainer;
            IContainer targetContainer = targetSlot.Storage as IContainer;
            if (sourceContainer == null || targetContainer == null)
            {
                return false;
            }

            // Both containers must be able to receive the other's stack's content
            ItemStack sourceSlotStack = slot.ItemStack;
            ItemStack targetSlotStack = targetSlot.ItemStack;

            bool canSourceReceiveTarget = targetSlot.IsEmpty || sourceContainer.CanAddItem(targetSlotStack);
            bool canTargetReceiveSource = slot.IsEmpty || targetContainer.CanAddItem(sourceSlotStack);

            return canSourceReceiveTarget && canTargetReceiveSource;
        }

        public static bool TransferOrSwapWithUntaggedContainer(this Slot slot, IReadOnlyList<IContainer> containers)
        {
            IContainer sourceContainer = slot.Storage as IContainer;

            foreach(IContainer container in containers)
            {
                if(sourceContainer == container)
                {
                    continue;
                }

                if(container.TryGetConstraint<TagConstraint>(out var constraint) && constraint.HasConstrainElements)
                {
                    continue;
                }

                if(slot.TransferOrSwapWithContainer(container))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class PickupUtility
    {
        /// <summary>
        /// Pickups the <paramref name="itemStack"/> into <paramref name="container"/> and returns
        /// <paramref name="result"/> message of the operation.
        /// </summary>
        /// <param name="container">Target container.</param>
        /// <param name="itemStack">Item to add.</param>
        /// <param name="result">Message about the result of the operation.</param>
        /// <returns>If added at least 1 item added to the <paramref name="container"/>.</returns>
        public static bool PickUpItem(IContainer container, ItemStack itemStack, out ItemPickupBase.PickupResult result)
        {
            if(CheckItemPickUpValid(itemStack, out result) == false)
            {
                return false;
            }

            var (addedAmount, rejectionMessage) = container.AddItem(itemStack);

            return GeneratePickUpResult(itemStack, addedAmount, rejectionMessage, out result);
        }

        /// <summary>
        /// Pickups the <paramref name="itemStack"/> into <paramref name="inventory"/> and returns
        /// <paramref name="result"/> message of the operation.
        /// </summary>
        /// <param name="inventory">Target container.</param>
        /// <param name="itemStack">Item to add.</param>
        /// <param name="result">Message about the result of the operation.</param>
        /// <returns>If added at least 1 item added to the <paramref name="inventory"/>.</returns>
        public static bool PickUpItem(IInventory inventory, ItemStack itemStack, out ItemPickupBase.PickupResult result)
        {
            if (CheckItemPickUpValid(itemStack, out result) == false)
            {
                return false;
            }

            var (addedAmount, rejectionMessage) = inventory.AddItem(itemStack);

            return GeneratePickUpResult(itemStack, addedAmount, rejectionMessage, out result);
        }

        /// <summary>
        /// Checks if <paramref name="itemStack"/> can be added to any container,
        /// if not generates <paramref name="result"/>.
        /// </summary>
        /// <returns>If <paramref name="itemStack"/> has a valid <see cref="IItem"/> and with quantity greater than 0.</returns>
        private static bool CheckItemPickUpValid(ItemStack itemStack, out ItemPickupBase.PickupResult result)
        {
            if(itemStack.IsValid == false)
            {
                result = new ItemPickupBase.PickupResult(PickupStatus.Failed,
                    new MessageArgs(MessageType.Error, "Item instance is null.", null));
                return false;
            }

            result = default;
            return true;
        }

        private static bool GeneratePickUpResult(ItemStack itemStack, int addedAmount, string rejectionMessage, out ItemPickupBase.PickupResult result)
        {
            if(addedAmount > 0)
            {
                result = new ItemPickupBase.PickupResult(
                    addedAmount == itemStack.Quantity ? PickupStatus.Full : PickupStatus.Partial,
                    new MessageArgs(MessageType.Info, GeneratePickupMessage(itemStack.Item, addedAmount), itemStack.Item.ItemDefinition.Icon));

                return true;
            }

            result = new ItemPickupBase.PickupResult(PickupStatus.Failed, new MessageArgs(MessageType.Error, rejectionMessage, null));
            return false;
        }

        private static string GeneratePickupMessage(IItem item, int addedAmount)
        {
            return item.IsStackable ? $"Acquired: {item.Name} x {addedAmount}" : $"Acquired: {item.Name}";
        }
    }
}