using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunShellEjectionBehaviour : IGunBehaviour
    {
        /// <summary>
        /// How long does a shell ejection takes.
        /// </summary>
        float EjectDuration { get; }

        /// <summary>
        /// Ejects a shell.
        /// </summary>
        void Eject();

        /// <summary>
        /// Called when a shell is ejected.
        /// </summary>
        event UnityAction ShellEjected;
    }

    public sealed class NullShellEjector : IGunShellEjectionBehaviour
    {
        public static readonly NullShellEjector Instance = new();

        public float EjectDuration => 0f;
        public event UnityAction ShellEjected { add { } remove { } }

        public void Attach() { }
        public void Detach() { }
        public void Eject() { }
    }

    public class GunShellEjectionBehaviour : 
        GunBehaviour,
        IGunShellEjectionBehaviour
    {
        [Tooltip("Total time the shell ejection takes.")]
        [SerializeField, Range(0f, 5f)]
        private float _ejectDuration;

        [Tooltip("Effects played when a shell is ejected.")]
        [ReorderableList(ElementLabel = "Effect")]
        [ReferencePicker(typeof(EjectionEffect), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private EjectionEffect[] _effects = Array.Empty<EjectionEffect>();

        public event UnityAction ShellEjected;

        public float EjectDuration => _ejectDuration;

        private void OnEnable()
        {
            if(Gun != null)
            {
                Gun.ShellEjector = this;

                foreach(var effect in _effects)
                {
                    effect.Initialize(Gun);
                }
            }
        }

        public virtual void Eject()
        {
            foreach(var effect in _effects)
            {
                effect.Trigger();
            }

            ShellEjected?.Invoke();
        }
    }
}