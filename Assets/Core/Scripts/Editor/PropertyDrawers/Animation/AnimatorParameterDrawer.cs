using Nexora.Animation;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Nexora.Editor
{
    /// <summary>
    /// Provides <see cref="AnimatorParameter"/> with an ease to easily select name
    /// of the parameter from <see cref="Animator.parameters"/>. Depending on the
    /// <see cref="AnimatorControllerParameterType"/>, it creates a dropdown for
    /// the list of parameters suitable for selection.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnimatorParameter))]
    public sealed class AnimatorParameterDrawer : PropertyDrawer
    {
        private AnimatorControllerParameterType _currentParameterType;
        private int _selectedParameterIndex;

        /// <summary>
        /// How many lines default property has, now Property name/Type field
        /// </summary>
        private const float LinesInProperty = 2f;

        private const float HelpBoxHeightMultiplier = 2f;

        /// <summary>
        /// Indent amount so that parameter selector and parameter value field will
        /// have indentation of the main parameters.
        /// </summary>
        private const float DropdownIndentation = 15f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (Application.isPlaying)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            // + 2, is for parameter selector dropdown and its value modifier field
            return EditorGUIUtils.SingleLineHeight * (LinesInProperty + 2);
        }

        public static float GetMaximumHeight()
        {
            return EditorGUIUtils.SingleLineHeight * (LinesInProperty + 2);
        }

        private float DefaultFieldsHeight()
            => EditorGUIUtils.SingleLineHeight * LinesInProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(Application.isPlaying)
            {
                return;
            }

            ConfigureInitialLayout(ref position, property);
            var (typeProperty, nameProperty, valueProperty) = GetPropertyReferences(property);

            // Default draw fields
            EditorGUI.PropertyField(position, property, true);

            var parameterType = (AnimatorControllerParameterType)typeProperty.intValue;
            UpdateCurrentParameterType(parameterType);

            using (new IndentedScope(DropdownIndentation))
            {
                // Move to line after default fields
                position = new Rect(
                    position.x,
                    position.y + DefaultFieldsHeight(),
                    position.width,
                    position.height);

                position = EditorGUI.IndentedRect(position);

                if (DrawParameterSelectionDropdown(position, nameProperty, parameterType, ref _selectedParameterIndex))
                {
                    position = new Rect(
                        position.x,
                        position.yMax + EditorGUIUtility.standardVerticalSpacing,
                        position.width,
                        position.height);

                    // If has a valid parameter, this line will be used to change the actual value.
                    DrawParameterValueField(position, valueProperty, parameterType);
                }
            }
        }

        private static void ConfigureInitialLayout(ref Rect position, SerializedProperty property)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = true;
        }

        private static (SerializedProperty typeProperty, SerializedProperty nameProperty, SerializedProperty valueProperty) 
            GetPropertyReferences(SerializedProperty property)
        {
            return (
                typeProperty: property.FindPropertyRelative("Type"),
                nameProperty: property.FindPropertyRelative("Name"),
                valueProperty: property.FindPropertyRelative("Value"));
        }

        private void UpdateCurrentParameterType(AnimatorControllerParameterType parameterType)
        {
            if(parameterType != _currentParameterType)
            {
                _currentParameterType = parameterType;
                _selectedParameterIndex = 0;
            }
        }

        /// <summary>
        /// Finds a valid <see cref="AnimatorController"/> and gets its parameters that has <see cref="AnimatorControllerParameterType"/>
        /// of the <see cref="AnimatorParameterAttribute"/>. From there opens a popup to pick from the list parameters.
        /// </summary>
        /// <remarks>
        /// <b>Uses <see cref="SerializedProperty.stringValue"/> to store the last selected parameter name.</b>
        /// </remarks>
        public static bool DrawParameterSelectionDropdown(
            Rect position, 
            SerializedProperty nameProperty, 
            AnimatorControllerParameterType parameterType, 
            ref int selectedParameterIndex,
            string dropdownFieldName = null,
            float? helpBoxHeightMultiplier = null)
        {
            dropdownFieldName ??= "Parameter";
            helpBoxHeightMultiplier ??= AnimatorParameterDrawer.HelpBoxHeightMultiplier;

            var animator = GetAnimatorController(nameProperty);

            //if(EditorGUIUtils.ValidateField<AnimatorController>(position, animator, helpBoxHeightMultiplier.Value) == false)
            //{
            //    return false;
            //}

            var availableParameters = animator.parameters
                                     .Where(parameter => parameter.type == parameterType)
                                     .Select(parameter => parameter.name)
                                     .ToArray();

            if(availableParameters.Length == 0)
            {
                EditorGUIUtils.DrawHelpBox(position, 
                    $"No animation parameters of type {parameterType} found.", UnityEditor.MessageType.Warning, helpBoxHeightMultiplier.Value);
                return false;
            }

            int matchingIndex = Array.FindIndex(availableParameters, 
                parameter => parameter.Equals(nameProperty.stringValue));

            if(matchingIndex != CollectionConstants.NotFound)
            {
                selectedParameterIndex = matchingIndex;
            }

            selectedParameterIndex = EditorGUI.IntPopup(
                position,
                "Parameter",
                selectedParameterIndex,
                availableParameters,
                Enumerable.Range(0, availableParameters.Length).ToArray());

            nameProperty.stringValue = availableParameters[selectedParameterIndex];

            return true;
        }

        private static AnimatorController GetAnimatorController(SerializedProperty property)
        {
            if(property.serializedObject.targetObject is not Component component)
            {
                return null;
            }

            var animator = component.GetComponentFromHierarchy<Animator>();
            if(animator == null)
            {
                Debug.LogWarning("No Animator component found in hierarchy.");
                return null;
            }

            return animator.runtimeAnimatorController as AnimatorController;
        }

        private void DrawParameterValueField(
            Rect position, SerializedProperty valueProperty, AnimatorControllerParameterType parameterType)
        {
            switch (parameterType)
            {
                case AnimatorControllerParameterType.Float:
                    DrawFloatField(position, valueProperty);
                    break;
                case AnimatorControllerParameterType.Int:
                    DrawIntField(position, valueProperty);
                    break;
                case AnimatorControllerParameterType.Bool:
                    DrawBoolField(position, valueProperty);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    DrawTriggerField(position);
                    break;
            }
        }

        private void DrawFloatField(Rect position, SerializedProperty valueProperty)
        {
            valueProperty.floatValue = EditorGUI.FloatField(position, "Float", valueProperty.floatValue);
        }

        private void DrawIntField(Rect position, SerializedProperty valueProperty)
        {
            int currentValue = Mathf.RoundToInt(valueProperty.floatValue);
            int newValue = EditorGUI.IntField(position, "Integer", currentValue);
            valueProperty.floatValue = Mathf.Clamp(newValue, int.MinValue, int.MaxValue);
        }

        private void DrawBoolField(Rect position, SerializedProperty valueProperty)
        {
            bool currentValue = Mathf.Approximately(valueProperty.floatValue, 0f) == false;
            bool newValue = EditorGUI.Toggle(position, "Bool", currentValue);
            valueProperty.floatValue = newValue ? 1f : 0f;
        }

        private void DrawTriggerField(Rect position)
        {
            EditorGUI.LabelField(position, "Trigger");
        }
    }
}