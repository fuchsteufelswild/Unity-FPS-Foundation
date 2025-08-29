using Nexora.InventorySystem;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    public interface IHandheldItem : IMonoBehaviour
    {
        /// <summary>
        /// Slot the item attached.
        /// </summary>
        Slot Slot { get; }
        
        /// <summary>
        /// Underlying handheld.
        /// </summary>
        IHandheld Handheld { get; }

        /// <summary>
        /// <see cref="ItemDefinition"/> that underlying item uses.
        /// </summary>
        DefinitionReference<ItemDefinition> ItemDefinition { get; }

        /// <summary>
        /// Attaches the item to the <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">Slot attach the item to.</param>
        void AttachToSlot(Slot slot);

        /// <summary>
        /// Called when the item's attached <see cref="Slot"/> changes.
        /// </summary>
        event UnityAction<Slot> AttachedSlotChanged;
    }

    /// <summary>
    /// This object should not be confused with <see cref="Item"/> or <see cref="IItem"/>. This is
    /// just an object representing a handheld object like Weapon, and connects to <see cref="IItem"/>
    /// backend and makes changes on its dynamic properties if needed. (like durability, attachment index, etc.)
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.EarlyGameLogic)]
    [RequireComponent(typeof(IHandheld))]
    public class HandheldItem : 
        MonoBehaviour, 
        IHandheldItem
    {
        [DefinitionReference(HasLabel = true, HasIcon = true, HasAssetReference = true)]
        [SerializeField]
        private DefinitionReference<ItemDefinition> _underlyingItem;

        [ReorderableList, LabelByChild("_dynamicPropertyDefinition")]
        [ReferencePicker(typeof(ItemDynamicPropertyTracker), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private ItemDynamicPropertyTracker[] _dynamicPropertyTrackers = Array.Empty<ItemDynamicPropertyTracker>();

        private IHandheld _handheld;

        public Slot Slot { get; private set; }
        public IHandheld Handheld
        {
            get
            {
                if(_handheld == null)
                {
                    _handheld = GetComponent<IHandheld>();
                    foreach(var propertyTracker in _dynamicPropertyTrackers)
                    {
                        propertyTracker.ChangeHandheld(_handheld);
                    }
                }

                return _handheld;
            }
        }

        public DefinitionReference<ItemDefinition> ItemDefinition => _underlyingItem;

        public event UnityAction<Slot> AttachedSlotChanged;

        private void Awake()
        {
            foreach (var propertyTracker in _dynamicPropertyTrackers)
            {
                propertyTracker.Initialize(this);
            }
        }

        public void AttachToSlot(Slot slot)
        {
            if(Slot == slot)
            {
                return;
            }

            Slot = slot;
            AttachedSlotChanged?.Invoke(slot);
            OnItemChanged(slot.ItemStack);
        }

        protected virtual void OnItemChanged(ItemStack itemStack) { }

        private void OnDisable() => AttachToSlot(Slot.None);
    }
}