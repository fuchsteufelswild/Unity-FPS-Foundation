using Nexora.InventorySystem;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{

    // TODO: Make it like ItemBasedSelector so that we can have list of indexed attachment selectors
    // and to provide query for the wanted DefinitionReference.
    public sealed class IndexedAttachmentSelector : AttachmentSelector
    {
        [SerializeField]
        private GunModeValidator _validator;

        [SerializeField]
        private IndexBasedAttachmentProvider _indexBasedAttachmentProvider;

        private IGun _gun;

        public override GunAttachment CurrentAttachment => _indexBasedAttachmentProvider.GetCurrentAttachment();

        protected override void Awake()
        {
            _gun = GetComponent<IGun>();
            base.Awake();
        }

        protected override void InitializeConfigurationProvider() => _configurationProvider = _indexBasedAttachmentProvider;

        public override void ToggleNextAttachment()
        {
            if(_validator.CanToggleMode(_gun) == false)
            {
                return;
            }

            int? nextIndex = _indexBasedAttachmentProvider.FindNextValidAttachmentIndex();
            if(nextIndex.HasValue == false)
            {
                return;
            }

            _validator.RecordToggleTime();
            _configurationProvider.AttachConfiguration(nextIndex.Value);
            OnAttachmentChanged(CurrentAttachment);
        }

        public override bool IsTarget(int targetPropertyID) => _indexBasedAttachmentProvider.IsTarget(targetPropertyID);

        [Serializable]
        private sealed class IndexBasedAttachmentProvider : AttachmentProvider
        {
            [Tooltip("Attachment index property, (e.g 'Sight Attachment', 'Supressor' etc.)")]
            [SerializeField]
            private DefinitionReference<DynamicItemPropertyDefinition> _indexProperty;

            [Tooltip("Attachments controlled by this provider, only one of them might be active at the same time.")]
            [SerializeField]
            private GunAttachment[] _attachments;

            private DynamicItemProperty _attachedProperty;
            private int _selectedIndex;

            public bool IsTarget(DefinitionReference<DynamicItemPropertyDefinition> property) => property == _indexProperty;

            public override int AttachmentCount => _attachments.Length;
            public override GunAttachment GetCurrentAttachment() => _attachments[_selectedIndex];

            protected override void OnAttachedSlotChanged(Slot slot)
            {
                if (_attachedProperty != null)
                {
                    _attachedProperty.ValueChanged -= OnPropertyChanged;
                }

                if (slot.Item?.TryGetDynamicProperty(_indexProperty, out _attachedProperty) ?? false)
                {
                    AttachConfiguration(_attachedProperty.IntegerValue);
                    _attachedProperty.ValueChanged += OnPropertyChanged;
                }
            }

            private void OnPropertyChanged(DynamicItemProperty property) => AttachConfiguration(property.IntegerValue);

            public override void AttachConfiguration(int attachmentIndex)
            {
                ValidateIndex(attachmentIndex);

                if (_selectedIndex == attachmentIndex)
                {
                    return;
                }

                DetachCurrentConfiguration();
                _selectedIndex = attachmentIndex;
                _attachments[_selectedIndex].Attach();

                if (_attachedProperty != null)
                {
                    _attachedProperty.IntegerValue = attachmentIndex;
                }
            }

            public override void DetachCurrentConfiguration()
            {
                ValidateIndex(_selectedIndex);

                _attachments[_selectedIndex].Detach();
            }

            private void ValidateIndex(int index)
            {
                if (_selectedIndex < 0 || _selectedIndex >= _attachments.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }

            /// <summary>
            /// Find the next valid attachment index in the range [<paramref name="startingIndex"/>, length] then [0, <paramref name="startingIndex"/> - 1].
            /// </summary>
            /// <param name="startingIndex">Starting index.</param>
            /// <returns>Next valid attachment index if found, else <see langword="null"/>.</returns>
            public int? FindNextValidAttachmentIndex(int? startingIndex = null)
            {
                startingIndex ??= _selectedIndex;

                for (int i = 1; i < _attachments.Length; i++)
                {
                    int nextIndex = (i + startingIndex.Value) % _attachments.Length;

                    if (_attachments[nextIndex].CanAttach())
                    {
                        return nextIndex;
                    }
                }

                return null;
            }
        }
    }
}