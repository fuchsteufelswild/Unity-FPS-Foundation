using Nexora.InventorySystem;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Ammo storage that uses ammo from the inventory of the character.
    /// </summary>
    public sealed class InventoryAmmoStorage : GunAmmoStorageBehaviour
    {
        [Tooltip("Item that represents the type of ammo this storage uses (e.g 5.56mm, 7.62mm etc.).")]
        [SerializeField]
        private DefinitionReference<ItemDefinition> _ammoItem;

        // TODO: Create ContainerTagDefinition class like ItemTagDefinition and use if needed to use structures like
        // 'Ammo Pouch'
        [Tooltip("[Optional]! tag of the inventory container that ammo is extracted from.")]
        [SerializeField]
        private DefinitionReference<ItemTagDefinition> _containerTag;

        private IContainer _ammoContainer;

        public override int CurrentAmmo => _ammoContainer?.GetItemCountWithID(_ammoItem) ?? 0;

        public override event UnityAction<int> AmmoCountChanged;

        public override bool CanAttach() => base.CanAttach() && CurrentAmmo > 0;

        protected override void OnEnable()
        {
            IInventory inventory = Handheld.Character.Inventory;
            if(inventory == null)
            {
                return;
            }

            _ammoContainer = string.IsNullOrEmpty(_containerTag)
                ? inventory.GetContainer(ContainerFilters.WithoutTag)
                : inventory.GetContainer(ContainerFilters.RequiringTag(_containerTag));

            if(_ammoContainer != null)
            {
                _ammoContainer.SlotStorageChanged += OnContainerChanged;
                OnContainerChanged();
            }

            base.OnEnable();
        }

        private void OnContainerChanged() => AmmoCountChanged?.Invoke(CurrentAmmo);

        private void OnDisable()
        {
            if(_ammoContainer != null)
            {
                _ammoContainer.SlotStorageChanged -= OnContainerChanged;
            }
        }

        public override int AddAmmo(int amount) => _ammoContainer?.AddItemsWithID(_ammoItem, amount).addedAmount ?? 0;
        public override int TryRemoveAmmo(int amount) => _ammoContainer?.RemoveItemsWithID(_ammoItem, amount) ?? 0;
    }
}