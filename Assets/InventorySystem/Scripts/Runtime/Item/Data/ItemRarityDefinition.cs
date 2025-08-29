using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Defines rarity level for items.
    /// </summary>
    /// <remarks>
    /// <b>Uses name property to distinguish one rarity from another.</b>
    /// </remarks>
    [CreateAssetMenu(menuName = "Nexora/Items/Create Item Rarity", fileName = "ItemRarity_")]
    public sealed class ItemRarityDefinition : Definition<ItemRarityDefinition>
    {
        [Tooltip("Color to be used for displaying items in the UI, " +
            "it might also be used to denote color of the effect on the item in gameplay.")]
        [SerializeField]
        private Color _rarityColor = Color.white;

        [Tooltip("Denotes how rare this rarity.")]
        [Range(0f, 10f)]
        [SerializeField]
        private float _rarityValue;

        public Color RarityColor => _rarityColor;
        public float RarityValue => _rarityValue;
    }
}