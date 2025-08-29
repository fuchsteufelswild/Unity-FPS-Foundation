using UnityEngine;

namespace Nexora.InteractionSystem
{
    /// <summary>
    /// Delegate used for informing other classes about the interaction events.
    /// </summary>
    public delegate void InteractableEventDelegate(IInteractable interactable, IInteractorContext interactorContext);

    /// <summary>
    /// Interface for objects that can be interacted by <see cref="IInteractorContext"/>.
    /// Provides interface for handling interaction, events for informing about interaction status.
    /// </summary>
    public interface IInteractable : IHoverable
    {
        /// <summary>
        /// Can this object currently be interactable?
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// Time (in seconds), interactor must hold button to complete interaction.
        /// </summary>
        float RequiredHoldTime { get; }

        /// <summary>
        /// Event fired when interaction progress started (the hold duration started).
        /// </summary>
        event InteractableEventDelegate InteractionStarted;

        /// <summary>
        /// Event fired when interaction progress has completed (interacted).
        /// </summary>
        event InteractableEventDelegate InteractionPerformed;

        /// <summary>
        /// Event fired when interaction progress is canceled before completing.
        /// </summary>
        event InteractableEventDelegate InteractionCanceled;

        /// <summary>
        /// Called when <paramref name="interactorContext"/> starts interaction process with this object.
        /// </summary>
        void HandleInteractionStarted(IInteractorContext interactorContext);

        /// <summary>
        /// Called when <paramref name="interactorContext"/> interacts with this object.
        /// </summary>
        void HandleInteractionPerformed(IInteractorContext interactorContext);

        /// <summary>
        /// Called when <paramref name="interactorContext"/> cancels interaction process with this object.
        /// </summary>
        void HandleInteractionCanceled(IInteractorContext interactorContext);

        /// <summary>
        /// Called before the interactable is destroyed or put back in pool.
        /// </summary>
        void Dispose();
    }

}