using Nexora.InventorySystem;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    //public sealed class DurabilityUseBlocker : MonoBehaviour
    //{
    //    private IHandheldItem _handheldItem;
    //    private ActionBlockerCore _useBlocker;
    //    private DynamicItemProperty _durability;

    //    private void Awake()
    //    {
    //        _handheldItem = GetComponent<IHandheldItem>();

    //        _useBlocker = _handheldItem.Handheld?.GetActionOfType<IUseActionHandler>()?.Blocker;
    //    }

    //    private void OnEnable()
    //    {
    //        if(_handheldItem == null)
    //        {
    //            return;
    //        }

    //        _handheldItem.AttachedSlotChanged += OnSlotChanged;
    //        OnSlotChanged(_handheldItem.Slot);
    //    }

    //    private void OnDisable() => _handheldItem.AttachedSlotChanged -= OnSlotChanged;

    //    private void OnSlotChanged(Slot slot) => HandleSlotChanged(slot.ItemStack);

    //    private void HandleSlotChanged(ItemStack itemStack)
    //    {
    //        ClearDurabilityHandling();

    //        if(itemStack.IsValid && _useBlocker != null
    //            && itemStack.Item.TryGetDynamicProperty(DynamicItemProperties.Durability, out _durability))
    //        {
    //            _durability.ValueChanged += OnDurabilityChanged;
    //            OnDurabilityChanged(_durability);
    //        }
    //    }

    //    private void ClearDurabilityHandling()
    //    {
    //        if (_durability == null)
    //        {
    //            return;
    //        }

    //        _durability.ValueChanged -= OnDurabilityChanged;
    //        _durability = null;
    //        _useBlocker?.RemoveBlocker(this);
    //    }

    //    private void OnDurabilityChanged(DynamicItemProperty property)
    //    {
    //        if(property.FloatValue < 0.01f)
    //        {
    //            _useBlocker.AddBlocker(this);
    //        }
    //        else
    //        {
    //            _useBlocker.RemoveBlocker(this);
    //        }
    //    }
    //}
}