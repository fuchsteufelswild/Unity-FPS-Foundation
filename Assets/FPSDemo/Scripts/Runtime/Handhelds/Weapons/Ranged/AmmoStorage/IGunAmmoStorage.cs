using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunAmmoStorage : IGunBehaviour
    {
        /// <summary>
        /// Current available ammo.
        /// </summary>
        int CurrentAmmo { get; }

        /// <summary>
        /// Called when ammo count in the storage changes.
        /// </summary>
        event UnityAction<int> AmmoCountChanged;

        /// <summary>
        /// Tries to remove <paramref name="amount"/> ammo from the storage.
        /// </summary>
        /// <param name="amount">Ammo amount to remove.</param>
        /// <returns>Actual amount of ammo removed from the storage.</returns>
        int TryRemoveAmmo(int amount);

        /// <summary>
        /// Tries to add <paramref name="amount"/> ammo into the storage.
        /// </summary>
        /// <param name="amount">Ammo amount to add.</param>
        /// <returns>Actual amount of ammo added to the storage.</returns>
        int AddAmmo(int amount);
    }

    public sealed class InfiniteAmmoStorage : IGunAmmoStorage
    {
        public static readonly InfiniteAmmoStorage Instance = new InfiniteAmmoStorage();

        public int CurrentAmmo => int.MaxValue;

        public event UnityAction<int> AmmoCountChanged { add { } remove { } }

        public int AddAmmo(int amount) => amount;
        public int TryRemoveAmmo(int amount) => amount;

        public void Attach() { }
        public void Detach() { }
    }

    public abstract class GunAmmoStorageBehaviour :
        GunBehaviour,
        IGunAmmoStorage
    {
        public abstract int CurrentAmmo { get; }

        public abstract event UnityAction<int> AmmoCountChanged;

        public abstract int AddAmmo(int amount);
        public abstract int TryRemoveAmmo(int amount);

        protected virtual void OnEnable()
        {
            if(Gun != null)
            {
                Gun.AmmoStorage = this;
            }
        }
    }
}