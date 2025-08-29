using Nexora.FPSDemo.Handhelds.RangedWeapon;
using Nexora.InventorySystem;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Tracks specific dynamic property of item, and makes sure <see cref="IHandheld"/> components
    /// align with the changes or takes action.
    /// </summary>
    [Serializable]
    public abstract class ItemDynamicPropertyTracker
    {
        [SerializeField]
        protected DefinitionReference<DynamicItemPropertyDefinition> _dynamicPropertyDefinition;

        public virtual void Initialize(IHandheldItem handheldItem) => handheldItem.AttachedSlotChanged += OnItemSlotChanged;
        protected abstract void OnItemSlotChanged(Slot slot);
        public abstract void ChangeHandheld(IHandheld handheld);
    }

    /// <summary>
    /// Tracks handheld's durability and applies blocker when it is too little.
    /// </summary>
    [Serializable]
    public sealed class DurabilityUseBlocker : ItemDynamicPropertyTracker
    {
        private DynamicItemProperty _durability;

        private ActionBlockerCore _useBlocker;
        private UnityEngine.Object _blockerObject;

        public override void Initialize(IHandheldItem handheldItem)
        {
            base.Initialize(handheldItem);

            _blockerObject = handheldItem as UnityEngine.Object;
        }

        public override void ChangeHandheld(IHandheld handheld)
            => _useBlocker = handheld?.GetActionOfType<IUseActionHandler>()?.Blocker;

        protected override void OnItemSlotChanged(Slot slot) => HandleSlotChanged(slot.ItemStack);

        private void HandleSlotChanged(ItemStack itemStack)
        {
            ClearDurabilityHandling();

            if (itemStack.IsValid && _useBlocker != null
                && itemStack.Item.TryGetDynamicProperty(DynamicItemProperties.Durability, out _durability))
            {
                _durability.ValueChanged += OnDurabilityChanged;
                OnDurabilityChanged(_durability);
            }
        }

        private void ClearDurabilityHandling()
        {
            if (_durability == null)
            {
                return;
            }

            _durability.ValueChanged -= OnDurabilityChanged;
            _durability = null;
            _useBlocker?.RemoveBlocker(_blockerObject);
        }

        private void OnDurabilityChanged(DynamicItemProperty property)
        {
            if (property.FloatValue < 0.01f)
            {
                _useBlocker.AddBlocker(_blockerObject);
            }
            else
            {
                _useBlocker.RemoveBlocker(_blockerObject);
            }
        }
    }

    /// <summary>
    /// Keeps tracks of the property of how many ammo available in the magazine,
    /// and makes sure <see cref="IGun"/>'s
    /// <see cref="DynamicItemProperty"/> is aligned with <see cref="IGunMagazineBehaviour"/>.
    /// </summary>
    [Serializable]
    public sealed class AmmoInMagazineTracker : ItemDynamicPropertyTracker
    {
        private DynamicItemProperty _ammoInMagazineProperty;
        private IGunMagazineBehaviour _magazine;
        private IGun _gun;

        public override void ChangeHandheld(IHandheld handheld)
        {
            _gun = handheld as IGun;
            
            if(_gun == null)
            {
                Debug.LogError("Only gun handhelds can have ammo in magazine property.");
                return;
            }

            _magazine = _gun.Magazine;
            _magazine.AmmoCountChanged += OnAmmoCountChanged;
            _gun.AddComponentChangedListener(GunBehaviourType.MagazineSystem, OnMagazineChanged);
        }

        private void OnAmmoCountChanged(int previousAmmo, int newAmmo)
        {
            if(_ammoInMagazineProperty != null)
            {
                _ammoInMagazineProperty.IntegerValue = newAmmo;
            }
        }

        private void OnMagazineChanged()
        {
            _magazine.AmmoCountChanged -= OnAmmoCountChanged;
            _magazine = _gun.Magazine;
            _magazine.AmmoCountChanged += OnAmmoCountChanged;
        }

        protected override void OnItemSlotChanged(Slot slot)
        {
            _ammoInMagazineProperty = null;

            if(slot.ItemStack.Item?.TryGetDynamicProperty(_dynamicPropertyDefinition.Name, out _ammoInMagazineProperty) ?? false)
            {
                _magazine.ForceSetAmmo(_ammoInMagazineProperty.IntegerValue);
            }
        }
    }
}