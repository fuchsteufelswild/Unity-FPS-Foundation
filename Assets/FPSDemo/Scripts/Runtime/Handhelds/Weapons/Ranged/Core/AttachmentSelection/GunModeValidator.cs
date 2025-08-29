using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunModeValidator
    {
        /// <returns>Can <paramref name="gun"/> toggle mode in this moment?</returns>
        bool CanToggleMode(IGun gun);
    }

    [Serializable]
    public class GunModeValidator : IGunModeValidator
    {
        [Tooltip("Wait between consecutive mode changes in the gun.")]
        [SerializeField, Range(0f, 5f)]
        private float _changeModeCooldown;

        private float _lastToggleTime;

        public void RecordToggleTime() => _lastToggleTime = Time.time;

        public virtual bool CanToggleMode(IGun gun)
        {
            return Time.time >= _lastToggleTime + _changeModeCooldown
                && gun.Magazine.IsReloading == false
                && gun.AimBehaviour.IsAiming == false
                && gun.TriggerMechanism.IsTriggerHeld == false;
        }
    }
}