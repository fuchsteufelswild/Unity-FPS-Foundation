using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    [DefaultExecutionOrder(ExecutionOrder.LateGameLogic)]
    public sealed class Gun : 
        Handheld, 
        IGun,
        ICrosshairHandler
    {
        [Tooltip("Modifier for the accuracy when not aiming.")]
        [SerializeField, Range(0f, 1f)]
        private float _hipAccuracyModifier = 0.85f;

        [SerializeField]
        private GunCrosshair _corsshairHandler;

        public float Accuracy => _accuracySystem.CurrentAccuracy;

        public int CrosshairID
        { 
            get => _corsshairHandler.CrosshairID;
            set => _corsshairHandler.CrosshairID = value;
        }

        float ICrosshairHandler.Charge => _corsshairHandler.Charge;

        #region Behaviours
        public IGunAimBehaviour AimBehaviour
        {
            get => _components.AimBehaviour;
            set => _components.AimBehaviour = value;
        }
        public IGunAmmoStorage AmmoStorage
        {
            get => _components.AmmoStorage;
            set => _components.AmmoStorage = value;
        }
        public IGunMuzzleEffectBehaviour MuzzleEffect
        {
            get => _components.MuzzleEffect;
            set => _components.MuzzleEffect = value;
        }
        public IGunDryFireEffectBehaviour DryFireEffect
        {
            get => _components.DryFireEffect;
            set => _components.DryFireEffect = value;
        }
        public IGunShellEjectionBehaviour ShellEjector
        {
            get => _components.ShellEjector;
            set => _components.ShellEjector = value;
        }
        public IGunCartridgeVisualizer CartridgeVisualizer
        {
            get => _components.CartridgeVisualizer;
            set => _components.CartridgeVisualizer = value;
        }
        public IGunMagazineBehaviour Magazine
        {
            get => _components.Magazine;
            set => _components.Magazine = value;
        }
        public IGunTriggerBehaviour TriggerMechanism
        {
            get => _components.TriggerMechanism;
            set => _components.TriggerMechanism = value;
        }
        public IGunImpactEffectBehaviour ImpactEffector
        {
            get => _components.ImpactEffect;
            set => _components.ImpactEffect = value;
        }
        public IGunFiringMechanismBehaviour FiringMechanism
        {
            get => _components.FiringMechanism;
            set => _components.FiringMechanism = value;
        }
        public IGunRecoilBehaviour RecoilMechanism
        {
            get => _components.RecoilMechanism;
            set => _components.RecoilMechanism = value;
        }
        #endregion

        private GunComponentManager _components;
        private GunInputManager _inputManager;
        private GunAccuracySystem _accuracySystem;
        private GunShootingSystem _shootingSystem;

        protected override void Awake()
        {
            base.Awake();

            _components = new GunComponentManager();
            _inputManager = new GunInputManager(this, _components);
            _accuracySystem = new GunAccuracySystem(_components, _hipAccuracyModifier);
            _shootingSystem = new GunShootingSystem(_components, _accuracySystem);
        }

        public void SetCharge(float charge) => _corsshairHandler.SetCharge(charge);
        public void ResetCrosshair() => _corsshairHandler.ResetCrosshair();
        public bool IsCrosshairActive() => _inputManager.IsCrosshairActive();

        protected override void OnCharacterChanged(ICharacter character) => _accuracySystem.SetCharacter(character);

        private void Start() => _inputManager.RegisterBlockerCallbacks();

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            _accuracySystem.UpdateAccuracy(deltaTime, Time.fixedTime);
            _shootingSystem.UpdateShooting(deltaTime);
        }

        protected override IEnumerator PreHolster(float transitionSpeed)
        {
            Magazine.TryCancelReload(AmmoStorage, transitionSpeed, out float endDuration);
            yield return new Delay(endDuration);
        }

        public void HandleUse(InputActionState inputState) => _inputManager.HandleUse(inputState);
        public void HandleAim(InputActionState inputState) => _inputManager.HandleAim(inputState);
        public void HandleReload(InputActionState inputState) => _inputManager.HandleReload(inputState);

        public void AddComponentChangedListener(GunBehaviourType type, UnityAction callback)
            => _components.AddComponentChangedListener(type, callback);

        public void RemoveComponentChangedListener(GunBehaviourType type, UnityAction callback)
            => _components.RemoveComponentChangedListener(type, callback);
    }
}