using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.InteractionSystem
{
    /// <summary>
    /// Helper class to attach events via Unity upon interaction events
    /// and with optional hooks in code as well.
    /// </summary>
    /// <typeparam name="TInteractor">Type of the interactor object.</typeparam>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(IInteractable))]
    public abstract class InteractableEventsHandler<TInteractor> : 
        MonoBehaviour
        where TInteractor : class, IInteractorContext
    {
        [SerializeField]
        private UnityEvent<TInteractor> _onHoverStarted;

        [SerializeField]
        private UnityEvent<TInteractor> _onHoverEnded;

        [SerializeField]
        private UnityEvent<TInteractor> _onInteractionStarted;

        [SerializeField]
        private UnityEvent<TInteractor> _onInteractionPerformed;

        [SerializeField]
        private UnityEvent<TInteractor> _onInteractionCanceled;

        void Start()
        {
            var interactable = GetComponent<IInteractable>();
            interactable.HoverStarted += OnHoverStarted;
            interactable.HoverEnded += OnHoverEnded;
            interactable.InteractionStarted += OnInteractionStarted;
            interactable.InteractionCanceled += OnInteractionCanceled;
            interactable.InteractionPerformed += OnInteractionPerformed;
            
        }

        private void OnDestroy()
        {
            var interactable = GetComponent<IInteractable>();
            interactable.HoverStarted -= OnHoverStarted;
            interactable.HoverEnded -= OnHoverEnded;
            interactable.InteractionStarted -= OnInteractionStarted;
            interactable.InteractionCanceled -= OnInteractionCanceled;
            interactable.InteractionPerformed -= OnInteractionPerformed;
        }

        private bool ValidateContextType(IInteractorContext interactorContext, out TInteractor result)
        {
            result = interactorContext as TInteractor;

            if(result == null)
            {
                UnityEngine.Debug.LogErrorFormat("{0} type does not match with {1}", interactorContext.GetType().Name, typeof(TInteractor).Name);
                return false;
            }

            return true;
        }

        private void OnHoverStarted(IHoverable hoverable, IInteractorContext interactorContext)
        {
            if (ValidateContextType(interactorContext, out TInteractor interactor))
            {
                return;
            }

            _onHoverStarted?.Invoke(interactor);
            OnHoverStartedInternal(interactor);
        }

        private void OnHoverEnded(IHoverable hoverable, IInteractorContext interactorContext)
        {
            if (ValidateContextType(interactorContext, out TInteractor interactor))
            {
                return;
            }

            _onHoverEnded?.Invoke(interactor);
            OnHoverEndedInternal(interactor);
        }

        private void OnInteractionStarted(IInteractable hoverable, IInteractorContext interactorContext)
        {
            if (ValidateContextType(interactorContext, out TInteractor interactor))
            {
                return;
            }

            _onInteractionStarted?.Invoke(interactor);
            OnInteractionStartedInternal(interactor);
        }

        private void OnInteractionPerformed(IInteractable hoverable, IInteractorContext interactorContext)
        {
            if (ValidateContextType(interactorContext, out TInteractor interactor))
            {
                return;
            }

            _onInteractionPerformed?.Invoke(interactor);
            OnInteractionPerformedInternal(interactor);
        }

        private void OnInteractionCanceled(IInteractable hoverable, IInteractorContext interactorContext)
        {
            if (ValidateContextType(interactorContext, out TInteractor interactor))
            {
                return;
            }

            _onInteractionCanceled?.Invoke(interactor);
            OnInteractionCanceledInternal(interactor);
        }

        protected virtual void OnHoverStartedInternal(TInteractor interactorContext) { }
        protected virtual void OnHoverEndedInternal(TInteractor interactorContext) { }
        protected virtual void OnInteractionStartedInternal(TInteractor interactorContext) { }
        protected virtual void OnInteractionPerformedInternal(TInteractor interactorContext) { }
        protected virtual void OnInteractionCanceledInternal(TInteractor interactorContext) { }

        [Conditional("UNITY_EDITOR")]
        private void Reset()
        {
            if(gameObject.HasComponent<IInteractable>() == false)
            {
                gameObject.AddComponent<Interactable>();
            }
        }
    }
}