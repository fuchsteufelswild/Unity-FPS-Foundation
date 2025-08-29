using Nexora.UI;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    [CustomPropertyDrawer(typeof(MustImplementInterfaceAttribute))]
    public sealed class MustImplementInterfaceDrawer : PropertyDrawer
    {
        private const float HelpBoxLineHeightMultiplier = 1.5f;

        public MustImplementInterfaceAttribute Attribute => (MustImplementInterfaceAttribute)attribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return base.GetPropertyHeight(property, label);
            }

            return base.GetPropertyHeight(property, label) + HelpBoxLineHeightMultiplier * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUIUtils.DrawInfoOverlay(position, EditorInfoType.Error,
                    "IntefacePicker works only with Object fields.");
                return;
            }

            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUIUtils.DrawHelpBox(position, 
                $"This field only allows classes that implements {Attribute.InterfaceType.Name}", 
                UnityEditor.MessageType.Info,
                HelpBoxLineHeightMultiplier);

            position = position.ShiftDown(HelpBoxLineHeightMultiplier * EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginChangeCheck();

            UnityEngine.Object newValue = DrawObjectPicker();

            if(EditorGUI.EndChangeCheck())
            {
                AssignValueIfCompatible(property, newValue);
            }

            return;

            UnityEngine.Object DrawObjectPicker()
            {
                UnityEngine.Object newValue = EditorGUI.ObjectField(
                position,
                label,
                property.objectReferenceValue,
                typeof(UnityEngine.Object),
                false);

                return newValue;
            }
        }
        
        private void AssignValueIfCompatible(
            SerializedProperty property,
            UnityEngine.Object newValue)
        {
            if (newValue == null || ((GameObject)newValue).ImplementsInterface(Attribute.InterfaceType))
            {
                property.objectReferenceValue = newValue;
            }
            else
            {
                Debug.LogError($"Assigned object must implement {Attribute.InterfaceType.Name}");
            }
        }
    }
}