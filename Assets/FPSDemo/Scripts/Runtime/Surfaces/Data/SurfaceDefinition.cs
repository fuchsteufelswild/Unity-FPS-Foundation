using Nexora.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo.SurfaceSystem
{
    /// <summary>
    /// Flags for what should be shown/played as effect.
    /// </summary>
    [Flags]
    public enum SurfaceEffectFlags
    {
        None = 0,
        Audio = 1 << 0,
        Visual = 1 << 1,
        Decal = 1 << 2,
        AudioVisual = Audio | Visual,
        All = Audio | Visual | Decal
    }

    [Serializable]
    public sealed class SurfaceEffects
    {
        [Tooltip("Visual effect to play.")]
        public SurfaceEffect Visual;

        [Tooltip("Decal to apply.")]
        public SurfaceEffect Decal;

        [Tooltip("Audio to play.")]
        public AudioCue Audio = new(null);
    }

    [CreateAssetMenu(menuName = "Nexora/Surfaces/Create Surface", fileName = "Surface_")]
    public sealed class SurfaceDefinition : Definition<SurfaceDefinition>
    {
        [Tooltip("Speed multiplier affects the object going through that surface.")]
        [SerializeField]
        private float _speedMultiplier = 1f;

        [Tooltip("Friction coefficient")]
        [SerializeField]
        private float _friction = 1f;

        [Tooltip("Resistance to penetration from external objects (e.g bullet).")]
        [SerializeField]
        private float _penetrationResistance = 0.25f;

        [Tooltip("Physics materials defining the collision behaviour.")]
        [SerializeField]
        private PhysicsMaterial[] _physicMaterials = Array.Empty<PhysicsMaterial>();

        /// <summary>
        /// Used only as a editor storage, in runtime it is converted into a dictionary for O(1) lookup.
        /// </summary>
        [Tooltip("Effect mappings defining what surface effect results in different visual effects.")]
        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false), IgnoreParent]
        private EffectMapping[] _effectMappings = Array.Empty<EffectMapping>();

        private Dictionary<SurfaceEffectType, SurfaceEffects> _effectsCache;

        public float SpeedMultiplier => _speedMultiplier;
        public float Friction => _friction;
        public float PenetrationResistance => _penetrationResistance;
        public IReadOnlyList<PhysicsMaterial> PhysicsMaterials => _physicMaterials;

        public bool TryGetEffect(SurfaceEffectType type, out SurfaceEffects effect)
        {
            _effectsCache ??= BuildEffectsCache();
            return _effectsCache.TryGetValue(type, out effect);
        }

        private Dictionary<SurfaceEffectType, SurfaceEffects> BuildEffectsCache()
        {
            _effectsCache = new Dictionary<SurfaceEffectType, SurfaceEffects>();

            foreach(EffectMapping mapping in _effectMappings)
            {
                _effectsCache[mapping.EffectType] = mapping.Effects;
            }

            return _effectsCache;
        }

        [Serializable]
        private struct EffectMapping
        {
            [BeginGroup]
            public SurfaceEffectType EffectType;

            [EndGroup]
            public SurfaceEffects Effects;

            public readonly bool Equals(EffectMapping other) => EffectType == other.EffectType;
            public override readonly bool Equals(object obj) => obj is EffectMapping other && Equals(other);
            public override readonly int GetHashCode() => (int)EffectType;

            public static bool operator ==(EffectMapping left, EffectMapping right) => left.Equals(right);
            public static bool operator !=(EffectMapping left, EffectMapping right) => left.Equals(right) == false;
        }
    }
}