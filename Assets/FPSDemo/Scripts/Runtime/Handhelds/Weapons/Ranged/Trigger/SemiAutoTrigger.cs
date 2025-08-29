using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Represents a trigger that fires single round mode, like full auto, and semi auto(single shot) mode on AR.
    /// </summary>
    public sealed class SemiAutoTrigger : GunTriggerBehaviour
    {
        [Tooltip("Cooldown between consecutive fires.")]
        [SerializeField, Range(0.1f, 10f)]
        private float _fireCooldown = 0.1f;

        [SerializeField]
        private BufferedFireInput _bufferedFireInput;

        private float _nextShootTime;

        public override float NextShootTime => _nextShootTime;

        public override void TriggerDown()
        {
            if(Time.time < _nextShootTime)
            {
                _bufferedFireInput.RecordInput();
            }

            if (Time.time >= _nextShootTime)
            {
                Fire();
            }
        }

        /// <summary>
        /// Processes the input buffered while the shoot couldn't be performed.
        /// </summary>
        private void Update()
        {
            if(_bufferedFireInput.HasInput && Time.time >= _nextShootTime)
            {
                Fire();
            }
        }

        private void Fire()
        {
            RaiseShootEvent();
            _nextShootTime = Time.time + _fireCooldown;
            _bufferedFireInput.Reset();
        }
    }
}