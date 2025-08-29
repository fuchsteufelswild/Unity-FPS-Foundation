using UnityEngine;

namespace Nexora.InventorySystem
{
    /// <summary>
    /// Data for <see cref="DynamicItemProperty"/>. Just defines the <see cref="ItemPropertyValueType"/>
    /// of the property, rest just depends on the gameplay logic.
    /// </summary>
    /// <remarks>
    /// It is useful when accessing specific <see cref="DynamicItemProperty"/> in the game.
    /// When you want to grab "Durability" of an item, you can just easily query it through
    /// all of the item's properties using <see cref="Definition.ID"/> as <see cref="DynamicItemPropertyDefinition"/>
    /// is also a <see cref="Definition"/>.
    /// <br></br><br></br>
    /// It is especially useful in the way the system is designed, as <see cref="DynamicItemProperty"/> can hold
    /// any value and through the data we can treat it as any type we like.
    /// </remarks>
    [CreateAssetMenu(menuName = "Nexora/Inventory/Create Dynamic Item Property", fileName = "Property_")]
    public sealed class DynamicItemPropertyDefinition : Definition<DynamicItemPropertyDefinition>
    {
        [Tooltip("What value this property really maps to, (int, double, or Item reference?)")]
        [SerializeField]
        private ItemPropertyValueType _propertyValueType;

        [Tooltip("Description of the property, explains what it controls.")]
        [SerializeField, Multiline(4)]
        private string _description;

        public ItemPropertyValueType PropertyValueType => _propertyValueType;

        public override string Description => _description;
    }
}