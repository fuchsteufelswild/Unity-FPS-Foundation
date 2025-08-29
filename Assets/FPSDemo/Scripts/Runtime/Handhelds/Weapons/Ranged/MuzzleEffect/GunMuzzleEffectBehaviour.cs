using System;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunMuzzleEffectBehaviour : IGunBehaviour
    {
        /// <summary>
        /// Triggers the start of the muzzle effect.
        /// </summary>
        void TriggerFireEffect();

        /// <summary>
        /// Triggers the the muzzle effect that will be played when .
        /// </summary>
        void TriggerStopFireEffect();
    }

    public sealed class NullMuzzleEffect : IGunMuzzleEffectBehaviour
    {
        public static readonly NullMuzzleEffect Instance = new();

        public void Attach() { }
        public void Detach() { }
        public void TriggerFireEffect() { }
        public void TriggerStopFireEffect() { }
    }

    public sealed class GunMuzzleEffectBehaviour : 
        GunBehaviour, 
        IGunMuzzleEffectBehaviour
    {
        [Tooltip("Effects played when firing starts.")]
        [ReorderableList(ElementLabel = "Effect")]
        [ReferencePicker(typeof(MuzzleEffect), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private MuzzleEffect[] _onFireEffects = Array.Empty<MuzzleEffect>();

        [SpaceArea]
        [Tooltip("Effects played when firing stops.")]
        [ReorderableList(ElementLabel = "Effect")]
        [ReferencePicker(typeof(MuzzleEffect), TypeGrouping = TypeGrouping.ByFlatName)]
#if UNITY_EDITOR
        [EditorButton(nameof(RefreshReferences))]
#endif
        [SerializeReference]
        private MuzzleEffect[] _onFireStopEffects = Array.Empty<MuzzleEffect>();

#if UNITY_EDITOR
        private void RefreshReferences()
        {
            foreach (var effect in _onFireEffects)
            {
                effect.RefreshReferences(transform);
            }

            foreach (var effect in _onFireStopEffects)
            {
                effect.RefreshReferences(transform);
            }
        }
#endif

        public void TriggerFireEffect()
        {
            foreach(var effect in _onFireEffects)
            {
                effect.Trigger();
            }
        }

        public void TriggerStopFireEffect()
        {
            foreach (var effect in _onFireStopEffects)
            {
                effect.Trigger();
            }
        }

        private void OnEnable()
        {
            if(Gun != null)
            {
                Gun.MuzzleEffect = this;
                foreach (var effect in _onFireEffects)
                {
                    effect.Initialize(Gun);
                }

                foreach (var effect in _onFireStopEffects)
                {
                    effect.Initialize(Gun);
                }
            }
        }
    }
}