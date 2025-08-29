using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunBehaviour
    {
        /// <summary>
        /// Attaches the component to the owner gun.
        /// </summary>
        void Attach();

        /// <summary>
        /// Detaches the component from the owner gun.
        /// </summary>
        void Detach();
    }

    /// <summary>
    /// Component that represents a part of <see cref="GunAttachment"/> 
    /// (e.g a red dot might have more than one component to attach).
    /// </summary>
    public abstract class GunBehaviour : MonoBehaviour
    {
        private enum ActivationMethod
        {
            /// <summary>
            /// Component script will be enabled/disabled.
            /// </summary>
            DisableComponent,

            /// <summary>
            /// The entire <see cref="GameObject"/> will be activated/deactivated.
            /// </summary>
            DeactivateGameObject
        }

        [Tooltip("Determines how this component enabled/disabled when attached/detached.")]
        [SerializeField]
        private ActivationMethod _activationMethod;

        [Tooltip("This component is currently attached?")]
        [SerializeField, HideInInspector]
        protected bool _isAttached;

        public bool IsAttached => _isAttached;

        protected IHandheld Handheld { get; private set; }
        protected IGun Gun { get; private set; }

        protected virtual void Awake()
        {
            FindHandheld();

            if (_isAttached == false)
            {
                Detach();
            }
        }
        
        /// <summary>
        /// Searches for a <see cref="IHandheld"/> in the parent component and assignes as owner.
        /// </summary>
        /// <returns>If has valid <see cref="IHandheld"/> found in the hierarchy for this component.</returns>
        private bool FindHandheld()
        {
            if(Handheld != null)
            {
                return true;
            }

            Handheld = GetComponentInParent<IHandheld>();
            Gun = Handheld as IGun;

            return Handheld != null;
        }

        /// <summary>
        /// Can be overridden in derived classes to enforce constraints on inventory, character stats, etc.
        /// </summary>
        /// <returns>Can this component be attached to its owner?</returns>
        public virtual bool CanAttach() => FindHandheld() || true;

        public void Attach()
        {
            if(_isAttached || FindHandheld() == false)
            {
                return;
            }

            _isAttached = true;
            Activate();
            OnAttach();
        }

        private void Activate()
        {
            if (_activationMethod == ActivationMethod.DisableComponent)
            {
                enabled = true;
            }
            else
            {
                enabled = true;
                gameObject.SetActive(true);
            }
        }

        public void Detach()
        {
            if(_isAttached == false || FindHandheld() == false)
            {
                return;
            }

            _isAttached = false;
            OnDetach();
            Deactivate();
        }

        private void Deactivate()
        {
            if (_activationMethod == ActivationMethod.DisableComponent)
            {
                enabled = false;
            }
            else
            {
                gameObject.SetActive(false);
                enabled = true;
            }
        }

        protected virtual void OnAttach() { }
        protected virtual void OnDetach() { }


#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            //Handheld ??= GetComponentInParent<IHandheld>();

            //if (Handheld == null)
            //{
            //    Debug.LogError($"{name}: A gun behaviour must have a valid 'IHandheld' to attach to.");
            //    return;
            //}

            //if (_activationMethod == ActivationMethod.DisableComponent && Handheld.gameObject != gameObject)
            //{
            //    Debug.LogWarning($"{name}: Gun behaviour only enables/disables itself, but don't control the 'gameObject'," +
            //        " it might result in 'gameObject' lingering. Consider double-checking.");
            //}
            //else if(_activationMethod == ActivationMethod.DeactivateGameObject && Handheld.gameObject == gameObject)
            //{
            //    Debug.LogWarning($"{name}: Gun behaviour activates/deactivates the whole object but it shares the same" +
            //        " 'gameObject' as owner, so when this component is disabled so it the owner. This might not be intended, consider double checking.");
            //}
        }
#endif
    }
}