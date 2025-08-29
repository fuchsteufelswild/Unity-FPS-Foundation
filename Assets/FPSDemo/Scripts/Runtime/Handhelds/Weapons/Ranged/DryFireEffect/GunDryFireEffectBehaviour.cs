using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunDryFireEffectBehaviour : IGunBehaviour
    {
        /// <summary>
        /// Triggers dry fire effect.
        /// </summary>
        void TriggerDryFireEffect();
    }

    public sealed class NullDryFireEffect : IGunDryFireEffectBehaviour
    {
        public static readonly NullDryFireEffect Instance = new();

        public void Attach() { }
        public void Detach() { }
        public void TriggerDryFireEffect() { }
    }

    public sealed class GunDryFireEffectBehaviour :
        GunBehaviour,
        IGunDryFireEffectBehaviour
    {
        [Tooltip("Effects played when dry fire is triggered.")]
        [ReorderableList(ElementLabel = "Effect")]
        [ReferencePicker(typeof(DryFireEffect), TypeGrouping = TypeGrouping.ByFlatName)]
        [SerializeReference]
        private DryFireEffect[] _effects = Array.Empty<DryFireEffect>();

        public void TriggerDryFireEffect()
        {
            foreach(var effect in _effects)
            {
                effect.Trigger();
            }
        }

        private void OnEnable()
        {
            if(Gun != null)
            {
                Gun.DryFireEffect = this;
                foreach(var effect in _effects)
                {
                    effect.Initialize(Gun);
                }
            }
        }
    }
}