using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    [Serializable]
    public sealed class InventoryActionsService : IInventoryActionsService
    {
        /// <summary>
        /// List of <see cref="ItemAction"/> that can be performed by the <see cref="IInventory"/>.
        /// </summary>
        [SerializeField]
        private ItemAction[] _itemActions;

        [NonSerialized]
        private IInventory _inventory;

        /// <summary>
        /// Context to use when performing actions of the items.
        /// </summary>
        [NonSerialized]
        private IItemActionContext _itemActionContext;

        public void Initialize(IInventory inventory, IItemActionContext actionContext)
        {
            _inventory = inventory;
            _itemActionContext = actionContext;
        }

        public void DropItem(ItemStack itemStack)
        {
            if(itemStack.IsValid == false)
            {
                return;
            }

            Slot containingSlot = Slot.None;

            foreach(IContainer container in _inventory.Containers)
            {
                Slot slot = container.GetSlot(SlotFilters.WithItem(itemStack.Item));
                if(slot.IsValid)
                {
                    containingSlot = slot;
                    break;
                }
            }

            if(TryGetActionOfType(out DropAction dropAction))
            {
                dropAction.Perform(_itemActionContext, containingSlot, itemStack);
            }
        }

        public bool TryGetActionOfType<T>(out T action) 
            where T : ItemAction
        {
            action = GetActionOfType<T>();
            return action != null;
        }

        public T GetActionOfType<T>() where T : ItemAction
        {
            foreach (ItemAction itemAction in _itemActions)
            {
                if (itemAction is T matchingAction)
                {
                    return matchingAction;
                }
            }

            return null;
        }
    }
}