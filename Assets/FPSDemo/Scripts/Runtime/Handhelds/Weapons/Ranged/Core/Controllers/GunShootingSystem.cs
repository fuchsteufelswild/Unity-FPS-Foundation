using Nexora.FPSDemo.Options;
using Nexora.InventorySystem;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public sealed class GunShootingSystem
    {
        private GunComponentManager _components;
        private GunAccuracySystem _accuracySystem;

        private bool _isShooting;

        public GunShootingSystem(GunComponentManager components, GunAccuracySystem accuracySystem)
        {
            _components = components;
            _accuracySystem = accuracySystem;
            _components.OnTriggerPulled += OnTriggerPulled;
        }

        private void OnTriggerPulled()
        {
            bool hasEnoughAmmo = CombatOptions.Instance.UnlimitedMagazine
                || _components.Magazine.CurrentAmmoCount >= _components.FiringMechanism.AmmoUsedPerShot;

            if(hasEnoughAmmo == false)
            {
                return;
            }

            _isShooting = true;
            _components.FiringMechanism.Fire(_accuracySystem.CurrentAccuracy, _components.ImpactEffect);
            _components.MuzzleEffect.TriggerFireEffect();
        }

        public void UpdateShooting(float deltaTime)
        {
            if(_isShooting == false)
            {
                return;
            }

            if(_components.TriggerMechanism.IsTriggerHeld && Time.time < _components.TriggerMechanism.NextShootTime)
            {
                return;
            }

            _components.MuzzleEffect.TriggerStopFireEffect();
            _isShooting = false;
        }
    }
}