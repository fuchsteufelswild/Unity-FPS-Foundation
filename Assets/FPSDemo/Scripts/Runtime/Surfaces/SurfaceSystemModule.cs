using Nexora.Audio;
using Nexora.ObjectPooling;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nexora.FPSDemo.SurfaceSystem
{
    public sealed class FPSDemoPoolCategory : PoolCategory
    {
        public static readonly PoolCategory SurfaceVisuals = new FPSDemoPoolCategory("SurfaceEffects", HashCode.Combine("SurfaceEffects"));
        public static readonly PoolCategory SurfaceDecals = new FPSDemoPoolCategory("SurfaceDecals", HashCode.Combine("SurfaceDecals"));

        public static readonly PoolCategory Shells = new FPSDemoPoolCategory("Shells", HashCode.Combine("Shells"));
        public static readonly PoolCategory Projectiles = new FPSDemoPoolCategory("Projectiles", HashCode.Combine("Projectiles"));

        public FPSDemoPoolCategory(string name, int hash) : base(name, hash)
        {
        }
    }

    [DefaultExecutionOrder(ExecutionOrder.GameModule)]
    [CreateAssetMenu(menuName = CreateMenuPath + nameof(SurfaceSystemModule), fileName = nameof(SurfaceSystemModule))]
    public sealed class SurfaceSystemModule : GameModule<SurfaceSystemModule>
    {
        [Tooltip("Default surface is used when no other surface is found.")]
        [SerializeField]
        private SurfaceDefinition _defaultSurface;

        [Title("Effect Pool Configuration")]
        [Tooltip("Initial size of the effect pool.")]
        [SerializeField, Range(2, 128)]
        private int _effectPoolSize = 8;

        [Tooltip("Maximum size of the effect pool.")]
        [SerializeField, Range(2, 128)]
        private int _effectPoolCapacity = 32;

        [Title("Decal Pool Configuration")]
        [Tooltip("Initial size of the decal pool.")]
        [SerializeField, Range(2, 128)]
        private int _decalPoolSize = 8;

        [Tooltip("Maximum size of the decal pool.")]
        [SerializeField, Range(2, 128)]
        private int _decalPoolCapacity = 32;

        private readonly Dictionary<PhysicsMaterial, SurfaceDefinition> _materialToSurface = new(16);
        private readonly Dictionary<int, SurfaceEffects> _cachedEffects = new(32);

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init() => LoadOrCreateInstance();

        protected override void OnInitialized()
        {
            ClearCaches();
            BuildSurfaceMaterialCache();
            SceneManager.sceneUnloaded += OnSceneUnload;

            void OnSceneUnload(Scene _) => _cachedEffects.Clear(); // [Revisit] how to unsubscribe?
        }

        private void ClearCaches()
        {
#if UNITY_EDITOR
            _materialToSurface.Clear();
            _cachedEffects.Clear();
#endif
        }

        /// <summary>
        /// Builds cache to access <see cref="SurfaceDefinition"/> quickly using <see cref="PhysicsMaterial"/>
        /// bound to them.
        /// </summary>
        private void BuildSurfaceMaterialCache()
        {
            SurfaceDefinition[] allSurfaces = DefinitionRegistry<SurfaceDefinition>.AllDefinitions;

            foreach(var surface in allSurfaces)
            {
                CacheSurfaceMaterials(surface);
            }
        }

        /// <summary>
        /// Caches all <see cref="PhysicsMaterial"/> owned by <paramref name="surface"/>
        /// to be linked to itself in the cache.
        /// </summary>
        private void CacheSurfaceMaterials(SurfaceDefinition surface)
        {
            if(surface == null || surface.PhysicsMaterials == null)
            {
                return;
            }

            foreach(PhysicsMaterial material in surface.PhysicsMaterials)
            {
                if (material == null)
                {
                    continue;
                }

                if(_materialToSurface.TryAdd(material, surface) == false)
                {
                    var existingSurface = _materialToSurface[material];
                    Debug.LogError(
                        $"Physics Material: '{material.name}' on surface '{surface.Name}' " +
                        $"conflicts with existing surface '{existingSurface.Name}'. " +
                        $"Each material can be assigned only to one surface", surface);
                }
            }
        }

        // Methods for getting 'SurfaceDefinition' from 'RayCastHit' or 'Collision'
        // Main idea is to get 'SurfaceDefinition' from 'Collider' (we can get it from both RayCastHit and from Collision)
        // If 'Collider' has a valid 'PhysicsMaterial' then we get the 'SurfaceDefinition' from cache we built in the initialization.
        // If not, then we try to acquire it through 'SurfaceIdentifier' component of the hit object. This component is used for
        // objects which doesn't have a 'PhysicsMaterial' but to provide access to underlying 'SurfaceDefinition'.
        #region Getting Surface
        /// <summary>
        /// Gets <see cref="SurfaceDefinition"/> from <paramref name="hit"/>
        /// through accessing <see cref="Collider"/>.
        /// </summary>
        /// <returns>Surface the hit done on.</returns>
        public SurfaceDefinition GetSurfaceFromHit(in RaycastHit hit)
        {

            if(hit.collider == null)
            {
                return _defaultSurface;
            }

            if(hit.collider.sharedMaterial == null
            && hit.collider.TryGetComponent(out SurfaceIdentifier surfaceIdentifier))
            {
                return surfaceIdentifier.GetSurfaceFromHit(hit);
            }

            // Find from physics material
            return GetSurfaceFromCollider(hit.collider);
        }

        public SurfaceDefinition GetSurfaceFromCollider(Collider collider)
        {
            if(collider == null)
            {
                return _defaultSurface;
            }

            SurfaceDefinition surface = GetSurfaceFromPhysicsMaterial(collider.sharedMaterial);
            return surface ?? _defaultSurface;
        }

        public SurfaceDefinition GetSurfaceFromPhysicsMaterial(PhysicsMaterial material)
        {
            if(material == null)
            {
                return null;
            }

            return _materialToSurface.TryGetValue(material, out var surface) ? surface : null;
        }

        public SurfaceDefinition GetSurfaceFromCollision(Collision collision)
        {
            if(collision?.collider == null)
            {
                return _defaultSurface;
            }

            if(collision.collider.sharedMaterial == null
            && collision.collider.TryGetComponent(out SurfaceIdentifier surfaceIdentifier))
            {
                return surfaceIdentifier.GetSurfaceFromCollision(collision);
            }

            return GetSurfaceFromCollider(collision.collider);
        }
        #endregion

        /// <summary>
        /// Plays visual effects using <paramref name="hit"/>.
        /// </summary>
        /// <param name="hit">Raycast hit data.</param>
        /// <param name="effectType">Type of the effect.</param>
        /// <param name="flag">Flag controlling which visual effects to play.</param>
        /// <param name="volumeMultiplier">Volume of the audio effect that is to be played.</param>
        /// <param name="parentEffects">Whether set hit object as <b>parent</b> to the effects.</param>
        /// <returns>The <see cref="SurfaceDefinition"/> associated with the hit.</returns>
        public SurfaceDefinition PlayEffectFromHit(
            in RaycastHit hit, 
            SurfaceEffectType effectType, 
            SurfaceEffectFlags flags = SurfaceEffectFlags.All, 
            float volumeMultiplier = 1f, 
            bool parentEffects = false)
        {
            SurfaceDefinition surface = GetSurfaceFromHit(hit);

            if(TryGetSurfaceEffects(surface, effectType, out SurfaceEffects surfaceEffects))
            {
                Quaternion rotation = CalculateEffectRotation(hit.normal);
                Transform parent = parentEffects ? hit.transform : null;
                PlayEffect(surfaceEffects, hit.point, rotation, flags, volumeMultiplier, parent);
            }

            return surface;
        }

        public SurfaceDefinition PlayEffectFromCollision(
            Collision collision,
            SurfaceEffectType effectType,
            SurfaceEffectFlags flags = SurfaceEffectFlags.All,
            float volumeMultiplier = 1f,
            bool parentEffects = false)
        {
            SurfaceDefinition surface = GetSurfaceFromCollision(collision);

            if(TryGetSurfaceEffects(surface, effectType, out SurfaceEffects surfaceEffects))
            {
                ContactPoint contact = collision.GetContact(0);
                Quaternion rotation = CalculateEffectRotation(contact.normal);
                Transform parent = parentEffects ? collision.collider.transform  : null;
                PlayEffect(surfaceEffects, contact.point, rotation, flags, volumeMultiplier, parent);
            }

            return surface;
        }

        public void PlayEffect(
            SurfaceEffects surfaceEffects, 
            Vector3 position, 
            Quaternion rotation, 
            SurfaceEffectFlags flags, 
            float volumeMultiplier, 
            Transform parent)
        {
            if (surfaceEffects == null)
            {
                return;
            }

            if(EnumFlagsComparer.HasFlag(flags, SurfaceEffectFlags.Audio))
            {
                AudioModule.Instance.PlayCueOneShot(surfaceEffects.Audio, position, volumeMultiplier);
            }
            if(EnumFlagsComparer.HasFlag(flags, SurfaceEffectFlags.Visual) && ObjectPoolingModule.Instance.TryGetElement(surfaceEffects.Visual, out var visualEffect))
            {
                visualEffect.PlayEffect(position, rotation);
            }
            if(EnumFlagsComparer.HasFlag(flags, SurfaceEffectFlags.Decal) && ObjectPoolingModule.Instance.TryGetElement(surfaceEffects.Decal, out var decalEffect))
            {
                decalEffect.PlayEffect(position, rotation, parent);
            }
        }

        /// <summary>
        /// Gets a <see cref="SurfaceEffects"/> from the pool given the <see cref="SurfaceDefinition"/>
        /// and <see cref="SurfaceEffectType"/>.
        /// </summary>
        /// <remarks>
        /// Idea is to create a pool for every <see cref="SurfaceEffects"/> that is for <see cref="SurfaceDefinition"/>-<see cref="SurfaceEffectType"/>
        /// pair.
        /// </remarks>
        /// <param name="surface"></param>
        /// <param name="effectType"></param>
        /// <param name="surfaceEffects"></param>
        /// <returns></returns>
        private bool TryGetSurfaceEffects(SurfaceDefinition surface, SurfaceEffectType effectType, out SurfaceEffects surfaceEffects)
        {
            surfaceEffects = null;

            if(surface == null)
            {
                return false;
            }

            int effectID = BuildEffectID(surface, effectType);

            if(_cachedEffects.TryGetValue(effectID, out surfaceEffects))
            {
                return surfaceEffects != null;
            }

            if(surface.TryGetEffect(effectType, out surfaceEffects))
            {
                _cachedEffects.Add(effectID, surfaceEffects);
                EnsureEffectPoolsExist(surfaceEffects);
                return true;
            }

            _cachedEffects[effectID] = null;
            return false;
        }

        private void EnsureEffectPoolsExist(SurfaceEffects surfaceEffects)
        {
            if(surfaceEffects == null)
            {
                return;
            }

            CreateVisualEffectPool(surfaceEffects);
            CreateDecalEffectPool(surfaceEffects);
        }

        private void CreateVisualEffectPool(SurfaceEffects surfaceEffects)
        {
            ObjectPoolingModule poolingModule = ObjectPoolingModule.Instance;
            if (surfaceEffects.Visual == null || poolingModule.HasPool(surfaceEffects.Visual))
            {
                return;
            }
            var pool = new ManagedSceneObjectPool<SurfaceEffect>(
                surfaceEffects.Visual,
                SceneManager.GetActiveScene(),
                FPSDemoPoolCategory.SurfaceVisuals,
                _effectPoolSize,
                _effectPoolCapacity);

            poolingModule.RegisterPool(surfaceEffects.Visual, pool);
        }

        private void CreateDecalEffectPool(SurfaceEffects surfaceEffects)
        {
            ObjectPoolingModule poolingModule = ObjectPoolingModule.Instance;
            if (surfaceEffects.Decal == null || poolingModule.HasPool(surfaceEffects.Decal))
            {
                return;
            }
            var pool = new ManagedSceneObjectPool<SurfaceEffect>(
                surfaceEffects.Decal,
                SceneManager.GetActiveScene(),
                FPSDemoPoolCategory.SurfaceDecals,
                _decalPoolSize,
                _decalPoolCapacity);

            poolingModule.RegisterPool(surfaceEffects.Decal, pool);
        }

        private static int BuildEffectID(SurfaceDefinition surface, SurfaceEffectType effectType)
            => HashCode.Combine(surface.ID, effectType);

        private static Quaternion CalculateEffectRotation(Vector3 normal) => Quaternion.LookRotation(normal, Vector3.up);
    }
}