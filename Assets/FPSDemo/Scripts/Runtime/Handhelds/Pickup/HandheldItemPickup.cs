using Nexora.FPSDemo.InventorySystem;
using Nexora.InteractionSystem;
using Nexora.InventorySystem;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public class HandheldItemPickup : ItemPickup
    {
        protected override void PerformInteraction(ICharacterInventory characterInventory, IInteractorContext interactorContext)
        {
            IContainer loadoutContainer = characterInventory.GetContainer(ContainerFilters.RequiringTag(ItemTags.HandheldTag));

            if(loadoutContainer == null)
            {
                base.PickupItem(characterInventory);
                return;
            }

            if(interactorContext.TryGetContextInterface(out IHandheldsInventory handheldsInventory) == false)
            {
                base.PickupItem(characterInventory);
                return;
            }

            if(AttachedItem.Item.IsStackable == false && loadoutContainer.IsFull())
            {
                handheldsInventory.TryDropHandheld(true);
            }

            if(PickupItem(characterInventory, loadoutContainer) != PickupResult.PickupStatus.Failed)
            {
                EquipPickup(loadoutContainer, handheldsInventory);
            }
            else
            {
                base.PickupItem(characterInventory);
            }
        }

        private void EquipPickup(IContainer loadoutContainer, IHandheldsInventory handheldsInventory)
        {
            Slot slot = loadoutContainer.GetSlot(SlotFilters.WithItem(AttachedItem.Item));
            if(slot.IsValid == false)
            {
                slot = loadoutContainer.GetSlot(SlotFilters.WithItemID(AttachedItem.Item.ID));
            }

            if(slot.IsValid && slot.SlotIndex != handheldsInventory.SelectedIndex)
            {
                handheldsInventory.SelectAtIndex(slot.SlotIndex, false);
            }
        }
    }
}