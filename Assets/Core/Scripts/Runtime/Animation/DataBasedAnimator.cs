using System;
using UnityEngine;

namespace Nexora.Animation
{
    /// <summary>
    /// Animation data defines animation rules and action data for the <see cref="DataBasedAnimator{TAnimationData}"/>.
    /// Works in a way that, this animation has an identifier of parameter type, required parameter value and a name.
    /// Derive from this class to add new custom values to use when action is played in <see cref="DataBasedAnimator{TAnimationData}"/>.
    /// </summary>
    /// <remarks>
    /// Action data can be anything, define it in your own class. This class is fully coupled with <see cref="DataBasedAnimator{TAnimationData}"/>.
    /// They are designed to work together, it is not subclass because of the generic limitations.
    /// </remarks>
    [Serializable]
    public abstract class AnimationData : 
        ISerializationCallbackReceiver,
        IComparable<AnimationData>
    {
        [DisableInPlayMode]
        public AnimatorControllerParameterType ParameterType = AnimatorControllerParameterType.Trigger;

        [DisableInPlayMode]
        [HideIf(nameof(ParameterType), AnimatorControllerParameterType.Trigger)]
        public int RequiredValue;

        [AnimatorParameter(nameof(ParameterType))]
        public string ParameterName;

        [NonSerialized]
        public int Hash;

        public void OnAfterDeserialize() 
        {
            if (Hash == 0)
            {
                Hash = Animator.StringToHash(ParameterName);
            }

            if (ParameterType == AnimatorControllerParameterType.Trigger)
            {
                RequiredValue = 0;
            }
        }

        public void OnBeforeSerialize() { }

        public int CompareTo(AnimationData other) => ParameterType.CompareTo(other.ParameterType);
    }

