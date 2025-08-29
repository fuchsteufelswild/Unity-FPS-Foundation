using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public sealed class GunAimBehaviour :
        GunBehaviour,
        IGunAimBehaviour
    {
        [Tooltip("Accuracy modifier to use when aiming.")]
        [SerializeField, Range(0f, 1f)]
        private float _accuracyModifier = 1f;

        [ReorderableList(ElementLabel = "Effector")]
        [ReferencePicker(typeof(AimEffector), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private AimEffector[] _effectors = Array.Empty<AimEffector>();

        public bool IsAiming { get; private set; }
        public float FireAccuracyModifier => _accuracyModifier;

        public event UnityAction OnAimingStarted;
        public event UnityAction OnAimingStopped;

        protected override void Awake()
        {
            base.Awake();

            foreach (var effector in _effectors)
            {
                effector.Initialize(this);
            }
        }

        private void OnEnable()
        {
            

            if (Gun != null)
            {
                Gun.AimBehaviour = this;
                foreach (var effector in _effectors)
                {
                    effector.Enable(Gun);
                }
            }
        }

        private void OnDisable()
        {
            foreach (var effector in _effectors)
            {
                effector.Disable(Gun);
            }
        }

        public bool StartAiming()
        {
            if (IsAiming)
            {
                return false;
            }

            IsAiming = true;
            OnAimingStarted?.Invoke();
            return true;
        }

        public bool StopAiming()
        {
            if (IsAiming == false)
            {
                return false;
            }

            if(UnityUtils.IsQuitting)
            {
                return true;
            }

            IsAiming = false;
            OnAimingStopped?.Invoke();
            return true;
        }

        protected override void OnDetach() => StopAiming();

        public T GetEffectorOfType<T>()
            where T : AimEffector
        {
            foreach (var effector in _effectors)
            {
                if (effector is T targetEffector)
                {
                    return targetEffector;
                }
            }

            return null;
        }
    }
}