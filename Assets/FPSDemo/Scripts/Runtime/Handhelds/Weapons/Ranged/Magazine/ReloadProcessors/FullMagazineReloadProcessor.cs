using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public readonly struct ReloadStartEventArgs
    {
        /// <summary>
        /// Is the reload done on the empty magazine.
        /// </summary>
        public readonly bool IsEmptyReload;

        /// <summary>
        /// Duration of the whole reload process.
        /// </summary>
        public readonly float Duration;

        /// <summary>
        /// Speed of the reload animation;
        /// </summary>
        public readonly float AnimationSpeed;

        /// <summary>
        /// Ammo to load in the reload process.
        /// </summary>
        public readonly int AmmoToLoad;

        public ReloadStartEventArgs(bool isEmptyReload, float duration, float animationSpeed, int ammoToLoad)
        {
            IsEmptyReload = isEmptyReload;
            Duration = duration;
            AnimationSpeed = animationSpeed;
            AmmoToLoad = ammoToLoad;
        }
    }

    public delegate void ReloadStartedDelegate(in ReloadStartEventArgs args);

    /// <summary>
    /// Reloads the whole magazine at once (like AR magazine).
    /// </summary>
    [Serializable]
    public sealed class FullMagazineReloadProcessor : IReloadProcessor
    {
        [SerializeField]
        private FullMagazineReloadConfig _reloadConfig;

        private IGunAmmoStorage _ammoStorage;

        private int _ammoToLoad;
        private float _reloadEndTime;

        public bool IsReloading { get; private set; }
        public int CurrentAmmo { get; private set; } = -1;


        public event ReloadStartedDelegate ReloadStarted;
        public event RoundLoadedDelegate RoundLoaded { add { } remove { } }
        public event UnityAction ReloadCompleted;
        public event UnityAction ReloadCanceled;
        public event UnityAction<float> ReloadEndPhaseStarted { add { } remove { } }

        public void Start(IGunAmmoStorage ammoStorage, int currentAmmo, int capacity)
        {
            if(IsReloading || currentAmmo >=  capacity)
            {
                return;
            }

            _ammoToLoad = CalculateAmmoToLoad(ammoStorage, currentAmmo, capacity);
            if(_ammoToLoad <= 0)
            {
                return;
            }

            _ammoStorage = ammoStorage;
            _ammoStorage.TryRemoveAmmo(_ammoToLoad);

            IsReloading = true;
            CurrentAmmo = currentAmmo;

            ReloadStartEventArgs args = BuildReloadStartEventArgs();
            _reloadEndTime = Time.time + args.Duration;

            ReloadStarted?.Invoke(in args);
        }

        /// <param name="ammoStorage">Ammo storage used by the gun.</param>
        /// <param name="currentAmmo">Current ammo in the magazine.</param>
        /// <param name="capacity">Max ammo magazine can have at a time.</param>
        /// <returns>amount of ammo that can be loaded.</returns>
        private int CalculateAmmoToLoad(IGunAmmoStorage ammoStorage, int currentAmmo, int capacity)
        {
            int availableSpace = capacity - currentAmmo;
            int ammoAvailable = ammoStorage.CurrentAmmo;
            return Mathf.Min(availableSpace, ammoAvailable);
        }

        private ReloadStartEventArgs BuildReloadStartEventArgs()
        {
            bool isEmptyReload = _reloadConfig.HasEmptyReload && CurrentAmmo == 0;
            float duration = isEmptyReload ? _reloadConfig.EmptyReloadDuration : _reloadConfig.ReloadDuration;
            float animationSpeed = isEmptyReload ? _reloadConfig.EmptyReloadAnimationSpeed : _reloadConfig.ReloadAnimationSpeed;

            return new(isEmptyReload, duration, animationSpeed, _ammoToLoad);
        }

        public void Cancel(float transitionSpeed)
        {
            if(IsReloading == false)
            {
                return;
            }

            _ammoStorage?.AddAmmo(_ammoToLoad);
            _ammoToLoad = 0;

            IsReloading = false;
            ReloadCanceled?.Invoke();
        }

        public void Update(float deltaTime)
        {
            if(IsReloading == false)
            {
                return;
            }

            if(Time.time >= _reloadEndTime)
            {
                EndReload();    
            }
        }

        private void EndReload()
        {
            IsReloading = false;
            CurrentAmmo += _ammoToLoad;
            _ammoToLoad = 0;

            ReloadCompleted?.Invoke();
        }

        public void SetAmmo(int newAmmo) => CurrentAmmo = newAmmo;

        [Serializable]
        private sealed class FullMagazineReloadConfig
        {
            [Tooltip("Time it takes for whole reload process to complete.")]
            [SerializeField, Range(0.1f, 20f)]
            private float _reloadDuration = 2f;

            [Tooltip("Speed of the reload animation.")]
            [SerializeField, Range(0.01f, 10f)]
            private float _reloadAnimationSpeed = 1f;

            [Tooltip("Denotes if it can perform empty reload.")]
            [SerializeField]
            private bool _hasEmptyReload;

            [Tooltip("Time it takes for whole 'Empty' magazine reload process to complete.")]
            [ShowIf(nameof(_hasEmptyReload), true)]
            [SerializeField, Range(0.1f, 30f)]
            private float _emptyReloadDuration = 5f;

            [Tooltip("Speed of the 'Empty' magazine reload animation.")]
            [ShowIf(nameof(_hasEmptyReload), true)]
            [SerializeField, Range(0.01f, 10f)]
            private float _emptyReloadAnimationSpeed = 1f;

            /// <summary>
            /// Time it takes for whole reload process to complete.
            /// </summary>
            public float ReloadDuration => _reloadDuration / _reloadAnimationSpeed;

            /// <summary>
            /// Speed of the reload animation.
            /// </summary>
            public float ReloadAnimationSpeed => _reloadAnimationSpeed;

            /// <summary>
            /// Denotes if it can perform empty reload.
            /// </summary>
            public bool HasEmptyReload => _hasEmptyReload;

            /// <summary>
            /// Time it takes for whole 'Empty' magazine reload process to complete.
            /// </summary>
            public float EmptyReloadDuration => _emptyReloadDuration / _emptyReloadAnimationSpeed;

            /// <summary>
            /// Speed of the 'Empty' magazine reload animation.
            /// </summary>
            public float EmptyReloadAnimationSpeed => _emptyReloadAnimationSpeed;
        }
    }
}