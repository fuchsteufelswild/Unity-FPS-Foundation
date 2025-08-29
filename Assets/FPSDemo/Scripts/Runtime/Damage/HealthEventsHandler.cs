using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo
{
    [Serializable]
    public class HealthEventsHandler : IDisposable
    {
        [Tooltip("Event invoked when listened health controller is healed (restores health).")]
        [SerializeField]
        private UnityEvent<float> OnHealthRestored;

        [Tooltip("Event invoked when listened health controller receives damage (health is reduced).")]
        [SerializeField]
        private UnityEvent<float, DamageContext> OnDamageReceived;

        [Tooltip("Event invoked when listened health controller respawns.")]
        [SerializeField]
        private UnityEvent OnRespawn;

        [Tooltip("Event invoked when listened health controller dies.")]
        [SerializeField]
        private UnityEvent<DamageContext> OnDeath;

        protected bool _isDisposed;

        private IHealthController _healthController;

        public virtual void Initialize(IHealthController healthController)
        {
            _healthController = healthController ?? throw new ArgumentNullException(nameof(healthController));

            _healthController.Death += OnControllerDied;
            _healthController.Respawned += OnControllerRespawned;
            _healthController.DamageReceived += OnControllerReceivedDamage;
            _healthController.HealthRestored += OnControllerRestoredHealth;
        }

        private void OnControllerReceivedDamage(float damage, in DamageContext damageContext)
        {
            OnDamageReceived?.Invoke(damage, damageContext);
            OnControllerReceivedDamageInternal(damage, in damageContext);
        }

        private void OnControllerRestoredHealth(float healAmount)
        {
            OnHealthRestored?.Invoke(healAmount);
            OnControllerRestoredHealthInternal(healAmount);
        }

        private void OnControllerRespawned()
        {
            OnRespawn?.Invoke();
            OnControllerRespawnedInternal();
        }

        private void OnControllerDied(in DamageContext damageContext)
        {
            OnDeath?.Invoke(damageContext);
            OnControllerDiedInternal(in damageContext);
        }

        protected virtual void OnControllerReceivedDamageInternal(float damage, in DamageContext damageContext) { }
        protected virtual void OnControllerRestoredHealthInternal(float healAmount) { }
        protected virtual void OnControllerRespawnedInternal() { }
        protected virtual void OnControllerDiedInternal(in DamageContext damageContext) { }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_healthController != null)
            {
                _healthController.Death -= OnControllerDied;
                _healthController.Respawned -= OnControllerRespawned;
                _healthController.DamageReceived -= OnControllerReceivedDamage;
                _healthController.HealthRestored -= OnControllerRestoredHealth;
            }

            _isDisposed = true;
        }
    }
}
