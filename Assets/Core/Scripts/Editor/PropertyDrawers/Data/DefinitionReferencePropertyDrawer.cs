using System;
using System.Linq;
using System.Reflection;
using Toolbox.Editor;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    [CustomPropertyDrawer(typeof(DefinitionReference<>))]
    [CustomPropertyDrawer(typeof(DefinitionReferenceAttribute))]
    public sealed class DefinitionReferencePropertyDrawer : PropertyDrawerBase
    {
        private DefinitionReferenceDrawer _definitionReferenceDrawer;
        private string _propertyPath;

        protected override float GetPropertyHeightSafe(SerializedProperty property, GUIContent label)
        {
            BuildDrawer(property);
            return _definitionReferenceDrawer.GetPropertyHeight();
        }

        protected override void OnGUISafe(Rect position, SerializedProperty property, GUIContent label)
        {
            BuildDrawer(property);
            SerializedProperty valueProperty = property.FindPropertyRelative("_definitionID") ?? property;
            _definitionReferenceDrawer.DrawProperty(position, valueProperty, label);
        }

        /// <summary>
        /// Builds drawer and makes sure that we always have a valid <see cref="DefinitionReferenceAttribute"/>
        /// whether it uses an attribute or not.
        /// </summary>
        private void BuildDrawer(SerializedProperty property)
        {
            // Already built, no need to build again as this is expensive
            if(_definitionReferenceDrawer != null && property.propertyPath == _propertyPath)
            {
                return;
            }

            _propertyPath = property.propertyPath;

            Type definitionType = ExtractDefinitionTypeFromFieldInfo(fieldInfo);

            var attr = PropertyUtility.GetAttribute<DefinitionReferenceAttribute>(property) ?? new DefinitionReferenceAttribute();

            if(definitionType != null)
            {
                attr.DefinitionType = definitionType;
            }

            _definitionReferenceDrawer = new DefinitionReferenceDrawer(attr);
        }

        private static Type ExtractDefinitionTypeFromFieldInfo(FieldInfo fieldInfo)
        {
            Type fieldType = fieldInfo.FieldType;

            // Extract 'T' in the 'DefinitionReference<T>'
            // If the field is generic or is an array of generic types
            if(fieldType.IsGenericType || (fieldType.IsArray && fieldType.GetElementType().IsGenericType))
            {
                Type typeWithGenericArgs = fieldType.IsArray ? fieldType.GetElementType() : fieldType;
                return typeWithGenericArgs?.GenericTypeArguments.FirstOrDefault();
            }

            // It is type deriving from 'Definition'
            // So that we can use this with fields that are a 'Defintion'
            if(typeof(Definition).IsAssignableFrom(fieldType))
            {
                return fieldType;
            }

            return null;
        }
    }
}