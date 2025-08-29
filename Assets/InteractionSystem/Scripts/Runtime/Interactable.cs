using Nexora.Rendering;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.InteractionSystem
{
    /// <summary>
    /// Base class for all interactable objects in the game.
    /// </summary>
    [DisallowMultipleComponent]
    public class Interactable : 
        MonoBehaviour,
        IInteractable
    {
        [Tooltip("Priority of hovering this object when multiple objects are in the same place. 0 for max.")]
        [SerializeField]
        private int _hoverPriority = 0;

        [Tooltip("Display name shown when interacting.")]
        [SerializeField]
        private string _title = "Interactable";

        [Tooltip("Detailed description shown when interacting.")]
        [SerializeField, Multiline]
        private string _description = "Press [E] to interact";

        [Tooltip("Required hold duration for interaction (0 by default).")]
        [Range(0f, 10f)]
        [SerializeField]
        private float _holdDuration = 0f;

        [Tooltip("Offset from object's center for interaction point.")]
        [SerializeField]
        private Vector3 _interactionOffset;

        [Tooltip("Optional effect for hover feedback (e.g outline).")]
        [SerializeField]
        private MaterialEffectApplier _hoverEffect;

        private Vector3 _calculatedCenter;
        private bool _isBeingHovered;

        public event UnityAction TitleChanged;
        public event UnityAction DescriptionChanged;
        public event InteractableEventDelegate InteractionStarted;
        public event InteractableEventDelegate InteractionPerformed;
        public event InteractableEventDelegate InteractionCanceled;
        public event HoverableEventDelegate HoverStarted;
        public event HoverableEventDelegate HoverEnded;

        public int HoverPriority => _hoverPriority;
        public bool CanInteract => enabled;
        public float RequiredHoldTime => _holdDuration;
        public string HoverTitle => _title;
        public string HoverDescription => _description;
        public Vector3 InteractionCenter => _calculatedCenter;


        public void HandleHoverStart(IInteractorContext interactorContext)
        {
            if(_isBeingHovered)
            {
                return;
            }

            _isBeingHovered = true;
            HoverStarted?.Invoke(this, interactorContext);
            _hoverEffect?.ApplyEffect();
        }

        public void HandleHoverEnd(IInteractorContext interactorContext)
        {
            if(_isBeingHovered == false)
            {
                return;
            }

            _isBeingHovered = false;
            HoverEnded?.Invoke(this, interactorContext);
            if (_hoverEffect != null)
            {
                _hoverEffect.RevertEffect();
            }
        }

        public void HandleInteractionPerformed(IInteractorContext interactorContext)
        {
            InteractionPerformed?.Invoke(this, interactorContext);
        }

        public virtual void HandleInteractionStarted(IInteractorContext interactorContext) { }
        public virtual void HandleInteractionCanceled(IInteractorContext interactorContext) { }

        public void UpdateDescription(string description)
        {
            if(ReferenceEquals(description, _description))
            {
                return;
            }

            _description = description;
            DescriptionChanged?.Invoke();
        }

        public void UpdateTitle(string title)
        {
            if (ReferenceEquals(title, _title))
            {
                return;
            }

            _title = title;
            TitleChanged?.Invoke();
        }

        private void Awake() => CalculateInteractionCenter();

        public void Dispose()
        {
            if(_hoverEffect != null)
            {
                _hoverEffect.RevertEffect();
            }
        }

        private void CalculateInteractionCenter()
        {
            if(TryGetComponent(out Collider col))
            {
                _calculatedCenter = transform.InverseTransformPoint(col.bounds.center) + _interactionOffset;
            }
            else
            {
                _calculatedCenter = _interactionOffset;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() => CalculateInteractionCenter();

        private void Reset()
        {
            _hoverEffect = GetComponentInChildren<MaterialEffectApplier>(true) ?? 
                gameObject.AddComponent<MaterialEffectApplier>();
        }

        private void OnDrawGizmosSelected()
        {
            CalculateInteractionCenter();

            Gizmos.color = new Color(1f, 0.8f, 0.4f, 0.85f);
            Gizmos.DrawSphere(transform.TransformPoint(_calculatedCenter), 0.1f);
            Gizmos.color = Color.white;
        }
#endif
    }
}