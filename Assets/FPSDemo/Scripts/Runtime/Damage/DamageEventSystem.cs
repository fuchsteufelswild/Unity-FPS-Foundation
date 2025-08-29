using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo
{
    public delegate void DamageDealtCallback(IDamageReceiver target, DamageOutcomeType damageOutcomeType, float damageDealt, in DamageContext context);

    public static class DamageEventSystem
    {
        private static readonly Dictionary<IDamageSource, DamageDealtCallback> _sourceCallbacks = new();

        public static void SubscribeSource(IDamageSource damageSource) => _sourceCallbacks.Add(damageSource, null);
        public static void UnsubscribeSource(IDamageSource damageSource) => _sourceCallbacks.Remove(damageSource);

        public static void BroadcastDamage(IDamageReceiver target, DamageOutcomeType damageOutcomeType, 
            float damageDealt, in DamageContext context)
        {
            _sourceCallbacks.TryGetValue(context.Source, out var callback);
            callback?.Invoke(target, damageOutcomeType, damageDealt, context);
        }

        /// <summary>
        /// Subscribes to damage, that is this <paramref name="callback"/> will be called as well when any
        /// damage is dealth to the <paramref name="damageSource"/>.
        /// </summary>
        /// <param name="damageSource">Source to register <paramref name="callback"/> to.</param>
        public static void SubscribeToDamage(IDamageSource damageSource, DamageDealtCallback callback)
        {
            _sourceCallbacks.TryGetValue(damageSource, out var existing);
            existing += callback;
            _sourceCallbacks[damageSource] = existing;
        }

        public static void UnsubscribeFromDamage(IDamageSource damageSource, DamageDealtCallback callback)
        {
            _sourceCallbacks.TryGetValue(damageSource, out var existing);
            existing -= callback;
            _sourceCallbacks[damageSource] = existing;
        }
    }
}