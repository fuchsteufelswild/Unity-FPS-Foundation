using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public sealed class HandheldEffectsController : MonoBehaviour
    {
        [ReorderableList]
        [ReferencePicker(typeof(IHandheldEffect), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private IHandheldEffect[] _effectsOnEnable;

        [ReorderableList]
        [ReferencePicker(typeof(IHandheldEffect), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private IHandheldEffect[] _effectsOnDisable;

        private IHandheld _handheld;

        private bool _canEnableEffects = true;
        private bool _effectsEnabled;

        private void Awake() => _handheld = GetComponent<IHandheld>();

        /// <summary>
        /// Sets controller that the effects can be enabled.
        /// </summary>
        /// <remarks>
        /// Disables the effects if <paramref name="canEnable"/> is <see langword="false"/>.
        /// </remarks>
        public void SetCanEnable(bool canEnable)
        {
            _canEnableEffects = canEnable;

            if(_effectsEnabled && canEnable == false)
            {
                DisableEffects();
            }
        }

        public void ToggleEffects()
        {
            if (_effectsEnabled)
            {
                DisableEffects();
            }
            else
            {
                EnableEffects();
            }
        }

        public void EnableEffects()
        {
            if(_effectsEnabled || _canEnableEffects == false)
            {
                return;
            }

            _effectsEnabled = true;

            // Stop already activated effects that are lingering
            foreach(IHandheldEffect effect in _effectsOnDisable)
            {
                effect.Stop();
            }

            foreach(IHandheldEffect effect in _effectsOnEnable)
            {
                effect.Execute(_handheld);
            }
        }

        public void DisableEffects()
        {
            if(_effectsEnabled == false)
            {
                return;
            }

            _effectsEnabled = false;

            // Stop already activated effects that are lingering
            foreach (IHandheldEffect effect in _effectsOnEnable)
            {
                effect.Stop();
            }

            foreach (IHandheldEffect effect in _effectsOnDisable)
            {
                effect.Execute(_handheld);
            }
        }
    }
}