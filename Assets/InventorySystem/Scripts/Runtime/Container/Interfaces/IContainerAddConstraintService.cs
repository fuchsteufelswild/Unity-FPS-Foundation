using System.Collections.Generic;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Handles validation and constraints for item placement.
    /// </summary>
    public interface IContainerAddConstraintService
    {
        IReadOnlyList<ContainerAddConstraint> AddConstraints { get; }

        /// <param name="itemStack">Item that wanted to be added.</param>
        /// <returns>If at least one item can be added.</returns>
        bool CanAddItem(ItemStack itemStack) => GetAllowedCount(itemStack).allowedCount > 0;

        /// <summary>
        /// How many items of the <paramref name="itemStack"/> can be added after processed through all the <see cref="AddConstraints"/>?
        /// </summary>
        /// <param name="itemStack">Item that wanted to be added.</param>
        /// <returns>(How many items can be added, Rejection message if it cannot any)</returns>
        (int allowedCount, string rejectionMessage) GetAllowedCount(ItemStack itemStack);
    }
}