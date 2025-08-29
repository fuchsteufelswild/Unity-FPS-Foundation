using Nexora.FPSDemo.Handhelds;
using Nexora.InventorySystem;
using System.Collections;
using UnityEngine;

namespace Nexora.FPSDemo.InventorySystem
{
    [CreateAssetMenu(menuName = CreateMenuPath + "Equip Action", fileName = "CharacterEquipAction_")]
    public sealed class CharacterEquipAction : ItemAction
    {
        public override bool CanPerform(IItemActionContext actionContext, ItemStack itemStack) 
            => itemStack.Item.ItemDefinition.ItemTag.IsNull == false;

        public override float GetDuration(IItemActionContext actionContext, ItemStack itemStack) => 0f;

        protected override IEnumerator ExecuteAction(IItemActionContext actionContext, Slot slot, ItemStack itemStack, float duration)
        {
            var itemTag = itemStack.Item.ItemDefinition.ItemTag;
            IContainer targetContainer = actionContext.Inventory.GetContainer(ContainerFilters.RequiringTag(itemTag));

            if(targetContainer == null)
            {
                yield break;
            }

            bool isHandheld = itemTag == ItemTags.HandheldTag;
            bool isInTargetContainer = slot.Storage == (targetContainer as ISlotStorage);

            // Is the item being equipped is a handheld and the character has a loadout designed for them?
            if (isHandheld && actionContext.TryGetContextInterface(out IHandheldsInventory loadout))
            {
                // Just select the item if already in the loadout
                if(isInTargetContainer)
                {
                    loadout.SelectAtIndex(targetContainer.GetSlot(SlotFilters.WithItem(itemStack.Item)).SlotIndex);
                    yield break;
                }

                // Add item to the loadout and remove from old container
                if (targetContainer.AddItem(itemStack).addedAmount > 0)
                {
                    loadout.SelectAtIndex(targetContainer.GetSlot(SlotFilters.WithItem(itemStack.Item)).SlotIndex);
                    slot.Clear();
                }
                else
                {
                    // If space is not available then swap with already equipped handheld
                    slot.TransferOrSwapWithSlot(targetContainer.GetSlot(loadout.SelectedIndex));
                }
            }
            else
            {
                // If the target container is not the handheld loadout, then just add to empty slot
                if(isInTargetContainer == false && targetContainer.AddItem(itemStack).addedAmount > 0)
                {
                    slot.Clear();
                }
                else
                {
                    // If container is full then just swap with the last slot
                    slot.TransferOrSwapWithSlot(targetContainer.GetSlot(targetContainer.SlotsCount - 1));
                }
            }
        }
    }
}