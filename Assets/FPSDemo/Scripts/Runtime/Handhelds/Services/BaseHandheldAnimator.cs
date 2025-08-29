using Nexora.Animation;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public abstract class BaseHandheldAnimator : 
        MonoBehaviour,
        IAnimatorController
    {
        [SerializeField]
        protected AnimatorOverrideProfile _overrideProfile;

        protected float _holsterSpeed;

        protected Animator TargetAnimator => GetTargetAnimator();

        public bool IsAnimating
        {
            get => TargetAnimator.speed != 0f;
            set => TargetAnimator.speed = value ? 1f : 0f;
        }

        public virtual bool IsObjectVisible { get; set; }

        public void SetFloat(int id, float value)
        {
            TargetAnimator.SetFloat(id, 
                id == HandheldAnimationConstants.HolsterSpeed 
                ? _holsterSpeed * value 
                : value);
        }
        public void SetBool(int id, bool value) => TargetAnimator.SetBool(id, value);
        public void SetInteger(int id, int value) => TargetAnimator.SetInteger(id, value);
        public void SetTrigger(int id) => TargetAnimator.SetTrigger(id);
        public void ResetTrigger(int id) => TargetAnimator.ResetTrigger(id);

        protected virtual void OnEnable()
        {
            if(TargetAnimator == null)
            {
                return;
            }

            foreach(var parameter in _overrideProfile.DefaultParameters)
            {
                parameter.ApplyTo(TargetAnimator);
            }
        }

        protected virtual void Awake()
        {
            if(TargetAnimator != null && _overrideProfile.BaseController != null)
            {
                TargetAnimator.runtimeAnimatorController = _overrideProfile.OverrideController;
            }

            _holsterSpeed = GetHolsterSpeed();
        }

        protected abstract Animator GetTargetAnimator();

        protected float GetHolsterSpeed()
        {
            foreach(var parameter in _overrideProfile.DefaultParameters)
            {
                if(parameter.Hash == HandheldAnimationConstants.HolsterSpeed)
                {
                    return parameter.Value;
                }
            }

            return 1f;
        }

        public void Play(string id, int layer, float speed) => TargetAnimator.Play(id, layer, speed);
    }
}