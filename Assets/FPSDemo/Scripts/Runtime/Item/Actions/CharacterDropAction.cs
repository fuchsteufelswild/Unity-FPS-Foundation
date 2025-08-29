using Nexora.Audio;
using Nexora.InventorySystem;
using UnityEngine;

namespace Nexora.FPSDemo.InventorySystem
{
    [CreateAssetMenu(menuName = CreateMenuPath + "Drop Action", fileName = "ItemDropAction_")]
    public sealed class CharacterDropAction : DropAction
    {
        public override bool CanPerform(IItemActionContext actionContext, ItemStack itemStack) => true;
        public override float GetDuration(IItemActionContext actionContext, ItemStack itemStack) => 0f;

        protected override void ApplyDropPhysics(IItemActionContext actionContext, ItemPickupBase pickup, Transform dropOrigin, bool isPathBlocked)
        {
            var pickupRigidbody = pickup.GetComponent<Rigidbody>();
            Vector3 dropForce = isPathBlocked == false
                ? (dropOrigin.forward + Vector3.up * 0.25f).normalized * _dropForceMagnitude
                : Vector3.zero;

            float dropTorque = isPathBlocked == false
                ? _dropForceMagnitude
                : 0f;

            actionContext.ThrowObject(pickupRigidbody, dropForce, dropTorque);
            PlayAudio(actionContext, default);
        }
    }
}