using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Constraint to either require any or exclude all specific list of <see cref="ItemCategoryDefinition"/>.
    /// </summary>
    [CreateAssetMenu(menuName = AssetMenuPath + nameof(CategoryConstraint), fileName = nameof(CategoryConstraint))]
    public sealed class CategoryConstraint :
        DefinitionFilterConstraint<CategoryConstraint, ItemCategoryDefinition>
    {
        private CategoryConstraint() { }

        public static CategoryConstraint Create(
            FilterPolicy filterPolicy, 
            params DefinitionReference<ItemCategoryDefinition>[] categories)
        {
            return CreateNew(filterPolicy, categories);
        }

        public override int GetAllowedCount(IContainer container, IItem item, int requestedAmount)
        {
            var itemCategory = item.ItemDefinition.OwningCategory;

            return FilteringPolicy == FilterPolicy.Require
                ? HandleRequireFilter(itemCategory, requestedAmount)
                : HandleExcludeFilter(itemCategory, requestedAmount);
        }
    }
}