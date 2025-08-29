using System.Collections.Generic;
using UnityEngine;

namespace Nexora.InventorySystem
{
    public sealed class ContainerAddConstraintService : IContainerAddConstraintService
    {
        private readonly IContainer _container;

        public IReadOnlyList<ContainerAddConstraint> AddConstraints { get; private set; }

        public ContainerAddConstraintService(
            IReadOnlyList<ContainerAddConstraint> addConstraints,
            IContainer container)
        {
            AddConstraints = addConstraints ?? new List<ContainerAddConstraint>();
            _container = container;
        }

        /// <summary>
        /// Gets how many items from <paramref name="itemStack"/> can be added passing through
        /// all the constraints.
        /// </summary>
        /// <param name="itemStack">Item wanted to be added.</param>
        /// <returns>(How many item can be added, Rejection message if it fails)</returns>
        public (int allowedCount, string rejectionMessage) GetAllowedCount(ItemStack itemStack)
        {
            if(itemStack.IsValid == false)
            {
                return (0, ContainerAddConstraint.InvalidItem);
            }

            int allowedCount = itemStack.Quantity;
            foreach(var constraint in AddConstraints)
            {
                allowedCount = constraint.GetAllowedCount(_container, itemStack.Item, allowedCount);

                if(allowedCount == 0)
                {
                    return (0, constraint.RejectionMessage);
                }
            }

            return (allowedCount, string.Empty);
        }
    }
}