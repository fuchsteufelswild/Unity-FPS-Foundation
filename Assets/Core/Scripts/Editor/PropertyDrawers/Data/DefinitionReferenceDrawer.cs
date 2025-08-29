using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    /// <summary>
    /// Main class used to draw <see cref="DefinitionReference{T}"/> on the inspector.
    /// </summary>
    public sealed class DefinitionReferenceDrawer
    {
        private readonly DefinitionReferenceAttribute _attribute;
        private readonly Definition[] _availableDefinitions;
        private readonly GUIContent[] _dropdownOptions;

        public DefinitionReferenceDrawer(DefinitionReferenceAttribute attribute)
        {
            _attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            _availableDefinitions = DefinitionProvider.GetDefinitions(attribute.DefinitionType);
            _dropdownOptions = DropdownContentBuilder.CreateOptions(_availableDefinitions, attribute.NullElement, attribute.HasIcon);
        }

        public float GetPropertyHeight()
        {
            float baseHeight = EditorGUIUtility.singleLineHeight;
            float assetReferenceHeight = _attribute.HasAssetReference ? EditorGUIUtility.singleLineHeight + 3f : 0f;
            return baseHeight + assetReferenceHeight;
        }

        public void DrawProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            if(_availableDefinitions == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("No definitions available."));
                return;
            }

            int selectedIndex = GetSelectedIndex(property);
            DrawDropdown(position, property, label, selectedIndex);

            if(_attribute.HasAssetReference)
            {
                DrawAssetReferenceField(position, selectedIndex);
            }
        }

        private void DrawDropdown(Rect position, SerializedProperty property, GUIContent label, int selectedIndex)
        {
            Rect dropdownRect = position.WithHeight(EditorGUIUtility.singleLineHeight);
            int newSelection = EditorGUI.Popup(dropdownRect, label, selectedIndex, _dropdownOptions);

            if(newSelection != selectedIndex)
            {
                ApplySelection(property, newSelection);
            }
        }

        /// <summary>
        /// Definition reference may support number of different types when used.
        /// </summary>
        private void ApplySelection(SerializedProperty property, int selectedIndex)
        {
            property.serializedObject.Update();

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = GetDefinitionIDAtIndex(selectedIndex);
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = GetDefinitionAtIndex(selectedIndex);
                    break;
                case SerializedPropertyType.Vector2:
                    int id = GetDefinitionIDAtIndex(selectedIndex);
                    property.vector2Value = new Vector2(id, property.vector2Value.y);
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = GetDefinitionNameAtIndex(selectedIndex);
                    break;
                default:
                    Debug.LogWarning("Unsupported property type for 'DefinitionReference'");
                    break;
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Show the <see cref="Definition"/> asset, currently held.
        /// </summary>
        private void DrawAssetReferenceField(Rect position, int selectedIndex)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                float lineLength = EditorGUIUtility.singleLineHeight;
                Rect assetRect = position.ShiftRight(lineLength * 0.5f)
                    .ShiftDown(lineLength + EditorGUIUtility.standardVerticalSpacing) 
                    .RemoveWidth(lineLength)
                    .WithHeight(lineLength);

                EditorGUI.ObjectField(
                    assetRect, 
                    "Asset Reference", 
                    GetDefinitionAtIndex(selectedIndex), 
                    _attribute.DefinitionType, 
                    false);
            }
        }

        /// <summary>
        /// As we can support many types, we need to find appropriate index dependign on these types.
        /// </summary>
        /// <remarks>
        /// +1 provided in the finders are due to the fact that we are proving a 'null' option as the first option in the list.
        /// </remarks>
        private int GetSelectedIndex(SerializedProperty property)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.Integer => FindIndexByID(property.intValue),
                SerializedPropertyType.String => FindIndexByName(property.stringValue),
                SerializedPropertyType.Vector2 => FindIndexByID(Mathf.RoundToInt(property.vector2Value.x)),
                SerializedPropertyType.ObjectReference => FindIndexByDefinition(property.objectReferenceValue as Definition),
                _ => 0
            };
        }

        private int FindIndexByID(int id) 
        {
            return Array.FindIndex(_availableDefinitions, definition => definition.ID == id) + 1;
        }

        private int FindIndexByName(string name) 
        {
            return string.IsNullOrEmpty(name) ? 0 : Array.FindIndex(_availableDefinitions, definition => definition.Name == name) + 1;
        }

        private int FindIndexByDefinition(Definition definition)
        {
            return definition != null ? Array.IndexOf(_availableDefinitions, definition) + 1 : 0;
        }

        private int GetDefinitionIDAtIndex(int index)
            => GetDefinitionAtIndex(index)?.ID ?? 0;

        private string GetDefinitionNameAtIndex(int index)
            => GetDefinitionAtIndex(index)?.Name ?? string.Empty;

        private Definition GetDefinitionAtIndex(int index)
        {
            return IsValidIndex(index) ? _availableDefinitions[index - 1] : null;
        }

        private bool IsValidIndex(int index)
        {
            return index > 0 && index <= _availableDefinitions.Length;
        }
    }

    public static class DefinitionProvider
    {
        public static Definition[] GetDefinitions(Type dataType)
        {
            if(dataType == null)
            {
                Debug.LogError("DefinitionProvider: Provided type is null");
                return null;
            }

            if(typeof(Definition).IsAssignableFrom(dataType) == false)
            {
                Debug.LogError($"DefinitionProvider: Type '{dataType.Name}' is not assignable from 'Definition'");
                return null;
            }

            try
            {
                var openGenericRegistryType = typeof(DefinitionRegistry<>);
                var closedGenericRegistryType = openGenericRegistryType.MakeGenericType(dataType);

                PropertyInfo allDefinitionsProperty = closedGenericRegistryType.GetProperty(
                    "AllDefinitions",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                if(allDefinitionsProperty?.CanRead == true)
                {
                    return (Definition[])allDefinitionsProperty.GetValue(null);
                }

                Debug.LogError($"DefinitionProvider: Could not find readable 'AllDefinitions' property for {dataType.Name}" +
                    $" on DefinitionRegistry");
            }
            catch (Exception ex)
            {
                Debug.LogError($"DefinitionProvider: Error accessing definitions for {dataType.Name}: {ex.Message}");
            }

            return null;
        }
    }
}