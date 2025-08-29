using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nexora.Rendering
{
    /// <summary>
    /// Applies and manages <see cref="MaterialEffectProfile"/> on multiple renderers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaterialEffectApplier : MonoBehaviour
    {
        [Tooltip("Default effect is applied when ApplyEffect() is called without any parameters.")]
        [SerializeField]
        private MaterialEffectProfile _defaultEffect;

        [Tooltip("Renderers that will be affected by material effect.")]
#if UNITY_EDITOR
        [EditorButton(nameof(UpdateRendererReferences)), SpaceArea]
#endif
        [ReorderableList(HasLabels = false)]
        [SerializeField]
        private Renderer[] _affectedRenderers;

        private MaterialEffectProfile _currentActiveEffect;
        private Material[][] _originalMaterialsBackup;
        private bool _hasInitialized;

        private MaterialReplacer _materialReplacer = new();
        private ShaderParameterApplier _shaderParameterApplier = new();

        /// <summary>
        /// Applies the <paramref name="effect"/> to all renderers,
        /// if <paramref name="effect"/> is <see langword="null"/>
        /// then uses the default effect provided in this class.
        /// </summary>
        /// <remarks>
        /// If default effect is not present and passed in <see langword="null"/>,
        /// then removes the effect altogether.
        /// </remarks>
        public void ApplyEffect(MaterialEffectProfile effect = null)
        {
            var effectToApply = effect ?? _defaultEffect;
            if (effectToApply == null)
            {
                RevertEffect();
                return;
            }

            EnsureInitialized();
            SetActiveEffect(effectToApply);
        }

        /// <summary>
        /// Removes the effect and resets renderer materials back to the original ones.
        /// </summary>
        public void RevertEffect()
        {
            if(_hasInitialized == false || _currentActiveEffect == null)
            {
                return;
            }

            RestoreOriginalMaterials();
            _currentActiveEffect = null;
        }

        /// <summary>
        /// Restores the materials of the renderers to the original(like it was).
        /// </summary>
        private void RestoreOriginalMaterials()
        {
            for(int i = 0; i < _affectedRenderers.Length; i++)
            {
                _affectedRenderers[i].sharedMaterials = _originalMaterialsBackup[i];
                _affectedRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        private void EnsureInitialized()
        {
            if(_hasInitialized)
            {
                return;
            }

            CacheOriginalMaterials();
            _hasInitialized = true;
        }

        /// <summary>
        /// Caches the original materials of the renderers.
        /// </summary>
        private void CacheOriginalMaterials()
        {
            _originalMaterialsBackup = new Material[_affectedRenderers.Length][];

            for(int i = 0; i < _affectedRenderers.Length; i++)
            {
                _originalMaterialsBackup[i] = _affectedRenderers[i].sharedMaterials
                    .Select(material => material) // Clone
                    .ToArray();
            }
        }

        private void SetActiveEffect(MaterialEffectProfile effect)
        {
            if(_currentActiveEffect == effect)
            {
                return;
            }

            _currentActiveEffect = effect;

            switch (effect.ModificationTechnique)
            {
                case MaterialEffectProfile.MaterialModificationTechnique.FullMaterialReplacement:
                    _materialReplacer.ReplaceMaterial(effect, _originalMaterialsBackup, _affectedRenderers);
                    break;
                case MaterialEffectProfile.MaterialModificationTechnique.ShaderParameterModification:
                    _shaderParameterApplier.ApplyShaderParameters(effect, _affectedRenderers);
                    break;
            }

            UpdateShadowState(effect.EnableShadows);
        }

        private void UpdateShadowState(bool enableShadows)
        {
            var mode = enableShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            foreach(Renderer renderer in _affectedRenderers)
            {
                renderer.shadowCastingMode = mode;
            }
        }

        /// <summary>
        /// Prevents memory leaks in the case of too many frequent material effect swaps.
        /// </summary>
        private void OnDestroy()
        {
            if(_hasInitialized == false)
            {
                return;
            }
        }

        /// <summary>
        /// Performs material replacing effect.
        /// </summary>
        private sealed class MaterialReplacer
        {
            public void ReplaceMaterial(
                MaterialEffectProfile effect, 
                Material[][] originalMaterials, 
                Renderer[] affectedRenderers)
            {
                switch (effect.ApplicationMode)
                {
                    case MaterialEffectProfile.EffectApplicationMode.Additive:
                        ApplyAdditiveEffect(effect, originalMaterials, affectedRenderers);
                        break;
                    case MaterialEffectProfile.EffectApplicationMode.Override:
                        ApplyOverrideEffect(effect, originalMaterials, affectedRenderers);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            /// <summary>
            /// Adds the replacement materials to the list of already existing materials.
            /// </summary>
            /// <param name="effect"></param>
            private void ApplyAdditiveEffect(
                MaterialEffectProfile effect,
                Material[][] originalMaterials,
                Renderer[] affectedRenderers)
            {
                for(int i = 0; i < affectedRenderers.Length; i++)
                {
                    var materials = originalMaterials[i]
                        .Append(effect.ReplacementMaterial)
                        .ToArray();

                        affectedRenderers[i].sharedMaterials = materials;
                }
            }

            /// <summary>
            /// Replaces all materials of all renderers to the replacement
            /// materials held in <paramref name="effect"/>.
            /// </summary>
            /// <param name="effect"></param>
            private void ApplyOverrideEffect(
                MaterialEffectProfile effect,
                Material[][] originalMaterials,
                Renderer[] affectedRenderers)
            {
                foreach(Renderer renderer in affectedRenderers)
                {
                    var materials = new Material[renderer.sharedMaterials.Length];
                    Array.Fill(materials, effect.ReplacementMaterial);
                    renderer.sharedMaterials = materials;
                }
            }
        }

        /// <summary>
        /// Performs shader parameter modifying effect.
        /// </summary>
        private sealed class ShaderParameterApplier
        {
            private MaterialPropertyBlock _propertyBlock;
            private bool _usingPropertyBlock;

            public void ApplyShaderParameters(MaterialEffectProfile effect, Renderer[] affectedRenderers)
            {
                InitializePropertyBlock();

                foreach(var param in effect.ShaderParameters.ColorParameters)
                {
                    _propertyBlock.SetColor(param.PropertyName, param.Value);
                }

                foreach(var param in effect.ShaderParameters.FloatParameters)
                {
                    _propertyBlock.SetFloat(param.PropertyName, param.Value);
                }

                foreach(var rendrerer in affectedRenderers)
                {
                    rendrerer.SetPropertyBlock(_propertyBlock);
                }

                _usingPropertyBlock = true;
            }

            public void InitializePropertyBlock()
            {
                if(_propertyBlock == null)
                {
                    _propertyBlock = new MaterialPropertyBlock();
                }
                else
                {
                    _propertyBlock.Clear();
                }
            }

            public void ClearShaderParameters(Renderer[] affectedRenderers)
            {
                if(_usingPropertyBlock == false)
                {
                    return;
                }

                InitializePropertyBlock();

                foreach(Renderer renderer in affectedRenderers)
                {
                    renderer.SetPropertyBlock(_propertyBlock);
                }
                _usingPropertyBlock = false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() => UpdateRendererReferences();
        private void Reset() => UpdateRendererReferences();

        private void UpdateRendererReferences()
        {
            _affectedRenderers = GetComponentsInChildren<Renderer>(true)
                .Where(r => r is MeshRenderer or SkinnedMeshRenderer)
                .Distinct()
                .ToArray();
        }
#endif
    }
}