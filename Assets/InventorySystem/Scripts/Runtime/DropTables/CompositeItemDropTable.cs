using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Nexora.InventorySystem
{
    public sealed class CompositeItemDropTable : ItemDropTable
    {
        [Tooltip("Drop tables, this composite item drop table will be generating items from.")]
        [ReorderableList]
        [SerializeField]
        private DropTableEntry[] _dropTableEntries;

        [SerializeField, HideInInspector]
        private float _totalProbability;

        public override List<ItemStack> GenerateItemStacks(IReadOnlyList<ContainerAddConstraint> constraints, int amountToGenerate, float rarityWeight = 1)
        {
            amountToGenerate = amountToGenerate.ClampAboveZero();

            if(_dropTableEntries.IsEmpty())
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
            ItemDropTable randomDropTable = GetRandomDropTable();
            return randomDropTable != null
                ? randomDropTable.GenerateItemStack(constraints, rarityWeight)
                : ItemStack.Empty;
        }

        private ItemDropTable GetRandomDropTable()
        {
            float randomProbability = _totalProbability;

            float probability = 0f;
            foreach(DropTableEntry tableEntry in  _dropTableEntries)
            {
                probability += tableEntry.Probability;
                if(randomProbability <= probability)
                {
                    return tableEntry.ItemDropTable;
                }
            }

            return null;
        }

        public override int PopulateInventory(IInventory inventory, int amountToGenerate, float rarityWeight = 1)
        {
            amountToGenerate = amountToGenerate.ClampAboveZero();

            if(inventory == null)
            {
                return 0;
            }

            int remainingToAdd = amountToGenerate;
            for(int i = 0; i < inventory.Containers.Count; i++)
            {
                int addedCount = PopulateContainer(inventory.Containers[i], remainingToAdd, rarityWeight);
                remainingToAdd -= addedCount;
            }

            return amountToGenerate - remainingToAdd;
        }

        public override int PopulateContainer(IContainer container, int amountToGenerate, float rarityWeight = 1)
        {
            amountToGenerate = amountToGenerate.ClampAboveZero();
            
            if(container == null)
            {
                return 0;
            }

            int addedCount = 0;
            for(int i = 0; i < amountToGenerate; i++)
            {
                ItemDropTable randomDropTable = GetRandomDropTable();
                if(randomDropTable != null)
                {
                    addedCount += randomDropTable.PopulateContainer(container, 1, rarityWeight);
                }
            }

            return addedCount;
        }

        [Serializable]
        private sealed class DropTableEntry
        {
            [InLineEditor]
            public ItemDropTable ItemDropTable;

            [Tooltip("The chance this entry will be selected over others.")]
            [Suffix("%"), Range(0, 100)]
            public float Probability;
        }

        [Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            _totalProbability = _dropTableEntries.Sum(entry => entry.Probability);
        }
    }
}