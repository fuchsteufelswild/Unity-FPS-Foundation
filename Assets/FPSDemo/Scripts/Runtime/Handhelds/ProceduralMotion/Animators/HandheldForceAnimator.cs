using Nexora.Animation;
using Nexora.FPSDemo.Handhelds;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    /// <summary>
    /// Applies spring-based force animations to the head camera.
    /// </summary>
    public sealed class HandheldForceAnimator : DataBasedAnimator<HandheldForceAnimator.AnimationData>
    {
        private AdditiveMotionApplier _additiveMotion;

        private void OnEnable()
        {
            if (_additiveMotion == null)
            {
                _additiveMotion = GetComponentInParent<IHandheld>().MotionTargets.HeadMotion.MotionMixer?.GetMotion<AdditiveMotionApplier>();
            }
        }

        protected override void PlayAnimation(AnimationData animationData)
        {
            var forces = animationData.SpringForces;
            for (int i = 0; i < forces.Length; i++)
            {
                if (forces[i].Delay < 0.01f)
                {
                    _additiveMotion.AddDelayedRotationForce(forces[i]);
                }
                else
                {
                    _additiveMotion.AddRotationForce(forces[i].SpringImpulse);
                }
            }
        }

        [Serializable]
        public sealed class AnimationData : Nexora.Animation.AnimationData
        {
            [SerializeField]
            [ReorderableList(ListStyle.Lined)]
            public DelayedSpringImpulseDefinition[] SpringForces;
        }
    }
}