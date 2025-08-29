using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexora.PostProcessing
{
    /// <summary>
    /// Main interface to define post process effect animation, 
    /// provides interface to set profile, animating and disposing animation.
    /// </summary>
    public interface IPostFxAnimation
    {
        /// <summary>
        /// Sets the profile for this animation, extracts all <see cref="VolumeParameter"/> from 
        /// <see cref="VolumeProfile"/> for the type <typeparamref name="T"/>.
        /// </summary>
        void SetProfile(VolumeProfile profile);

        /// <summary>
        /// Animates all <see cref="PostFxParameterAnimation"/> extracted for this type.
        /// </summary>
        void Animate(float t);

        /// <summary>
        /// Disposes all <see cref="PostFxParameterAnimation"/> and clears the list,
        /// also sets the modified component to its original state.
        /// </summary>
        void Dispose(VolumeProfile profile);
    }

    /// <summary>
    /// Abstract class to define post process effect animation, implements <see cref="IPostFxAnimation"/>.
    /// Extracts all <see cref="VolumeParameter{T}"/> from <see cref="VolumeProfile"/> and animates them. 
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="VolumeComponent"/> this animation works on.</typeparam>
    [Serializable]
    public abstract class PostFxAnimation<T> :
        IPostFxAnimation
        where T : VolumeComponent
    {
        private PostFxParameterAnimationList _parameterAnimations;

        /// <summary>
        /// This class modifies the <see cref="VolumeComponent"/> that is already attached to
        /// <see cref="VolumeProfile"/>. Keep tracks if modified <see cref="VolumeComponent"/> was enabled before.
        /// </summary>
        private bool _wasComponentEnabled; 

        /// <inheritdoc/>
        public void SetProfile(VolumeProfile profile)
        {
            T volumeComponent = GetOrAddComponent();

            volumeComponent.active = true;
            _parameterAnimations ??= new PostFxParameterAnimationList();

            FillParameterAnimations(_parameterAnimations, volumeComponent);

            return;

            // Gets the component if its type already exists, or creates a new one and adds it to the profile.
            // Sets the boolean if the component was active earlier so that it can revert back to original state
            // when animation finished.
            T GetOrAddComponent()
            {
                if (profile.TryGet(out T volumeComponent))
                {
                    _wasComponentEnabled = volumeComponent.active;
                }
                else
                {
                    volumeComponent = profile.Add<T>();
                    _wasComponentEnabled = false;
                }

                return volumeComponent;
            }
        }

        /// <summary>
        /// Fills the parameter animation list using <paramref name="volumeComponent"/>.
        /// Like what parameters it should work on, e.g LensDistortion might work on intensity.
        /// </summary>
        protected abstract void FillParameterAnimations(PostFxParameterAnimationList list, T volumeComponent);

        /// <inheritdoc/>
        public void Animate(float t)
        {
            foreach (PostFxParameterAnimation animation in _parameterAnimations)
            {
                animation.Animate(t);
            }
        }

        /// <inheritdoc/>
        public void Dispose(VolumeProfile profile)
        {
            ClearParameterAnimationsList();
            
            if(_wasComponentEnabled == false && profile.TryGet(out T volumeComponent))
            {
                volumeComponent.active = false;
            }

            return;

            void ClearParameterAnimationsList()
            {
                foreach (PostFxParameterAnimation parameterAnimation in _parameterAnimations)
                {
                    parameterAnimation.Dispose();
                }

                _parameterAnimations.Clear();
            }
        }

        /// <summary>
        /// Container for easier access and management of <see cref="PostFxParameterAnimation"/> stored.
        /// </summary>
        protected sealed class PostFxParameterAnimationList :
            IEnumerable<PostFxParameterAnimation>
        {
            private readonly List<PostFxParameterAnimation> _list = new(1);

            public PostFxParameterAnimation this[int index] => _list[index];

            public void Add(PostFxParameterAnimation animation)
            {
                if(CanAdd(animation))
                {
                    _list.Add(animation);
                }
            }

            private bool CanAdd(PostFxParameterAnimation animation) 
                => animation != null && animation.Enabled && _list.Contains(animation) == false;

            public bool Remove(PostFxParameterAnimation animation) => _list.Remove(animation);
            public void Clear() => _list.Clear();

            public IEnumerator<PostFxParameterAnimation> GetEnumerator() => _list.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
        }
    }
}
