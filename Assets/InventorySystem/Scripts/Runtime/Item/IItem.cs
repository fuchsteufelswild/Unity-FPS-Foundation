using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nexora.InventorySystem
{
    public interface IItem 
    {
        /// <summary>
        /// ID of the underlying <see cref="InventorySystem.ItemDefinition"/>.
        /// </summary>
        int ID { get; }

        /// <summary>
        /// Underlying item data.
        /// </summary>
        ItemDefinition ItemDefinition { get; }

        /// <summary>
        /// Name of the item.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Weight of the single item.
        /// </summary>
        float Weight { get; }

        /// <summary>
        /// Is this item can be stacked in a single slot?
        /// </summary>
        bool IsStackable { get; }

        /// <summary>
        /// Maximum stack size allowed for this item.
        /// </summary>
        int MaxStackSize { get; }

        /// <param name="propertyName">Name of the property.</param>
        /// <param name="property">Found property.</param>
        /// <returns>If the item has a dynamic property with <paramref name="propertyName"/>.</returns>
        bool TryGetDynamicProperty(string propertyName, out DynamicItemProperty property)
            => TryGetDynamicProperty(DefinitionRegistry<DynamicItemPropertyDefinition>.GetByName(propertyName), out property);

        /// <param name="propertyID">Reference of the property data.</param>
        /// <param name="property">Found property.</param>
        /// <returns>If the item has a dynamic property with <paramref name="propertyID"/>.</returns>
        bool TryGetDynamicProperty(DefinitionReference<DynamicItemPropertyDefinition> propertyID, out DynamicItemProperty property);

        /// <param name="propertyID">Reference of the property data.</param>
        /// <param name="property">Found property.</param>
        /// <returns>Dynamic property with a <paramref name="propertyID"/>, <see langword="null"/> if not found.</returns>
        DynamicItemProperty GetDynamicProperty(DefinitionReference<DynamicItemPropertyDefinition> propertyID);
    }

    /// <summary>
    /// Generator for <see cref="IItem"/>. Generates a random number of <see cref="IItem"/>
    /// using a <see cref="ItemGenerationMethod"/> and returns <see cref="ItemStack"/>.
    /// </summary>
    [Serializable]
    public struct ItemGenerator
#if UNITY_EDITOR
        : ISerializationCallbackReceiver
#endif
    {
        /// <summary>
        /// Determines from what data item should be created.
        /// </summary>
        public enum ItemGenerationMethod
        {
            /// <summary>
            /// Generates the specific <see cref="IItem"/> from the 
            /// <see cref="ItemDefinition"/> provided in the generator.
            /// </summary>
            SpecificItem,

            /// <summary>
            /// Generates an <see cref="IItem"/> random within the <see cref="ItemCategoryDefinition"/> 
            /// provided in the generator.
            /// </summary>
            RandomFromCategory,

            /// <summary>
            /// Generates a solely random <see cref="IItem"/> from all of the <see cref="ItemDefinition"/>
            /// available in the project.
            /// </summary>
            RandomFromAllItems
        }

        [Tooltip("Defines how the item should be generated.")]
        [HideLabel, BeginHorizontal]
        [SerializeField]
        private ItemGenerationMethod _generationMethod;

        [Tooltip("Specific item to generate.")]
        [ShowIf(nameof(_generationMethod), ItemGenerationMethod.SpecificItem)]
        [DefinitionReference, HideLabel]
        [SerializeField]
        private ItemDefinition _specificItem;

        [Tooltip("Denotes item category from which a random item should be generated.")]
        [ShowIf(nameof(_generationMethod), ItemGenerationMethod.RandomFromCategory)]
        [DefinitionReference, HideLabel]
        [SerializeField]
        private ItemCategoryDefinition _itemCategory;

        [Tooltip("Range denotes how many items should be created? [min, max]")]
        [EndHorizontal, HideLabel]
        [MinMaxSlider(1, 200)]
        [SerializeField]
        private Vector2Int _quantityRange;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseConstraints">Base constraints that might be passed from container, inventory, holster etc.</param>
        /// <param name="additionalConstraints">Conditional constraints, might be passed due to the level in the game, or from specific loot constraints.</param>
        public ItemStack GenerateItem(
            IReadOnlyList<ContainerAddConstraint> baseConstraints, 
            params ContainerAddConstraint[] additionalConstraints)
        {
            return _generationMethod switch
            {
                ItemGenerationMethod.SpecificItem => GenerateSpecificItem(baseConstraints),
                ItemGenerationMethod.RandomFromCategory => GenerateRandomItemFromCategory(_itemCategory, baseConstraints),
                ItemGenerationMethod.RandomFromAllItems => GenerateRandomItemFromProject(baseConstraints),
                _ => throw new InvalidOperationException("Invalid generation method")
            };
        }

        private ItemStack GenerateSpecificItem(
            IReadOnlyList<ContainerAddConstraint> baseConstraints, 
            params ContainerAddConstraint[] additionalConstraints)
        {
            if(_specificItem == null)
            {
                throw new ArgumentNullException(nameof(_specificItem));
            }

            if(baseConstraints.AllowsItem(_specificItem) == false
            || additionalConstraints.AllowsItem(_specificItem) == false)
            {
                Debug.LogErrorFormat("Couldn't generate specific item: {0}, constraints don't allow.", _specificItem.DisplayName);
                return ItemStack.Empty;
            }

            return CreateItemStack(_specificItem);
        }

        private ItemStack CreateItemStack(ItemDefinition itemDefinition)
        {
            return new ItemStack(
                new Item(itemDefinition), 
                _quantityRange.GetRandomFromRange());
        }

        /// <summary>
        /// Gets a random <see cref="ItemCategoryDefinition"/> that has at least one member 
        /// then picks a random <see cref="ItemDefinition"/> that passes the <paramref name="baseConstraints"/>.
        /// </summary>
        /// <remarks>
        /// One could argue that this is not fully random, as there might be category that might pass
        /// all the constraints but the selected random is not. But it would be too cumbersome to filter
        /// all categories and filter out all eligible items onto another list, then take a random from it.
        /// And performance-wise, not viable at all. This doesn't pose a problem as long as constraints 
        /// are well checked, or if they are too tight, then should provide a category for performance sake.
        /// </remarks>
        private ItemStack GenerateRandomItemFromProject(
            IReadOnlyList<ContainerAddConstraint> baseConstraints,
            params ContainerAddConstraint[] additionalConstraints)
        {
            List<ItemCategoryDefinition> eligibleCategories = DefinitionRegistry<ItemCategoryDefinition>.AllDefinitions
                .Where(ItemCategoryFilters.WithItemDefinition(baseConstraints.AllowsItem))
                .Where(ItemCategoryFilters.WithItemDefinition(additionalConstraints.AllowsItem))
                .ToList();

            if(eligibleCategories.IsEmpty())
            {
                Debug.LogError("There is no category in the project that contains a member");
                return ItemStack.Empty;
            }

            ItemCategoryDefinition randomCategory = eligibleCategories.SelectRandom();
            return GenerateRandomItemFromCategory(randomCategory, baseConstraints);
        }

        private ItemStack GenerateRandomItemFromCategory(
            ItemCategoryDefinition itemCategory,
            IReadOnlyList<ContainerAddConstraint> baseConstraints,
            params ContainerAddConstraint[] additionalConstraints)
        {
            if(itemCategory == null)
            {
                throw new ArgumentNullException(nameof(itemCategory));
            }

            if(itemCategory.Members.IsEmpty())
            {
                Debug.LogErrorFormat("Item couldn't be generated, {0} does not contain any item.", itemCategory.DisplayName);
                return ItemStack.Empty;
            }

            Func<ItemDefinition, bool> compositeFilter = 
                item => baseConstraints.AllowsItem(item) && additionalConstraints.AllowsItem(item);

            ItemDefinition randomItemDefinition =
                itemCategory.Members.SelectRandomFiltered(compositeFilter);

            if(randomItemDefinition == null)
            {
                Debug.LogErrorFormat("Item couldn't be generated, there are no items in the {0} that can pass" +
                    " the given constraints.", itemCategory.DisplayName);
                return ItemStack.Empty;
            }

            return CreateItemStack(randomItemDefinition);
        }

#if UNITY_EDITOR
        public void OnAfterDeserialize()
        {
            // Ensure quantity range is never negative and always x is smaller than y
            _quantityRange.x = Mathf.Max(_quantityRange.x, 0);
            _quantityRange.y = Mathf.Max(_quantityRange.x, _quantityRange.y);
            _quantityRange.y = Mathf.Max(_quantityRange.y, 1);

            if(_generationMethod == ItemGenerationMethod.SpecificItem && _specificItem == null)
            {
                Debug.LogWarning("ItemGenerator: SpecificItem generation method is selected but no specific item is provided.");
            }
            else if(_generationMethod == ItemGenerationMethod.RandomFromCategory && _itemCategory == null)
            {
                Debug.LogWarning("ItemGenerator: RandomFromCategory generation method is selected but no category is provided.");
            }
        }

        public void OnBeforeSerialize() { }
#endif
    }
}