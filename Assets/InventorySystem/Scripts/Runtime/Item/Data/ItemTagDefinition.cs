using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Defines tags for items, can be used group items or mark item as a special effect.
    /// </summary>
    /// <remarks>
    /// <b>Uses name property to distinguish one tag from another.</b>
    /// </remarks>
    [CreateAssetMenu(menuName = "Nexora/Items/Create Item Tag", fileName = "ItemTag_")]
    public sealed class ItemTagDefinition : Definition<ItemTagDefinition>
    {
        public const string Untagged = "Untagged";
    }
}