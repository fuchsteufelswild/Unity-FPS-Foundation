using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Interface that provides Add/Remove/Query/Drop for an inventory.
    /// It uses various helper services to provide most of the functionality.
    /// </summary>
    /// <remarks>
    /// Implementing class must be a <see cref="MonoBehaviour"/>.
    /// </remarks>
    public interface IInventory :
        IMonoBehaviour,
        IInventoryOperationsService,
        IInventoryQueryService,
        IInventoryWeightService,
        IInventoryActionsService
    {
        /// <summary>
        /// All <see cref="IContainer"/> that are part of this inventory.
        /// </summary>
        IReadOnlyList<IContainer> Containers { get; }

        /// <summary>
        /// Sorts all the containers in the inventory.
        /// </summary>
        /// <param name="comparison">Comparison method to use when sorting.</param>
        void SortItems(Comparison<ItemStack> comparison);
        
        /// <summary>
        /// Clears all the containers in the inventory.
        /// </summary>
        void Clear();

        /// <summary>
        /// Event fired when any change is done on the inventory.
        /// </summary>
        event UnityAction InventoryChanged;

        /// <summary>
        /// Event fired when any slot in any container is changed in the inventory.
        /// </summary>
        event SlotChangedDelegate InventorySlotChanged;
    }

    /// <summary>
    /// A class representing null/empty inventory.
    /// </summary>
    public sealed class NullInventory : IInventory
    {
        private GameObject _owner;

        public NullInventory(GameObject owner) => _owner = owner;

        #region IMonoBehaviour
        public GameObject gameObject => _owner;
        public Transform transform => _owner.transform;
        public bool enabled { get => true; set { } }
        public Coroutine StartCoroutine(IEnumerator routine) => CoroutineUtils.StartGlobalCoroutine(routine);
        public void StopCoroutine(Coroutine routine) => CoroutineUtils.StopGlobalCoroutine(ref routine);
        #endregion

        public float CurrentWeight => 0f;
        public float MaxWeight => 0f;
        public IReadOnlyList<IContainer> Containers => Array.Empty<IContainer>();

        public (int addedAmount, string rejectionMessage) AddItem(ItemStack itemStack)
            => (0, ContainerAddConstraint.InventoryFull);

        public (int addedAmount, string rejectionMessage) AddItemsWithID(int itemID, int amount)
            => (0, ContainerAddConstraint.InventoryFull);

        public bool ContainsItem(IItem item) => false;
        public bool ContainsItem(Func<IItem, bool> filter) => false;
        public void DropItem(ItemStack itemStack) { }
        public IContainer GetContainer(Func<IContainer, bool> filter) => null;
        public List<IContainer> GetContainers(Func<IContainer, bool> filter) => new();
        public int GetItemCount(Func<IItem, bool> filter) => 0;
        public int RemoveItems(Func<IItem, bool> filter, int amount) => 0;
        public void SortItems(Comparison<ItemStack> comparison) { }
        public void Clear() { }

        public bool TryGetActionOfType<T>(out T action) where T : ItemAction
        {
            action = null;
            return false;
        }

        public T GetActionOfType<T>() where T : ItemAction => null;

        public event UnityAction InventoryChanged { add { } remove { } }
        public event SlotChangedDelegate InventorySlotChanged { add { } remove { } }
    }
}