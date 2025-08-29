using Nexora.InventorySystem;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public readonly struct HandheldSelectionChangeEventArgs
    {
        public readonly int OldIndex;
        public readonly int NewIndex;
        public readonly IHandheld OldHandheld;
        public readonly IHandheld NewHandheld;
        public readonly float TransitionSpeed;
        public readonly DateTime ChangedAt;

        public HandheldSelectionChangeEventArgs(int oldIndex, int newIndex, IHandheld oldHandheld, IHandheld newHandheld, float transitionSpeed = 1f)
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
            OldHandheld = oldHandheld;
            NewHandheld = newHandheld;
            TransitionSpeed = transitionSpeed;
            ChangedAt = DateTime.UtcNow;
        }
    }

    public delegate void HandheldSelectionChangedDelegate(in HandheldSelectionChangeEventArgs args);

    /// <summary>
    /// Handles selection of handheld in an inventory.
    /// </summary>
    public interface IHandheldSelector
    {
        public const int InvalidSelectorID = -1;

        /// <summary>
        /// Currently selected index.
        /// </summary>
        int SelectedIndex { get; }

        /// <summary>
        /// Previous selected index before the current was selected.
        /// </summary>
        int PreviousIndex { get; }

        /// <summary>
        /// Selects the handheld at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index to select.</param>
        /// <param name="allowRequip">Can reselect the item, if it's already selected.</param>
        /// <returns>If selection performed.</returns>
        bool SelectAtIndex(int index, bool allowRequip = true, float transitionSpeed = 1f);

        /// <summary>
        /// Resets the selection.
        /// </summary>
        void ResetSelection();

        event HandheldSelectionChangedDelegate SelectionChanged;
    }

    public sealed class HandheldSelector : IHandheldSelector
    {
        private IContainer _loadout;
        private IHandheldItemCache _itemCache;
        private Func<int, int> _indexValidator;

        private int _selectedIndex = IHandheldSelector.InvalidSelectorID;
        private int _previousIndex = IHandheldSelector.InvalidSelectorID;

        public int SelectedIndex => _selectedIndex;
        public int PreviousIndex => _previousIndex;

        public event HandheldSelectionChangedDelegate SelectionChanged;

        public void Initialize(IContainer loadout, IHandheldItemCache itemCache, Func<int, int> indexValidator = null)
        {
            _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
            _itemCache = itemCache ?? throw new ArgumentNullException(nameof(itemCache));
            _indexValidator = indexValidator ?? DefaultIndexValidator;
        }

        private int DefaultIndexValidator(int index) => Mathf.Clamp(index, IHandheldSelector.InvalidSelectorID, _loadout.SlotsCount - 1);

        public void LoadState(int selectedIndex, int previousIndex)
        {
            _selectedIndex = selectedIndex;
            _previousIndex = previousIndex;
        }

        public void ResetSelection() => ChangeSelection(newIndex: IHandheldSelector.InvalidSelectorID);

        private void ChangeSelection(int newIndex)
        {
            _previousIndex = _selectedIndex;
            _selectedIndex = newIndex;
        }

        public bool SelectAtIndex(int index, bool allowRequip = true, float transitionSpeed = 1f)
        {
            index = _indexValidator(index);

            if(index == _selectedIndex)
            {
                if(allowRequip)
                {
                    TriggerRefresh();
                }

                return false;
            }

            IHandheld oldHandheld = GetCurrentHandheld();
            ChangeSelection(index);
            IHandheld newHandheld = GetCurrentHandheld();

            var eventArgs = new HandheldSelectionChangeEventArgs(_previousIndex, _selectedIndex, oldHandheld, newHandheld, transitionSpeed);
            SelectionChanged?.Invoke(in eventArgs);

            return true;
        }

        private void TriggerRefresh()
        {
            IHandheld currentHandheld = GetCurrentHandheld();
            var eventArgs = new HandheldSelectionChangeEventArgs(_selectedIndex, _selectedIndex, currentHandheld, currentHandheld);
            SelectionChanged?.Invoke(in eventArgs);
        }

        private IHandheld GetCurrentHandheld()
        {
            if(_selectedIndex == IHandheldSelector.InvalidSelectorID)
            {
                return null;
            }

            Slot slot = _loadout.GetSlot(_selectedIndex);
            return slot.TryGetItem(out IItem item) ? GetHandheldForItem(item.ID) : null;
        }

        private IHandheld GetHandheldForItem(int itemID)
        {
            return _itemCache.GetHandheldWithID(itemID);
        }
    }
}