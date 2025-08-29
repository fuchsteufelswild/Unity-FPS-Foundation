using Nexora.ObjectPooling;
using Nexora.SaveSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    public sealed class HealthController :
        MonoBehaviour,
        IHealthController,
        ISaveableComponent,
        IUnityObjectPoolListener
    {
        [Tooltip("Initial health, used when it is initialized.")]
#if UNITY_EDITOR
        [DynamicRange(nameof(GetMinHealth), nameof(GetMaxHealth))]
#endif
        [SerializeField]
        private float _health = 1f;

        [Tooltip("Initial max health value, it can be changed in runtime to be higher or less.")]
#if UNITY_EDITOR
        [OnValueChanged(nameof(ValidateHealth))]
#endif
        [SerializeField, Range(1f, 1000f)]
        private float _maxHealth = 5f;

        public float CurrentHealth => _health;
        public float MaxHealth => _maxHealth;

        public event HealthRestoredDelegate HealthRestored;
        public event DamageReceivedDelegate DamageReceived;
        public event DeathDelegate Death;
        public event UnityAction Respawned;

        public float Heal(float healAmount)
        {
            bool wasAlive = this.IsAlive();
            float effectiveHealing = Mathf.Abs(healAmount);

            if(TryApplyHealthChange(ref effectiveHealing) == false)
            {
                return 0f;
            }

            HealthRestored?.Invoke(effectiveHealing);

            if(wasAlive == false && this.IsAlive())
            {
                Respawned?.Invoke();
            }

            return effectiveHealing;
        }

        private bool TryApplyHealthChange(ref float delta)
        {
            if(Mathf.Abs(delta) < HealthExtensions.HealthZeroThreshold)
            {
                return false;
            }

            float previousHealth = _health;
            _health = Mathf.Clamp(_health + delta, 0f, _maxHealth);
            delta = _health - previousHealth;
            return true;
        }

        public float TakeDamage(float damageAmount) => TakeDamage(damageAmount, DamageContext.Empty);

        public float TakeDamage(float damageAmount, in DamageContext context)
        {
            if(this.IsAlive() == false)
            {
                return 0f;
            }

            float effectiveDamage = -Mathf.Abs(damageAmount);
            if(TryApplyHealthChange(ref effectiveDamage) == false)
            {
                return 0f;
            }

            float absoluteDamage = Mathf.Abs(effectiveDamage);
            DamageReceived?.Invoke(absoluteDamage, in context);

            if (this.IsAlive() == false)
            {
                Death?.Invoke(in context);
            }

            return absoluteDamage;
        }

        public void OnAcquired() => Heal(_maxHealth);
        public void OnPreAcquired() { }
        public void OnPreReleased() { }
        public void OnReleased() { }

        #region Saving
        private sealed class SaveData
        {
            public float Health;
            public float MaxHealth;
        }

        public void Load(object data)
        {
            SaveData save = (SaveData)data;
            _maxHealth = save.MaxHealth;
            _health = save.Health;
        }

        public object Save()
        {
            return new SaveData
            {
                MaxHealth = _maxHealth,
                Health = _health
            };
        }
        #endregion

#if UNITY_EDITOR
        protected void ValidateHealth() => _health = Mathf.Min(_maxHealth, _health);
        protected float GetMaxHealth() => _maxHealth;
        protected float GetMinHealth() => 0f;
#endif
    }
}