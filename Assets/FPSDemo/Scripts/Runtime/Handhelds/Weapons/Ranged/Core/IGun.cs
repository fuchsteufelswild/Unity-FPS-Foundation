using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGun : 
        IMonoBehaviour,
        IGunInputManager,
        IGunComponentManager
    {
        bool IsAiming => AimBehaviour.IsAiming;
        bool IsReloading => Magazine.IsReloading;
        bool IsUsing => TriggerMechanism.IsTriggerHeld;
    }

    /// <summary>
    /// Represents different behaviour types that can be a part of <see cref="IGun"/>.
    /// </summary>
    public enum GunBehaviourType
    {
        /// <summary>
        /// Handles aiming functionality and sight alignment.
        /// </summary>
        AimingSystem,

        /// <summary>
        /// Manages the trigger pull detection and firing initiation.
        /// </summary>
        TriggerMechanism,

        /// <summary>
        /// Controls the core firing process and projectile launch.
        /// </summary>
        FiringMechanism,

        /// <summary>
        /// Manages ammunition supply.
        /// </summary>
        AmmunitionSystem,

        /// <summary>
        /// Handles magazine operations and reloading procedures.
        /// </summary>
        MagazineSystem,

        /// <summary>
        /// Controls recoil behaviour and weapon kick.
        /// </summary>
        RecoilSystem,

        /// <summary>
        /// Manages effects when projectiles hit targets.
        /// </summary>
        ImpactSystem,

        /// <summary>
        /// Provides feedback when firing with empty chamber.
        /// </summary>
        DryFireEffect,

        /// <summary>
        /// Manages spent cartrige ejection.
        /// </summary>
        EjectionSystem,

        /// <summary>
        /// Handles muzzle and barrel related effects.
        /// </summary>
        MuzzleEffect,

        /// <summary>
        /// Changes visualization of cartridges (Can be used for Revolver, Crossbow, Flare etc. which changes visuals in 
        /// the cartridge chambers).
        /// </summary>
        CartridgeVisualizer,
    }
}