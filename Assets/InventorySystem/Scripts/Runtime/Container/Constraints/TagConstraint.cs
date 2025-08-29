using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Constraint to either require any or exclude all specific list of <see cref="ItemTagDefinition"/>.
    /// </summary>
    [CreateAssetMenu(menuName = AssetMenuPath + nameof(TagConstraint), fileName = "TagConstraint_")]
    public sealed class TagConstraint : 
        DefinitionFilterConstraint<TagConstraint, ItemTagDefinition>
    {
        private TagConstraint() { }

        public static TagConstraint Create(
            FilterPolicy filterPolicy,
            params DefinitionReference<ItemTagDefinition>[] tags)
        {
            return CreateNew(filterPolicy, tags);
        }

        public override int GetAllowedCount(IContainer container, IItem item, int requestedAmount)
        {
            var itemTag = item.ItemDefinition.ItemTag;

            return FilteringPolicy == FilterPolicy.Require
                ? HandleRequireFilter(itemTag, requestedAmount)
                : HandleExcludeFilter(itemTag, requestedAmount);
        }
    }
}