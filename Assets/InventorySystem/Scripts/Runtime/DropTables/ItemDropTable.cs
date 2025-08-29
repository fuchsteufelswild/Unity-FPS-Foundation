using System.Collections.Generic;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Represents a drop table that generates and distributes items based on predefined rules.
    /// </summary>
    public abstract class ItemDropTable : ScriptableObject
    {
        protected const string CreateMenuPath = "Nexora/Items/Item Drop Tables/";

        /// <summary>
        /// Generates collection of <see cref="ItemStack"/> based on <paramref name="constraints"/> and <paramref name="rarityWeight"/>.
        /// </summary>
        /// <param name="constraints">Constraints that filters what can kind of items can be generated.</param>
        /// <param name="amountToGenerate">How many <see cref="ItemStack"/> should be generated.</param>
        /// <param name="rarityWeight">
        /// Multiplier that affects the generation of more unique items, values above 1 increases the likelihood of
        /// generating more unique items, and values below one increases the likelihood of generating more common items.
        /// </param>
        public abstract List<ItemStack> GenerateItemStacks(IReadOnlyList<ContainerAddConstraint> constraints, int amountToGenerate, float rarityWeight = 1f);

        /// <summary>
        /// Generates a single <see cref="ItemStack"/> based on <paramref name="constraints"/> and <paramref name="rarityWeight"/>.
        /// </summary>
        /// <param name="rarityWeight"><inheritdoc cref="GenerateItemStacks(IReadOnlyList{ContainerAddConstraint}, int, float)" path="/param[@name='constraints']"/></param>
        /// <param name="rarityWeight"><inheritdoc cref="GenerateItemStacks(IReadOnlyList{ContainerAddConstraint}, int, float)" path="/param[@name='rarityWeight']"/></param>
        public abstract ItemStack GenerateItemStack(IReadOnlyList<ContainerAddConstraint> constraints, float rarityWeight = 1f);


        // [Revisit] ?AmountToGenerate implementation?
        /// <summary>
        /// Generates collection of <see cref="ItemStack"/> and puts then into <paramref name="inventory"/>.
        /// </summary>
        /// <param name="inventory">Inventory to put generated item stacks in.</param>
        /// <param name="amountToGenerate"><inheritdoc cref="GenerateItemStacks(IReadOnlyList{ContainerAddConstraint}, int, float)" path="/param[@name='amountToGenerate']"/></param>
        /// <param name="rarityWeight"><inheritdoc cref="GenerateItemStacks(IReadOnlyList{ContainerAddConstraint}, int, float)" path="/param[@name='rarityWeight']"/></param>
        /// <returns>How many items are generated and added to the <paramref name="inventory"/>.</returns>
        public abstract int PopulateInventory(IInventory inventory, int amountToGenerate, float rarityWeight = 1f);

        /// <summary>
        /// Generates collection of <see cref="ItemStack"/> and puts then into <paramref name="container"/>.
        /// </summary>
        /// <param name="container">Container to put generated item stacks in.</param>
        /// <param name="amountToGenerate"><inheritdoc cref="GenerateItemStacks(IReadOnlyList{ContainerAddConstraint}, int, float)" path="/param[@name='amountToGenerate']"/></param>
        /// <param name="rarityWeight"><inheritdoc cref="GenerateItemStacks(IReadOnlyList{ContainerAddConstraint}, int, float)" path="/param[@name='rarityWeight']"/></param>
        /// <returns>How many items are generated and added to the <paramref name="container"/>.</returns>
        public abstract int PopulateContainer(IContainer container, int amountToGenerate, float rarityWeight = 1f);
    }
}