using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Represents a drop table that generates and distributes items based on predefined rules.
    /// </summary>
    [CreateAssetMenu(menuName = CreateMenuPath + nameof(SimpleItemDropTable), fileName = "DropTable_")]
    public sealed class SimpleItemDropTable : ItemDropTable
    {
        [Tooltip("The list of possible items to drop and their probabilities.")]
        [SerializeField]
        ItemDropEntry[] _itemDropEntries;

        [SerializeField, HideInInspector]
        private float _totalDropProbability;

        public override List<ItemStack> GenerateItemStacks(
            IReadOnlyList<ContainerAddConstraint> constraints, 
            int amountToGenerate, 
            float rarityWeight = 1)
        {
            if(_itemDropEntries.IsEmpty() || amountToGenerate <= 0)
            {
                return new List<ItemStack>();
            }

            var result = new List<ItemStack>();
            for(int i = 0; i < amountToGenerate; i++)
            {
                ItemStack generatedStack = GenerateItemStack(constraints, rarityWeight);
                if(generatedStack.IsValid)
                {
                    result.Add(generatedStack);
                }
            }

            return result;
        }

        public override ItemStack GenerateItemStack(IReadOnlyList<ContainerAddConstraint> constraints, float rarityWeight = 1)
        {
            // How should integrate the rarity weight?

            float probabilityRandom = UnityEngine.Random.Range(0, _totalDropProbability);

            float probability = 0f;
            foreach (ItemDropEntry itemDropEntry in _itemDropEntries)
            {
                probability += itemDropEntry.Probability;
                if (probabilityRandom <= probability)
                {
                    ItemStack generatedItemStack = itemDropEntry.ItemGenerator.GenerateItem(constraints);
                    if (generatedItemStack.HasItem)
                    {
                        return generatedItemStack;
                    }
                }
            }

            return ItemStack.Empty;
        }

        public override int PopulateInventory(IInventory inventory, int amountToGenerate, float rarityWeight = 1)
        {
            if (inventory == null || amountToGenerate <= 0)
            {
                return 0;
            }

            int remainingToAdd = amountToGenerate;
            int totalAdded = 0;
            for(int i = 0; i < inventory.Containers.Count && remainingToAdd > 0; i++)
            {
                int addedAmount = PopulateContainer(inventory.Containers[i], remainingToAdd, rarityWeight);
                totalAdded += addedAmount;
                remainingToAdd -= addedAmount;
            }

            return totalAdded;
        }

        public override int PopulateContainer(IContainer container, int amountToGenerate, float rarityWeight = 1)
        {
            if (container == null || amountToGenerate <= 0)
            {
                return 0;
            }

            int totalAdded = 0;
            for(int i = 0; i < amountToGenerate; i++)
            {
                ItemStack generatedStack = GenerateItemStack(container.AddConstraints, rarityWeight);
                if(generatedStack.IsValid)
                {
                    totalAdded += container.AddItem(generatedStack).addedAmount;
                }
            }

            return totalAdded;
        }

        [Serializable]
        private sealed class ItemDropEntry
        {
            [Tooltip("Item to loot.")]
            public ItemGenerator ItemGenerator;

            [Tooltip("Chance to drop this item.")]
            [Suffix("%"), Range(0f, 100f)]
            public float Probability;
        }

        [Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            _totalDropProbability = _itemDropEntries.Sum(dropEntry => dropEntry.Probability);

            for(int i = 0; i < _itemDropEntries.Length; i++)
            {
                _itemDropEntries[i].Probability = Mathf.Max(0, _itemDropEntries[i].Probability);
            }
        }
    }
}