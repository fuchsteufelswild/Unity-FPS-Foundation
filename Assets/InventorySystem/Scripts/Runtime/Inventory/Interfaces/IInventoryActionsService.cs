namespace Nexora.InventorySystem
{
    /// <summary>
    /// Provides interface for actions that can be performed by the <see cref="IInventory"/>.
    /// </summary>
    public interface IInventoryActionsService
    {
        /// <summary>
        /// Drops <paramref name="itemStack"/> into the world.
        /// </summary>
        void DropItem(ItemStack itemStack);

        /// <summary>
        /// Checks if can handle <see cref="ItemAction"/> of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type of the action.</typeparam>
        /// <returns><see langword="true"/> if found, <see langword="false"/> otherwise.</returns>
        bool HasActionOfType<T>() where T : ItemAction
            => TryGetActionOfType<T>(out _);

        /// <summary>
        /// Gets an <see cref="ItemAction"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the action.</typeparam>
        /// <returns><see langword="true"/> if found, <see langword="false"/> otherwise.</returns>
        bool TryGetActionOfType<T>(out T action) where T : ItemAction;

        /// <typeparam name="T">Type of the action.</typeparam>
        /// <returns><see cref="ItemAction"/> of type <typeparamref name="T"/> if found, else <see langword="null"/>.</returns>
        T GetActionOfType<T>() where T : ItemAction;
    }
}