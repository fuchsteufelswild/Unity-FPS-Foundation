namespace Nexora.InventorySystem
{
    /// <summary>
    /// Controls the weight of the <see cref="IInventory"/>.
    /// </summary>
    public interface IInventoryWeightService
    {
        /// <summary>
        /// Maximum weight that can be carried in the inventory.
        /// </summary>
        float MaxWeight { get; }

        /// <summary>
        /// Current amount weight carried in the inventory.
        /// </summary>
        float CurrentWeight { get; }
    }
}