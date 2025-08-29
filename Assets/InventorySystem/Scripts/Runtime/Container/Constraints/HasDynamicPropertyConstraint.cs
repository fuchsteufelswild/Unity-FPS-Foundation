using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Constraint to either require any or exclude all specific list of <see cref="DynamicItemPropertyDefinition"/>.
    /// </summary>
    [CreateAssetMenu(menuName = AssetMenuPath + nameof(HasDynamicPropertyConstraint), fileName = "HasDynamicPropertyConstraint_")]
    public sealed class HasDynamicPropertyConstraint : 
        DefinitionFilterConstraint<HasDynamicPropertyConstraint, DynamicItemPropertyDefinition>
    {
        private HasDynamicPropertyConstraint() { }

        public static HasDynamicPropertyConstraint Create(
            FilterPolicy filterPolicy,
            params DefinitionReference<DynamicItemPropertyDefinition>[] dynamicProperties)
        {
            return CreateNew(filterPolicy, dynamicProperties);
        }

        public override int GetAllowedCount(IContainer container, IItem item, int requestedAmount)
        {
            var itemTag = item.ItemDefinition.ItemTag;

            if(FilteringPolicy == FilterPolicy.Require)
            {
                foreach(var property in _constraintElements)
                {
                    if(item.ItemDefinition.HasProperty(property) == false)
                    {
                        return 0;
                    }
                }

                return requestedAmount;
            }
            else // Exclude
            {
                foreach (var property in _constraintElements)
                {
                    if (item.ItemDefinition.HasProperty(property))
                    {
                        return 0;
                    }
                }

                return requestedAmount;
            }
        }
    }
}