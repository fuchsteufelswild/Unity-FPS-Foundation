using Nexora.FPSDemo.InventorySystem;
using Nexora.InteractionSystem;
using Nexora.InventorySystem;
using System;
using UnityEngine;

namespace Nexora.FPSDemo
{
    public class ItemPickup : ItemPickupBase
    {
        private IInteractable _interactable;

        protected IInteractable Interactable
        {
            get
            {
                if (_interactable == null && TryGetComponent(out _interactable))
                {
                    _interactable.InteractionPerformed += OnInteractionPerformed;
                }

                return _interactable;
            }
        }

        protected override void PostAttachItem(ItemStack itemStack)
        {
            if (itemStack.Item == null)
            {
                return;
            }

            Interactable?.UpdateTitle(itemStack.Item.ItemDefinition.Name);
            Interactable?.UpdateDescription(itemStack.Item.ItemDefinition.Description);
        }

        private void OnInteractionPerformed(IInteractable interactable, IInteractorContext interactorContext)
        {
            if (interactorContext.TryGetContextInterface(out ICharacterInventory characterInventory) == false)
            {
                return;
            }

            PerformInteraction(characterInventory, interactorContext);
        }

        protected virtual void PerformInteraction(ICharacterInventory characterInventory, IInteractorContext interactorContext)
        {
            base.PickupItem(characterInventory);
        }

        protected override void PlayAudio(PickupResult.PickupStatus pickupStatus) { }

        protected override void Dispose()
        {
            _interactable.Dispose();
            base.Dispose();
        }
    }
}