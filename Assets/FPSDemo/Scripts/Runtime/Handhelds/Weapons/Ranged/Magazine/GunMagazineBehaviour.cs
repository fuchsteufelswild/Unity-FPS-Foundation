using Nexora.FPSDemo.Options;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public delegate void AmmoCountChangedDelegate(int previousAmmo, int currentAmmo);

    public interface IGunMagazineBehaviour : IGunBehaviour
    {
        /// <inheritdoc cref="IReloadProcessor.CurrentAmmo"/>
        int CurrentAmmoCount { get; }

        /// <summary>
        /// Capacity of the magazine.
        /// </summary>
        int Capacity { get; }

        /// <inheritdoc cref="IReloadProcessor.IsReloading"/>
        bool IsReloading { get; }

        /// <summary>
        /// Tries remove <paramref name="useAmount"/> ammo from the magazine.
        /// </summary>
        /// <param name="useAmount">Ammo to remove from magazine.</param>
        /// <returns>If it could remove the requested amount.</returns>
        bool TryUseAmmo(int useAmount);

        /// <summary>
        /// Sets the ammo in the magazine to <paramref name="newAmmo"/>.
        /// </summary>
        /// <param name="newAmmo">New ammo in the magazine.</param>
        void ForceSetAmmo(int newAmmo);

        public bool TryBeginReload(IGunAmmoStorage ammoStorage);

        public bool TryCancelReload(IGunAmmoStorage ammoStorage, float transitionSpeed, out float endDuration);

        /// <summary>
        /// Called when ammo count of the magazine is changed.
        /// </summary>
        event AmmoCountChangedDelegate AmmoCountChanged;

        /// <inheritdoc cref="IReloadProcessor.ReloadStarted"/>
        event ReloadStartedDelegate ReloadStarted;

        /// <inheritdoc cref="IReloadProcessor.RoundLoaded"/>
        event RoundLoadedDelegate RoundLoaded;

        /// <inheritdoc cref="IReloadProcessor.ReloadCompleted"/>
        event UnityAction ReloadCompleted;

        /// <inheritdoc cref="IReloadProcessor.ReloadCompleted"/>
        event UnityAction ReloadCanceled;

        /// <inheritdoc cref="IReloadProcessor.ReloadEndPhaseStarted"/>
        event UnityAction<float> ReloadEndPhaseStarted;
    }

    public sealed class NullMagazine : IGunMagazineBehaviour
    {
        public static readonly NullMagazine Instance = new();

        public int CurrentAmmoCount => 0;
        public int Capacity => 0;
        public bool IsReloading => false;

        public event AmmoCountChangedDelegate AmmoCountChanged { add { } remove { } }
        public event ReloadStartedDelegate ReloadStarted { add { } remove { } }
        public event RoundLoadedDelegate RoundLoaded { add { } remove { } }
        public event UnityAction ReloadCompleted { add { } remove { } }
        public event UnityAction ReloadCanceled { add { } remove { } }
        public event UnityAction<float> ReloadEndPhaseStarted { add { } remove { } }

        public void Attach() { }
        public void Detach() { }
        public void ForceSetAmmo(int newAmmo) { }

        public bool TryBeginReload(IGunAmmoStorage ammoStorage) => false;
        public bool TryCancelReload(IGunAmmoStorage ammoStorage, float transitionSpeed, out float endDuration)
        {
            endDuration = 0f;
            return false;
        }

        public bool TryUseAmmo(int useAmount) => false;
    }

    public sealed class GunMagazineBehaviour : 
        GunBehaviour,
        IGunMagazineBehaviour
    {
        [Tooltip("Maximum ammo magazine can hold.")]
        [SerializeField, Range(0f, 5000)]
        private int _capacity;

        [Tooltip("Reload strategy used for this magazine.")]
        [ReferencePicker(typeof(IReloadProcessor))]
        [SerializeReference]
        private IReloadProcessor _reloadProcessor;

        [Tooltip("Effects played for different phases of reload process (empty started, started, ended, round loaded).")]
        [ReorderableList(ElementLabel = "Effector")]
        [ReferencePicker(typeof(ReloadEffector), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private ReloadEffector[] _reloadEffectors = Array.Empty<ReloadEffector>();

        public int CurrentAmmoCount => _reloadProcessor.CurrentAmmo;
        public int Capacity => _capacity;
        public bool IsReloading => _reloadProcessor.IsReloading;

        public event AmmoCountChangedDelegate AmmoCountChanged;
        
        public event ReloadStartedDelegate ReloadStarted
        {
            add => _reloadProcessor.ReloadStarted += value;
            remove => _reloadProcessor.ReloadStarted -= value;
        }
        public event RoundLoadedDelegate RoundLoaded
        {
            add => _reloadProcessor.RoundLoaded += value;
            remove => _reloadProcessor.RoundLoaded -= value;
        }
        public event UnityAction ReloadCompleted
        {
            add => _reloadProcessor.ReloadCompleted += value;
            remove => _reloadProcessor.ReloadCompleted -= value;
        }
        public event UnityAction ReloadCanceled
        {
            add => _reloadProcessor.ReloadCanceled += value;
            remove => _reloadProcessor.ReloadCanceled -= value;
        }
        public event UnityAction<float> ReloadEndPhaseStarted
        {
            add => _reloadProcessor.ReloadEndPhaseStarted += value;
            remove => _reloadProcessor.ReloadEndPhaseStarted -= value;
        }

        protected override void Awake()
        {
            base.Awake();
            foreach(var effector in _reloadEffectors)
            {
                effector.Initialize(this);
            }
        }

        private void OnEnable()
        {
            if(Gun != null)
            {
                Gun.Magazine = this;
                foreach(var effector in _reloadEffectors)
                {
                    effector.Enable(Gun);
                }
            }
        }

        private void OnDisable()
        {
            if (Gun != null)
            {
                TryCancelReload(Gun.AmmoStorage, 1f, out _);
            }
        }

        public bool TryUseAmmo(int useAmount)
        {
            if(CurrentAmmoCount < useAmount)
            {
                return false;
            }

            int previousAmmoCount = CurrentAmmoCount;

            _reloadProcessor.SetAmmo(CurrentAmmoCount - useAmount);

            AmmoCountChanged?.Invoke(previousAmmoCount, CurrentAmmoCount);
            return true;
        }

        public void ForceSetAmmo(int newAmmo)
        {
            int previousAmmoCount = CurrentAmmoCount;
            _reloadProcessor.SetAmmo(newAmmo);

            AmmoCountChanged?.Invoke(previousAmmoCount, newAmmo);
        }

        private void Update()
        {
            if(_isAttached == false)
            {
                return;
            }

            int previousAmmo = _reloadProcessor.CurrentAmmo;
            _reloadProcessor.Update(Time.deltaTime);

            if(previousAmmo != _reloadProcessor.CurrentAmmo)
            {
                AmmoCountChanged?.Invoke(previousAmmo, _reloadProcessor.CurrentAmmo);
            }
        }

        public bool TryBeginReload(IGunAmmoStorage ammoStorage)
        {
            if(IsReloading)
            {
                return false;
            }

            _reloadProcessor.Start(ammoStorage, CurrentAmmoCount, Capacity);
            return _reloadProcessor.IsReloading;
        }

        public bool TryCancelReload(IGunAmmoStorage ammoStorage, float transitionSpeed, out float endDuration)
        {
            if(transitionSpeed >= HandheldAnimationConstants.MaximumHolsteringSpeed)
            {
                endDuration = 0f;
                _reloadProcessor.Cancel(transitionSpeed);
                return true;
            }


            endDuration = _reloadProcessor.ReloadEndDuration;
            if(IsReloading == false)
            {
                return false;
            }

            _reloadProcessor.Cancel(transitionSpeed);
            return true;
        }

        private void LateUpdate()
        {
            foreach(var effector in _reloadEffectors)
            {
                effector.LateUpdate();
            }
        }
    }
}