using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public readonly struct RoundLoadedArgs
    {
        /// <summary>
        /// How many rounds loaded so far.
        /// </summary>
        public readonly int TotalRoundsLoadedSoFar;

        /// <summary>
        /// Total rounds need to load.
        /// </summary>
        public readonly int AmmoToLoadTotal;

        /// <summary>
        /// Has round being loaded as part of empty reload?
        /// </summary>
        public readonly bool IsEmptyReload;

        /// <summary>
        /// Animation speed when round is loaded, it might be result of starting new phase of animations 
        /// or a temporary change in the speed for the feel.
        /// </summary>
        public readonly float? AnimationSpeed;

        public RoundLoadedArgs(int totalRoundsLoadedSoFar, int ammoToLoadTotal, bool isEmptyReload, float? animationSpeed)
        {
            TotalRoundsLoadedSoFar = totalRoundsLoadedSoFar;
            AmmoToLoadTotal = ammoToLoadTotal;
            IsEmptyReload = isEmptyReload;
            AnimationSpeed = animationSpeed;
        }
    }

    public delegate void RoundLoadedDelegate(in RoundLoadedArgs args);

    /// <summary>
    /// Reloads the gun progressively, like revolver, shotguns, or bolt-action rifles.
    /// </summary>
    /// <remarks>
    /// Magazine still refers to how many ammo that a gun at one point held. Think of this
    /// as a way to progressively reloading the magazine.
    /// <br></br><br></br>
    /// We are not using a state machine for the state management, because states are so simple and
    /// state machine would be an overkill for this situation. Because, there is no new states that can be
    /// added to this system as it is a standard.
    /// </remarks>
    [Serializable]
    public sealed class ProgressiveReloadProcessor : IReloadProcessor
    {
        private enum ReloadState
        {
            Inactive = 0,
            
            /// <summary>
            /// Used when the chamber empty and we start empty chamber reload, 
            /// this might be used for guns that might have different reload begin animation
            /// when the chamber is empty (e.g Shotgun's first shell insert animation when the chamber is empty).
            /// Then it will continue with 'Begin'.
            /// </summary>
            EmptyReloadBegin = 1,

            /// <summary>
            /// Begin state of the sequence, completes and goes to 'Loop'.
            /// </summary>
            Begin = 2,

            /// <summary>
            /// Inserts shells one by one to the chamber, if cancelled or finished goes to 'End'.
            /// </summary>
            Loop = 3,

            /// <summary>
            /// Ending animation, like cocking the shotgun after animation is finished.
            /// </summary>
            End = 4,
        }

        [SerializeField]
        private ProgressiveReloadConfig _reloadConfig;

        private IGunAmmoStorage _ammoStorage;

        /// <summary>
        /// Amount of ammo to be loaded from start to end of the animation.
        /// </summary>
        private int _ammoToLoadTotal;

        /// <summary>
        /// Rounds loaded so far.
        /// </summary>
        private int _roundsLoaded;
        private int _magazineCapacity;

        private ReloadState _currentReloadState;

        /// <summary>
        /// When the current phase ends (begin, empty begin, loop, or end).
        /// </summary>
        private float _phaseEndTime;

        /// <summary>
        /// Cancel call given?
        /// </summary>
        private bool _isCanceling;

        public bool IsReloading { get; private set; }
        public int CurrentAmmo { get; private set; } = -1;

        public float ReloadEndDuration => IsReloading ? _reloadConfig.ReloadEndDuration + _reloadConfig.ReloadSingleLoopDuration : 0f;

        public event ReloadStartedDelegate ReloadStarted;
        public event RoundLoadedDelegate RoundLoaded;
        public event UnityAction ReloadCompleted;
        public event UnityAction ReloadCanceled;
        public event UnityAction<float> ReloadEndPhaseStarted;

        public void Start(IGunAmmoStorage ammoStorage, int currentAmmo, int capacity)
        {
            if(IsReloading || currentAmmo >= capacity)
            {
                return;
            }

            _ammoToLoadTotal = CalculateAmmoToLoad(ammoStorage, currentAmmo, capacity);
            if(_ammoToLoadTotal <= 0f)
            {
                return;
            }

            PrepareForReload(ammoStorage, currentAmmo, capacity);

            if(_reloadConfig.HasEmptyReload && CurrentAmmo == 0)
            {
                TransitionTo(ReloadState.EmptyReloadBegin);
            }
            else
            {
                TransitionTo(ReloadState.Begin);
            }
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

        private void PrepareForReload(IGunAmmoStorage ammoStorage, int currentAmmo, int capacity)
        {
            _ammoStorage = ammoStorage;
            CurrentAmmo = currentAmmo;
            _magazineCapacity = capacity;
            _roundsLoaded = 0;
            _isCanceling = false;

            IsReloading = true;
        }

        private void TransitionTo(ReloadState newState)
        {
            _currentReloadState = newState;
            float duration = 0f;

            switch (newState)
            {
                case ReloadState.EmptyReloadBegin:
                    duration = _reloadConfig.EmptyReloadBeginDuration;
                    var emtptyReloadStartArgs = new ReloadStartEventArgs(true, duration, _reloadConfig.EmptyReloadAnimationSpeed, _ammoToLoadTotal);
                    ReloadStarted?.Invoke(in emtptyReloadStartArgs);
                    break;
                case ReloadState.Begin:
                    duration = _reloadConfig.ReloadBeginDuration;
                    var reloadStartArgs = new ReloadStartEventArgs(false, duration, _reloadConfig.ReloadBeginAnimationSpeed, _ammoToLoadTotal);
                    ReloadStarted?.Invoke(in reloadStartArgs);
                    break;
                case ReloadState.Loop:
                    duration = _reloadConfig.ReloadSingleLoopDuration;
                    break;
                case ReloadState.End:
                    ReloadEndPhaseStarted?.Invoke(_reloadConfig.ReloadEndAnimationSpeed);
                    duration = _reloadConfig.ReloadEndDuration;
                    break;
            }

            _phaseEndTime = Time.time + duration;
        }

        public void Update(float deltaTime)
        {
            if(IsReloading == false || Time.time < _phaseEndTime)
            {
                return;
            }

            switch (_currentReloadState)
            {
                case ReloadState.EmptyReloadBegin:
                    LoadOneRound(true);
                    TransitionTo(ReloadState.Begin);
                    break;
                case ReloadState.Begin:
                    RoundLoaded?.Invoke(new RoundLoadedArgs(0, 0, true, _reloadConfig.ReloadLoopAnimationSpeed));
                    TransitionTo(ReloadState.Loop);
                    break;
                case ReloadState.Loop:
                    HandleLoopPhase();
                    break;
                case ReloadState.End:
                    EndReload();
                    break;
            }
        }

        /// <summary>
        /// Loads one round into the chamber and ends the animation if being cancelled or finished loading rounds.
        /// </summary>
        private void HandleLoopPhase()
        {
            LoadOneRound(false);

            if(_isCanceling || CurrentAmmo >= _magazineCapacity || _roundsLoaded >= _ammoToLoadTotal)
            {
                TransitionTo(ReloadState.End);
            }
            else
            {
                TransitionTo(ReloadState.Loop);
            }
        }

        /// <summary>
        /// Loads one round into the chamber.
        /// </summary>
        private void LoadOneRound(bool isEmptyReload)
        {
            _ammoStorage.TryRemoveAmmo(1);
            CurrentAmmo++;
            _roundsLoaded++;

            var args = new RoundLoadedArgs(_roundsLoaded, _ammoToLoadTotal, isEmptyReload, 
                isEmptyReload ? null : _reloadConfig.ReloadLoopAnimationSpeed);
            RoundLoaded?.Invoke(in args);
        }

        private void EndReload()
        {
            IsReloading = false;
            ReloadCompleted?.Invoke();
            TransitionTo(ReloadState.Inactive);
        }

        public void Cancel(float transitionSpeed)
        {
            // In the case of force end, we abandon the process (just like in the case of weapon drop)
            if(transitionSpeed >= HandheldAnimationConstants.MaximumHolsteringSpeed)
            {
                TransitionTo(ReloadState.Inactive);
                IsReloading = false;
                ReloadCanceled?.Invoke();
                return;
            }

            if(IsReloading == false || _isCanceling)
            {
                return;
            }

            _isCanceling = true;
            ReloadCanceled?.Invoke();

            if(_currentReloadState is ReloadState.Loop or ReloadState.Begin)
            {
                TransitionTo(ReloadState.End);
            }
        }

        public void SetAmmo(int newAmmo) => CurrentAmmo = newAmmo;

        [Serializable]
        private sealed class ProgressiveReloadConfig
        {
            [Tooltip("Time it takes for the 'begin' part of the reload animation to complete.")]
            [SerializeField, Range(0.1f, 20f)]
            private float _reloadBeginDuration = 2f;

            [Tooltip("Speed of the 'begin' part of the reload animation.")]
            [SerializeField, Range(0.01f, 10f)]
            private float _reloadBeginAnimationSpeed = 1f;

            [Tooltip("Time it takes for a single loop to complete (like pushing single shell when reloading shotgun).")]
            [SerializeField, Range(0.1f, 20f), Line]
            private float _reloadSingleLoopDuration = 0.4f;

            [Tooltip("Speed of the 'loop' part of the reload animation.")]
            [SerializeField, Range(0.1f, 20f)]
            private float _reloadLoopAnimationSpeed = 2f;

            [Tooltip("Time it takes for the 'end' part of the reload animation to complete.")]
            [SerializeField, Range(0.1f, 20f), Line]
            private float _reloadEndDuration = 2f;

            [Tooltip("Speed of the 'end' part of the reload animation.")]
            [SerializeField, Range(0.01f, 10f)]
            private float _reloadEndAnimationSpeed = 1f;

            [Tooltip("Denotes if it can perform empty reload.")]
            [SerializeField]
            private bool _hasEmptyReload;

            [Tooltip("Time it takes for the 'begin' part of the 'Empty' magazine reload process to complete.")]
            [ShowIf(nameof(_hasEmptyReload), true)]
            [SerializeField, Range(0.1f, 30f)]
            private float _emptyReloadBeginDuration = 5f;

            [Tooltip("Speed of the 'Empty' magazine reload animation.")]
            [ShowIf(nameof(_hasEmptyReload), true)]
            [SerializeField, Range(0.01f, 10f)]
            private float _emptyReloadAnimationSpeed = 1f;

            /// <summary>
            /// Time it takes for the 'begin' part of the reload animation to complete.
            /// </summary>
            public float ReloadBeginDuration => _reloadBeginDuration / _reloadBeginAnimationSpeed;

            /// <summary>
            /// Speed of the 'begin' part of the reload animation.
            /// </summary>
            public float ReloadBeginAnimationSpeed => _reloadBeginAnimationSpeed;

            /// <summary>
            /// Time it takes for a single loop to complete (like pushing single shell when reloading shotgun).
            /// </summary>
            public float ReloadSingleLoopDuration => _reloadSingleLoopDuration / _reloadLoopAnimationSpeed;

            /// <summary>
            /// Speed of the 'loop' part of the reload animation.
            /// </summary>
            public float ReloadLoopAnimationSpeed => _reloadLoopAnimationSpeed;

            /// <summary>
            /// Time it takes for the 'end' part of the reload animation to complete.
            /// </summary>
            public float ReloadEndDuration => _reloadEndDuration / _reloadEndAnimationSpeed;

            /// <summary>
            /// Speed of the 'end' part of the reload animation.
            /// </summary>
            public float ReloadEndAnimationSpeed => _reloadEndAnimationSpeed;

            /// <summary>
            /// Denotes if it can perform empty reload.
            /// </summary>
            public bool HasEmptyReload => _hasEmptyReload;

            /// <summary>
            /// Time it takes for the 'begin' part of the 'Empty' magazine reload process to complete.
            /// </summary>
            public float EmptyReloadBeginDuration => _emptyReloadBeginDuration / _emptyReloadAnimationSpeed;

            /// <summary>
            /// Speed of the 'Empty' magazine reload animation.
            /// </summary>
            public float EmptyReloadAnimationSpeed => _emptyReloadAnimationSpeed;
        }
    }
}