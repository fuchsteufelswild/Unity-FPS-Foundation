using Nexora.Animation;
using Nexora.Motion;
using System;

namespace Nexora.FPSDemo.ProceduralMotion
{
    /// <summary>
    /// Applies shake defined by <see cref="ShakeInstance"/> to both hands and the head camera.
    /// </summary>
    public sealed class HandheldShakeAnimator : DataBasedAnimator<HandheldShakeAnimator.AnimationData>
    {
        private IShakeHandler _headShakeHandler;
        private IShakeHandler _handsShakeHandler;

        private void OnEnable()
        {
            if(_headShakeHandler != null && _handsShakeHandler != null)
            {
                return;
            }
            
            IHandheldMotionTargets motionTargets = GetComponent<IHandheldMotionTargets>();
            _headShakeHandler ??= motionTargets.HeadMotion.ShakeHandler;
            _handsShakeHandler ??= motionTargets.HandsMotion.ShakeHandler;
        }

        protected override void PlayAnimation(AnimationData animationData)
        {
            if(animationData.HeadShake.IsPlayable)
            {
                _headShakeHandler.AddShake(animationData.HeadShake);
            }

            if(animationData.HandsShake.IsPlayable)
            {
                _handsShakeHandler.AddShake(animationData.HandsShake);
            }
        }

        [Serializable]
        public sealed class AnimationData : Nexora.Animation.AnimationData
        {
            public ShakeInstance HeadShake;
            public ShakeInstance HandsShake;
        }
    }
}