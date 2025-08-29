using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.FPSDemo.ProceduralMotion;
using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Abstract base class for any effect to be triggered by dry fire.
    /// </summary>
    [Serializable]
    public abstract class DryFireEffect
    {
        /// <summary>
        /// Initialize with the <paramref name="gun"/>.
        /// </summary>
        /// <param name="gun">Gun that triggers the ejection.</param>
        public virtual void Initialize(IGun gun) { }

        /// <summary>
        /// Plays the effect.
        /// </summary>
        public abstract void Trigger();
    }

    /// <summary>
    /// Plays audio effect upon dry fire is triggered.
    /// </summary>
    [Serializable]
    public sealed class DryFireAudioEffect : DryFireEffect
    {
        [Tooltip("Audio played when triggered.")]
        [SerializeField]
        private AudioCue _dryFireAudio = new(null);

        private IHandheld _handheld;

        public override void Initialize(IGun gun) => _handheld = gun as IHandheld;

        public override void Trigger() => _handheld.AudioPlayer.PlayClip(_dryFireAudio, BodyPart.Hands);
    }

    /// <summary>
    /// Triggers animator upon dry fire is triggered.
    /// </summary>
    [Serializable]
    public sealed class DryFireAnimatorEffect : DryFireEffect
    {
        private IHandheld _handheld;

        public override void Initialize(IGun gun) => _handheld = (gun as IHandheld);

        public override void Trigger()
        {
            _handheld.Animator.SetBool(HandheldAnimationConstants.IsEmpty, true);
            _handheld.Animator.SetTrigger(HandheldAnimationConstants.Shoot);
        }
    }

    /// <summary>
    /// Adds a shake to the designated target upon dry fire is triggered.
    /// </summary>
    [Serializable]
    public sealed class DryFireShakeEffect : DryFireEffect
    {
        private enum ShakeTarget
        {
            Head = 0,
            Hands = 1
        }

        [Tooltip("Target to shake when triggered.")]
        [SerializeField]
        private ShakeTarget _target = ShakeTarget.Hands;

        [Tooltip("Shake to be applied to the target.")]
        [SerializeField]
        private ShakeInstance _shake;

        private IShakeHandler _targetShakeHandler;

        public override void Initialize(IGun gun)
        {
            IHandheldMotionTargets motionTargets = (gun as IHandheld).MotionTargets;
            _targetShakeHandler = _target == ShakeTarget.Head 
                ? motionTargets.HeadMotion.ShakeHandler 
                : motionTargets.HandsMotion.ShakeHandler;
        }

        public override void Trigger()
        {
            if(_shake.IsPlayable == false)
            {
                return;
            }

            _targetShakeHandler.AddShake(_shake);
        }
    }
}