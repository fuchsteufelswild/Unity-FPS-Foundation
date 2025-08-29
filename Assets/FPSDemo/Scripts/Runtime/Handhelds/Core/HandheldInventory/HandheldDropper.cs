using Nexora.InventorySystem;
using System;
using System.Drawing.Text;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Denotes that a handheld is dropped from a handheld inventory.
    /// </summary>
    /// <param name="handheld">Handheld being dropped.</param>
    public delegate void HandheldDroppedDelegate(IHandheld handheld);

    public interface IHandheldDropper
    {
        /// <summary>
        /// Can drop a handheld now?
        /// </summary>
        bool CanDrop { get; }

        /// <summary>
        /// Drops the handheld that is in hands.
        /// </summary>
        /// <param name="forceDrop">Can drop if it is doing an action (e.g Equipping/Holstering etc.).</param>
        /// <returns>If successfuly dropped.</returns>
        bool TryDropHandheld(bool forceDrop = false);

        /// <summary>
        /// Called when the drop action is performed.
        /// </summary>
        event HandheldDroppedDelegate HandheldDropped;
    }

    [Serializable]
    public sealed class HandheldDropper : IHandheldDropper
    {
        [Tooltip("Speed of the holster when dropping handheld.")]
        [SerializeField, Range(0f, 10f)]
        private float _holsterSpeed = 1f;

        [Tooltip("Delay of the drop after action is given.")]
        [SerializeField, Range(0f, 5f)]
        private float _dropDelay;

        [Tooltip("Should drop upon death?")]
        [SerializeField]
        private bool _dropOnDeath;

        private IHandheldSelector _selector;
        private IHandheldItemCache _handheldCache;
        private IHandheldsManager _handheldsManager;
        private IInventory _characterInventory;
        private IContainer _handheldLoadout;
        private MonoBehaviour _coroutineRunner;

        public bool DropOnDeath => _dropOnDeath;
        public bool CanDrop => _selector.SelectedIndex != IHandheldSelector.InvalidSelectorID && _handheldsManager.EquipmentState == ControllerState.None;

        public event HandheldDroppedDelegate HandheldDropped;

        public void Initialize(IHandheldSelector selector, IHandheldItemCache handheldCache, 
            IHandheldsManager handheldsManager, IInventory characterInventory, 
            IContainer loadout, MonoBehaviour coroutineRunner)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
            _handheldCache = handheldCache ?? throw new ArgumentNullException(nameof(handheldCache));
            _handheldsManager = handheldsManager ?? throw new ArgumentNullException(nameof(handheldsManager));
            _characterInventory = characterInventory ?? throw new ArgumentNullException(nameof(characterInventory));
            _handheldLoadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
            _coroutineRunner = coroutineRunner ?? throw new ArgumentNullException(nameof(coroutineRunner));
        }

        public bool TryDropHandheld(bool forceDrop = false)
        {
            if(ValidateDropConditions(forceDrop) == false)
            {
                return false;
            }

            // Slot empty
            ItemStack itemStack = _handheldLoadout.GetItemAtIndex(_selector.SelectedIndex);
            if(itemStack.HasItem == false)
            {
                return false;
            }

            // Item at the slot is not a handheld
            if(_handheldCache.TryGetHandheldItem(itemStack.Item.ID, out IHandheldItem handheldItem) == false)
            {
                return false;
            }

            // Can't drop handheld like this, which is not the handheld that is in hands
            if(handheldItem.Handheld != _handheldsManager.ActiveHandheld)
            {
                return false;
            }

            ExecuteDrop(itemStack, forceDrop);
            HandheldDropped?.Invoke(handheldItem.Handheld);

            return true;
        }

        private bool ValidateDropConditions(bool forceDrop)
        {
            if(_selector.SelectedIndex == IHandheldSelector.InvalidSelectorID)
            {
                return false;
            }

            if(forceDrop == false && _handheldsManager.EquipmentState != ControllerState.None)
            {
                return false;
            }

            return true;
        }

        private void ExecuteDrop(ItemStack itemStack, bool forceDrop)
        {
            float holsterSpeed = forceDrop ? float.MaxValue : _holsterSpeed;
            float delay = forceDrop ? 0f : _dropDelay;

            _selector.SelectAtIndex(IHandheldSelector.InvalidSelectorID, true, holsterSpeed);

            _coroutineRunner.InvokeDelayed(() => _characterInventory.DropItem(itemStack), delay);
        }
    }
}