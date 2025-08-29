using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo
{
    [Serializable]
    public delegate void DamageReceivedDelegate(float damage, in DamageContext context);

    [Serializable]
    public delegate void DeathDelegate(in DamageContext context);

    [Serializable]
    public delegate void HealthRestoredDelegate(float healAmount);

    public interface IRespawnable
    {
        /// <summary>
        /// Called when respawn happened.
        /// </summary>
        event UnityAction Respawned;
    }

    public interface IHealthController :
        IRespawnable
    {
        /// <summary>
        /// Current health.
        /// </summary>
        float CurrentHealth { get; }

        /// <summary>
        /// Maximum health that can be set.
        /// </summary>
        float MaxHealth { get; }

        /// <summary>
        /// Called when some amount of health is restored.
        /// </summary>
        event HealthRestoredDelegate HealthRestored;

        /// <summary>
        /// Called when some damage is received.
        /// </summary>
        event DamageReceivedDelegate DamageReceived;

        /// <summary>
        /// Called when the controller is dead.
        /// </summary>
        event DeathDelegate Death;

        /// <summary>
        /// Tries to heal <paramref name="healAmount"/>.
        /// </summary>
        /// <param name="healAmount">How much health is being tried to restore.</param>
        /// <returns>Actual amount healed.</returns>
        float Heal(float healAmount);

        /// <summary>
        /// Processes <paramref name="damageAmount"/> damage on itself.
        /// </summary>
        /// <param name="damageAmount">Damage to be done.</param>
        /// <returns>Actual amount of damage taken.</returns>
        float TakeDamage(float damageAmount);

        /// <summary>
        /// Processes <paramref name="damageAmount"/> damage on itself.
        /// </summary>
        /// <param name="damageAmount">Damage to be done.</param>
        /// <param name="context">Extra context about the damage applied.</param>
        /// <returns>Actual amount of damage taken.</returns>
        float TakeDamage(float damageAmount, in DamageContext context);
    }

    public static class HealthExtensions
    {
        /// <summary>
        /// Denotes a value a little more than zero, that represents being zero health.
        /// </summary>
        public const float HealthZeroThreshold = 0.001f;

        public static bool IsAtFullHealth(this IHealthController health)
            => Mathf.Abs(health.MaxHealth - health.CurrentHealth) < HealthZeroThreshold;

        public static bool IsAlive(this IHealthController health) => health.CurrentHealth >= HealthZeroThreshold;
        public static bool IsDead(this IHealthController health) => health.CurrentHealth < HealthZeroThreshold;
        public static void SetMaxHeal(this IHealthController health) => health.Heal(health.MaxHealth);

        public static DamageOutcomeType EvaluateDamageResult(this IHealthController health, bool isCriticalHit)
        {
            return health.IsAlive()
                ? isCriticalHit ? DamageOutcomeType.Critical : DamageOutcomeType.Normal
                : DamageOutcomeType.Fatal;
        }
    }

    public sealed class NullHealthController : IHealthController
    {
        public static readonly NullHealthController Instance = new NullHealthController();

        public float CurrentHealth => 100f;
        public float MaxHealth { get => 100f; set { } }

        public event HealthRestoredDelegate HealthRestored { add { } remove { } }
        public event DeathDelegate Death { add { } remove { } }
        public event DamageReceivedDelegate DamageReceived { add { } remove { } }
        public event UnityAction Respawned { add { } remove { } }

        public float Heal(float healAmount) => 0f;
        public float TakeDamage(float damageAmount) => 0f;
        public float TakeDamage(float damageAmount, in DamageContext context) => 0f;
    }
}