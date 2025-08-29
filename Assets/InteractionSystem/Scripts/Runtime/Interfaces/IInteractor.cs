using Nexora.Audio;
using UnityEngine.Events;

namespace Nexora.InteractionSystem
{
    /// <summary>
    /// Interface that should be implemented by classes to be able to interact with
    /// <see cref="IInteractable"/> and <see cref="IHoverable"/> objects.
    /// </summary>
    public interface IInteractor
    {
        /// <summary>
        /// Can this interactor interact with objects?
        /// </summary>
        bool IsInteractionEnabled { get; set; }

        /// <summary>
        /// Currently hovered object.
        /// </summary>
        IHoverable CurrentHoverable { get; }

        /// <summary>
        /// Event fired when the current hovered object is changed.
        /// </summary>
        event UnityAction<IHoverable> HoverableChanged;

        /// <summary>
        /// Event triggered when the interaction progress changes,
        /// like in the cases of holding for 2 seconds before collecting item.
        /// </summary>
        event UnityAction<float> InteractionProgressChanged;

        /// <summary>
        /// Event fired when interaction enabled state changes.
        /// That is, state of this object can interact or not.
        /// </summary>
        event UnityAction<bool> InteractionEnabledStateChanged;

        /// <summary>
        /// Starts the interaction process with the object it is currently hovering.
        /// </summary>
        void StartInteraction();

        /// <summary>
        /// Stops the interaction process.
        /// </summary>
        void StopInteraction();
    }

    /// <summary>
    /// Abstraction used to not make any outer type not depend on this system.
    /// For example, this way we don't force how the interactor should be implemented.
    /// An interactor can be a player class implementing <see cref="IInteractorContext"/>
    /// and has a subcomponent that implements <see cref="IInteractor"/> and gets us the
    /// whatever interface we want, specifically <see cref="IInteractor"/> for the framework's case.
    /// </summary>
    /// <remarks>
    /// Main class itself can implement <see cref="IInteractor"/> as well, this is one of the flexibility we give.
    /// </remarks>
    public interface IInteractorContext
    {
        /// <summary>
        /// Returns <see langword="object"/> that implements the <see langword="interface"/>
        /// of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the subobject/component asked.</typeparam>
        /// <returns>If context interface is found.</returns>
        bool TryGetContextInterface<T>(out T result) where T : class;

        /// <summary>
        /// Plays the <paramref name="audio"></paramref>.
        /// </summary>
        void PlayAudio(AudioCue audio);
    }
}