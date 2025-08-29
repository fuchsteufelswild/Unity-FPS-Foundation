using Nexora.SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Inventory that is composed of one or many <see cref="IContainer"/>.
    /// Provides initialization fields for initial containers and a max carriable weight.
    /// Provides fields for actions, which can be performed on the <see cref="IItemActionContext"/>
    /// interface of the owner of this inventory (e.g Character, Working Table, Storage Chest).
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.EarlyGameLogic)]
    public class Inventory :
        MonoBehaviour,
        IInventory,
        IInventoryOperationsService,
        IInventoryQueryService,
        IInventoryWeightService,
        IInventoryActionsService,
        ISaveableComponent
    {
        [Tooltip("Starting containers for the inventory.")]
        [SpaceArea]
        [ReorderableList, LabelByChild("ContainerName")]
        [SerializeField]
        private ContainerGenerator[] _initialContainers = Array.Empty<ContainerGenerator>();

        [Tooltip("Maximum weight that can be carried in this inventory.")]
        [Range(0f, 1000f)]
        [SerializeField]
        private float _maxWeight = 50f;

        [SerializeField]
        private InventoryActionsService _inventoryActionsService;

        [NonSerialized]
        private Container[] _containers;
        private IItemActionContext _actionContext;

        [NonSerialized]
        private IInventoryOperationsService _operationsService;
        [NonSerialized]
        private IInventoryQueryService _queryService;
        [NonSerialized]
        private IInventoryWeightService _weightService;
        [NonSerialized]
        private IInventoryActionsService _actionsService;

        public event UnityAction InventoryChanged;
        public event SlotChangedDelegate InventorySlotChanged;

        public IReadOnlyList<IContainer> Containers => _containers;

        public float MaxWeight => _maxWeight;
        public float CurrentWeight => _weightService.CurrentWeight;

        private void Start()
        {
            _actionContext = GetComponentInParent<IItemActionContext>();

            if(_containers == null)
            {
                InitializeServices();
                _containers = GenerateDefaultContainers();
            }
        }

        /// <summary>
        /// Initializes the subservices inventory used to operate.
        /// </summary>
        private void InitializeServices()
        {
            _inventoryActionsService.Initialize(this, _actionContext);
            _actionsService = _inventoryActionsService;
            _operationsService = new InventoryOperationsService(this);
            _queryService = new InventoryQueryService(this);
            _weightService = new InventoryWeightService(this, _maxWeight);
        }

        /// <summary>
        /// Generates the default containers using the list of <see cref="ContainerGenerator"/>
        /// </summary>
        private Container[] GenerateDefaultContainers()
        {
            if(_initialContainers.IsEmpty())
            {
                Debug.LogWarning("Inventory component with an empty container list cannot contain" +
                    " any elements!");
                return Array.Empty<Container>();
            }

            var containers = new Container[_initialContainers.Length];

            for(int i = 0; i < _initialContainers.Length; i++)
            {
                var container = (Container)_initialContainers[i].GenerateContainer(this, false);
                container.SlotStorageChanged += OnContainerChanged;
                container.SlotChanged += OnSlotChanged;
                containers[i] = container;
            }

            PopulateContainers();

            return containers;

            void PopulateContainers()
            {
                for(int i = 0; i < containers.Length; i++)
                {
                    _initialContainers[i].PopulateContainerWithGeneratedItems(containers[i]);
                }
            }
        }

        #region Services
        public (int addedAmount, string rejectionMessage) AddItem(ItemStack itemStack)
            => _operationsService.AddItem(itemStack);

        public (int addedAmount, string rejectionMessage) AddItemsWithID(int itemID, int amount)
            => _operationsService.AddItemsWithID(itemID, amount);

        public int RemoveItems(Func<IItem, bool> filter, int amount)
            => _operationsService.RemoveItems(filter, amount);

        public bool ContainsItem(IItem item) => _queryService.ContainsItem(item);
        public bool ContainsItem(Func<IItem, bool> filter) => _queryService.ContainsItem(filter);
        public IContainer GetContainer(Func<IContainer, bool> filter) => _queryService.GetContainer(filter);
        public List<IContainer> GetContainers(Func<IContainer, bool> filter) => _queryService.GetContainers(filter);
        public int GetItemCount(Func<IItem, bool> filter) => _queryService.GetItemCount(filter);

        public void DropItem(ItemStack itemStack) => _actionsService.DropItem(itemStack);
        public T GetActionOfType<T>() where T : ItemAction => _actionsService.GetActionOfType<T>();
        public bool TryGetActionOfType<T>(out T action) where T : ItemAction => _actionsService.TryGetActionOfType<T>(out action);
        #endregion

        public void SortItems(Comparison<ItemStack> comparison)
        {
            foreach(IContainer container in Containers)
            {
                container.SortItems(comparison);
            }
        }

        public void Clear()
        {
            foreach (IContainer container in Containers)
            {
                container.Clear();
            }
        }

        private void OnContainerChanged()
        {
            InventoryChanged?.Invoke();
        }

        private void OnSlotChanged(in Slot itemSlot, SlotChangeType slotChangeType)
        {
            InventorySlotChanged?.Invoke(in itemSlot, slotChangeType);
        }

        #region SaveableComponent
        [Serializable]
        private sealed class SaveData
        {
            public Container[] Containers;
            public float MaxWeight;
        }

        object ISaveableComponent.Save()
        {
            return new SaveData
            {
                Containers = _containers,
                MaxWeight = _maxWeight
            };
        }

        void ISaveableComponent.Load(object data)
        {
            var save = (SaveData)data;

            _maxWeight = save.MaxWeight;
            _containers = save.Containers;

            InitializeServices();

            for (int i = 0; i < _containers.Length; i++)
            {
                Container container = _containers[i];
                container.InitializeAfterDeserialize(this, _initialContainers[i].ContainerAddConstraints);
                container.SlotStorageChanged += OnContainerChanged;
                container.SlotChanged += OnSlotChanged;
            }
        }
        #endregion
    }
}