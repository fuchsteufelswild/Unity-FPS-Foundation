using System.Collections;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public sealed class BurstTrigger : GunTriggerBehaviour
    {
        [Tooltip("Number of shots will be fired with each trigger push.")]
        [SerializeField, Range(0f, 100f)]
        private int _shotsPerBurst = 3;

        [Tooltip("Total duration of the burst sequence in seconds.")]
        [SerializeField, Range(0f, 10f)]
        private float _burstDuration = 1f;

        [Tooltip("Cooldown between consecutive burst sequences.")]
        [SerializeField, Range(0f, 10f)]
        private float _burstCooldown = 0.3f;

        private float _nextShootTime;

        public override void TriggerDown()
        {
            if(Time.time < _nextShootTime)
            {
                return;
            }

            if(Gun.Magazine.IsReloading || Gun.Magazine.CurrentAmmoCount == 0)
            {
                RaiseShootEvent();
            }
            else
            {
                StartCoroutine(ExecuteBurstSequence());
            }

            _nextShootTime = Time.time + _burstCooldown + _burstDuration;
        }

        private IEnumerator ExecuteBurstSequence()
        {
            float timeBetweenShots = _burstDuration / _shotsPerBurst;

            for(int shotIndex = 0;  shotIndex < _shotsPerBurst; shotIndex++)
            {
                RaiseShootEvent();
                yield return new Delay(timeBetweenShots);
            }
        }
    }
}