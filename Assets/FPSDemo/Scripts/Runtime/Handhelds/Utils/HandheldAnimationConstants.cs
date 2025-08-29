using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    public static class HandheldAnimationConstants
    {
        public static int Equip = Animator.StringToHash("Equip");
        public static int Holster = Animator.StringToHash("Holster");
        public static int EquipSpeed = Animator.StringToHash("Equip Speed");
        public static int HolsterSpeed = Animator.StringToHash("Holster Speed");

        public static int ChangeMode = Animator.StringToHash("Change Mode");

        public static int IsAiming = Animator.StringToHash("Is Aiming");

        public static int IsEmpty = Animator.StringToHash("Is Empty"); // No ammo in the chamber
        public static int Shoot = Animator.StringToHash("Shoot");

        public static int Eject = Animator.StringToHash("Eject");

        public static int IsEmptyReload = Animator.StringToHash("Is Empty");
        public static int IsReloading = Animator.StringToHash("Is Reloading");

        public static int ReloadSpeed = Animator.StringToHash("Reload Speed");

        public static int IsCharging = Animator.StringToHash("Is Charging");
        public static int FullCharge = Animator.StringToHash("Full Charge");

        public const float MinimumHolsteringSpeed = 0.5f;
        public const float MaximumHolsteringSpeed = 5f;
    }
}