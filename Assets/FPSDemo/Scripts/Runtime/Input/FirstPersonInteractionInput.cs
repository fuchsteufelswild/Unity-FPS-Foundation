using Nexora.InputSystem;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nexora.FPSDemo
{
    public interface IFirstPersonInteractionInputHandler : ICharacterBehaviour
    {

    }

    public sealed class FirstPersonInteractionInput :
        InputHandler<ICharacterBehaviour, ICharacter>,
        IFirstPersonInteractionInputHandler
    {
        [SerializeField]
        private InputActionReference _interactionInput;

        private IInteractionHandler _interactionHandler;

        protected override void OnBehaviourStart(ICharacter parent) => _interactionHandler = parent.GetCC<IInteractionHandler>();

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            _interactionInput.AddStartedListener(OnInteractionStarted);
            _interactionInput.AddCanceledListener(OnInteractionStopped);

            this.InvokeNextFrame(() => _interactionHandler.IsInteractionEnabled = true);
        }

        protected override void OnBehaviourDisable(ICharacter parent)
        {
            _interactionInput.RemoveStartedListener(OnInteractionStarted);
            _interactionInput.RemoveCanceledListener(OnInteractionStopped);

            _interactionHandler.IsInteractionEnabled = false;
        }

        private void OnInteractionStopped(InputAction.CallbackContext context) => _interactionHandler.StartInteraction();
        private void OnInteractionStarted(InputAction.CallbackContext context) => _interactionHandler.StopInteraction();
    }
}