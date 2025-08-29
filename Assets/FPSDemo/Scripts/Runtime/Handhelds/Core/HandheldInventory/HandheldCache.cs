using System.Collections.Generic;
using UnityEngine;
using Nexora.InventorySystem;
using System;

namespace Nexora.FPSDemo.Handhelds
{
    public interface IHandheldItemCache
    {
        /// <summary>
        /// Cache of handheld of items registered.
        /// </summary>
        IReadOnlyDictionary<int, IHandheldItem> Cache { get; }

        /// <param name="id">ID of the <see cref="IItem"/> being searched.</param>
        /// <returns><see cref="IHandheld"/> that has the <paramref name="id"/> in underlying <see cref="IItem"/> object.</returns>
        IHandheld GetHandheldWithID(int id);

        /// <summary>
        /// Gets the <see cref="IHandheldItem"/> of the <see cref="IItem"/> with <paramref name="itemID"/>.
        /// </summary>
        /// <param name="itemID">ID of the <see cref="IItem"/> being searched.</param>
        /// <param name="handheldItem">Found item.</param>
        /// <returns>If item with the given <paramref name="itemID"/> has been found.</returns>
        bool TryGetHandheldItem(int itemID, out IHandheldItem handheldItem);

        /// <summary>
        /// Rebuilds all the cache.
        /// </summary>
        void RebuildCache();
    }

    public sealed class HandheldItemCache : IHandheldItemCache
    {
        private readonly Dictionary<int, IHandheldItem> _handheldItems = new();
        private readonly IHandheldsManager _handheldsManager;
        private readonly Transform _handheldsRoot;

        public IReadOnlyDictionary<int, IHandheldItem> Cache => _handheldItems;

        public HandheldItemCache(IHandheldsManager handheldsManager)
        {
            _handheldsManager = handheldsManager ?? throw new ArgumentNullException(nameof(handheldsManager));
            _handheldsRoot = _handheldsManager.HandheldsRoot;
        }

        public IHandheld GetHandheldWithID(int itemID)
        {
            return _handheldItems.TryGetValue(itemID, out IHandheldItem handheldItem)
                ? handheldItem.Handheld
                : null;
        }

        public bool TryGetHandheldItem(int itemID, out IHandheldItem handheldItem)
            => _handheldItems.TryGetValue(itemID, out handheldItem);

        public void RebuildCache()
        {
            _handheldItems.Clear();
            InitializeFromHandheldRoot();
        }

        private void InitializeFromHandheldRoot()
        {
            var childHandhelds = _handheldsRoot.gameObject.GetComponentsInDirectChildren<IHandheldItem>(32, false);

            foreach(IHandheldItem handheldItem in childHandhelds)
            {
                if(ValidateHandheldItem(handheldItem) == false)
                {
                    continue;
                }

                if(_handheldItems.TryAdd(handheldItem.ItemDefinition, handheldItem) == false)
                {
                    Debug.LogWarning("'HandheldCache': Duplicate handheld item ID");
                    continue;
                }

                _handheldsManager.RegisterHandheld(handheldItem.Handheld);
            }
        }

        private static bool ValidateHandheldItem(IHandheldItem handheldItem)
        {
            if(UnityUtils.IsValidUnityObject(handheldItem) == false)
            {
                Debug.LogError("'HandheldCache': Null handheld item is found in the cache.");
                return false;
            }

            if(handheldItem.gameObject.HasComponent<IHandheld>() == false)
            {
                Debug.LogError("'HandheldCache': Handheld item with missing interface is found in the cache.");
                return false;
            }

            if(handheldItem.ItemDefinition.IsNull)
            {
                Debug.LogError("'HandheldCache': Handheld item has a null item reference.");
                return false;
            }

            return true;
        }
    }
}