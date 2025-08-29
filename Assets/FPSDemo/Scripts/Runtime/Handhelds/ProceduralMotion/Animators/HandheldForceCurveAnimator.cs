using Nexora.Animation;
using Nexora.FPSDemo.Handhelds;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    /// <summary>
    /// Applies curve-based force animations to the head camera.
    /// </summary>
    public sealed class HandheldForceCurveAnimator : DataBasedAnimator<HandheldForceCurveAnimator.AnimationData>
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
            => _additiveMotion.AddRotationCurveForce(animationData.ForceCurve);

        [Serializable]
        public sealed class AnimationData : Nexora.Animation.AnimationData
        {
            public AnimationCurve3D ForceCurve;
        }
    }
}