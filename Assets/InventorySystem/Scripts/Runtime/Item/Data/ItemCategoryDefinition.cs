using System;
using UnityEngine;

namespace Nexora.InventorySystem
{
    [CreateAssetMenu(menuName = "Nexora/Items/Create Item Category", fileName = "ItemCategory_")]
    public class ItemCategoryDefinition : 
        CategoryDefinition<ItemCategoryDefinition, ItemDefinition>
    {
        [Tooltip("Base actions that every item under this category has.")]
        [ReorderableList]
        [SerializeField]
        private ItemAction[] _baseActions = Array.Empty<ItemAction>();

        [Tooltip("Default tag for items under this category.")]
        [DefinitionReference(NullElement = ItemTagDefinition.Untagged)]
        [SerializeField]
        private DefinitionReference<ItemTagDefinition> _defaultItemTag;

        public ItemAction[] BaseActions => _baseActions;

        public DefinitionReference<ItemTagDefinition> DefaultItemTag => _defaultItemTag;
    }
}