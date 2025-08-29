using Nexora.InventorySystem;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Selects attachments based on <see cref="ItemDefinition"/>.
    /// </summary>
    public sealed class ItemBasedAttachmentSelector : AttachmentSelector
    {
        [SerializeField]
        private ItemBasedAttachmentProvider _itemBasedAttachmentProvider;

        public override GunAttachment CurrentAttachment => _itemBasedAttachmentProvider.GetCurrentAttachment();
        protected override void InitializeConfigurationProvider()
        {
            _itemBasedAttachmentProvider.SetAttachmentChangeCallback(AttachmentChangedCallback);
            _configurationProvider = _itemBasedAttachmentProvider;
        }

        private void AttachmentChangedCallback() => OnAttachmentChanged(CurrentAttachment);

        public override void ToggleNextAttachment()
        {
            // There is no need or logic for item-based attachments to toggle one after another at the moment,
            // If it is required can be implemented through configurations array and an enumerator.
        }

        public override bool IsTarget(int targetPropertyID) => _itemBasedAttachmentProvider.IsTarget(targetPropertyID);

        [Serializable]
        private sealed class ItemBasedAttachmentProvider : AttachmentProvider
        {
            [Tooltip("Configurations for attached items.")]
            [ReorderableList(ListStyle.Lined, ElementLabel = "Configuration", Foldable = false)]
            [SerializeField]
            private AttachmentItemConfigurations[] _configurations = Array.Empty<AttachmentItemConfigurations>();

            private IItem _attachedItem;

            public override int AttachmentCount => _configurations.Length;

            public bool IsTarget(DefinitionReference<DynamicItemPropertyDefinition> targetProperty)
            {
                foreach (var configuration in _configurations)
                {
                    if (configuration.IsTarget(targetProperty))
                    {
                        return true;
                    }
                }

                return false;
            }

            public override GunAttachment GetCurrentAttachment()
            {
                foreach(var configuration in _configurations)
                {
                    if(configuration.ActiveAttachment != null)
                    {
                        return configuration.ActiveAttachment;
                    }
                }

                return null;
            }

            public void SetAttachmentChangeCallback(UnityAction attachmentChangedCallback)
            {
                foreach (var configuration in _configurations)
                {
                    configuration.AttachmentChangedCallback = attachmentChangedCallback;
                }
            }

            protected override void OnAttachedSlotChanged(Slot slot)
            {
                DetachCurrentConfiguration();

                IItem item = slot.Item;
                if (item != null)
                {
                    foreach (var configuration in _configurations)
                    {
                        configuration.AttachToItem(item);
                    }
                }

                _attachedItem = item;
            }

            public override void DetachCurrentConfiguration()
            {
                if (_attachedItem == null)
                {
                    return;
                }

                foreach (var configuration in _configurations)
                {
                    configuration.DetachFromItem(_attachedItem);
                }
            }
        }
    }

    /// <summary>
    /// Keeps a <see cref="DynamicItemPropertyDefinition"/> which is for determining the dynamic property
    /// like 'Sight Scope', which should require different <see cref="ItemDefinition"/> to attach the attachment.
    /// Like scope, red dot.
    /// </summary>
    /// <remarks>
    /// On attaching to and detaching from <see cref="IItem"/>, it finds the <see cref="IItem"/>'s dynamic property
    /// of the same type if had, and subscribe/unsubscribe from that property to 'react/not react' to changes in the attachment change.
    /// </remarks>
    public sealed class AttachmentItemConfigurations
    {
        [Tooltip("Property type of the item, 'Scope', 'Supressor' etc.")]
        [SerializeField]
        private DefinitionReference<DynamicItemPropertyDefinition> _attachmentTypeProperty;

        [Tooltip("Attachment configurations available for the attachment property.")]
        [ReorderableList(ListStyle.Lined), LabelByChild("_attachment")]
        [SerializeField]
        private AttachmentItemConfiguration[] _configurations = Array.Empty<AttachmentItemConfiguration>();

        private AttachmentItemConfiguration _activeConfiguration;

        public bool IsTarget(DefinitionReference<DynamicItemPropertyDefinition> targetProperty)
        {
            return _attachmentTypeProperty == targetProperty;
        }

        public GunAttachment ActiveAttachment => _activeConfiguration?.Attachment;

        public UnityAction AttachmentChangedCallback { get; set; }

        public void DetachFromItem(IItem item)
        {
            if (item.TryGetDynamicProperty(_attachmentTypeProperty, out var dynamicProperty))
            {
                dynamicProperty.ValueChanged -= OnPropertyChanged;
            }
        }

        /// <summary>
        /// Attaches this configuration to the <paramref name="item"/>. Main task is to listen to
        /// the changes in the listened dynamic property value of the <paramref name="item"/>.
        /// </summary>
        /// <param name="item"></param>
        public void AttachToItem(IItem item)
        {
            if (item.TryGetDynamicProperty(_attachmentTypeProperty, out var dynamicProperty))
            {
                AttachConfigurationWithID(dynamicProperty.LinkedItemID);
                dynamicProperty.ValueChanged += OnPropertyChanged;
            }
        }

        /// <summary>
        /// Called when the property of the <see cref="IItem"/> of the same property type has changed.
        /// </summary>
        /// <param name="property">New linked item property.</param>
        private void OnPropertyChanged(DynamicItemProperty property)
        {
            AttachConfigurationWithID(property.LinkedItemID);
            AttachmentChangedCallback?.Invoke();
        }

        /// <summary>
        /// Attaches configuration which contains <see cref="ItemDefinition"/> with the same <paramref name="linkedItemID"/>.
        /// </summary>
        /// <param name="linkedItemID">Target linked item ID.</param>
        private void AttachConfigurationWithID(int linkedItemID)
        {
            AttachmentItemConfiguration targetConfiguration = Array.Find(_configurations,
                config => config.ItemID == linkedItemID);

            if (targetConfiguration == null)
            {
                return;
            }

            _activeConfiguration?.Attachment.Detach();
            targetConfiguration.Attachment.Attach();
            _activeConfiguration = targetConfiguration;
        }
    }

    /// <summary>
    /// Keeps an <see cref="ItemDefinition"/> ID, and its corresponding <see cref="GunAttachment"/>.
    /// </summary>
    [Serializable]
    public sealed class AttachmentItemConfiguration
    {
        [Tooltip("Underlying item definition of the attachment (e.g 'Sight Scope' item).")]
        [SerializeField]
        private DefinitionReference<ItemDefinition> _attachmentItem;

        [Tooltip("Concrete attachment that is a behaviour.")]
        [SerializeField, NotNull]
        private GunAttachment _attachment;

        public GunAttachment Attachment => _attachment;
        public int ItemID => _attachmentItem;
    }
}