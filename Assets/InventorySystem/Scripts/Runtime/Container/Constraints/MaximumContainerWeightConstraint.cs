using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Constraint to limit addition based on <see cref="IContainer"/>'s maximum carried weight.
    /// </summary>
    public class MaximumContainerWeightConstraint : ContainerAddConstraint
    {
        private static MaximumContainerWeightConstraint _default;
        public static MaximumContainerWeightConstraint Default 
            => _default ??= CreateInstance<MaximumContainerWeightConstraint>();

        private MaximumContainerWeightConstraint() { }

        public override int GetAllowedCount(IContainer container, IItem item, int requestedAmount)
        {
            float availableWeight = container.GetAvailableWeight();

            int possibleAmount = (int)(availableWeight / item.Weight);
            return Mathf.Min(possibleAmount, requestedAmount);
        }
    }
}