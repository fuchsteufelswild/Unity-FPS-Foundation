using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunCartridgeVisualizer : IGunBehaviour
    {
        /// <summary>
        /// Updates the cartridge visuals.
        /// </summary>
        void UpdateVisuals();
    }

    public sealed class NullCartridgeVisualizer : IGunCartridgeVisualizer
    {
        public static readonly NullCartridgeVisualizer Instance = new();

        public void Attach() { }
        public void Detach() { }
        public void OnFired(float ejectionDelay) { }
        public void UpdateVisuals() { }
    }

    /// <summary>
    /// Used for systems like Revolver where in the reload you see the cartridges as empty and full depending on
    /// reload phase.
    /// </summary>
    public class MagazineCartridgeVisualizer : 
        GunBehaviour,
        IGunCartridgeVisualizer
    {
        [SerializeField, Range(0, 100)]
        private int _cartridgeCount = 5;

        [SerializeField, Range(0f, 10f)]
        private float _reloadDelay = 0.6f;

        [Tooltip("Effects played when fired.")]
        [ReorderableList(ElementLabel = "Effect")]
        [ReferencePicker(typeof(CartridgeVisualizationEffect), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private CartridgeVisualizationEffect[] _onFireEffects = Array.Empty<CartridgeVisualizationEffect>();

        [ReorderableList(ElementLabel = "Cartridge")]
        [SerializeField]
        private CartridgeRenderers[] _cartridges = Array.Empty<CartridgeRenderers>();

        private IGunMagazineBehaviour _magazine;
        private IGunShellEjectionBehaviour _shellEjector;

        private void OnEnable()
        {
            if(Gun != null)
            {
                Gun.CartridgeVisualizer = this;

                BindToMagazine();
                BindToEjector();
            }
        }

        private void BindToMagazine()
        {
            Gun.AddComponentChangedListener(GunBehaviourType.MagazineSystem, OnMagazineChanged);
            _magazine = Gun.Magazine;

            if (_magazine == null)
            {
                return;
            }

            _magazine.ReloadStarted += OnReloadStarted;
        }

        private void BindToEjector()
        {
            Gun.AddComponentChangedListener(GunBehaviourType.EjectionSystem, OnEjectorChanged);
            _shellEjector = Gun.ShellEjector;

            if(_shellEjector == null)
            {
                return;
            }

            _shellEjector.ShellEjected += OnShellEjected;
        }

        private void OnDisable()
        {
            if (_magazine != null)
            {
                _magazine.ReloadStarted -= OnReloadStarted;
            }

            if(_shellEjector != null)
            {
                _shellEjector.ShellEjected -= OnShellEjected;
            }

            Gun.RemoveComponentChangedListener(GunBehaviourType.MagazineSystem, OnMagazineChanged);
            Gun.AddComponentChangedListener(GunBehaviourType.EjectionSystem, OnEjectorChanged);
        }

        private void OnMagazineChanged()
        {
            _magazine.ReloadStarted -= OnReloadStarted;
            _magazine = Gun.Magazine;
            _magazine.ReloadStarted += OnReloadStarted;
        }

        private void OnReloadStarted(in ReloadStartEventArgs args) => this.StartCoroutine(ReloadCartridges(_magazine.CurrentAmmoCount, args.AmmoToLoad));

        /// <summary>
        /// Sets cartridges to enable before reload start. The moment before it, character might be taking out
        /// the empty cartridges.
        /// </summary>
        private IEnumerator ReloadCartridges(int inMagazine, int ammoToLoad)
        {
            yield return new Delay(_reloadDelay);

            if(_magazine.IsReloading == false)
            {
                yield break;
            }

            int numberOfCartridgesToEnable = Mathf.Clamp(inMagazine + ammoToLoad, 0, _cartridgeCount);

            for(int i = 0; i < numberOfCartridgesToEnable; i++)
            {
                _cartridges[i].Toggle(true);
            }
        }


        private void OnEjectorChanged()
        {
            _shellEjector.ShellEjected -= OnShellEjected;
            _shellEjector = Gun.ShellEjector;
            _shellEjector.ShellEjected += OnShellEjected;
        }

        private void OnShellEjected()
        {
            foreach (var effect in _onFireEffects)
            {
                effect.Trigger();
            }

            this.InvokeDelayed(UpdateVisuals, _shellEjector.EjectDuration);
        }

        /// <summary>
        /// Empty the shell.
        /// </summary>
        public void UpdateVisuals()
        {
            if (_magazine == null)
            {
                return;
            }

            if(_magazine.CurrentAmmoCount < _cartridgeCount)
            {
                _cartridges[_magazine.CurrentAmmoCount].Toggle(false);
            }
        }

        [Serializable]
        private sealed class CartridgeRenderers
        {
            [ReorderableList(ListStyle.Lined, HasLabels = false)]
            [SerializeField]
            private Renderer[] _emptyCartridgeRenderers;

            [ReorderableList(ListStyle.Lined, HasLabels = false)]
            [SerializeField]
            private Renderer[] _fullCartridgeRenderers;

            public void Toggle(bool isFull)
            {
                foreach (var renderer in _emptyCartridgeRenderers)
                {
                    renderer.enabled = !isFull;
                }

                foreach (var renderer in _fullCartridgeRenderers)
                {
                    renderer.enabled = isFull;
                }
            }
        }
    }
}