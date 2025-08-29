using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunComponentManager
    {
        public static readonly int BehaviourTypeCount = EnumUtility.GetAllEnumValues<GunBehaviourType>().Length;

        IGunAimBehaviour AimBehaviour { get; set; }
        IGunAmmoStorage AmmoStorage { get; set; }
        IGunMuzzleEffectBehaviour MuzzleEffect { get; set; }
        IGunDryFireEffectBehaviour DryFireEffect { get; set; }
        IGunShellEjectionBehaviour ShellEjector { get; set; }
        IGunCartridgeVisualizer CartridgeVisualizer { get; set; }
        IGunMagazineBehaviour Magazine { get; set; }
        IGunTriggerBehaviour TriggerMechanism { get; set; }
        IGunImpactEffectBehaviour ImpactEffector { get; set; }
        IGunFiringMechanismBehaviour FiringMechanism { get; set; }
        IGunRecoilBehaviour RecoilMechanism { get; set; }

        public void AddComponentChangedListener(GunBehaviourType type, UnityAction callback);

        public void RemoveComponentChangedListener(GunBehaviourType type, UnityAction callback);
    }

    public sealed class GunComponentManager
    {
        private UnityAction[] _componentChangedCallbacks;

        private IGunAimBehaviour _aimingBehaviour = NullAimBehaviour.Instance;
        private IGunTriggerBehaviour _triggerMechanism = NullTrigger.Instance;
        private IGunFiringMechanismBehaviour _firingMechanism = NullFiringMechanism.Instance;
        private IGunAmmoStorage _ammoStorage = InfiniteAmmoStorage.Instance;
        private IGunMagazineBehaviour _magazine = NullMagazine.Instance;
        private IGunRecoilBehaviour _recoilMechanism = NullGunRecoil.Instance;
        private IGunImpactEffectBehaviour _impactEffect = NullImpactEffect.Instance;
        private IGunShellEjectionBehaviour _shellEjector = NullShellEjector.Instance;
        private IGunDryFireEffectBehaviour _dryFireEffect = NullDryFireEffect.Instance;
        private IGunMuzzleEffectBehaviour _muzzleEffect = NullMuzzleEffect.Instance;
        private IGunCartridgeVisualizer _cartridgeVisualizer = NullCartridgeVisualizer.Instance;

        public event UnityAction OnTriggerPulled;
        public event UnityAction OnShoot;
        public event UnityAction OnAmmoStorageChanged;
        public event UnityAction OnShellEjected;

        public IGunAimBehaviour AimBehaviour
        {
            get => _aimingBehaviour;
            set => SetComponent(ref _aimingBehaviour, value, GunBehaviourType.AimingSystem,
                getState: oldAimer => oldAimer.IsAiming ? oldAimer.StopAiming() : false,
                onAttach: newAimer => { });
        }

        public IGunTriggerBehaviour TriggerMechanism
        {
            get => _triggerMechanism;
            set => SetComponent(ref _triggerMechanism, value, GunBehaviourType.TriggerMechanism,
                getState: oldTrigger => oldTrigger.IsTriggerHeld,
                onAttach: newTrigger => newTrigger.TriggerPulled += OnTriggerPulled,
                onDetach: oldTrigger =>
                {
                    oldTrigger.TriggerPulled -= OnTriggerPulled;
                    return oldTrigger.IsTriggerHeld;
                },
                onReattach: (newTrigger, wasHeld) =>
                {
                    if (wasHeld)
                    {
                        newTrigger.TriggerHold();
                    }
                });
        }

        public IGunFiringMechanismBehaviour FiringMechanism
        {
            get => _firingMechanism;
            set => SetComponent(ref _firingMechanism, value, GunBehaviourType.FiringMechanism,
                onAttach: newFiringMechanism => newFiringMechanism.Shoot += OnShoot,
                onDetach: oldFiringMechanism =>
                {
                    oldFiringMechanism.Shoot -= OnShoot;
                    return true;
                });
        }

        public IGunAmmoStorage AmmoStorage
        {
            get => _ammoStorage;
            set => SetComponent(ref _ammoStorage, value, GunBehaviourType.AmmunitionSystem,
                getState: oldAmmoStorage => (oldAmmoStorage is InfiniteAmmoStorage) == false,
                onAttach: newAmmoStorage => OnAmmoStorageChanged?.Invoke());
        }

        public IGunMagazineBehaviour Magazine
        {
            get => _magazine;
            set => SetComponent(ref _magazine, value, GunBehaviourType.MagazineSystem,
                getState: oldMagazine => oldMagazine.CurrentAmmoCount,
                onReattach: (newMagazine, ammoCount) =>
                {
                    newMagazine.ForceSetAmmo(ammoCount);
                });
        }

        public IGunRecoilBehaviour RecoilMechanism
        {
            get => _recoilMechanism;
            set => SetComponent(ref _recoilMechanism, value, GunBehaviourType.RecoilSystem);
        }

        public IGunImpactEffectBehaviour ImpactEffect
        {
            get => _impactEffect;
            set => SetComponent(ref _impactEffect, value, GunBehaviourType.ImpactSystem);
        }

        public IGunShellEjectionBehaviour ShellEjector
        {
            get => _shellEjector;
            set => SetComponent(ref _shellEjector, value, GunBehaviourType.EjectionSystem,
                onAttach: newEjector => newEjector.ShellEjected += OnShellEjected,
                onDetach: oldEjector => 
                { 
                    oldEjector.ShellEjected -= OnShellEjected; 
                    return true; 
                });
        }

        public IGunDryFireEffectBehaviour DryFireEffect
        {
            get => _dryFireEffect;
            set => SetComponent(ref _dryFireEffect, value, GunBehaviourType.DryFireEffect);
        }

        public IGunMuzzleEffectBehaviour MuzzleEffect
        {
            get => _muzzleEffect;
            set => SetComponent(ref _muzzleEffect, value, GunBehaviourType.MuzzleEffect);
        }

        public IGunCartridgeVisualizer CartridgeVisualizer
        {
            get => _cartridgeVisualizer;
            set => SetComponent(ref _cartridgeVisualizer, value, GunBehaviourType.CartridgeVisualizer);
        }

        private void SetComponent<TBehaviour>(ref TBehaviour field, TBehaviour newValue, GunBehaviourType type, Action<TBehaviour> onSetup = null)
            where TBehaviour : class
        {
            // Set component without preserving any state or cleanup, just deattach/attach.
            SetComponent<TBehaviour, bool>(ref field, newValue, type, null, onSetup, null, null);
        }

        /// <summary>
        /// Sets new behaviour for <typeparamref name="TBehaviour"/>. <br></br>
        /// Gracefully detaches the old behaviour and attaches the new one,
        /// preserves previous state if it is required using callback available.
        /// </summary>
        /// <typeparam name="TBehaviour"><see cref="Type"/> of the behaviour (e.g <see cref="IGunFiringMechanismBehaviour"/>).</typeparam>
        /// <typeparam name="TState">
        /// State data of the behaviour, it can be anything, <br></br>
        /// for ammo storage -> int, stored ammo, <br></br>
        /// for trigger mechanism -> bool, was trigger held,<br></br>
        /// or anything that can come in mind.
        /// </typeparam>
        /// <param name="field">Current behaviour, this will be changed.</param>
        /// <param name="newValue">New behaviour.</param>
        /// <param name="type">Type of the behaviour.</param>
        /// <param name="getState">Gets the current state data of <typeparamref name="TBehaviour"/>.</param>
        /// <param name="onAttach">Informs about the attachment of the <paramref name="newValue"/>.</param>
        /// <param name="onDetach">Informs about the detachment of current behaviour(<paramref name="field"/>).</param>
        /// <param name="onReattach">Called to transfer previous state to the new behaviour if needed.</param>
        private void SetComponent<TBehaviour, TState>(ref TBehaviour field, TBehaviour newValue, GunBehaviourType type,
            Func<TBehaviour, TState> getState = null,
            Action<TBehaviour> onAttach = null,
            Func<TBehaviour, TState> onDetach = null,
            Action<TBehaviour, TState> onReattach = null)
            where TBehaviour : class
        {
            if(newValue == field)
            {
                return;
            }

            var state = default(TState);

            // Get previous state
            if(onDetach != null)
            {
                state = onDetach(field);
            }
            else if(getState != null)
            {
                state = getState(field);
            }

            (field as IGunBehaviour)?.Detach();

            field = newValue ?? GetDefaultComponent<TBehaviour>(type);
            (field as IGunBehaviour)?.Attach();

            onAttach?.Invoke(field);
            onReattach?.Invoke(field, state);

            RaiseComponentChangedEvent(type);
        }

        private T GetDefaultComponent<T>(GunBehaviourType type)
            where T : class
        {
            return type switch
            {
                GunBehaviourType.AimingSystem => NullAimBehaviour.Instance as T,
                GunBehaviourType.TriggerMechanism => NullTrigger.Instance as T,
                GunBehaviourType.FiringMechanism => NullFiringMechanism.Instance as T,
                GunBehaviourType.AmmunitionSystem => InfiniteAmmoStorage.Instance as T,
                GunBehaviourType.MagazineSystem => NullMagazine.Instance as T,
                GunBehaviourType.RecoilSystem => NullGunRecoil.Instance as T,
                GunBehaviourType.ImpactSystem => NullImpactEffect.Instance as T,
                GunBehaviourType.DryFireEffect => NullDryFireEffect.Instance as T,
                GunBehaviourType.EjectionSystem => NullShellEjector.Instance as T,
                GunBehaviourType.MuzzleEffect => NullMuzzleEffect.Instance as T,
                GunBehaviourType.CartridgeVisualizer => NullCartridgeVisualizer.Instance as T,
                _ => throw new NotImplementedException(),
            };
        }

        public void AddComponentChangedListener(GunBehaviourType type, UnityAction callback)
        {
            _componentChangedCallbacks ??= new UnityAction[IGunComponentManager.BehaviourTypeCount];
            int index = (int)type;
            _componentChangedCallbacks[index] += callback;
        }

        public void RemoveComponentChangedListener(GunBehaviourType type, UnityAction callback)
        {
            _componentChangedCallbacks ??= new UnityAction[IGunComponentManager.BehaviourTypeCount];
            int index = (int)type;
            _componentChangedCallbacks[index] -= callback;
        }

        private void RaiseComponentChangedEvent(GunBehaviourType type)
            => _componentChangedCallbacks?[(int)type]?.Invoke();
    }
}