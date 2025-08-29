using Nexora.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nexora.InteractionSystem.UI
{
    /// <summary>
    /// Provides interface for displaying the prompt and
    /// setting interaction progress for timed interactions.
    /// </summary>
    public interface IInteractionPrompt
    {
        /// <summary>
        /// Displays prompt with <paramref name="hoveredObject"/>.
        /// Hides prompt if <see langword="null"/>.
        /// </summary>
        /// <param name="hoveredObject">Currently hovered object.</param>
        /// <returns><see langword="true"/> if prompt is displayed, <see langword="false"/> otherwise.</returns>
        bool DisplayPrompt(IHoverable hoveredObject);

        /// <summary>
        /// Sets the current interaction progress (0-1).
        /// </summary>
        /// <param name="progress">Current interaction progress (0-1).</param>
        void SetInteractionProgress(float progress);
    }

    public class InteractionPrompt : 
        MonoBehaviour,
        IInteractionPrompt
    {
        [Tooltip("Panel to display this object in the UI.")]
        [SerializeField, NotNull]
        private PanelBase _uiPanel;

        [Tooltip("Object to move UI panel in the screen to match the hovered object's " +
            "position and orientation to the camera view.")]
        [SerializeField, NotNull]
        private WorldSpaceUIFollower _worldPositionTracker;

        [Tooltip("Title in the prompt (e.g 'Fire Sword').")]
        [SerializeField]
        private TextMeshProUGUI _titleLabel;

        [Tooltip("Description below the title (e.g Press [E] to pick up item).")]
        [SerializeField]
        private TextMeshProUGUI _descriptionLabel;

        [Tooltip("Object to represent current interaction progress (e.g hold duration).")]
        [SerializeField]
        private Image _progressIndicator;

        [Tooltip("Container for input prompt visuals, activated for 'interactable' objects.")]
        [SerializeField]
        private GameObject _inputPromptContainer;

        private IHoverable _currentInteractable;
        private bool _isShowingPrompt;

        public bool DisplayPrompt(IHoverable interactable)
        {
            CleanupPreviousInteractable();

            if(IsValidInteractable() == false)
            {
                HidePrompt();
                return false;
            }

            SetupNewInteractable(interactable);
            return true;

            bool IsValidInteractable()
            {
                return interactable != null && string.IsNullOrEmpty(interactable.HoverTitle) == false;
            }
        }

        private void OnDestroy() => CleanupPreviousInteractable();

        private void CleanupPreviousInteractable()
        {
            if(_currentInteractable == null)
            {
                return;
            }
            _currentInteractable.DescriptionChanged -= OnDescriptionChanged;
            _currentInteractable = null;
        }

        private void SetupNewInteractable(IHoverable interactable)
        {
            _currentInteractable = interactable;
            interactable.DescriptionChanged += OnDescriptionChanged;

            UpdatePromptContent();
            if(_worldPositionTracker != null)
            {
                _worldPositionTracker.FollowTransform(interactable.transform, interactable.InteractionCenter);
            }
            ShowPrompt(interactable is IInteractable { CanInteract: true });
        }

        private void OnDescriptionChanged()
        {
            if (_isShowingPrompt == false)
            {
                return;
            }

            UpdatePromptContent();
        }

        private void UpdatePromptContent()
        {
            _titleLabel.text = _currentInteractable.HoverTitle;
            _descriptionLabel.text = _currentInteractable.HoverDescription;
        }

        private void ShowPrompt(bool showInputPrompt)
        {
            _inputPromptContainer.SetActive(showInputPrompt);
            _uiPanel.Activate();
            _isShowingPrompt = true;
        }

        private void HidePrompt()
        {
            if(_worldPositionTracker != null)
            {
                _worldPositionTracker.StopTracking();
            }

            _uiPanel.Deactivate();
            _isShowingPrompt = false;
        }

        public void SetInteractionProgress(float normalizedProgress) 
            => _progressIndicator.fillAmount = Mathf.Clamp01(normalizedProgress);
    }
}