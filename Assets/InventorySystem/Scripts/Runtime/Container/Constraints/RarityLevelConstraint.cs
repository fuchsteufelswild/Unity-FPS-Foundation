using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Constraint to either require any or exclude all specific list of <see cref="RarityLevelConstraint"/>.
    /// </summary>
    [CreateAssetMenu(menuName = AssetMenuPath + nameof(RarityLevelConstraint), fileName = nameof(RarityLevelConstraint))]
    public sealed class RarityLevelConstraint : 
        DefinitionFilterConstraint<RarityLevelConstraint, ItemRarityDefinition>
    {
        private RarityLevelConstraint() { }

        public static RarityLevelConstraint Create(
            FilterPolicy filterPolicy,
            params DefinitionReference<ItemRarityDefinition>[] rarityLevels)
        {
            return CreateNew(filterPolicy, rarityLevels);
        }

        public override int GetAllowedCount(IContainer container, IItem item, int requestedAmount)
        {
            var itemRarityLevel = item.ItemDefinition.ItemRarity;

            return FilteringPolicy == FilterPolicy.Require
                ? HandleRequireFilter(itemRarityLevel, requestedAmount)
                : HandleExcludeFilter(itemRarityLevel, requestedAmount);
        }
    }
}