using Nexora.Audio;
using Nexora.ObjectPooling;
using Nexora.SaveSystem;
using System;
using System.Diagnostics;
using UnityEngine;

namespace Nexora.InventorySystem
{
    public abstract class ItemPickupBase : 
        MonoBehaviour,
        ISaveableComponent,
        IUnityObjectPoolListener
    {
        [Tooltip("Denotes default item that will be attached to this item pickup, when no other item is attached.")]
        [DefinitionReference(HasAssetReference = true, HasIcon = true)]
        [SerializeField]
        private DefinitionReference<ItemDefinition> _defaultItem;

        [Tooltip("Audio that will be played when this item is picked up.")]
        [SerializeField]
        private AudioCue _pickupSound;

        [MinMaxSlider(1, 200)]
#if UNITY_EDITOR
        [ShowIf(nameof(IsStackable), true)]
#endif
        [SerializeField]
        private Vector2Int _defaultItemCountRange;

        [Tooltip("Links to define item attachments that are attached to this object pickup.")]
        [ReorderableList(elementLabel: "Link")]
        [SerializeField]
        private ItemAttachmentLink[] _attachmentLinks;


        [NonSerialized]
        protected ItemStack _attachedItem;

        private bool _resetOnAcquiredFromPool = true;

        public ItemStack AttachedItem => _attachedItem;

        protected virtual void Start()
        {
            if(_attachedItem.HasItem == false)
            {
                AttachItem(CreateDefaultItem());
            }
        }

        /// <summary>
        /// Creates a default item based on the <see cref="ItemDefinition"/> attached to
        /// this pickup as a default item.
        /// </summary>
        protected ItemStack CreateDefaultItem()
        {
            if(_defaultItem.IsNull)
            {
                return ItemStack.Empty;
            }

            int stackSize = _defaultItem.Definition.MaxStackSize > 1 
                ? _defaultItemCountRange.GetRandomFromRange() 
                : 1;

            var createdItem = new ItemStack(new Item(_defaultItem), stackSize);

            UpdateAttachmentItems(createdItem.Item);

            return createdItem;
        }

        /// <summary>
        /// Updates the item attachments of the <paramref name="item"/> as given in the attachment link array.
        /// </summary>
        private void UpdateAttachmentItems(IItem item)
        {
            foreach (var attachment in _attachmentLinks)
            {
                if (item.TryGetDynamicProperty(attachment.Property, out var property))
                {
                    property.LinkedItemID = attachment.CurrentAttachment;
                }
            }
        }

        /// <summary>
        /// Makes <paramref name="itemStack"/> contained item in this pickup.
        /// </summary>
        public void AttachItem(ItemStack itemStack)
        {
            if(itemStack.HasItem == false)
            {
                _attachedItem = ItemStack.Empty;
            }

            _attachedItem = itemStack;
            PostAttachItem(itemStack);

            foreach (var attachmentLink in _attachmentLinks)
            {
                attachmentLink.UpdateVisuals(_attachedItem);
            }
        }

        /// <summary>
        /// Post action just before the new item is attached to <see langword="override"/> in derived classes.
        /// </summary>
        protected virtual void PreAttachItem(ItemStack itemStack) { }

        /// <summary>
        /// Post action just after the new item is attached to <see langword="override"/> in derived classes.
        /// </summary>
        protected virtual void PostAttachItem(ItemStack itemStack) { }

        /// <summary>
        /// Pickups the item into <paramref name="targetInventory"/>, if possible and provided 
        /// tries into <paramref name="specificContainer"/> first.
        /// </summary>
        /// <param name="specificContainer">Specific container of the <paramref name="targetInventory"/>, like holster.</param>
        protected PickupResult.PickupStatus PickupItem(IInventory targetInventory, IContainer specificContainer = null)
        {
            if(targetInventory == null)
            {
                return PickupResult.PickupStatus.Failed;
            }

            PrePickupItem(targetInventory, specificContainer);

            PickupResult pickupResultMessage;

            if (specificContainer != null)
            {
                PickupUtility.PickUpItem(specificContainer, _attachedItem, out pickupResultMessage);
                if(pickupResultMessage.Status == PickupResult.PickupStatus.Failed)
                {
                    PickupUtility.PickUpItem(targetInventory, _attachedItem, out pickupResultMessage);
                }
            }
            else
            {
                PickupUtility.PickUpItem(targetInventory, _attachedItem, out pickupResultMessage);
            }

            PostPickupItem(targetInventory, specificContainer, ref pickupResultMessage);
            PlayAudio(pickupResultMessage.Status);

            if(pickupResultMessage.Status == PickupResult.PickupStatus.Full)
            {
                Dispose();
            }

            return pickupResultMessage.Status;
        }

        protected virtual void PrePickupItem(IInventory targetInventory, IContainer specificContainer) { }
        protected virtual void PostPickupItem(IInventory targetInventory, IContainer specificContainer, ref PickupResult pickupResult) { }

        protected abstract void PlayAudio(PickupResult.PickupStatus pickupStatus);

        protected virtual void Dispose()
        {
            if(TryGetComponent<PooledSceneObject>(out var pooledObject))
            {
                pooledObject.Release();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool SetResetOnAcquiredFromPool(bool resetOnAcquiredFromPool) => _resetOnAcquiredFromPool = resetOnAcquiredFromPool;

        public void OnPreAcquired() { }
        public void OnAcquired()
        {
            if(_resetOnAcquiredFromPool)
            {
                AttachItem(CreateDefaultItem());
            }
        }
        public void OnPreReleased() { }
        public void OnReleased() { }

        void ISaveableComponent.Load(object data) => _attachedItem = (ItemStack)data;
        object ISaveableComponent.Save() => _attachedItem;

#if UNITY_EDITOR
        private bool IsStackable() => _defaultItem.IsNull == false && _defaultItem.Definition.MaxStackSize > 1;
#endif

        /// <summary>
        /// Struct to denote the result when pickup an item.
        /// </summary>
        public readonly struct PickupResult
        {
            public enum PickupStatus
            { 
                /// <summary>
                /// Picked up some of the stack/contents.
                /// </summary>
                Partial = 0,

                /// <summary>
                /// Picked everything in the pickup.
                /// </summary>
                Full = 1,

                /// <summary>
                /// Failed to take anything from the pickup.
                /// </summary>
                Failed = 2
            }

            public readonly PickupStatus Status;
            public readonly MessageArgs MessageArgs;

            public PickupResult(PickupStatus status, MessageArgs messageArgs)
            {
                Status = status;
                MessageArgs = messageArgs;
            }
        }


        /// <summary>
        /// Provides a link for a <see cref="DynamicItemPropertyDefinition"/> which has references
        /// to another ItemID.
        /// </summary>
        [Serializable]
        private sealed class ItemAttachmentLink
        {
            [Tooltip("The ID of the propety, the property can be 'Sight', 'DoubleMagazine', 'EnchamentGem' etc.")]
            public DefinitionReference<DynamicItemPropertyDefinition> Property;

            /// <summary>
            /// Current item assigned for the property currently.
            /// </summary>
            public DefinitionReference<ItemDefinition> CurrentAttachment;

            [Tooltip("Visuals that are matched with specific item ID.")]
            [ReorderableList, LabelByChild("Visual")]
            public AttahcmentVisualPair[] AttachmentVisuals;

            public void UpdateVisuals(ItemStack itemStack)
            {
                if(itemStack.Item.TryGetDynamicProperty(Property, out var property))
                {
                    EnableVisualWithID(property.LinkedItemID);
                }
            }

            public void EnableVisualWithID(int itemID)
            {
                for(int i = 0; i < AttachmentVisuals.Length; i++)
                {
                    AttachmentVisuals[i].Visual.SetActive(AttachmentVisuals[i].Item == itemID);
                }
            }

            [Serializable]
            public struct AttahcmentVisualPair
            {
                public DefinitionReference<ItemDefinition> Item;
                public GameObject Visual;
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if(Application.isPlaying == false || DefinitionRegistry<ItemDefinition>.Initialized == false)
            {
                return;
            }

            UnityEditorUtils.SafeOnValidate(this, () =>
            {
                ItemStack defaultItem = CreateDefaultItem();
                foreach(var attachmentLink in _attachmentLinks)
                {
                    attachmentLink.UpdateVisuals(defaultItem);
                }
            });
        }
#endif
    }
}