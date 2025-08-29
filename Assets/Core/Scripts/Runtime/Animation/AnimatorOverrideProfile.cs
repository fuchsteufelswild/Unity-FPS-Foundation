using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nexora.Animation
{
    /// <summary>
    /// Class that represents an override profile for Unity animator. It provides
    /// easier to setup and control mechanism for override controller. It is more
    /// extendable and easy to track. Has values for original/override clip pair list
    /// and a list for default parameters. Lazily creates <see cref="AnimatorOverrideController"/>
    /// using fed in <see cref="RuntimeAnimatorController"/> and sets the overrides using the list provided.
    /// </summary>
    [Serializable]
    public sealed class AnimatorOverrideProfile
    {
        [SerializeField]
        private RuntimeAnimatorController _baseController;

        [Tooltip("Pairs of overrides, these will be applied to override controller.")]
        [SerializeField]
        private ClipOverridePair[] _clipOverrides;

        [Tooltip("Default parameters to use for this animator.")]
        [SerializeField]
        private AnimatorParameter[] _defaultParameters;

        private AnimatorOverrideController _cachedOverrideController;

        public ClipOverridePair[] ClipOverrides => _clipOverrides;
        public AnimatorParameter[] DefaultParameters => _defaultParameters;

        public RuntimeAnimatorController BaseController => _baseController;

        public AnimatorOverrideController OverrideController
        {
            get
            {
                if( _cachedOverrideController == null )
                {
                    _cachedOverrideController = new AnimatorOverrideController(_baseController);

                    var overrides = _clipOverrides
                        .Select(clipPair => new KeyValuePair<AnimationClip, AnimationClip>(clipPair.Original, clipPair.Override))
                        .ToList();

                    _cachedOverrideController.ApplyOverrides(overrides);
                }

                return _cachedOverrideController;
            }
        }

        [Serializable]
        public struct ClipOverridePair
        {
            public AnimationClip Original;
            public AnimationClip Override;
        }
    }
}