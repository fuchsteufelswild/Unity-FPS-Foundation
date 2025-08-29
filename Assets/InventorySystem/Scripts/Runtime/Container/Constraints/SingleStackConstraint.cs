using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Constraint to allow only single stack of an item in the container.
    /// </summary>
    [CreateAssetMenu(menuName = AssetMenuPath + nameof(SingleStackConstraint), fileName = nameof(SingleStackConstraint))]
    public sealed class SingleStackConstraint : ContainerAddConstraint
    {
        private static SingleStackConstraint _default;
        public static SingleStackConstraint Default => _default ??= CreateInstance<SingleStackConstraint>();

        private SingleStackConstraint() { }

        public override int GetAllowedCount(IContainer container, IItem item, int requestedAmount)
        {
            int existingCount = container.GetItemCountWithID(item.ID);

            int remainingForMaxStack = item.MaxStackSize - existingCount;

            return Mathf.Min(remainingForMaxStack, requestedAmount);
        }
    }
}