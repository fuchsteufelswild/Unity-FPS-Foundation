using System;
using System.Collections.Generic;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Provides query interface for the <see cref="IInventory"/>.
    /// </summary>
    /// <remarks>
    /// It derives from an <see langword="interface"/> that is for <see cref="IContainer"/>, 
    /// as <see cref="IInventory"/> is a collection of <see cref="IContainer"/> 
    /// and can be thought as a container itself as well.
    /// </remarks>
    public interface IInventoryQueryService : IContainerQueryService
    {
        /// <returns><see cref="IContainer"/> in <see cref="IInventory"/> that passes the <paramref name="filter"/>.</returns>
        IContainer GetContainer(Func<IContainer, bool> filter);

        /// <returns>All <see cref="IContainer"/> in <see cref="IInventory"/> that pass the <paramref name="filter"/>.</returns>
        List<IContainer> GetContainers(Func<IContainer, bool> filter);
    }
}