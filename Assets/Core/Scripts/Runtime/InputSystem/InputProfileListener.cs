using UnityEngine;
using UnityEngine.Events;

namespace Nexora.InputSystem
{
    /// <summary>
    /// Listens to <see cref="InputModule.OnInputProfileChanged"/> event and 
    /// invokes events when the new active profile is specific <see cref="_targetProfile"/>.
    /// </summary>
    public class InputProfileListener : MonoBehaviour
    {
        [Tooltip("Target input profile this object is listens for.")]
        [SerializeField, NotNull]
        private InputProfile _targetProfile;

        [Tooltip("Event that will be invoked when target profile becomes the active profile.")]
        [SerializeField]
        private UnityEvent _onProfileActivated;

        [Tooltip("Event that will be invoked when target profile is deactivated when it was active.")]
        [SerializeField]
        private UnityEvent _onProfileDeactivated;

        private bool _isProfileActive;

        private void OnEnable() => InputModule.Instance.OnInputProfileChanged += HandleActiveProfileChanged;
        private void OnDisable() => InputModule.Instance.OnInputProfileChanged -= HandleActiveProfileChanged;
        
        /// <summary>
        /// If the target profile is the new active input profile, or 
        /// if the target profile was the active profile, it invokes the
        /// corresponding event and updates the active flag.
        /// </summary>
        /// <param name="activeProfile">New active input profile</param>
        private void HandleActiveProfileChanged(InputProfile activeProfile)
        {
            if(activeProfile == _targetProfile)
            {
                if(_isProfileActive == false)
                {
                    _isProfileActive = true;
                    _onProfileActivated?.Invoke();
                }
            }
            else if(_isProfileActive)
            {
                _isProfileActive = false;
                _onProfileDeactivated?.Invoke();
            }
        }
    }
}
