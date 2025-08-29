using System;
using UnityEngine;

namespace Nexora.PostProcessing
{
    /// <summary>
    /// Animation mode for the effect.
    /// </summary>
    public enum AnimateMode
    {
        /// <summary>
        /// Plays the animation one time.
        /// </summary>
        SingleShot = 0,

        /// <summary>
        /// Plays the animation forward and then reverse.
        /// </summary>
        PingPong = 1,

        /// <summary>
        /// Plays the animation and holds the end value until manually stopped.
        /// </summary>
        Hold = 2
    }

    /// <summary>
    /// Defines group of effect animations, their duration and <see cref="AnimateMode"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Nexora/Post Processing/Effect Animation Preset", fileName = "Effect_")]
    public class PostFxAnimationPreset : ScriptableObject
    {
        [SerializeField]
        private AnimateMode _animateMode;

        [SerializeField]
        [Tooltip("Total duration of the effect from start to end")]
        private float _effectDuration;

        [SerializeField]
        [Tooltip("When cancelled, time it effects for the effect to slowly fade")]
        private float _cancelDuration;

        [SerializeReference]
        [NewLabel("Animations")]
        [ReorderableList(elementLabel: "Animation")]
        [ReferencePicker(typeof(IPostFxAnimation), TypeGrouping.ByFlatName)]
        private IPostFxAnimation[] _animations = Array.Empty<IPostFxAnimation>();

        public AnimateMode AnimateMode => _animateMode;
        public float EffectDuration => _effectDuration;
        public float CancelDuration => _cancelDuration;
        public IPostFxAnimation[] Animations => _animations;
    }
}
