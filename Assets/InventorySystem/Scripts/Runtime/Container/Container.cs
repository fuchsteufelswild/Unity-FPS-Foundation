using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Container class that represents any type of container that is customizable through <see cref="ContainerAddConstraint"/>
    /// and provides customizable queries through <see cref="ContainerQueryService"/>, and with rich interface of
    /// Add/Remove methods to modified the storage, along with extension methods for additional support.
    /// <br></br><br></br>
    /// <b><see cref="Container"/> must be created through <see cref="Builder"/>.</b>
    /// </summary>
    /// <remarks>
    /// One could argue about the proliferation of <see langword="delegates"/> is bad and that it would 
    /// be better with <see cref="IEventListener{TEvent}"/>, <see cref="IEventPublisher{TEvent}"/> system.
    /// But, in the system of propagating events through the upstream hierarchy, the intent gets murky and
    /// it becomes more complex than it is, not to mention the code bloat it results in (or the confusion as a result of mixed approaches).
    /// With the explicit comments, it is apparent now, and it is just a one variable rather than invoking the event system.
    /// </remarks>
    [Serializable]
    public sealed class Container :
        IContainer,
        IDeserializationCallback
    {
        /// <summary>
        /// Field that denotes the name of the container, it is used for identifying it.
        /// </summary>
        [SerializeField]
        private string _containerName;

        [SerializeField]
        private bool _allowStacking;

        [SerializeField]
        private ItemStack[] _slots;

        [SerializeField]
        private float _maxWeight;

        [NonSerialized]
        private float _currentWeight;

        [NonSerialized]
        private IInventory _inventory;

        [NonSerialized]
        private ContainerAddConstraint[] _addConstraints;

        /// <summary>
        /// Controls intialization status of this object as there are more than one way for this
        /// object to be initialized (created from save, or directly created), so that either
        /// <see langword="constructor"/> or <see langword="deserializer"/> is called.
        /// </summary>
        [NonSerialized]
        private bool _isInitialized;

        /// <summary>
        /// One delegate for each slot.
        /// </summary>
        [NonSerialized]
        private SlotChangedDelegate[] _slotChangedActions;

        // Helper services
        [NonSerialized]
        private IContainerAddConstraintService _addConstraintService;
        [NonSerialized]
        private IContainerOperationsService _containerOperationsService;
        [NonSerialized]
        private IContainerQueryService _containerQueryService;

        /// <summary>
        /// Inventory that owns this container.
        /// </summary>
        public IInventory Inventory => _inventory;

        public string Name => _containerName;
        public int SlotsCount => _slots.Length;
        public bool AllowStacking => _allowStacking;
        public float CurrentWeight => _currentWeight;
        public float MaxWeight => _maxWeight;

        public IReadOnlyList<ContainerAddConstraint> AddConstraints => _addConstraints;

        public event UnityAction SlotStorageChanged;
        public event SlotChangedDelegate SlotChanged;

        public static Builder StartBuilder() => new Builder();

        private Container() { }

        private Container(
            string containerName, 
            bool allowStacking,
            int slotCount,
            float maximumWeight, 
            IInventory inventory, 
            params ContainerAddConstraint[] addConstraints)
        {
            _containerName = containerName;
            _allowStacking = allowStacking;
            _slots = new ItemStack[slotCount];
            _maxWeight = maximumWeight;
            _inventory = inventory;
            _addConstraints = addConstraints ?? Array.Empty<ContainerAddConstraint>();
            _isInitialized = true;

            _slotChangedActions = new SlotChangedDelegate[_slots.Length];
            InitializeServices();
        }

        private void InitializeServices()
        {
            _containerQueryService = new ContainerQueryService(this);
            _addConstraintService = new ContainerAddConstraintService(_addConstraints, this);
            _containerOperationsService = new ContainerOperationsService(this, _addConstraintService);
        }

        #region Deserialize/Loading
        public void OnDeserialization(object sender)
        {
            InitializeServices();
            _slotChangedActions = new SlotChangedDelegate[_slots.Length];

            _currentWeight = _slots.Sum(itemStack => itemStack.IsValid ? itemStack.TotalWeight : 0f);
        }

        /// <summary>
        /// Called when loading saves, after the deserialization process. 
        /// As in that case, we will be using deserialization service and not the constructor.
        /// </summary>
        /// <remarks>
        /// We put in the unserialized parts that are omitted during the save.
        /// </remarks>
        public void InitializeAfterDeserialize(IInventory inventory, params ContainerAddConstraint[] addConstraints)
        {
            if(_isInitialized)
            {
                return;
            }

            _inventory = inventory;
            _addConstraints = addConstraints ?? Array.Empty<ContainerAddConstraint>();
            _isInitialized = true;
        }
        #endregion


        public ItemStack GetItemAtIndex(int slotIndex) => _slots[slotIndex];

        public int SetItemAtIndex(int slotIndex, ItemStack newStack)
        {
            if(newStack.HasItem == false)
            {
                newStack = ItemStack.Empty;
            }    

            return _slots[slotIndex].HasItem
                ? ReplaceItemAtIndex(slotIndex, newStack)
                : AddNewItemToIndex(slotIndex, newStack);
        }

        private int ReplaceItemAtIndex(int slotIndex, ItemStack newStack)
        {
            ClearSlot(slotIndex);

            int addedAmount = AddNewItemToIndex(slotIndex, newStack);
            if(addedAmount == 0)
            {
                // We cleared the slot, so firing event for this 
                // even though no new item is put to the slot
                InvokeSlotChangedEvent(slotIndex, SlotChangeType.ItemChanged);
            }

            return addedAmount;
        }

        private void ClearSlot(int slotIndex)
        {
            ItemStack existingItem = _slots[slotIndex];
            _currentWeight -= existingItem.TotalWeight;
            _slots[slotIndex] = ItemStack.Empty;
        }

        private int AddNewItemToIndex(int slotIndex, ItemStack newStack)
        {
            if(newStack.HasItem == false)
            {
                return 0;
            }

            newStack.Quantity = _addConstraintService.GetAllowedCount(newStack).allowedCount;
            _slots[slotIndex] = newStack;
            _currentWeight += newStack.TotalWeight;

            InvokeSlotChangedEvent(slotIndex, SlotChangeType.ItemChanged);

            return newStack.Quantity;
        }

        private void InvokeSlotChangedEvent(int slotIndex, SlotChangeType slotChangeType)
        {
            var dummySlot = new Slot(this, slotIndex);
            _slotChangedActions[slotIndex]?.Invoke(in dummySlot, slotChangeType);
            SlotChanged?.Invoke(in dummySlot, slotChangeType);
            SlotStorageChanged?.Invoke();
        }

        public int AdjustStackAtIndex(int slotIndex, int addAmount)
        {
            if(addAmount == 0 || _slots[slotIndex].HasItem == false)
            {
                return 0;
            }

            bool shouldAdd = addAmount > 0;
            return shouldAdd
                ? AddStackToIndex(slotIndex, addAmount)
                : RemoveStackFromIndex(slotIndex, addAmount);
        }

        private int AddStackToIndex(int slotIndex, int addition)
        {
            ItemStack existingItem = _slots[slotIndex];

            int countToAdd = Math.Min(addition, existingItem.MaxStackSize - existingItem.Quantity);
            countToAdd = _addConstraintService.GetAllowedCount(new ItemStack(existingItem.Item, countToAdd)).allowedCount;

            _slots[slotIndex] = new ItemStack(existingItem.Item, countToAdd + existingItem.Quantity);
            _currentWeight += existingItem.Item.Weight * countToAdd;

            InvokeSlotChangedEvent(slotIndex, SlotChangeType.StackChanged);

            return countToAdd;
        }

        private int RemoveStackFromIndex(int slotIndex, int removal)
        {
            ItemStack existingItem = _slots[slotIndex];

            int countToRemove = Math.Min(-removal, existingItem.Quantity);

            _currentWeight -= countToRemove * existingItem.Item.Weight;
            existingItem.Quantity -= countToRemove;

            bool removedAll = existingItem.Quantity == 0;
            _slots[slotIndex] = removedAll ? ItemStack.Empty : existingItem;

            InvokeSlotChangedEvent(slotIndex, removedAll ? SlotChangeType.ItemChanged : SlotChangeType.StackChanged);

            return countToRemove;
        }

        private void InvokeAllEvents()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                var dummySlot = new Slot(this, i);
                _slotChangedActions[i]?.Invoke(in dummySlot, SlotChangeType.ItemChanged);
                SlotChanged?.Invoke(in dummySlot, SlotChangeType.ItemChanged);
            }

            SlotStorageChanged?.Invoke();
        }
        
        public float GetAvailableWeight()
        {
            float availableWeightInContainer = _maxWeight - _currentWeight;
            float availableWeightInInventory = _inventory != null
                ? _inventory.MaxWeight - _inventory.CurrentWeight
                : float.PositiveInfinity;

            return Math.Min(availableWeightInContainer, availableWeightInInventory);
        }

        public void SortItems(Comparison<ItemStack> comparison)
        {
            Array.Sort(_slots, comparison);
            InvokeAllEvents();
        }

        public void Clear()
        {
            Array.Clear(_slots, 0, _slots.Length);
            InvokeAllEvents();
        }

        public IEnumerator<ItemStack> GetEnumerator() => ((IEnumerable<ItemStack>)_slots).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _slots.GetEnumerator();

        // Propagate calls to the services
        public void AddSlotChangedListener(int slotIndex, SlotChangedDelegate listenerAction)
            => _slotChangedActions[slotIndex] += listenerAction;

        public void RemoveSlotChangedListener(int slotIndex, SlotChangedDelegate listenerAction)
            => _slotChangedActions[slotIndex] -= listenerAction;

        public (int addedAmount, string rejectionMessage) AddItem(ItemStack itemStack)
            => _containerOperationsService.AddItem(itemStack);

        public (int addedAmount, string rejectionMessage) AddItemsWithID(int itemID, int amount)
            => _containerOperationsService.AddItemsWithID(itemID, amount);

        public int RemoveItems(Func<IItem, bool> filter, int amount)
            => _containerOperationsService.RemoveItems(filter, amount);

        public (int allowedCount, string rejectionMessage) GetAllowedCount(ItemStack itemStack)
            => _addConstraintService.GetAllowedCount(itemStack);

        public bool ContainsItem(IItem item)
            => _containerQueryService.ContainsItem(item);

        public bool ContainsItem(Func<IItem, bool> filter)
            => _containerQueryService.ContainsItem(filter);

        public int GetItemCount(Func<IItem, bool> filter)
            => _containerQueryService.GetItemCount(filter);

        /// <summary>
        /// Builder to easily construct <see cref="Container"/> instances with chain methods.
        /// </summary>
        public sealed class Builder
        {
            private IInventory _inventory;
            private string _containerName;
            private int _slotsCount = 16;
            private float _maxWeight = 50;
            private bool _allowStacking = true;
            private ContainerAddConstraint[] _addConstraints;

            internal Builder() { }

            public Builder WithName(string name)
            {
                _containerName = name;
                return this;
            }

            public Builder WithInventory(IInventory inventory)
            {
                _inventory = inventory;
                return this;
            }

            public Builder WithSlotsCount(int slotsCount)
            {
                _slotsCount = slotsCount;
                return this;
            }

            public Builder WithMaxWeight(float maxWeight)
            {
                _maxWeight = maxWeight; 
                return this;
            }

            public Builder WithAddConstraints(params ContainerAddConstraint[] addConstraints)
            {
                _addConstraints = addConstraints;
                return this;
            }

            public Builder WithStackingBehaviour(bool stackingBehaviour)
            {
                _allowStacking = stackingBehaviour;
                return this;
            }

            public Container Build()
            {
                if(_inventory == null)
                {
                    return null;
                }

                return new Container(_containerName, _allowStacking, _slotsCount, _maxWeight, _inventory, _addConstraints);
            }
        }
    }
}