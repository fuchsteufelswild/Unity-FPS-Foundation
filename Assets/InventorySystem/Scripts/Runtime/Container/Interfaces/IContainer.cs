using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Interface that defines a container that stores a list of <see cref="ItemStack"/> and
    /// can be used as an <see cref="IEnumerable{T}"/> of it.
    /// </summary>
    /// <remarks>
    /// Provides Add/Remove/Query operations through its interfaces.
    /// </remarks>
    public interface IContainer : 
        ISlotStorage,
        IContainerAddConstraintService,
        IContainerOperationsService,
        IContainerQueryService
    {
        const int MaxSlotsCount = 128;

        /// <returns>How much another weight can be added to this container?</returns>
        float GetAvailableWeight();
    }

    /// <summary>
    /// Generator that can be used to generate containers provided by the rules.
    /// </summary>
    [Serializable]
    public struct ContainerGenerator
#if UNITY_EDITOR
        : ISerializationCallbackReceiver
#endif
    {
        public string ContainerName;

        [Range(1, 100)]
        public int SlotsCount;

        [Tooltip("Can items be stacked inside any slot of this container?")]
        public bool AllowStacking;

        [Tooltip("Maximum weight this container can carry in total.")]
        [Range(0f, 1000f)]
        public float MaxCarriedWeight;

        [Tooltip("Constraints to be followed when adding items to this container.")]
        [ReorderableList(ListStyle.Lined, HasLabels = false, Foldable = true)]
        public ContainerAddConstraint[] ContainerAddConstraints;

        [Tooltip("Items to be generated default when this container is generated.")]
        [ReorderableList(ListStyle.Lined, Foldable = true)]
        public ItemGenerator[] PredefinedItems;

        [Tooltip("Optional drop table that can be used to define additional items to generate in this container." +
            " Unlike 'PredefinedItems', this is can be more controlled and also can be from reused 'ScriptableObject' assets.")]
        public ItemDropTable AdditionalItems;

        [Tooltip("Determines weight for rarity level of items that can be chosen from 'ItemDropTable'.")]
        [ShowIf(nameof(AdditionalItems), true)]
        [Range(0f, 10f)]
        public float RarityWeight;

        [Tooltip("Determines how many items can be generated from 'ItemDropTable'.")]
        [ShowIf(nameof(AdditionalItems), true)]
        [MinMaxSlider(1, 1500)]
        public Vector2 ItemDropCountRange;

        public IContainer GenerateContainer(IInventory inventory, bool populateWithItems = true, string customContainerName = null)
        {
            IContainer container = Container.StartBuilder()
                .WithInventory(inventory)
                .WithName(customContainerName ?? ContainerName)
                .WithMaxWeight(MaxCarriedWeight)
                .WithAddConstraints(ContainerAddConstraints)
                .WithSlotsCount(SlotsCount)
                .WithStackingBehaviour(AllowStacking)
                .Build();

            if(populateWithItems)
            {
                PopulateContainerWithGeneratedItems(container);
            }

            return container;
        }

        public void PopulateContainerWithGeneratedItems(IContainer container)
        {
            foreach(ItemGenerator itemGenerator in PredefinedItems)
            {
                container.AddItem(itemGenerator.GenerateItem(ContainerAddConstraints));
            }

            AdditionalItems?.PopulateContainer(container, (int)ItemDropCountRange.GetRandomFromRange(), RarityWeight);
        }


#if UNITY_EDITOR
        public void OnAfterDeserialize() 
        {
            // Might fix values of the fields that are not correct
        }

        public void OnBeforeSerialize() { }
#endif
    }
}