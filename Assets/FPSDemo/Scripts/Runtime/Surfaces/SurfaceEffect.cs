using System.Diagnostics;
using UnityEngine;

namespace Nexora.FPSDemo.SurfaceSystem
{
    /// <summary>
    /// Represents a visual particle surface effect.
    /// </summary>
    public sealed class SurfaceEffect : MonoBehaviour
    {
        [SerializeField, ReorderableList(HasLabels = false)]
        private ParticleSystem[] _particleSystems;

        private Transform _cachedTransform;

        private void Awake() => _cachedTransform = transform;

        /// <summary>
        /// Plays the effect at given <paramref name="position"/> with given <paramref name="rotation"/>
        /// as a child of <paramref name="parent"/>(if it is not <see langword="null"/> else just plays at the given location).
        /// </summary>
        /// <param name="position">Target position.</param>
        /// <param name="rotation">Target rotation.</param>
        /// <param name="parent">To be set as child of.</param>
        public void PlayEffect(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if(parent != null)
            {
                _cachedTransform.SetParent(parent);
            }

            _cachedTransform.SetPositionAndRotation(position, rotation);

            foreach(var particleSystem in _particleSystems)
            {
                particleSystem.Play(false);
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void OnValidate() => _particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    public enum SurfaceEffectType
    {
        /// <summary>
        /// Default generic impact if the type doesn't match any other.
        /// </summary>
        GenericImpact = 0,

        /// <summary>
        /// Default effect produced as a result of walking on the surface.
        /// </summary>
        WalkFootstep = 1,

        /// <summary>
        /// Default effect produced as a result of running on the surface.
        /// </summary>
        RunFootstep = 2,

        /// <summary>
        /// Effect of starting to jump (lunge from the ground).
        /// </summary>
        JumpStart = 3,

        /// <summary>
        /// Effect of falling of character or any other object to the surface light.
        /// </summary>
        LightFallImpact = 4,

        /// <summary>
        /// Effect of falling of character or any other object to the surface medium.
        /// </summary>
        MediumFallImpact = 5,

        /// <summary>
        /// Effect of falling of character or any other object to the surface hard.
        /// </summary>
        HardFallImpact = 6,

        /// <summary>
        /// Interaction with wet surfaces, like running on a water pond.
        /// </summary>
        WaterSplash = 7,

        /// <summary>
        /// Interaction with muddy surface, like running on a mud pond.
        /// </summary>
        MudSplash = 8,

        /// <summary>
        /// Shattering, like glass or place where something might be broken.
        /// </summary>
        Shatter = 9,

        /// <summary>
        /// Bullet impacts on the surface.
        /// </summary>
        BulletImpact = 10,
    }
}