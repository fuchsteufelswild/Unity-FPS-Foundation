using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;


namespace Nexora.PostProcessing
{
    /// <summary>
    /// Main control mechanism for the <see cref="IPostFxAnimation"/>.
    /// Provides interface to Play/Stop <see cref="PostFxAnimationPreset"/> which is 
    /// a collection of <see cref="IPostFxAnimation"/> with their play settings.
    /// </summary>
    /// <remarks>
    /// Pools list of <see cref="IPostFxAnimator"/> and <b>acts on one single <see cref="Volume"/>.
    /// There can't be more than one active global <see cref="Volume"/></b>.
    /// </remarks>
    public class PostFxModule : GameModule<PostFxModule>
    {
        private readonly IPostFxAnimator[] _animators = new IPostFxAnimator[10];
        public Volume ActiveVolume { get; set; }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init() => CreateInstance();

        protected override void OnInitialized()
        {
            // Newly creates animators to reset them
            // Might inject them if needed flexibility of different animators but they are simple and straight
            for(int i = 0;  i < _animators.Length; i++)
            {
                _animators[i] = new PostFxAnimator();
            }

            ActiveVolume = null;
        }

        /// <summary>
        /// Safely plays animation, has checks for the invalid cases.
        /// </summary>
        public bool TryPlayAnimation(
            MonoBehaviour owner, 
            PostFxAnimationPreset animationPreset, 
            float durationScale = 1f, 
            bool useUnscaledTime = false)
        {
            if(animationPreset == null || Mathf.Approximately(durationScale, 0f))
            {
                return false;
            }

            PlayAnimation(owner, animationPreset, durationScale, useUnscaledTime);
            return true;
        }

        /// <summary>
        /// Gets an available animator and plays the <paramref name="animationPreset"/> on it 
        /// and attaching the process to the <paramref name="owner"/>.
        /// </summary>
        public void PlayAnimation(
            MonoBehaviour owner,
            PostFxAnimationPreset animationPreset,
            float durationScale = 1f,
            bool useUnscaledTime = false)
        {
            if(TryGetAnimator(out var animator))
            {
                Assert.IsTrue(ActiveVolume != null, "Scene does not contain an active post process volume");

                animator.Play(owner, ActiveVolume.profile, animationPreset, durationScale, useUnscaledTime);
            }
        }

        /// <summary>
        /// Tries to get an empty animator from the <see cref="_animators"/> list.
        /// </summary>
        /// <returns>If available animator is found</returns>
        private bool TryGetAnimator(out IPostFxAnimator availableAnimator)
        {
            foreach (var animator in _animators)
            {
                if(animator.IsAvailable)
                {
                    availableAnimator = animator;
                    return true;
                }
            }

            availableAnimator = null;
            return false;
        }

        /// <summary>
        /// Stops all active animations instantly.
        /// </summary>
        public void StopAllAnimations()
        {
            foreach(IPostFxAnimator animator in _animators)
            {
                animator.Stop(true);
            }
        }

        /// <summary>
        /// Stops animation attached to <paramref name="owner"/> and using <paramref name="animationPreset"/>.
        /// </summary>
        public void StopAnimation(
            MonoBehaviour owner,
            PostFxAnimationPreset animationPreset,
            bool instant = false)
        {
            if(TryGetAnimator(owner, animationPreset, out IPostFxAnimator targetAnimator))
            {
                targetAnimator.Stop(instant);
            }
        }

        /// <summary>
        /// Tries to get if there is an active animator with 
        /// <paramref name="owner"/> and <paramref name="animationPreset"/> exits.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="animationPreset"></param>
        /// <param name="targetAnimator"></param>
        /// <returns>If the animator is found</returns>
        private bool TryGetAnimator(
            MonoBehaviour owner, 
            PostFxAnimationPreset animationPreset, 
            out IPostFxAnimator targetAnimator)
        {
            foreach(var animator in _animators)
            {
                if(animator.IsPlaying == false)
                {
                    continue;
                }

                if(animator.Owner == owner && animator.AnimationPreset == animationPreset)
                {
                    targetAnimator = animator;
                    return true;
                }
            }

            targetAnimator = null;
            return false;
        }
    }
}
