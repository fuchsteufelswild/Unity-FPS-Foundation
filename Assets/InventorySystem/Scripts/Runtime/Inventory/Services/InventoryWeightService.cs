using UnityEngine;

namespace Nexora.InventorySystem
{
    public class InventoryWeightService : IInventoryWeightService
    {
        private readonly IInventory _inventory;
        private readonly float _maxWeight;

        private float? _cachedWeight;

        public float MaxWeight => _maxWeight;

        public InventoryWeightService(IInventory inventory, float maxWeight)
        {
            _inventory = inventory;
            _maxWeight = maxWeight;

            _inventory.InventoryChanged += OnInventoryChanged;
        }

        public float CurrentWeight
        {
            get
            {
                _cachedWeight ??= CalculateWeight();
                return _cachedWeight.Value;
            }
        }

        private float CalculateWeight()
        {
            float totalWeight = 0f;
            foreach(IContainer container in _inventory.Containers)
            {
                totalWeight += container.CurrentWeight;
            }

            return totalWeight;
        }

        /// <summary>
        /// Reset every time there is a change in the inventory so that
        /// we can lazily execute calculation next time.
        /// </summary>
        private void OnInventoryChanged() => _cachedWeight = null;
    }
}