    /// <summary>
    /// Custom data based animator that has methods like a <see cref="Animator"/>.
    /// It has an array of animation data that it works with. They both act like a
    /// transition rule, and the extra information for the action when animation played.
    /// There are methods for setting parameters, these methods find all animation data
    /// that are suitable to play when setting that parameter(satifsying the transition rule).
    /// </summary>
    /// <remarks>
    /// It has no relation with the Unity's <see cref="Animator"/>, it is a animator designed 
    /// to use with procedural animations. It is not a state machine, there are no rules to move
    /// from one state to another. It can play more than one animation at a time, think of it like
    /// every animation data listens to every change and can be played if the transition rule is satisfied.
    /// <br></br>
    /// <b>Transition rule is having the same <see cref="AnimatorControllerParameterType"/>, value, and parameter name.</b>
    /// </remarks>
    /// <typeparam name="TAnimationData">Type of the animation data this animator plays.</typeparam>
    public abstract class DataBasedAnimator<TAnimationData> :
        MonoBehaviour,
        IAnimatorController
        where TAnimationData : AnimationData
    {
        [LabelByChild(nameof(AnimationData.ParameterName))]
        [ReorderableList(ElementLabel = "Animation")]
        [SerializeField]
        private TAnimationData[] _animations;

        private ParameterRanges _parameterRanges = new();  

        private bool _isAnimating;

        public bool IsAnimating
        {
            get => _isAnimating;
            set => _isAnimating = value;
        }

        public bool IsObjectVisible
        {
            get => true;
            set { }
        }

        private void Awake()
        {
            _isAnimating = true;
            _parameterRanges.BuildBorderIndices(_animations);
        }

        public void ResetTrigger(int parameterId) { }

        public void SetFloat(int parameterId, float value)
            => SetParameter(parameterId, AnimatorControllerParameterType.Float, (int)value);

        public void SetInteger(int parameterId, int value)
            => SetParameter(parameterId, AnimatorControllerParameterType.Int, value);

        public void SetBool(int parameterId, bool value)
            => SetParameter(parameterId, AnimatorControllerParameterType.Bool, value ? 1 : 0);

        public void SetTrigger(int parameterId)
            => SetParameter(parameterId, AnimatorControllerParameterType.Trigger, 0);

        /// <summary>
        /// Gets range of animations that satisfies the <paramref name="parameterType"/>
        /// and plays animation on it.
        /// </summary>
        private void SetParameter(
            int parameterId, 
            AnimatorControllerParameterType parameterType, 
            int value)
        {
            if(_isAnimating && _parameterRanges.TryGetParameterRange(_animations, parameterType, out var range))
            {
                Play(parameterId, range, value);
            }
        }

        /// <summary>
        /// For every animation in <paramref name="animations"/>, if the rule is satisfied
        /// an animation is played.
        /// </summary>
        private void Play(int parameterId, Span<TAnimationData> animations, int value)
        {
            foreach(var animationData in animations)
            {
                if (animationData.Hash == parameterId && animationData.RequiredValue == value)
                {
                    PlayAnimation(animationData);
                }
            }
        }

        /// <summary>
        /// Main section of the action. Using your own <see cref="AnimationData"/> implementation,
        /// you can define any action when deriving from this class.
        /// </summary>
        /// <param name="animationData">Animation data to use for when playing animation.</param>
        protected abstract void PlayAnimation(TAnimationData animationData);

        /// <summary>
        /// Class to provide <see cref="Span{T}"/> when queried to
        /// retrieve specific <see cref="AnimatorControllerParameterType"/>
        /// range in the sorted <typeparamref name="TAnimationData"/> array.
        /// </summary>
        private sealed class ParameterRanges
        {
            private int _floatIntParameterEndIndex;
            private int _boolParameterEndIndex;

            // As trigger is the last <AnimatorControllerParameterType>, we get start index
            // end index will be the *Count*
            private int _triggerStartIndex;

            /// <summary>
            /// Builds indices where each parameter type starts/ends.
            /// <br></br>
            /// <b><paramref name="animations"/> should be sorted by
            /// <see cref="AnimatorControllerParameterType"/></b>.
            /// </summary>
            /// <param name="animations">Sorted array of animation data.</param>
            public void BuildBorderIndices(TAnimationData[] animations)
            {
                _floatIntParameterEndIndex = Array.FindLastIndex(animations, 
                    animation => animation.ParameterType == AnimatorControllerParameterType.Float
                              || animation.ParameterType == AnimatorControllerParameterType.Int);

                _boolParameterEndIndex = Array.FindLastIndex(animations, 
                    animation => animation.ParameterType == AnimatorControllerParameterType.Bool);

                _triggerStartIndex = Array.FindIndex(animations, 
                    animation => animation.ParameterType == AnimatorControllerParameterType.Trigger);
            }

            /// <summary>
            /// Fills out a <see cref="Span{T}"/> from the <paramref name="animations"/> array that
            /// are matching the <paramref name="parameterType"/>. 
            /// </summary>
            /// <param name="animations">Animations to look for the parameter type. 
            /// <b>Must be the same array indices are built with.</b></param>
            /// <param name="parameterType">Type of animation parameter.</param>
            /// <param name="range">Returned range of parameters of the given <paramref name="parameterType"/>.</param>
            /// <returns>If that parameter type is existent in the list of <paramref name="animations"/>.</returns>
            public bool TryGetParameterRange(
                TAnimationData[] animations,
                AnimatorControllerParameterType parameterType, 
                out Span<TAnimationData> range)
            {
                var (rangeStart, rangeEnd) = GetRangeBounds(parameterType);

                if(rangeStart == null)
                {
                    range = null;
                    return false;
                }

                rangeEnd ??= animations.Length;

                range = animations.AsSpan(rangeStart.Value, rangeEnd.Value - rangeStart.Value);
                return true;
            }

            private (int? rangeStart, int? rangeEnd) GetRangeBounds(AnimatorControllerParameterType parameterType)
            {
                int? rangeStart = null;
                int? rangeEnd = null;

                switch (parameterType)
                {
                    case AnimatorControllerParameterType.Float:
                    case AnimatorControllerParameterType.Int:
                        if (_floatIntParameterEndIndex != -1)
                        {
                            rangeStart = 0;
                            rangeEnd = _floatIntParameterEndIndex + 1;
                        }
                        break;
                    case AnimatorControllerParameterType.Bool:
                        if (_boolParameterEndIndex != -1)
                        {
                            rangeStart = _floatIntParameterEndIndex + 1;
                            rangeEnd = _boolParameterEndIndex + 1;
                        }
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        if (_triggerStartIndex != -1)
                        {
                            rangeStart = _triggerStartIndex;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return (rangeStart, rangeEnd);
            }
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure that animations are always sorted and bool parameter type has value 0 or 1
            if(_animations != null)
            {
                Array.Sort(_animations);
                foreach(var animation in _animations)
                {
                    if(animation.ParameterType == AnimatorControllerParameterType.Bool)
                    {
                        animation.RequiredValue = (int)Mathf.Clamp01(animation.RequiredValue);
                    }
                }
            }
            else if(Application.isPlaying)
            {
                _parameterRanges.BuildBorderIndices(_animations);
            }
        }
#endif
    }
}
