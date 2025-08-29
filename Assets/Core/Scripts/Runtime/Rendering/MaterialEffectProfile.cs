using System;
using UnityEngine;

namespace Nexora.Rendering
{
    /// <summary>
    /// Configurable material effects for 3D objects having options to override materials by 
    /// modifying shader parameters or replacing materials.
    /// </summary>
    [CreateAssetMenu(menuName = "Nexora/Rendering/Material Effect Profile", fileName = "MaterialEffect_")]
    public sealed class MaterialEffectProfile : ScriptableObject
    {
        /// <summary>
        /// How material effects be combined with the existing materials.
        /// </summary>
        public enum EffectApplicationMode
        {
            [Tooltip("Add the new material effects on top of existing materials.")]
            Additive = 0,

            [Tooltip("Replace the base materials completely.")]
            Override = 1
        }

        /// <summary>
        /// How to apply material changes.
        /// </summary>
        public enum MaterialModificationTechnique
        {
            [Tooltip("Use a new material instance.")]
            FullMaterialReplacement,

            [Tooltip("Modify the shader parameters on existing materials.")]
            ShaderParameterModification
        }

        [Tooltip("Whether shadows should be rendered for affected materials.")]
        [SerializeField] 
        private bool _enableShadows = true;

        [Tooltip("How these effects should be combined with existing materials (Additive or Override).")]
        [SerializeField]
        private EffectApplicationMode _applicationMode;

        [Tooltip("Technique used to apply effects (Modify shader parameters or replace material).")]
        [SerializeField]
        private MaterialModificationTechnique _modificationTechnique;

        [Tooltip("Material replace the existing one.")]
        [ShowIf(nameof(_modificationTechnique), MaterialModificationTechnique.FullMaterialReplacement)]
        [SerializeField]
        private Material _replacementMaterial;

        [Tooltip("Settings to modify existing material's shader parameters.")]
        [ShowIf(nameof(_modificationTechnique), MaterialModificationTechnique.ShaderParameterModification)]
        [SerializeField]
        private ShaderParameterCollection _shaderParameters = new();

        /// <summary>
        /// Whether shadows should be rendered for affected materials.
        /// </summary>
        public bool EnableShadows => _enableShadows;

        /// <summary>
        /// How these effects should be combined with existing materials (Additive or Override).
        /// </summary>
        public EffectApplicationMode ApplicationMode => _applicationMode;

        /// <summary>
        /// Technique used to apply effects (Modify shader parameters or replace material).
        /// </summary>
        public MaterialModificationTechnique ModificationTechnique => _modificationTechnique;

        /// <summary>
        /// Replacement material to use when ModificationTechnique is FullMaterialReplacement
        /// </summary>
        public Material ReplacementMaterial => _replacementMaterial;

        /// <summary>
        /// Shader parameters to modify when ModificationTechnique is ShaderParameterModification
        /// </summary>
        public ShaderParameterCollection ShaderParameters => _shaderParameters;

        public sealed class ShaderParameterCollection
        {
            [ReorderableList(elementLabel: "Color Parameter")]
            [SerializeField]
            private ColorParameter[] _colorParameters = Array.Empty<ColorParameter>();

            [ReorderableList(elementLabel: "Float Parameters")]
            [SerializeField]
            private FloatParameter[] _floatParameters = Array.Empty<FloatParameter>();

            public ReadOnlySpan<ColorParameter> ColorParameters => _colorParameters;
            public ReadOnlySpan<FloatParameter> FloatParameters => _floatParameters;
        }

        [Serializable]
        public struct ColorParameter
        {
            [Tooltip("Name of the shader property (must match exactly).")]
            public string PropertyName;

            [ColorUsage(true, true), Tooltip("Target color value.")]
            public Color Value;
        }

        [Serializable]
        public struct FloatParameter
        {
            [Tooltip("Name of the shader property (must match exactly).")]
            public string PropertyName;

            [Tooltip("Target float value.")]
            public float Value;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validation method to validate material parameters in the Editor.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="IEditorValidatable.RunValidation" path="/remarks"/>
        /// </remarks>
        /// <returns>If all shader properties is already defined in <paramref name="material"/>.</returns>
        public bool ValidateParameters(Material material)
        {
            foreach(var param in _shaderParameters.ColorParameters)
            {
                if(material.HasProperty(param.PropertyName) == false)
                {
                    return false;
                }
            }

            foreach (var param in _shaderParameters.FloatParameters)
            {
                if (material.HasProperty(param.PropertyName) == false)
                {
                    return false;
                }
            }

            return true;
        }
#endif
    }
}