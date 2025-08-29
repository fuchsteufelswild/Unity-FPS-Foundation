using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Used for trigger that fires automatically, like full auto mode on Assault Rifle.
    /// </summary>
    public sealed class FullAutoTrigger : GunTriggerBehaviour
    {
        [Tooltip("Rounds per minute.")]
        [SerializeField, Range(1f, 5000f)]
        private float _rpm = 400;

        private float _nextShootTime;

        public override float NextShootTime => _nextShootTime;

        public override void TriggerHold()
        {
            base.TriggerHold();

            if(Time.time >= _nextShootTime)
            {
                Fire();
            }
        }

        private void Fire()
        {
            RaiseShootEvent();
            // Correctly translate rounds per minute to rounds per second (minutes / rpm)
            _nextShootTime = Time.time + 60f / _rpm;
        }
    }
}