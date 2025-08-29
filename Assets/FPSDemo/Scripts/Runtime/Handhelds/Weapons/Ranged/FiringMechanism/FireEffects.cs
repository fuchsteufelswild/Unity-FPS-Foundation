using Nexora.InventorySystem;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Effects to be played when gun firing mechanism is triggered.
    /// </summary>
    [Serializable]
    public abstract class FireEffect
    {
        public virtual void Initialize(IHandheldItem handheldItem) { }

        public virtual void Enable(IGun gun) { }
        public virtual void Disable(IGun gun) { }

        /// <summary>
        /// Executes the fire effect.
        /// </summary>
        /// <param name="gun">The gun that fired the shot.</param>
        /// <param name="shotResult">Information about the shot's result.</param>
        public abstract void Play(IGun gun, in ShotResult shotResult);
    }

    /// <summary>
    /// Removes ammo from pouch.
    /// </summary>
    [Serializable]
    public sealed class ConsumeAmmoEffect : FireEffect
    {
        [SerializeField, Range(1, 100)]
        private int _ammoPerShot = 1;

        public int AmmoPerShot => _ammoPerShot;

        public override void Play(IGun gun, in ShotResult shotResult) => gun.Magazine.TryUseAmmo(_ammoPerShot);
    }

    /// <summary>
    /// Plays visual effects of the muzzle.
    /// </summary>
    [Serializable]
    public sealed class MuzzleVisualsEffect : FireEffect
    {
        public override void Play(IGun gun, in ShotResult shotResult) => gun.MuzzleEffect.TriggerFireEffect();
    }

    /// <summary>
    /// Triggers animator parameters in the event of shot.
    /// </summary>
    [Serializable]
    public sealed class FireAnimatorEffect : FireEffect
    {
        private IHandheld _handheld;

        public override void Enable(IGun gun) => _handheld = gun as IHandheld;

        public override void Play(IGun gun, in ShotResult shotResult)
        {
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsEmpty, false);
            _handheld.Animator.SetTrigger(HandheldAnimationConstants.Shoot);
        }
    }

    /// <summary>
    /// Reduces durability of the item in the inventory after the shot.
    /// </summary>
    [Serializable]
    public sealed class DurabilityDamageEffect : FireEffect
    {
        [Tooltip("Amount of durability reduced after each shot.")]
        [SerializeField, Range(0f, 50f)]
        private float _durabilityUsage = 1f;

        private IHandheldItem _handheldItem;
        private DynamicItemProperty _durabilityProperty;

        public override void Initialize(IHandheldItem handheldItem)
        {
            handheldItem.AttachedSlotChanged += OnSlotChanged;
            OnSlotChanged(handheldItem.Slot);
        }

        private void OnSlotChanged(Slot slot)
        {
            _durabilityProperty = slot.Item?.GetDynamicProperty(DynamicItemProperties.Durability);
        }

        public override void Play(IGun gun, in ShotResult shotResult)
        {
            if(_durabilityProperty != null)
            {
                _durabilityProperty.FloatValue -= _durabilityUsage;
            }
        }
    }
}