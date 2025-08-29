using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    public interface IHandheldsManager :
        ICharacterBehaviour,
        IHandheldRegistry,
        IHandheldEquipmentController
    {
        /// <summary>
        /// Root transform of all handhelds.
        /// </summary>
        Transform HandheldsRoot { get; }
    }

    /// <summary>
    /// Manages all the <see cref="IHandheld"/> registered to a <see cref="ICharacter"/>.
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.LateGameLogic)]
    public sealed class HandheldsManager :
        CharacterBehaviour,
        IHandheldsManager
    {
        [Tooltip("The parent of the spawned handheld objects.")]
        [SerializeField]
        private Transform _spawnRoot;

        [Tooltip("The default handheld, that is used for when no item is equipped (no weapon, no food), just hands, or nothing.")]
        [SerializeField, SceneObjectOnly]
        private Handheld _defaultHandheld;

        private HandheldRegistry _registry;
        private HandheldEquipmentController _equipmentController = new();
        private IHandheld _nullHandheld;

        public Transform HandheldsRoot => _spawnRoot != null ? _spawnRoot : transform;
        public IReadOnlyList<IHandheld> RegisteredHandhelds => _registry?.RegisteredHandhelds ?? Array.Empty<IHandheld>();
        public IHandheld ActiveHandheld => _equipmentController?.ActiveHandheld;
        public ControllerState EquipmentState => _equipmentController?.EquipmentState ?? ControllerState.None;

        public event ControllerEquipmentStateChangedDelegate EquipmentStateChanged
        {
            add => _equipmentController.EquipmentStateChanged += value;
            remove => _equipmentController.EquipmentStateChanged -= value;
        }

        public event ControllerEquipmentDelegate EquipBegin
        {
            add => _equipmentController.EquipBegin += value;
            remove => _equipmentController.EquipBegin -= value;
        }

        public event ControllerEquipmentDelegate EquipEnd
        {
            add => _equipmentController.EquipEnd += value;
            remove => _equipmentController.EquipEnd -= value;
        }    

        public event ControllerEquipmentDelegate HolsterBegin
        {
            add => _equipmentController.HolsterBegin += value;
            remove => _equipmentController.HolsterBegin -= value;
        }

        public event ControllerEquipmentDelegate HolsterEnd
        {
            add => _equipmentController.HolsterEnd += value;
            remove => _equipmentController.HolsterEnd -= value;
        }

        protected override void OnBehaviourStart(ICharacter parent)
        {
            _registry = HandheldsManagerFactory.CreateRegistry(_spawnRoot, null, parent);
            _equipmentController.Initialize(_registry, this);
            SetupDefaultHandheld();
            TryEquip(null);
        }

        private void SetupDefaultHandheld()
        {
            _nullHandheld = HandheldsManagerFactory.CreateDefaultHandheld(_defaultHandheld);
            _nullHandheld = RegisterHandheld(_nullHandheld);
            _equipmentController.SetDefaultHandheld(_nullHandheld);
        }

        protected override void OnDestroy()
        {
            _equipmentController?.Dispose();
            base.OnDestroy();
        }

        #region Public API
        public IHandheld RegisterHandheld(IHandheld handheld, bool disable = true) => _registry.RegisterHandheld(handheld, disable);
        public bool UnregisterHandheld(IHandheld handheld, bool destroy = false)
        {
            // Ensure the handheld is holstered before unregistering
            if(TryHolster(handheld, HandheldAnimationConstants.MaximumHolsteringSpeed) == false)
            {
                return false;
            }

            return _registry.UnregisterHandheld(handheld, destroy);
        }
        public bool IsRegistered(IHandheld handheld) => _registry.IsRegistered(handheld);

        public void HolsterAll() => _equipmentController.HolsterAll();

        public bool TryEquip(IHandheld handheld, float transitionSpeed = 1f, UnityAction onEquipBegin = null)
        {
            handheld ??= _nullHandheld;
            return _equipmentController.TryEquip(handheld, transitionSpeed, onEquipBegin);
        }

        public bool TryHolster(IHandheld handheld, float transitionSpeed = 1f) 
            => _equipmentController.TryHolster(handheld, transitionSpeed);
        #endregion
    }


    public static class HandheldsManagerFactory
    {
        public static IHandheld CreateDefaultHandheld(Handheld defaultHandheld)
        {
            return defaultHandheld != null ? defaultHandheld : NullHandheld.Instance;
        }

        public static HandheldRegistry CreateRegistry(Transform spawnRoot, Transform fallbackRoot, ICharacter character)
        {
            var root = spawnRoot != null ? spawnRoot : fallbackRoot;
            return new HandheldRegistry(spawnRoot, character);
        }
    }
}