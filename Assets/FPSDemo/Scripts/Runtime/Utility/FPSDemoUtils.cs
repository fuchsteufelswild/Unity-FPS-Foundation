using Nexora.InventorySystem;
using UnityEngine;

namespace Nexora
{
    public static partial class ItemTags
    {
        public static readonly DefinitionReference<ItemTagDefinition> HandheldTag
            = DefinitionRegistry<ItemTagDefinition>.GetByName("Handheld")?.ID ?? DataConstants.NullID;

        public static readonly DefinitionReference<ItemTagDefinition> SightAttachmentTag
            = DefinitionRegistry<ItemTagDefinition>.GetByName("Sight Attachment")?.ID ?? DataConstants.NullID;
    }

    public static partial class ItemRarities
    {
        public static readonly DefinitionReference<ItemRarityDefinition> Common
            = DefinitionRegistry<ItemRarityDefinition>.GetByName("Common")?.ID ?? DataConstants.NullID;

        public static readonly DefinitionReference<ItemRarityDefinition> Rare
            = DefinitionRegistry<ItemRarityDefinition>.GetByName("Rare")?.ID ?? DataConstants.NullID;
    }

    public static partial class DynamicItemProperties
    {
        public static readonly DefinitionReference<DynamicItemPropertyDefinition> Durability
            = DefinitionRegistry<DynamicItemPropertyDefinition>.GetByName("Durability")?.ID ?? DataConstants.NullID;

        public static readonly DefinitionReference<DynamicItemPropertyDefinition> AmmoInMagazine
            = DefinitionRegistry<DynamicItemPropertyDefinition>.GetByName("Ammo In Magazine")?.ID ?? DataConstants.NullID;

        public static readonly DefinitionReference<DynamicItemPropertyDefinition> FireMode
            = DefinitionRegistry<DynamicItemPropertyDefinition>.GetByName("Fire Mode")?.ID ?? DataConstants.NullID;
    }
}
