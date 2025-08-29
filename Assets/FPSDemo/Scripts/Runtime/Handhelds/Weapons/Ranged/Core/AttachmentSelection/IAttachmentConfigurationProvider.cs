using Nexora.InventorySystem;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Class that provides attachments depending on the underlying implementation, 
    /// index-based, item-based etc.
    /// </summary>
    public interface IAttachmentConfigurationProvider
    {
        /// <summary>
        /// Number of attachments contained.
        /// </summary>
        int AttachmentCount { get; }

        /// <summary>
        /// Initializes the provider with <paramref name="handheldItem"/>, it will provide 
        /// attachment configuration for <paramref name="handheldItem"/>.
        /// </summary>
        void Initialize(IHandheldItem handheldItem);

        /// <summary>
        /// Detaches the current <see cref="GunAttachment"/> and attaches the <see cref="GunAttachment"/>
        /// that is on the <paramref name="attachmentIndex"/>.
        /// </summary>
        /// <param name="attachmentIndex">Index of the attachment in the list of attachments.</param>
        void AttachConfiguration(int attachmentIndex);

        /// <summary>
        /// Detaches the current <see cref="GunAttachment"/> if had.
        /// </summary>
        void DetachCurrentConfiguration();

        /// <returns>Current attachment in the provider.</returns>
        GunAttachment GetCurrentAttachment();
    }

    public abstract class AttachmentProvider : IAttachmentConfigurationProvider
    {
        private IHandheldItem _handheldItem;

        public abstract int AttachmentCount { get; }

        public void Initialize(IHandheldItem handheldItem)
        {
            _handheldItem = handheldItem ?? throw new ArgumentNullException(nameof(handheldItem));
            _handheldItem.AttachedSlotChanged += OnAttachedSlotChanged;
            OnAttachedSlotChanged(_handheldItem.Slot);
        }

        /// <summary>
        /// Called upon the owner <see cref="IHandheldItem"/> has its <see cref="Slot"/> changed.
        /// </summary>
        /// <param name="slot"></param>
        protected abstract void OnAttachedSlotChanged(Slot slot);

        public virtual void AttachConfiguration(int attachmentIndex) { }
        public abstract GunAttachment GetCurrentAttachment();
        public abstract void DetachCurrentConfiguration();
    }
}