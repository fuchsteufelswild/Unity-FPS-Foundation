using UnityEngine;
using UnityEngine.Events;


namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public sealed class GunAccuracySystem
    {
        private const float RecoilRecoveryMultiplier = 5f;

        private IAccuracyController _accuracyController = new NoOpAccuracyController();
        private GunComponentManager _components;

        private float _hipAccuracyModifier;

        private float _recoilRecoveryCooldownTimer;
        private float _accuracyPenalty;
        private float _baseAccuracy = 1f;
        private float _recoilProgress;

        /// <summary>
        /// Called when the heat of the gun is changed.
        /// </summary>
        public event GunHeatChangedDelegate GunHeatChanged;

        /// <summary>
        /// Called when the heat value is maxed.
        /// </summary>
        public event UnityAction GunHeatMaxed;

        public float CurrentAccuracy { get; private set; }
        public float RecoilProgress => _recoilProgress;

        public GunAccuracySystem(GunComponentManager components, float hipAccuracyModifier)
        {
            _components = components;
            _hipAccuracyModifier = hipAccuracyModifier;
            _components.OnShoot += OnShoot;
        }

        public void SetCharacter(ICharacter character)
        {
            if (character == null || character.TryGetCC(out _accuracyController) == false)
            {
                _accuracyController = new NoOpAccuracyController();
            }
        }

        private void OnShoot()
        {
            bool isAiming = _components.AimBehaviour.IsAiming;

            float accuracyKick = _components.RecoilMechanism.GetAccuracyKick(isAiming);
            _accuracyPenalty += accuracyKick;

            _recoilRecoveryCooldownTimer = Time.fixedTime + _components.RecoilMechanism.HeatRecoveryDelay;
            _components.RecoilMechanism.Apply(CurrentAccuracy, _recoilProgress, isAiming);

            HandleGunHeat();
        }

        private void HandleGunHeat()
        {
            float previousRecoilProgress = _recoilProgress;

            int magazineCapacity = Mathf.Clamp(_components.Magazine.Capacity, 0, 30);
            _recoilProgress = Mathf.Clamp01(_recoilProgress + 1f / magazineCapacity);

            if(Mathf.Approximately(previousRecoilProgress, _recoilProgress) == false)
            {
                GunHeatChanged?.Invoke(previousRecoilProgress, _recoilProgress);
            }

            if(previousRecoilProgress < 1f && _recoilProgress >= 1f)
            {
                GunHeatMaxed?.Invoke();
            }
        }

        public void UpdateAccuracy(float deltaTime, float currentTime)
        {
            UpdateBaseAccuracy(deltaTime);
            UpdateRecoilRecovery(deltaTime, currentTime);
        }

        private void UpdateBaseAccuracy(float deltaTime)
        {
            bool isAiming = _components.AimBehaviour.IsAiming;
            CalculateBaseAccuracy(isAiming);

            float targetAccuracy = Mathf.Clamp01(_baseAccuracy - _accuracyPenalty);
            CurrentAccuracy = targetAccuracy;

            UpdateAccuracyPenalty(deltaTime, isAiming);
        }

        private void CalculateBaseAccuracy(bool isAiming)
        {
            float accuracyModifier = isAiming ? _components.AimBehaviour.FireAccuracyModifier : _hipAccuracyModifier;
            _baseAccuracy = _accuracyController.GetAccuracyModifier() * accuracyModifier;
        }

        private void UpdateAccuracyPenalty(float deltaTime, bool isAiming)
        {
            float recoveryRate = _components.RecoilMechanism.GetAccuracyRecoveryRate(isAiming);
            float accuracyRecoverDelta = deltaTime * recoveryRate;
            _accuracyPenalty = Mathf.Clamp01(_accuracyPenalty - accuracyRecoverDelta);
        }

        private void UpdateRecoilRecovery(float deltaTime, float currentTime)
        {
            if(currentTime < _recoilRecoveryCooldownTimer)
            {
                return;
            }

            float previousRecoilProgress = _recoilProgress;

            float recoverDelta = deltaTime * _components.RecoilMechanism.HeatRecoveryRate;
            _recoilProgress = Mathf.Clamp01(_recoilProgress -
                Mathf.Max(recoverDelta * _recoilProgress * RecoilRecoveryMultiplier, recoverDelta));

            GunHeatChanged?.Invoke(previousRecoilProgress, _recoilProgress);
        }
    }
}