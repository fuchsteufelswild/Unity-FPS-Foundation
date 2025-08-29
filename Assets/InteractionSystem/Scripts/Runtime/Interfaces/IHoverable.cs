using UnityEngine;
using UnityEngine.Events;

namespace Nexora.InteractionSystem
{
    /// <summary>
    /// Delegate used for informing other classes about the hover events.
    /// </summary>
    public delegate void HoverableEventDelegate(IHoverable hoverable, IInteractorContext interactorContext);

    /// <summary>
    /// Interface for objects that can be hovered by <see cref="IInteractorContext"/>.
    /// Provides interface about fields for drawing on the UI, and for handling 
    /// hover events along with hooks about hover status.
    /// </summary>
    public interface IHoverable : IMonoBehaviour
    {
        /// <summary>
        /// Used when more than one object that hoverable are in the view of the interactor.
        /// </summary>
        int HoverPriority { get; }

        /// <summary>
        /// Display name shown when hovered (e.g "Health Pack").
        /// </summary>
        string HoverTitle { get; }

        /// <summary>
        /// Detailed description showed when hovered (e.g "Restores 50 HP").
        /// </summary>
        string HoverDescription { get; }

        /// <summary>
        /// Center where hover detection originates (local space).
        /// </summary>
        Vector3 InteractionCenter { get; }

        /// <summary>
        /// Called after the title of the hoverable changed.
        /// </summary>
        event UnityAction TitleChanged;

        /// <summary>
        /// Called after the description of the hoverable changed.
        /// </summary>
        event UnityAction DescriptionChanged;

        /// <summary>
        /// Called when hovering started by the interactor.
        /// </summary>
        event HoverableEventDelegate HoverStarted;

        /// <summary>
        /// Called when hovering stopped by the interactor.
        /// </summary>
        event HoverableEventDelegate HoverEnded;

        /// <summary>
        /// Handles hover enter logic.
        /// </summary>
        /// <param name="interactorContext">Interactor starting the hover.</param>
        void HandleHoverStart(IInteractorContext interactorContext);

        /// <summary>
        /// Handles hover exit logic.
        /// </summary>
        /// <param name="interactorContext">Interactor that has been hovering this object.</param>
        void HandleHoverEnd(IInteractorContext interactorContext);

        /// <summary>
        /// Updates UI display name.
        /// </summary>
        void UpdateTitle(string title);

        /// <summary>
        /// Updates UI description.
        /// </summary>
        void UpdateDescription(string description);
    }
}