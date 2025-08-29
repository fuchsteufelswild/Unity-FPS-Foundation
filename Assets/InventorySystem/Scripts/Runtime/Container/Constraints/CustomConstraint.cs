using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <inheritdoc cref="ContainerAddConstraint.GetAllowedCount(IContainer, IItem, int)"/>.
    public delegate int ContainerAddConstraintDelegate(IContainer container, IItem item, int requestedCount);

    /// <summary>
    /// A class that can be serve as a constraint that runs on the specific delegate.
    /// Can be used to very specific to gameplay constraints.
    /// </summary>
    public sealed class CustomConstraint : ContainerAddConstraint
    {
        private ContainerAddConstraintDelegate _constrainDelegate;

        private CustomConstraint() { }

        public static CustomConstraint Create(ContainerAddConstraintDelegate constrainDelegate)
        {
            var instance = CreateInstance<CustomConstraint>();
            instance._constrainDelegate = constrainDelegate;
            return instance;
        }

        public override int GetAllowedCount(IContainer container, IItem item, int requestedAmount)
        {
            return _constrainDelegate?.Invoke(container, item, requestedAmount) ?? requestedAmount;
        }
    }
}