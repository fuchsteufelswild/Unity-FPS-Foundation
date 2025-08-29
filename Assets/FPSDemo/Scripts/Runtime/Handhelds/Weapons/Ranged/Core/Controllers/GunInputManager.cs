using Nexora.FPSDemo.Options;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunInputManager
    {
        void HandleUse(InputActionState inputState);
        void HandleAim(InputActionState inputState);
        void HandleReload(InputActionState inputState);
    }

    public interface IGunInputOrchestrator
    {
        event UnityAction OnShoot;

        bool EndReload();
        bool HandleEmptyMagazine();

        void BlockFor<T>(float duration) where T : IInputActionHandler;
    }

    public sealed class GunInputManager : 
        IGunInputManager,
        IGunInputOrchestrator
    {
        /// <summary>
        /// Cooldown between consecutive actions.
        /// </summary>
        public const float ActionCooldown = 0.3f;

        private GunReloadActionHandler _reloadActionHandler;
        private GunAimActionHandler _aimActionHandler;
        private GunUseActionHandler _useActionHandler;

        private Gun _gun;
        private GunComponentManager _components;

        public event UnityAction OnShoot;

        public void BlockFor<T>(float duration)
            where T : IInputActionHandler
        {
            T action = _gun.GetActionOfType<T>();
            action.Blocker.BlockFor(duration);
        }

        public bool IsCrosshairActive()
            => _useActionHandler.Blocker.IsBlocked == false && _components.Magazine.CurrentAmmoCount > 0
            && _components.Magazine.IsReloading == false;

        public void HandleUse(InputActionState inputState) => _useActionHandler.HandleInput(inputState);
        public void HandleAim(InputActionState inputState) => _aimActionHandler.HandleInput(inputState);
        public void HandleReload(InputActionState inputState) => _aimActionHandler.HandleInput(inputState);

        public GunInputManager(Gun gun, GunComponentManager components)
        {
            _gun = gun;
            _components = components;
            _components.OnShoot += OnShootInternal;

            _reloadActionHandler = gun.GetActionOfType<GunReloadActionHandler>();
            _aimActionHandler = gun.GetActionOfType<GunAimActionHandler>();
            _useActionHandler = gun.GetActionOfType<GunUseActionHandler>();

            _reloadActionHandler.Initialize(this, components);
            _aimActionHandler.Initialize(this, components);
            _useActionHandler.Initialize(this, components);
        }

        private void OnShootInternal()
        {
            OnShoot?.Invoke();
            _reloadActionHandler.Blocker.BlockFor(ActionCooldown);
        }

        public void RegisterBlockerCallbacks()
        {
            _reloadActionHandler.Blocker.Blocked += () => EndReload();
            _useActionHandler.Blocker.Blocked += () => _components.TriggerMechanism.TriggerUp();
            _aimActionHandler.Blocker.Blocked += () => _components.AimBehaviour.StopAiming();
        }

        public bool EndReload() => _reloadActionHandler.EndReload();

        public bool HandleEmptyMagazine()
        {
            if(CombatOptions.Instance.AutoReloadOnEmptyMagazine && StartReload())
            {
                return true;
            }

            _components.DryFireEffect.TriggerDryFireEffect();
            return true;
        }

        public bool StartReload()
        {
            if(_reloadActionHandler.StartReload())
            {
                _useActionHandler.Blocker.BlockFor(ActionCooldown);

                if (_components.AimBehaviour.IsAiming && CombatOptions.Instance.AimWhileReloading == false)
                {
                    _aimActionHandler.EndAim();
                }

                return true;
            }

            return false;
        }

        
    }

    [Serializable]
    public abstract class GunActionHandler
    {
        protected IGunInputOrchestrator _inputManager;
        protected GunComponentManager _components;

        public virtual void Initialize(IGunInputOrchestrator inputManager, GunComponentManager components)
        {
            _inputManager = inputManager;
            _components = components;
        }
    }

    [Serializable]
    public sealed class GunUseActionHandler :
        GunActionHandler,
        IUseActionHandler
    {
        private ActionBlockerCore _blocker = new();

        private bool _requiresEject;

        public bool IsPerforming => _components.TriggerMechanism.IsTriggerHeld;

        public ActionBlockerCore Blocker => _blocker;

        public override void Initialize(IGunInputOrchestrator inputManager, GunComponentManager components)
        {
            base.Initialize(inputManager, components);

            _inputManager.OnShoot += OnShoot;
        }

        public void OnShoot()
        {
            if (CombatOptions.Instance.ManualShellEjection && _components.ShellEjector.EjectDuration > 0.01f)
            {
                _requiresEject = true;
            }
            else
            {
                _components.ShellEjector.Eject();
            }
        }

        public bool HandleInput(InputActionState inputState)
        {
            return inputState switch
            {
                InputActionState.Start => StartUse(),
                InputActionState.Hold => HoldUse(),
                InputActionState.End => EndUse(),
                _ => throw new System.NotImplementedException(),
            };
        }

        public bool StartUse()
        {
            if(_requiresEject)
            {
                return HandleShellEjection();
            }

            if(_components.Magazine.IsReloading)
            {
                return _inputManager.EndReload();
            }

            if(_components.Magazine.CurrentAmmoCount == 0)
            {
                return _inputManager.HandleEmptyMagazine();
            }

            return HoldUse();
        }

        private bool HandleShellEjection()
        {
            _components.ShellEjector.Eject();
            _blocker.BlockFor(_components.ShellEjector.EjectDuration);
            _requiresEject = false;
            return true;
        }

        public bool HoldUse()
        {
            if (_blocker.IsBlocked == false && _components.Magazine.IsReloading == false)
            {
                _components.TriggerMechanism.TriggerHold();
                return _components.TriggerMechanism.IsTriggerHeld;
            }

            _components.TriggerMechanism.TriggerUp();
            return false;
        }

        public bool EndUse()
        {
            _components.TriggerMechanism.TriggerUp();
            return true;
        }
    }

    [Serializable]
    public sealed class GunReloadActionHandler :
        GunActionHandler,
        IReloadActionHandler
    {
        private ActionBlockerCore _blocker = new();

        public bool IsPerforming => _components.Magazine.IsReloading;

        public ActionBlockerCore Blocker => _blocker;

        public bool HandleInput(InputActionState inputState)
        {
            return inputState switch
            {
                InputActionState.Start => StartReload(),
                InputActionState.End => EndReload(),
                _ => false
            };
        }

        public bool StartReload()
        {
            if (_blocker.IsBlocked)
            {
                return false;
            }

            IGunAmmoStorage ammoStorage = CombatOptions.Instance.UnlimitedReserveAmmo
                ? InfiniteAmmoStorage.Instance
                : _components.AmmoStorage;

            if (_components.Magazine.TryBeginReload(ammoStorage))
            {
                return true;
            }

            return false;
        }

        public bool EndReload()
        {
            if (_components.Magazine.CurrentAmmoCount > 0 &&
             _components.Magazine.TryCancelReload(_components.AmmoStorage, 1f, out float endDuration))
            {
                if (endDuration == 0f)
                {
                    return false;
                }

                _inputManager.BlockFor<IUseActionHandler>(endDuration + 0.05f);
                return true;
            }

            return false;
        }
    }

    [Serializable]
    public sealed class GunAimActionHandler :
        GunActionHandler,
        IAimActionHandler
    {
        private ActionBlockerCore _blocker = new();

        public bool IsPerforming => _components.AimBehaviour.IsAiming;

        public ActionBlockerCore Blocker => _blocker;

        public bool HandleInput(InputActionState inputState)
        {
            return inputState switch
            {
                InputActionState.Start => StartAim(),
                InputActionState.End => EndAim(),
                _ => false
            };
        }

        public bool StartAim()
        {
            if (_blocker.IsBlocked)
            {
                return false;
            }

            if (_components.Magazine.IsReloading && CombatOptions.Instance.AimWhileReloading == false)
            {
                return false;
            }

            return _components.AimBehaviour.StartAiming();
        }

        public bool EndAim()
        {
            if (_components.AimBehaviour.StopAiming())
            {
                _blocker.BlockFor(GunInputManager.ActionCooldown);
                return true;
            }

            return false;
        }
    }
}