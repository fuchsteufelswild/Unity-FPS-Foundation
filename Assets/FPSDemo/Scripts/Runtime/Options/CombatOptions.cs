using Nexora.Options;
using UnityEngine;

namespace Nexora.FPSDemo.Options
{
    [CreateAssetMenu(menuName = CreateAssetMenuPath + "Combat Options", fileName = nameof(CombatOptions))]
    public sealed class CombatOptions : Options<CombatOptions>
    {
        [Title("HUD")]
        [SerializeField]
        private Option<Color> _crosshairColor = new(Color.white);

        [Title("Ammunition")]
        [SerializeField]
        private Option<bool> _unlimitedMagazine = new();

        [SerializeField]
        private Option<bool> _unlimitedReserveAmmo = new();

        [Title("Firing")]
        [SerializeField]
        private Option<bool> _manualShellEjection = new();

        [Title("Aiming")]
        [SerializeField]
        private Option<bool> _aimWhileReloading = new();

        [Title("Reloading")]
        [SerializeField]
        private Option<bool> _cancelReloadOnFire = new();

        [SerializeField]
        private Option<bool> _autoReloadOnEmptyMagazine = new(true);

        public Option<Color> CrosshairColor => _crosshairColor;
        public Option<bool> UnlimitedMagazine => _unlimitedMagazine;
        public Option<bool> UnlimitedReserveAmmo => _unlimitedReserveAmmo;
        public Option<bool> ManualShellEjection => _manualShellEjection;
        public Option<bool> AimWhileReloading => _aimWhileReloading;
        public Option<bool> CancelReloadOnFire => _cancelReloadOnFire;
        public Option<bool> AutoReloadOnEmptyMagazine => _autoReloadOnEmptyMagazine;
    }
}