using Nexora.Animation;
using System.Diagnostics;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public sealed class HandheldAnimator : BaseHandheldAnimator
    {
        [Tooltip("The animator component for the handheld item.")]
        [SerializeField, NotNull]
        private Animator _animator;

        public AnimatorOverrideProfile OverrideProfile => _overrideProfile;

        protected override Animator GetTargetAnimator() => _animator;

        [Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            if(_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }
    }

    
}