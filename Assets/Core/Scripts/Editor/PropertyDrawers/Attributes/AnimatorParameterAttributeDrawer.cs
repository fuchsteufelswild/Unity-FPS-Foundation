using System;
using System.Linq;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;


namespace Nexora.Editor
{
    [CustomPropertyDrawer(typeof(AnimatorParameterAttribute))]
    public sealed class AnimatorParameterAttributeDrawer : PropertyDrawerBase
    {
        public AnimatorParameterAttribute AnimatorParameterAttribute => (AnimatorParameterAttribute)attribute;

        /// <inheritdoc cref="AnimatorParameterDrawer.DrawParameterSelectionDropdown(Rect, SerializedProperty, AnimatorControllerParameterType, ref int, string, float?)"/>
        protected override void OnGUISafe(Rect position, SerializedProperty property, GUIContent label)
        {
            if(Application.isPlaying)
            {
                return;
            }

            int selectedParameterIndex = AnimatorParameterAttribute.SelectedParameterIndex;
            AnimatorParameterDrawer.DrawParameterSelectionDropdown(
                position, 
                property, 
                parameterType: ExtractType(property), 
                ref selectedParameterIndex, 
                dropdownFieldName: property.displayName, 
                helpBoxHeightMultiplier: 1f);

            AnimatorParameterAttribute.SelectedParameterIndex = selectedParameterIndex;
        }

        /// <summary>
        /// Gets a <see cref="AnimatorControllerParameterType"/> from a field named,
        /// <see cref="AnimatorParameterAttribute.ParameterTypeFieldName"/>. Or just 
        /// a <see cref="AnimatorParameterAttribute.ParameterType"/> if the <see langword="string"/>
        /// field is empty.
        /// </summary>
        private AnimatorControllerParameterType ExtractType(SerializedProperty property)
        {
            if(string.IsNullOrEmpty(AnimatorParameterAttribute.ParameterTypeFieldName) == false)
            {
                if(ValueExtractionHelper.TryGetValue(
                    AnimatorParameterAttribute.ParameterTypeFieldName,
                    property,
                    out var parameterType,
                    out _))
                {
                    AnimatorParameterAttribute.ParameterType = (AnimatorControllerParameterType)parameterType;
                }
            }

            return AnimatorParameterAttribute.ParameterType;
        }
    }
}