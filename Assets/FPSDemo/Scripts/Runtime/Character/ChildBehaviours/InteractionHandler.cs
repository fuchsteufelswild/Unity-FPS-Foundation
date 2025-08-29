using Nexora.Audio;
using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.InteractionSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo
{
    public interface IInteractionHandler : 
        ICharacterBehaviour, 
        IInteractor
    {

    }

    /// <summary>
    /// Handles detecting hoverables/interactables and initiates the Interaction with them
    /// to let <see cref="IInteractable"/> handle the interaction logic.
    /// </summary>
    public sealed class InteractionHandler : 
        CharacterBehaviour, 
        IInteractionHandler
    {
        [SerializeField]
        private InteractorCore _interactorCore = new();

        [SerializeField]
        private AudioCue _interactionFailedSound = new(null);

        private IInteractable _currentInteractable;
        private IHoverable _currentHoverable;

        public bool IsInteractionEnabled
        {
            get => enabled;
            set
            {
                if(enabled == value)
                {
                    return;
                }

                enabled = value;
                InteractionEnabledStateChanged?.Invoke(value);
            }
        }

        public IHoverable CurrentHoverable => _currentHoverable;

        public event UnityAction<IHoverable> HoverableChanged;
        public event UnityAction<float> InteractionProgressChanged;
        public event UnityAction<bool> InteractionEnabledStateChanged;

        private void Awake() => enabled = false;

        protected override void OnDisable()
        {
            base.OnDisable();
            ClearCurrentInteraction();
        }

        private void ClearCurrentInteraction()
        {
            if (_currentHoverable != null)
            {
                _currentHoverable.HandleHoverEnd(Parent);
                HoverableChanged?.Invoke(null);
                _currentHoverable = null;
            }

            StopInteraction();
        }

        public void StopInteraction()
        {
            if (_currentInteractable == null)
            {
                return;
            }

            StopAllCoroutines();
            _interactorCore.ResetInteractionProgress();
            _currentInteractable = null;
        }

        protected override void OnBehaviourStart(ICharacter parent)
        {
            base.OnBehaviourStart(parent);
            _interactorCore.SetIgnoredRoot(parent.transform);
        }

        private void FixedUpdate()
        {
            IHoverable detectedHoverable = _interactorCore.DetectInteractables();
            UpdateHoverState(detectedHoverable);
        }

        private void UpdateHoverState(IHoverable newHoverable)
        {
            if(newHoverable == _currentHoverable)
            {
                return;
            }

            StopHoveringCurrentHoverable();
            StartHovering(newHoverable);
        }

        private void StopHoveringCurrentHoverable()
        {
            if (_currentHoverable != null)
            {
                _currentHoverable.HandleHoverEnd(Parent);
                StopInteraction();
            }
        }

        private void StartHovering(IHoverable newHoverable)
        {
            _currentHoverable = newHoverable;
            HoverableChanged?.Invoke(newHoverable);

            if (newHoverable != null)
            {
                newHoverable.HandleHoverStart(Parent);
            }
        }

        public void StartInteraction()
        {
            // We are interacting, or have no interaction and no hoverable object in sight as well
            if(_currentInteractable != null || _currentHoverable == null)
            {
                return;
            }

            if(_currentHoverable is IInteractable interactable && interactable.CanInteract)
            {
                _currentInteractable = interactable;

                if(_currentInteractable.RequiredHoldTime > 0.05f)
                {
                    StartCoroutine(_interactorCore.DelayedInteraction(
                        _currentInteractable.RequiredHoldTime, 
                        InteractionProgressChanged, 
                        delayCompletedCallback: ExecuteInteraction));
                }
                else
                {
                    ExecuteInteraction();
                }
            }
            else
            {
                PlayFailedInteractionSound();
            }
        }

        
        private void ExecuteInteraction()
        {
            _currentInteractable?.HandleInteractionPerformed(Parent);
            _interactorCore.ResetInteractionProgress();
            InteractionProgressChanged?.Invoke(_interactorCore.InteractionProgress);
            _currentInteractable = null;
        }

        private void PlayFailedInteractionSound()
        {
            Parent.AudioPlayer.PlayClip(_interactionFailedSound, BodyPart.Chest);
        }
    }
}