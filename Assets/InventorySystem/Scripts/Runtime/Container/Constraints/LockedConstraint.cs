using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Constraint to lock the storage, cannot add any items.
    /// </summary>
    [CreateAssetMenu(menuName = AssetMenuPath + nameof(LockedConstraint), fileName = nameof(LockedConstraint))]
    public sealed class LockedConstraint : ContainerAddConstraint
    {
        private static LockedConstraint _default;
        public static LockedConstraint Default => _default ??= CreateInstance<LockedConstraint>();
        private LockedConstraint() { }

        public override int GetAllowedCount(IContainer container, IItem item, int requestedAmount)
        {
            return 0;
        }
    }
}