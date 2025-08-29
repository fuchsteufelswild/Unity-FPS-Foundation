using Nexora.Audio;
using System;
using System.Collections;
using UnityEngine;

namespace Nexora.InventorySystem
{
    public enum ItemDropPoint
    {
        Body = 0,
        Feet = 1
    }

    /// <summary>
    /// Abstract drop action for items. Provides base functionality and
    /// should override <see cref="SpawnPickup"/> method to specialize the functionality.
    /// </summary>
    [Serializable]
    public abstract class DropAction : ItemAction
    {
        [Tooltip("Offset from the drop origin point.")]
        [SerializeField]
        private Vector3 _dropPositionOffset;

        [Tooltip("Where the item pickup should be spawned, body, feet etc.")]
        [SerializeField]
        private ItemDropPoint _dropPoint;

        [Tooltip("The force applied to the dropped item.")]
        [Range(0f, 100f)]
        [SerializeField]
        protected float _dropForceMagnitude;

        [Tooltip("Sound will be played when this item is dropped.")]
        [SerializeField]
        protected AudioCue _dropSound;

        [Tooltip("Should this item be removed from the source inventory when it is dropped.")]
        [SerializeField]
        private bool _removeFromInventoryOnDrop = true;

        private const float RandomRotationInfluence = 0.1f;
        private const float ObstacleDetectionDistance = 0.5f;
        private const float ObstacleDetectionRadius = 0.5f;

        protected override IEnumerator ExecuteAction(IItemActionContext actionContext, Slot slot, ItemStack itemStack, float duration)
        {
            if(TryGetPickupPrefab(itemStack, out ItemPickupBase pickupPrefab) == false)
            {
                Debug.LogError("Items without a pickup prefab, cannot be dropped.");
                yield break;
            }

            if(duration > 0f)
            {
                yield return new Delay(duration);
            }

            ItemPickupBase pickupInstance = SpawnPickup(actionContext, slot, pickupPrefab); 
            if(pickupInstance == null)
            {
                yield break;
            }
            pickupInstance.AttachItem(itemStack);

            if(_removeFromInventoryOnDrop)
            {
                slot.Clear();
            }
        }

        protected bool TryGetPickupPrefab(ItemStack itemStack, out ItemPickupBase pickupPrefab)
        {
            pickupPrefab = itemStack.Item.ItemDefinition.GetWorldPickupPrefab(itemStack.Quantity);
            return pickupPrefab != null;
        }

        protected ItemPickupBase SpawnPickup(IItemActionContext actionContext, Slot slot, ItemPickupBase pickupPrefab)
        {
            if (actionContext == null)
            {
                return DropPickup(slot, pickupPrefab);
            }

            return ThrowPickup(actionContext, pickupPrefab);
        }

        /// <summary>
        /// Drops pickup from the origin of the inventory directly.
        /// </summary>
        protected ItemPickupBase DropPickup(Slot slot, ItemPickupBase pickupPrefab)
        {
            Transform inventoryTransform = slot.Storage?.Inventory?.transform;
            if(inventoryTransform == null)
            {
                Debug.LogError("Missing container transform.", this);
                return null;
            }

            AudioModule.Instance.PlayCueOneShot(_dropSound, inventoryTransform.position);
            return Instantiate(pickupPrefab, inventoryTransform.position, Quaternion.identity);
        }

        /// <summary>
        /// Throws the pickup in the direction of the facing of the <paramref name="actionContext"/>.
        /// </summary>
        protected ItemPickupBase ThrowPickup(IItemActionContext actionContext, ItemPickupBase pickupPrefab)
        {
            Transform dropOrigin = GetDropOrigin(actionContext);
            bool isPathBlocked = CheckForObstruction(dropOrigin);

            Vector3 spawnPosition = CalculateSpawnPosition(dropOrigin, isPathBlocked);
            Quaternion spawnRotation = CalculateSpawnRotation(dropOrigin);

            ItemPickupBase pickupInstance = Instantiate(pickupPrefab, spawnPosition, spawnRotation);
            ApplyDropPhysics(actionContext, pickupInstance, dropOrigin, isPathBlocked);

            PlayAudio(actionContext, spawnPosition);
            return pickupInstance;
        }

        /// <returns><paramref name="actionContext"/> drop origin.</returns>
        protected Transform GetDropOrigin(IItemActionContext actionContext) => actionContext.GetDropPointTransform(_dropPoint);

        /// <summary>
        /// Checks if any obstacle is present in the direction of the throw.
        /// </summary>
        /// <returns></returns>
        private bool CheckForObstruction(Transform dropOrigin)
        {
            var ray = new Ray(dropOrigin.position, dropOrigin.forward);
            return PhysicsUtils.SphereCastOptimized(ray, ObstacleDetectionRadius, ObstacleDetectionDistance, out _, Layers.SimpleSolidObjectsMask);
        }

        private Vector3 CalculateSpawnPosition(Transform dropOrigin, bool isPathBlocked)
        {
            if(isPathBlocked)
            {
                // Drop origin is too close to some different object, we choose root transform as the horizontal plane
                return dropOrigin.root.position.WithY(dropOrigin.position.y);
            }

            return dropOrigin.position + dropOrigin.TransformVector(_dropPositionOffset);
        }

        private Quaternion CalculateSpawnRotation(Transform dropOrigin)
        {
            Quaternion baseRotation = Quaternion.LookRotation(dropOrigin.forward);
            Quaternion randomRotation = UnityEngine.Random.rotationUniform;
            return Quaternion.Slerp(baseRotation, randomRotation, RandomRotationInfluence.Jitter(0.03f));
        }

        /// <summary>
        /// Applies physics to the <paramref name="pickup"/>, just after it was created.
        /// </summary>
        /// <remarks>
        /// No action means regular drop.
        /// </remarks>
        /// <param name="actionContext">The owner of the inventory, this object is dropped from.</param>
        /// <param name="pickup">Pickup to apply physics on.</param>
        /// <param name="dropOrigin">Origin of the drop.</param>
        /// <param name="isPathBlocked">Is the path in front of the <paramref name="actionContext"/> blocked?</param>
        protected abstract void ApplyDropPhysics(
            IItemActionContext actionContext, 
            ItemPickupBase pickup, 
            Transform dropOrigin, 
            bool isPathBlocked);

        /// <summary>
        /// Plays audio at the <paramref name="actionContext"/>.
        /// </summary>
        /// <remarks>
        /// Like it can be played by <paramref name="actionContext"/> specific audio player.
        /// </remarks>
        protected void PlayAudio(IItemActionContext actionContext, Vector3 spawnPosition)
        {
            if (actionContext != null)
            {
                actionContext.PlayAudio(_dropSound);
            }
            else
            {
                AudioModule.Instance.PlayCueOneShot(_dropSound, spawnPosition);
            }
        }
    }
}