using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexora.PostProcessing
{
    /// <summary>
    /// Animator to play <see cref="PostFxAnimationPreset"/> on specific <see cref="VolumeProfile"/>.
    /// And attaches it onto a <see cref="MonoBehaviour"/>.
    /// </summary>
    public interface IPostFxAnimator
    {
        bool IsAvailable { get; }
        bool IsPlaying { get; }
        MonoBehaviour Owner { get; }
        PostFxAnimationPreset AnimationPreset { get; }

        /// <summary>
        /// Plays the animation if it is not already started playing.
        /// </summary>
        /// <param name="owner">MonoBehaviour animation <see cref="Coroutine"/> will be running on.</param>
        /// <param name="volumeProfile">Profile to work on, we will grab components from this profile and modify them.</param>
        /// <param name="animationPreset">Preset of animations to animate.</param>
        /// <param name="durationScale">Parameter to scale duration, duration * durationScale.</param>
        /// <param name="useUnscaledTime">Use unscaled delta time or not.</param>
        void Play(MonoBehaviour owner,
                VolumeProfile volumeProfile,
                PostFxAnimationPreset animationPreset,
                float durationScale,
                bool useUnscaledTime);

        /// <summary>
        /// Cancels the all animations and reverts the profile as it was.
        /// </summary>
        void Stop(bool instant);
    }

    /// <summary>
    /// Animator to play many <see cref="IPostFxAnimation"/> from <see cref="PostFxAnimationPreset"/>.
    /// Plays the animations, slowly/instantly stops, and cleans them up.
    /// </summary>
    public class PostFxAnimator :
        IPostFxAnimator
    {
        public enum PlaybackState
        {
            Stopped = 0,
            Playing = 1,
            Stopping = 2
        }

        private const float MinProgress = 0f;
        private const float MaxProgress = 1f;

        private PlaybackState _playbackState;

        /// <summary>
        /// Behaviour the coroutine is attached to.
        /// </summary>
        private MonoBehaviour _owner;

        /// <summary>
        /// Animations to play on this animator.
        /// </summary>
        private PostFxAnimationPreset _animationPreset;

        /// <summary>
        /// Volume profile to use animations on.
        /// </summary>
        private VolumeProfile _volumeProfile;

        private float _animationProgress;
        private Coroutine _animationRoutine;

        public MonoBehaviour Owner => _owner;
        public PostFxAnimationPreset AnimationPreset => _animationPreset;
        public VolumeProfile VolumeProfile => _volumeProfile;

        public bool IsAvailable => _owner == null;
        public bool IsPlaying => _playbackState != PlaybackState.Stopped;

        /// <summary>
        /// <inheritdoc cref="IPostFxAnimator.Play(MonoBehaviour, VolumeProfile, PostFxAnimationPreset, float, bool)" path="/summary"/> using a coroutine.
        /// </summary>
        /// <param name="owner"><inheritdoc cref="IPostFxAnimator.Play(MonoBehaviour, VolumeProfile, PostFxAnimationPreset, float, bool)" path="/param[@name='owner']"/></param>
        /// <param name="volumeProfile"><inheritdoc cref="IPostFxAnimator.Play(MonoBehaviour, VolumeProfile, PostFxAnimationPreset, float, bool)" path="/param[@name='volumeProfile']"/></param>
        /// <param name="animationPreset"><inheritdoc cref="IPostFxAnimator.Play(MonoBehaviour, VolumeProfile, PostFxAnimationPreset, float, bool)" path="/param[@name='animationPreset']"/></param>
        /// <param name="durationScale"><inheritdoc cref="IPostFxAnimator.Play(MonoBehaviour, VolumeProfile, PostFxAnimationPreset, float, bool)" path="/param[@name='durationScale']"/></param>
        public void Play(MonoBehaviour owner, VolumeProfile volumeProfile, PostFxAnimationPreset animationPreset, float durationScale, bool useUnscaledTime)
        {
            if (IsPlaying)
            {
                return;
            }

            _playbackState = PlaybackState.Playing;
            _owner = owner;
            _volumeProfile = volumeProfile;
            _animationPreset = animationPreset;
            _animationRoutine = owner.StartCoroutine(PlayRoutine(durationScale, useUnscaledTime));
        }

        #region Play Routine
        /// <summary>
        /// Plays the animation first forward, then holds if its the animation mode and then reverses.
        /// If early stop is done while animation is playing forward then also slowly reverses if not instantly stopped.
        /// </summary>
        private IEnumerator PlayRoutine(float durationScale, bool useUnscaledTime)
        {
            InitializeAnimations(_volumeProfile);

            _animationProgress = MinProgress;
            yield return PlayForwardPhase(MinProgress, _animationPreset.EffectDuration * durationScale, useUnscaledTime);
            yield return HoldAnimationOnPostPlay(_animationPreset.Animations);
            if(_playbackState == PlaybackState.Stopping || _animationPreset.AnimateMode == AnimateMode.PingPong)
            {
                yield return PlayReversePhase(durationScale, useUnscaledTime);
            }

            Cleanup(_animationPreset.Animations);
        }

        /// <summary>
        /// Initializes all animations of <see cref="_animationPreset"/> with the <paramref name="profile"/>.
        /// </summary>
        private void InitializeAnimations(VolumeProfile profile)
        {
            foreach(IPostFxAnimation animation in _animationPreset.Animations)
            {
                animation.SetProfile(profile);
            }
        }

        /// <summary>
        /// Plays the animations forward for the <paramref name="duration"/> seconds.
        /// Snaps to the end if it overshots progress. Stops routine if the animator is stopped manually.
        /// </summary>
        private IEnumerator PlayForwardPhase(float startProgress, float duration, bool useUnscaledTime)
        {
            _animationProgress = startProgress;
            float inverseDuration = 1f / duration;

            IPostFxAnimation[] animations = _animationPreset.Animations;
            int animationCount = animations.Length;

            while (_animationProgress < MaxProgress &&  _playbackState == PlaybackState.Playing)
            {
                UpdateAnimations(animations);
                _animationProgress += inverseDuration * GetDeltaTime(useUnscaledTime);
                yield return null;
            }

            // Snap to end if we overshot
            if(_animationProgress > MaxProgress)
            {
                _animationProgress = MaxProgress;
                UpdateAnimations(animations);
            }
        }

        /// <summary>
        /// Animates all animations with <see cref="_animationProgress"/>.
        /// </summary>
        private void UpdateAnimations(IPostFxAnimation[] animations)
        {
            int animationCount = animations.Length;
            for (int i = 0; i < animationCount; i++)
            {
                animations[i].Animate(_animationProgress);
            }
        }

        private float GetDeltaTime(bool useUnscaledTime)
            => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        /// <summary>
        /// If <see cref="PostFxAnimationPreset"/> has mode <see cref="AnimateMode.Hold"/> then holds the
        /// animations at the <see cref="MaxProgress"/> until manual stop done.
        /// </summary>
        private IEnumerator HoldAnimationOnPostPlay(IPostFxAnimation[] animations)
        {
            while(_playbackState == PlaybackState.Playing && _animationPreset.AnimateMode == AnimateMode.Hold)
            {
#if UNITY_EDITOR
                for(int i = 0; i < animations.Length; i++)
                {
                    animations[i].Animate(MaxProgress);
                }
#endif

                yield return null;
            }
        }

        /// <summary>
        /// If <see cref="PostFxAnimationPreset"/> has mode <see cref="AnimateMode.PingPong"/> or just 
        /// stop signal is given to the animator. Then animates in the reverse direction starting from the
        /// current progress down to <see cref="MinProgress"/>.
        /// Snaps if undershot.
        /// </summary>
        private IEnumerator PlayReversePhase(float durationScale, bool useUnscaledTime)
        {
            IPostFxAnimation[] animations = _animationPreset.Animations;
            int animationCount = animations.Length;

            float duration = _animationProgress * _animationPreset.CancelDuration * durationScale;
            float inverseDuration = 1f / duration;

            while(_animationProgress > MinProgress)
            {
                UpdateAnimations(animations);
                _animationProgress -= inverseDuration * GetDeltaTime(useUnscaledTime);
                yield return null;
            }

            // Snap to start if undershot
            if(_animationProgress < MinProgress)
            {
                _animationProgress = MinProgress;
                UpdateAnimations(_animationPreset.Animations);
            }
        }

        #endregion

        /// <summary>
        /// Stops and dispose all animations and resets animator state.
        /// </summary>
        private void Cleanup(IPostFxAnimation[] animations)
        {
            int animationCount = animations.Length;
            for (int i = 0; i <= animationCount; i++)
            {
                animations[i].Dispose(_volumeProfile);
            }

            _playbackState = PlaybackState.Stopped;
            _owner = null;
            _animationRoutine = null;
        }

        /// <summary>
        /// Stops the animator and all animations.
        /// If immediate is set to false then slowly returns back to original state before stopping (see <see cref="PlayReversePhase(float, bool)"/>).
        /// Else it directly kills the <see cref="PlayRoutine(float, bool)"/>.
        /// </summary>
        public void Stop(bool instant)
        {
            if(_playbackState == PlaybackState.Stopped)
            {
                return;
            }

            // There is no owner to play Coroutine, state is invalid, just cleanup
            if(_owner == null)
            {
                Cleanup(_animationPreset.Animations);
            }
            else if(instant)
            {
                _owner.StopCoroutine(_animationRoutine);
                Cleanup(_animationPreset.Animations);
            }
            else
            {
                _playbackState = PlaybackState.Stopping;
            }
        }
    }
}
