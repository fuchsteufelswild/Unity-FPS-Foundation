using System;
using System.ComponentModel;
using UnityEngine;
using Nexora.InventorySystem;
using IContainer = Nexora.InventorySystem.IContainer;
using UnityEngine.Events;
using Nexora.SaveSystem;
using System.Collections.Generic;

namespace Nexora.FPSDemo.Handhelds
{
    public interface IHandheldsInventory :
        ICharacterBehaviour,
        IHandheldSelector,
        IHandheldDropper,
        IHandheldItemCache
    {
        /// <summary>
        /// Called when the selected index in the inventory changed.
        /// </summary>
        event UnityAction<int> SelectedIndexChanged;
    }

    /// <summary>
    /// Inventory representing the loadout of the character. We use this class to 
    /// equip/unequip handhelds in the loadout.
    /// </summary>
    /// <remarks>
    /// One thing to understand here in this system is that, <see cref="IItem"/> might had
    /// in every slot of the slot. But, <see cref="IHandheldItem"/> is already had as a prefab
    /// initialized and ready. So, whenever we equip a new weapon, we match <see cref="IItem"/>
    /// in that slot to the <see cref="IHandheldItem"/> which corresponds to the <see cref="ItemDefinition"/>
    /// that is of <see cref="IItem"/>.
    /// <br></br>
    /// <br></br>
    /// <see cref="IItem"/>(through <see cref="Slot"/>) Equipping slot 5 which has rifle -> finds the instanced 
    /// rifle in the prefabs, and binds them together. This allows the system to manipulate dynamic properties
    /// of <see cref="IItem"/> and also react to them.
    /// <br></br>
    /// <see cref="IHandheldItem"/> prefabs -> Pistol, rifle, bow ... 
    /// </remarks>
    [RequireCharacterBehaviour(typeof(IHandheldsManager))]
    [DefaultExecutionOrder(ExecutionOrder.EarlyPresentation)]
    public sealed class HandheldsInventory :
        CharacterBehaviour,
        IHandheldsInventory,
        ISaveableComponent
    {
        [Tooltip("Default selected index of the selector.")]
        [SerializeField, Range(0, 6)]
        private int _startingSelectedIndex;

        [SerializeField]
        private HandheldHealthEventsHandler _healthEventsHandler;

        [SerializeField]
        private HandheldDropper _handheldDropper;

        [Tooltip("How fast should drop happen when selection changes.")]
        [SerializeField, Range(0f, 5f)]
        private float _dropSpeed = 1f;

        private readonly HandheldSelector _selector = new();
        private IHandheldItemCache _handheldCache;

        private HandheldSlotChangeHandler _slotChangeHandler;
        private HandheldEquipmentCoordinator _equipmentCoordinator;

        private IHandheldsManager _handheldsManager;
        private IContainer _loadout;

        public int SelectedIndex => _selector?.SelectedIndex ?? IHandheldSelector.InvalidSelectorID;
        public int PreviousIndex => _selector?.SelectedIndex ?? IHandheldSelector.InvalidSelectorID;
        public bool CanDrop => _handheldDropper.CanDrop;
        public IReadOnlyDictionary<int, IHandheldItem> Cache => _handheldCache?.Cache ?? new Dictionary<int, IHandheldItem>();

        public event UnityAction<int> SelectedIndexChanged;
        public event HandheldSelectionChangedDelegate SelectionChanged;
        public event HandheldDroppedDelegate HandheldDropped;

        protected override void OnBehaviourStart(ICharacter parent)
        {
            InitializeComponents(parent);
            SubscribeEvents();

            int initialIndex = SelectedIndex == IHandheldSelector.InvalidSelectorID ? _startingSelectedIndex : SelectedIndex;
            SelectAtIndex(initialIndex);
        }

        private void InitializeComponents(ICharacter character)
        {
            _handheldsManager = character.GetCC<IHandheldsManager>();
            _loadout = HandheldInventoryUtility.FindLoadoutContainer(character.Inventory);

            _handheldCache = new HandheldItemCache(_handheldsManager);
            _handheldCache.RebuildCache();

            _selector.Initialize(_loadout, _handheldCache, 
                index => Mathf.Clamp(index, IHandheldSelector.InvalidSelectorID, _loadout.SlotsCount - 1));

            _handheldDropper.Initialize(_selector, _handheldCache, _handheldsManager, character.Inventory, _loadout, this);
            _equipmentCoordinator = new HandheldEquipmentCoordinator(_handheldsManager, _handheldCache);

            _slotChangeHandler = new HandheldSlotChangeHandler(_selector, _loadout, OnSlotEquipmentChanged, _dropSpeed);
            _healthEventsHandler.Initialize(character.HealthController);
            _healthEventsHandler.InitializeHandheldComponents(_handheldDropper, _selector);
        }

        private void SubscribeEvents()
        {
            _selector.SelectionChanged += OnSelectionChanged;
            _handheldDropper.HandheldDropped += OnHandheldDropped;
        }

        private void OnSlotEquipmentChanged(Slot slot, float transitionSpeed) => _equipmentCoordinator.EquipHandheld(slot, transitionSpeed);

        private void OnSelectionChanged(in HandheldSelectionChangeEventArgs args)
        {
            SelectionChanged?.Invoke(in args);
            SelectedIndexChanged?.Invoke(args.NewIndex);

            Slot slot = args.NewIndex != IHandheldSelector.InvalidSelectorID ? _loadout.GetSlot(args.NewIndex) : Slot.None;
            OnSlotEquipmentChanged(slot, args.TransitionSpeed);
        }

        private void OnHandheldDropped(IHandheld handheld) => HandheldDropped?.Invoke(handheld);

        protected override void OnBehaviourDestroy(ICharacter parent)
        {
            UnsubscribeEvents();
            _slotChangeHandler?.Dispose();
            _healthEventsHandler?.Dispose();
        }

        private void UnsubscribeEvents()
        {
            _selector.SelectionChanged -= OnSelectionChanged;
            _handheldDropper.HandheldDropped -= OnHandheldDropped;
        }

        #region Public API
        public IHandheld GetHandheldWithID(int id) => _handheldCache.GetHandheldWithID(id);
        public bool TryGetHandheldItem(int itemID, out IHandheldItem handheldItem)
            => _handheldCache.TryGetHandheldItem(itemID, out handheldItem);
        public void RebuildCache() => _handheldCache.RebuildCache();

        public void ResetSelection() => _selector.ResetSelection();

        public bool SelectAtIndex(int index, bool allowRequip = true, float transitionSpeed = 1f)
        {
            if(allowRequip && index == SelectedIndex && _equipmentCoordinator?.EquippedHandheld != null)
            {
                _equipmentCoordinator.RefreshCurrentHandheld();
                return false;
            }

            return _selector.SelectAtIndex(index, allowRequip, transitionSpeed);
        }

        public bool TryDropHandheld(bool forceDrop = false) => _handheldDropper.TryDropHandheld(forceDrop);
        #endregion

        #region Saving
        private sealed class SaveData
        {
            public int SelectedIndex;
            public int PreviousIndex;
        }

        void ISaveableComponent.Load(object data)
        {
            var saveData = (SaveData)data;
            _selector.LoadState(saveData.SelectedIndex, saveData.PreviousIndex);
        } 

        object ISaveableComponent.Save()
        {
            return new SaveData
            {
                SelectedIndex = SelectedIndex,
                PreviousIndex = PreviousIndex
            };
        }
            
        #endregion

        /// <summary>
        /// Manages changes done in the <see cref="Slot"/> in which the current <b>active</b> <see cref="IHandheld"/> is had.
        /// (it can be null handheld too).
        /// </summary>
        private sealed class HandheldSlotChangeHandler : IDisposable
        {
            private readonly IHandheldSelector _selector;
            private readonly IContainer _loadout;
            private readonly Action<Slot, float> _equipHandler;
            private readonly float _dropSpeed;

            private int _subscribedSlotIndex = -1;
            private bool _isDisposed;

            public HandheldSlotChangeHandler(IHandheldSelector selector, IContainer loadout, 
                Action<Slot, float> equipHandler, float dropSpeed)
            {
                _selector = selector ?? throw new ArgumentNullException(nameof(selector));
                _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
                _equipHandler = equipHandler ?? throw new ArgumentNullException(nameof(equipHandler));
                _dropSpeed = dropSpeed;

                _selector.SelectionChanged += OnSelectionChanged;
            }

            private void OnSelectionChanged(in HandheldSelectionChangeEventArgs args)
            {
                if(_isDisposed)
                {
                    return;
                }

                UpdateSlotSubscription(args.OldIndex, args.NewIndex);
            }

            private void UpdateSlotSubscription(int oldIndex, int newIndex)
            {
                if(oldIndex != -1)
                {
                    _loadout.RemoveSlotChangedListener(oldIndex, OnSlotChanged);
                }

                if(newIndex != -1)
                {
                    _loadout.AddSlotChangedListener(newIndex, OnSlotChanged);
                }

                _subscribedSlotIndex = newIndex;
            }

            private void OnSlotChanged(in Slot slot, SlotChangeType changeType)
            {
                if(_isDisposed || changeType != SlotChangeType.ItemChanged)
                {
                    return;
                }

                _equipHandler(slot, _dropSpeed);
            }

            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                if(_selector != null)
                {
                    _selector.SelectionChanged -= OnSelectionChanged;
                }

                if(_subscribedSlotIndex != -1)
                {
                    _loadout.RemoveSlotChangedListener(_subscribedSlotIndex, OnSlotChanged);
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Class that handles the actual equipping logic.
        /// </summary>
        private sealed class HandheldEquipmentCoordinator
        {
            private readonly IHandheldsManager _handheldsManager;
            private readonly IHandheldItemCache _itemCache;

            private IHandheld _equippedHandheld;

            public IHandheld EquippedHandheld => _equippedHandheld;

            public HandheldEquipmentCoordinator(IHandheldsManager handheldsManager, IHandheldItemCache itemCache)
            {
                _handheldsManager = handheldsManager ?? throw new ArgumentNullException(nameof(handheldsManager));
                _itemCache = itemCache ?? throw new ArgumentNullException(nameof(itemCache));
            }

            public void EquipHandheld(Slot slot, float transitionSpeed = 1f)
            {
                HolsterCurrentHandheld(transitionSpeed);

                if(TryGetHandheldFromSlot(slot, out IHandheldItem handheldItem))
                {
                    _equippedHandheld = handheldItem.Handheld;
                    _handheldsManager.TryEquip(_equippedHandheld, transitionSpeed, () => handheldItem.AttachToSlot(slot));
                }
            }

            private void HolsterCurrentHandheld(float holsterSpeed)
            {
                if(_equippedHandheld == null)
                {
                    return;
                }

                _handheldsManager.TryHolster(_equippedHandheld, holsterSpeed);
                _equippedHandheld = null;
            }

            private bool TryGetHandheldFromSlot(Slot slot, out IHandheldItem handheldItem)
            {
                handheldItem = null;

                if(slot.TryGetItem(out IItem item) == false)
                {
                    return false;
                }

                return _itemCache.TryGetHandheldItem(item.ID, out handheldItem);
            }

            public void RefreshCurrentHandheld(float transitionSpeed = 1f)
            {
                if(_equippedHandheld != null && _handheldsManager.ActiveHandheld != _equippedHandheld)
                {
                    _handheldsManager.TryEquip(_equippedHandheld, transitionSpeed);
                }
            }
        }

        [Serializable]
        private sealed class HandheldHealthEventsHandler : HealthEventsHandler
        {
            private HandheldDropper _handheldDropper;
            private IHandheldSelector _handheldSelector;

            public void InitializeHandheldComponents(HandheldDropper handheldDropper, IHandheldSelector handheldSelector)
            {
                _handheldDropper = handheldDropper ?? throw new ArgumentNullException(nameof(handheldDropper));
                _handheldSelector = handheldSelector ?? throw new ArgumentNullException(nameof(handheldSelector));
            }

            protected override void OnControllerDiedInternal(in DamageContext damageContext)
            {
                if(_isDisposed)
                {
                    return;
                }

                if(_handheldDropper.DropOnDeath)
                {
                    _handheldDropper.TryDropHandheld(true);
                }

                _handheldSelector.SelectAtIndex(IHandheldSelector.InvalidSelectorID);
            }

            protected override void OnControllerRespawnedInternal()
            {
                if(_isDisposed)
                {
                    return;
                }

                _handheldSelector.SelectAtIndex(_handheldSelector.PreviousIndex);
            }
        }
    }

    public static class HandheldInventoryUtility
    {
        public static IContainer FindLoadoutContainer(IInventory inventory)
        {
            var handheldTag = ItemTags.HandheldTag;
            return inventory.GetContainer(ContainerFilters.RequiringTag(handheldTag));
        }
    }
}