using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.InventorySystem
{
    [CreateAssetMenu(menuName = "Nexora/Items/Create Item", fileName = "Item_")]
    public class ItemDefinition : 
        CategoryMemberDefinition<ItemDefinition, ItemCategoryDefinition>
    {
        [Tooltip("Brief description for tooltips or summaries.")]
        [SerializeField]
        private string _shortDescription;

        [Tooltip("Detailed description for expanded UI views. Falls back to short description if not provided.")]
        [SerializeField, Multiline(2)]
        private string _longDescription;

        [SerializeField, SpriteThumbnail(), SpaceArea]
        private Sprite _icon;

        [Tooltip("Prefab for item's physical representation when dropped in world.")]
        [SerializeField]
        private ItemPickupBase _worldPickupPrefab;

        [Tooltip("Prefab used when dropping stacked items (if different from single pickup prefab).")]
        [SerializeField]
        private ItemPickupBase _stackedWorldPickupPrefab;

        [Tooltip("How much does the item weigh (in kilograms).")]
        [SerializeField, Range(0f, 100f)]
        private float _weight;

        [Tooltip("Maximum stack size in one slot of any container. 1 for non-stackables.")]
        [SerializeField, Range(0f, 100f)]
        private int _maxStackSize;

        [Tooltip("Tag to denote group, like 'Attachment', 'Headwear' etc.")]
        [DefinitionReference(NullElement = ItemTagDefinition.Untagged)]
        [SerializeField]
        private DefinitionReference<ItemTagDefinition> _itemTag;

        [Tooltip("Denotes the rarity of the item.")]
        [DefinitionReference]
        [SerializeField]
        private DefinitionReference<ItemRarityDefinition> _itemRarity;

        [SpaceArea, Tooltip("Actions applicaple to this item, 'Equip', 'Drop', etc.")]
        [ReorderableList]
        [SerializeField]
        private ItemAction[] _itemActions = Array.Empty<ItemAction>();

        [Tooltip("Runtime-modifiable properties of the item, 'durability', 'charges' etc.")]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        [SerializeField]
        private DynamicItemPropertyGenerator[] _dynamicProperties;

        [SerializeReference]
        [Tooltip("Static data that is shared all of this item's instances, " +
            "also denotes the if specific operation can be performed on the item.")]
        [ReorderableList(elementLabel: "Shared Data")]
        [ReferencePicker(typeof(ItemBehaviour), TypeGrouping.ByFlatName)]
        private ItemBehaviour[] _sharedItemBehaviours;

        public override string Description => _shortDescription;
        public string LongDescription => string.IsNullOrEmpty(_longDescription) ? _shortDescription : _longDescription;
        public override Sprite Icon => _icon;

        public ItemPickupBase WorldPickupPrefab => _worldPickupPrefab;
        public ItemPickupBase StackedWorldPickupPrefab => _stackedWorldPickupPrefab;

        public int MaxStackSize => _maxStackSize;
        public float Weight => _weight;

        public DefinitionReference<ItemTagDefinition> ItemTag => _itemTag;
        public DefinitionReference<ItemRarityDefinition> ItemRarity => _itemRarity;

        public ItemAction[] ItemActions => _itemActions;
        public ItemBehaviour[] SharedItemBehaviours => _sharedItemBehaviours;

        public ItemPickupBase GetWorldPickupPrefab(int count = 1)
            => count > 1 && _stackedWorldPickupPrefab != null ? _stackedWorldPickupPrefab : _worldPickupPrefab;

        public Color RarityColor => _itemRarity.Definition.RarityColor;

        public IReadOnlyList<DynamicItemPropertyGenerator> DynamicItemPropertyGenerators => _dynamicProperties;

#if UNITY_EDITOR
        public override void EditorValidate(in EditorValidationArgs validationArgs)
        {
            base.EditorValidate(validationArgs);

            if(validationArgs.ValidationTrigger is ValidationTrigger.ManualRefresh)
            {
                CollectionExtensions.RemoveDuplicates(ref _itemActions);
                CollectionExtensions.RemoveDuplicates(ref _dynamicProperties);
            }
            else if(validationArgs.ValidationTrigger is ValidationTrigger.Creation)
            {
                if(BelongsToCategory && _itemTag.IsNull == false)
                {
                    _itemTag = OwningCategory.DefaultItemTag;
                }
            }
        }
#endif
    }

    public static class ItemDefinitionUtility
    {
        /// <returns>All <see cref="ItemDefinition"/> defined with the <see cref="ItemTagDefinition"/> matching <paramref name="tag"/>.</returns>
        public static List<ItemDefinition> GetAllItemsWithTag(ItemTagDefinition tag)
        {
            if(tag == null)
            {
                return null;
            }

            DefinitionReference<ItemTagDefinition> tagReference = tag;
            if(tagReference.IsNull)
            {
                return null;
            }

            var result = new List<ItemDefinition>();
            foreach(var item in DefinitionRegistry<ItemDefinition>.AllDefinitions)
            {
                if(item.ItemTag == tagReference)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <returns>All <see cref="ItemDefinition"/> that has any attached <see cref="ItemAction"/> of type <typeparamref name="T"/>.</returns>
        public static List<ItemDefinition> GetAllItemsWithActionOfType<T>()
            where T : ItemAction
        {
            var result = new List<ItemDefinition>();
            foreach(var item in DefinitionRegistry<ItemDefinition>.AllDefinitions)
            {
                if(item.HasActionOfType<T>())
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <returns>All <see cref="ItemDefinition"/> defined with the <see cref="DynamicItemPropertyDefinition"/> matching <paramref name="dynamicProperty"/>.</returns>
        public static List<ItemDefinition> GetAllItemsWithProperty(DynamicItemPropertyDefinition dynamicProperty)
        {
            if (dynamicProperty == null)
            {
                return null;
            }

            DefinitionReference<DynamicItemPropertyDefinition> dynamicPropertyReference = dynamicProperty;
            if (dynamicPropertyReference.IsNull)
            {
                return null;
            }

            var result = new List<ItemDefinition>();
            foreach (var item in DefinitionRegistry<ItemDefinition>.AllDefinitions)
            {
                if (item.HasProperty(dynamicProperty))
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }
}