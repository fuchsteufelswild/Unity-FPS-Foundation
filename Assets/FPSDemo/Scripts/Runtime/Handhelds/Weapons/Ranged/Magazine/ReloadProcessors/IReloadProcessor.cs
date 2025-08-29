using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    /// <summary>
    /// Reload processor of a gun, implementations provide different ways to reload the weapon.
    /// </summary>
    public interface IReloadProcessor
    {
        /// <summary>
        /// Is currently reloading?
        /// </summary>
        bool IsReloading { get; }

        /// <summary>
        /// How many ammo currently in the magazine.
        /// </summary>
        int CurrentAmmo { get; }

        /// <summary>
        /// How long takes it for duration to end, to cancel, or to complete after the whole process is finished.
        /// </summary>
        float ReloadEndDuration => 0f;

        /// <summary>
        /// Starts the reloading process.
        /// </summary>
        /// <param name="character">Owner character.</param>
        /// <param name="ammoStorage">Ammo storage used by the gun.</param>
        /// <param name="currentAmmo">Current ammo of the magazine.</param>
        /// <param name="capacity">Maximum ammo that magazine can take.</param>
        void Start(IGunAmmoStorage ammoStorage, int currentAmmo, int capacity);

        /// <summary>
        /// Called by the main handler each frame.
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// Cancels the reload operation.
        /// </summary>
        void Cancel(float transitionSpeed);

        /// <summary>
        /// Sets the ammo in the magazine to <paramref name="newAmmo"/>.
        /// </summary>
        /// <param name="newAmmo">New ammo count in the magazine.</param>
        void SetAmmo(int newAmmo);

        /// <summary>
        /// Called when the reloading process is started.
        /// </summary>
        event ReloadStartedDelegate ReloadStarted;

        /// <summary>
        /// Called when a round is loaded (like loading a shotgun round).
        /// </summary>
        event RoundLoadedDelegate RoundLoaded;

        /// <summary>
        /// Called when the reloading process is completed.
        /// </summary>
        event UnityAction ReloadCompleted;

        /// <summary>
        /// Called when the reloading process is canceled (stopped prematurely).
        /// </summary>
        event UnityAction ReloadCanceled;

        /// <summary>
        /// Called when the ending phase is started, like all finished but needs time to be able to move or use gun again.
        /// </summary>
        event UnityAction<float> ReloadEndPhaseStarted;
    }

    [Serializable]
    public sealed class NullReloadProcessor : IReloadProcessor
    {
        public bool IsReloading => false;
        public int CurrentAmmo => 0;

        public event UnityAction<float> ReloadEndPhaseStarted { add { } remove { } }
        public event ReloadStartedDelegate ReloadStarted { add { } remove { } }
        public event RoundLoadedDelegate RoundLoaded { add { } remove { } }
        public event UnityAction ReloadCompleted { add { } remove { } }
        public event UnityAction ReloadCanceled { add { } remove { } }

        public void Cancel(float transitionSpeed) { }

        public void SetAmmo(int newAmmo) { }
        public void Start(IGunAmmoStorage ammoStorage, int currentAmmo, int capacity) { }
        public void Update(float deltaTime) { }
    }
}
