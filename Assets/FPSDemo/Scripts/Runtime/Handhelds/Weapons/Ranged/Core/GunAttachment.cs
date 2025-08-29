using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// A composite module of <see cref="GunBehaviour"/>s that can be attached or deattached through this class.
    /// (e.g Supressor, Red Dot, etc.)
    /// </summary>
    public sealed class GunAttachment : MonoBehaviour
    {
        [Tooltip("Name of this module to represent it.")]
        [SerializeField]
        private string _name;

        [Tooltip("Icon of this module.")]
        [SerializeField]
        private Sprite _icon;

        [Tooltip("Scaler for the icon display.")]
        [SerializeField, Range(0f, 5f)]
        private float _iconSizeScale = 1f;

        [Title("Events")]
        [SerializeField]
        private UnityEvent _onAttach;

        [SerializeField]
        private UnityEvent _onDetach;

        private GunBehaviour[] _behaviours;

        public bool IsAttached { get; private set; }

        public string Name => _name;
        public Sprite Icon => _icon;
        public float IconSizeScale => _iconSizeScale;

        private void Awake() => _behaviours = GetComponentsInChildren<GunBehaviour>(true) ?? Array.Empty<GunBehaviour>();

        public bool CanAttach()
        {
            _behaviours ??= GetComponentsInChildren<GunBehaviour>(true);
            return _behaviours.All(behaviour => behaviour.CanAttach());
        }

        /// <summary>
        /// Attaches the module to the gun.
        /// </summary>
        public void Attach()
        {
            if(IsAttached)
            {
                return;
            }

            gameObject.SetActive(true);
            _behaviours ??= GetComponentsInChildren<GunBehaviour>(true);

            foreach (GunBehaviour behaviour in _behaviours)
            {
                behaviour.Attach();
            }

            _onAttach?.Invoke();
            IsAttached = true;
        }

        /// <summary>
        /// Detaches the module from the gun.
        /// </summary>
        public void Detach()
        {
            if(IsAttached == false)
            {
                return;
            }

            gameObject.SetActive(false);
            _behaviours ??= GetComponentsInChildren<GunBehaviour>(true);

            foreach (GunBehaviour behaviour in _behaviours)
            {
                behaviour.Detach();
            }

            _onDetach?.Invoke();

            IsAttached = false;
        }    
    }
}