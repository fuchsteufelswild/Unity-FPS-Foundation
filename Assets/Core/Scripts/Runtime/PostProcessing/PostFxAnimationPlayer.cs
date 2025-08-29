using UnityEngine;

namespace Nexora.PostProcessing
{
    /// <summary>
    /// Component to play/stop post process animations. Uses <see cref="PostFxAnimationPreset"/> to feed animations into the module.
    /// Makes calls to <see cref="PostFxModule"/> to utilize calls.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu(ComponentMenuPaths.PostProcessing + nameof(PostFxAnimationPlayer))]
    public class PostFxAnimationPlayer : MonoBehaviour
    {
        [SerializeField, NotNull]
        [Tooltip("Contains the animations and their animate settings")]
        private PostFxAnimationPreset _animationPreset;

        [SerializeField]
        private bool _useUnscaledTime;

        [SerializeField]
        [Tooltip("Value to multiply duration with, it can to make animation slower/faster")]
        private float _durationScale;

        public void Play()
        {
            PostFxModule.Instance.PlayAnimation(this, _animationPreset, _durationScale, _useUnscaledTime);
        }

        public void Stop()
        {
            PostFxModule.Instance.StopAnimation(this, _animationPreset, instant: false);
        }
    }
}
