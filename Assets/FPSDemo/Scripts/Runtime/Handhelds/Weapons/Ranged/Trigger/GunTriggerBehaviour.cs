using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunTriggerBehaviour : IGunBehaviour
    {
        /// <summary>
        /// Is trigger currently being held down?
        /// </summary>
        bool IsTriggerHeld { get; }

        /// <summary>
        /// Gets when trigger can be pulled for triggering fire again.
        /// </summary>
        float NextShootTime { get; }

        /// <summary>
        /// Additional multiplier held for affecting recoil calculation.
        /// </summary>
        float RecoilMultiplier { get; }

        /// <summary>
        /// Called when the trigger is held down.
        /// </summary>
        void TriggerHold();

        /// <summary>
        /// Called when the trigger is up.
        /// </summary>
        void TriggerUp();

        /// <summary>
        /// Called when the shoot signal is given by the trigger.
        /// </summary>
        event UnityAction TriggerPulled;
    }

    public sealed class NullTrigger : IGunTriggerBehaviour
    {
        public static readonly NullTrigger Instance = new();

        public bool IsTriggerHeld => false;
        public float RecoilMultiplier => 1f;
        public float NextShootTime => 0f;
        public event UnityAction TriggerPulled { add { } remove { } }

        public void Attach() { }
        public void Detach() { }
        public void TriggerHold() { }
        public void TriggerUp() { }
    }

    public abstract class GunTriggerBehaviour : 
        GunBehaviour,
        IGunTriggerBehaviour
    {
        public bool IsTriggerHeld { get; protected set; }
        public virtual float RecoilMultiplier => 1f;
        public virtual float NextShootTime => 0f;

        public event UnityAction TriggerPulled;

        protected virtual void OnEnable()
        {
            if(Gun != null)
            {
                Gun.TriggerMechanism = this;
            }
        }

        /// <summary>
        /// Called when the trigger is down (once called when the trigger is hold).
        /// </summary>
        public virtual void TriggerDown() { }
        
        public virtual void TriggerHold()
        {
            if(IsTriggerHeld == false)
            {
                TriggerDown();
            }

            IsTriggerHeld = true;
        }

        public virtual void TriggerUp() => IsTriggerHeld = false;

        protected void RaiseShootEvent() => TriggerPulled?.Invoke();
    }

    [Serializable]
    public sealed class BufferedFireInput
    {
        [Tooltip("The duration for which the input will be valid.")]
        [SerializeField, Range(0f, 1f)]
        private float _inputBufferDuration = 0f;

        private float _inputValidUntil;

        public bool HasInput => Time.time < _inputValidUntil;

        public void RecordInput()
        {
            if (_inputBufferDuration > 0.01f)
            {
                _inputValidUntil = Time.time + _inputBufferDuration;
            }
        }

        public void Reset() => _inputValidUntil = 0f;
    }
}