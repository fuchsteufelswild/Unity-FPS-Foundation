using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IChargedTriggerBehaviour : IGunTriggerBehaviour
    {
        /// <summary>
        /// The percentage triggered has been charged [0, 1].
        /// </summary>
        float TriggerCharge { get; }

        /// <summary>
        /// Called when the charge is released and it resulted in a shoot event.
        /// </summary>
        event UnityAction<float> OnFired;

        /// <summary>
        /// Called when the charging process has started.
        /// </summary>
        event UnityAction ChargeStarted;

        /// <summary>
        /// Called when the charge reached its maximum value -> 1.
        /// </summary>
        event UnityAction ChargeMaxed;

        /// <summary>
        /// Called when the charge is released but it didn't result in a shoot.
        /// </summary>
        event UnityAction ChargeCanceled;

        /// <summary>
        /// Called when the charge is updated one frame further.
        /// </summary>
        event UnityAction<float> ChargeUpdated;
    }

    /// <summary>
    /// Trigger that is activated only after fixed amount of time trigger is held.
    /// Like for systems like Revolver, Bow, Charged Energy Weapons, Or like shotguns that works the same idea as revolver.
    /// </summary>
    public sealed class ChargedTrigger : 
        GunTriggerBehaviour, 
        IChargedTriggerBehaviour
    {
        private enum TriggerState
        {
            Idle,
            Charging,
            MaxCharge
        }

        [Tooltip("Cooldown between consecutive charges.")]
        [SerializeField, Range(0f, 10f)]
        private float _chargeCooldown = 0.5f;

        [Tooltip("How much trigger should be charged for to be fired [0,1].")]
        [SerializeField, Range(0.1f, 0.98f)]
        private float _minChargeToFire = 0.5f;

        [Tooltip("The time it takes for the trigger to be fully charged.")]
        [SerializeField, Range(0f, 20f)]
        private float _maxChargeTime = 3f;

        [Tooltip("Automatically releases the trigger when it is reached max?")]
        [SerializeField]
        private bool _autoReleaseOnMax;

        [ReorderableList(ElementLabel = "Effector")]
        [ReferencePicker(typeof(ChargeTriggerEffector), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private ChargeTriggerEffector[] _effectors = Array.Empty<ChargeTriggerEffector>();

        private float _nextChargeTime;
        private TriggerState _currentState;

        private float _chargeStartTime;

        public event UnityAction<float> OnFired;
        public event UnityAction ChargeStarted;
        public event UnityAction ChargeMaxed;
        public event UnityAction ChargeCanceled;
        public event UnityAction<float> ChargeUpdated;

        public float TriggerCharge { get; private set; }
        public override float RecoilMultiplier => TriggerCharge;

        protected override void OnEnable()
        {
            base.OnEnable();

            foreach(var effector in _effectors)
            {
                effector.Enable(Gun);
            }

            ResetCharge();    
        }

        private void OnDisable()
        {
            foreach (var effector in _effectors)
            {
                effector.Disable(Gun);
            }
        }

        private void Update()
        {
            CalculateTriggerCharge();

            foreach (var effector in _effectors)
            {
                effector.Tick(TriggerCharge, Time.deltaTime);
            }
        }

        private void CalculateTriggerCharge()
        {
            if(_currentState == TriggerState.Idle)
            {
                return;
            }

            // Normalize charge to [0, 1]
            TriggerCharge = Mathf.Clamp01((Time.time - _chargeStartTime) / _maxChargeTime);
        }

        /// <summary>
        /// Starts the charging process.
        /// </summary>
        public override void TriggerDown()
        {
            if(Time.time < _nextChargeTime || _currentState == TriggerState.Idle)
            {
                return;
            }

            _currentState = TriggerState.Charging;
            _chargeStartTime = Time.time;
            ChargeStarted?.Invoke();
        }

        /// <summary>
        /// Charges the trigger every frame.
        /// </summary>
        public override void TriggerHold()
        {
            if(_currentState != TriggerState.Charging)
            {
                return;
            }

            CalculateTriggerCharge();
            ChargeUpdated?.Invoke(TriggerCharge);

            if(TriggerCharge >= 1f)
            {
                _currentState = TriggerState.MaxCharge;
                ChargeMaxed?.Invoke();

                if(_autoReleaseOnMax)
                {
                    this.InvokeNextFrame(TriggerUp);
                }
            }
        }

        /// <summary>
        /// Finishes the charging process, fires if min charge condition is met.
        /// </summary>
        public override void TriggerUp()
        {
            if(_currentState == TriggerState.Idle)
            {
                return;
            }

            bool fireBlocked = Handheld.TryGetActionOfType<IUseActionHandler>(out var actionHandler) && actionHandler.Blocker.IsBlocked;

            if (fireBlocked == false && TriggerCharge >= _minChargeToFire)
            {
                RaiseShootEvent();
                OnFired?.Invoke(TriggerCharge);
            }
            else
            {
                ChargeCanceled?.Invoke();
            }

            ResetCharge();
        }

        private void ResetCharge()
        {
            _currentState = TriggerState.Idle;
            TriggerCharge = 0f;
            ChargeUpdated?.Invoke(0f);
            _nextChargeTime = Time.time + _chargeCooldown;
        }
    }
}