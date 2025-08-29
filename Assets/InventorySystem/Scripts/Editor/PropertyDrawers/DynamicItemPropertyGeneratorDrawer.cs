using Nexora.Editor;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace Nexora.InventorySystem.Editor
{
    [CustomPropertyDrawer(typeof(DynamicItemPropertyGenerator))]
    public sealed class DynamicItemPropertyGeneratorDrawer : PropertyDrawerBase
    {
        private const float ColumnSplitRatio = 0.75f;

        private readonly DefinitionReferenceDrawer _propertyReferenceDrawer =
            new(new DefinitionReferenceAttribute()
            {
                DefinitionType = typeof(DynamicItemPropertyDefinition),
                NullElement = "None"
            });

        private DefinitionReferenceDrawer _itemReferenceDrawer;

        protected override float GetPropertyHeightSafe(SerializedProperty property, GUIContent label)
        {
            return (_propertyReferenceDrawer.GetPropertyHeight() * 2) + EditorGUIUtility.standardVerticalSpacing * 2;
        }

        protected override void OnGUISafe(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            DynamicItemPropertyDefinition selectedDynamicItemProperty = DrawItemPropertySelection(position, property);

            if(selectedDynamicItemProperty != null)
            {
                DrawValueSelection(position, property, selectedDynamicItemProperty);
            }
        }

        private DynamicItemPropertyDefinition DrawItemPropertySelection(Rect position, SerializedProperty property)
        {
            SerializedProperty definitionProperty = property.FindPropertyRelative("_itemPropertyDefinition");

            Rect selectionRect = RectUtils.GetLeftColumnRect(position, ColumnSplitRatio);

            _propertyReferenceDrawer.DrawProperty(position, definitionProperty, GUIContent.none);
            return DefinitionRegistry<DynamicItemPropertyDefinition>.GetByID(definitionProperty.intValue);
        }

        private void DrawValueSelection(Rect position, SerializedProperty property, DynamicItemPropertyDefinition selectedDynamicItemProperty)
        {
            Rect typeRect = RectUtils.GetRightColumnRect(position, ColumnSplitRatio);

            ItemPropertyValueType itemPropertyValueType = selectedDynamicItemProperty.PropertyValueType;

            EditorGUI.LabelField(typeRect, $"Type: {ObjectNames.NicifyVariableName(itemPropertyValueType.ToString())}", EditorStyles.miniBoldLabel);

            position = RectUtils.GetNextRow(position, EditorGUIUtility.standardVerticalSpacing);

            SerializedProperty valueRangeProperty = property.FindPropertyRelative("_valueRange");

            switch(itemPropertyValueType)
            {
                case ItemPropertyValueType.Float or ItemPropertyValueType.Integer or ItemPropertyValueType.Double:
                    DrawNumberValueSelection(position, valueRangeProperty, itemPropertyValueType, property.FindPropertyRelative("_generateRandomValue"));
                    break;
                case ItemPropertyValueType.Boolean:
                    DrawBoolValueSelectoin(position, valueRangeProperty);
                    break;
                case ItemPropertyValueType.Item:
                    DrawLinkedItemIDSelection(position, valueRangeProperty);
                    break;
            }
        }

        private static void DrawNumberValueSelection(
            Rect position, 
            SerializedProperty valueRangeProperty, 
            ItemPropertyValueType itemPropertyValueType, 
            SerializedProperty generateRandomValueProperty)
        {
            Rect selectedRandomModeRect = position.WithWidth(position.width * 0.35f);

            int selectedRandomMode = GUI.Toolbar(selectedRandomModeRect, generateRandomValueProperty.boolValue ? 1 : 0,
                new[] { "Fixed", "Random" });

            generateRandomValueProperty.boolValue = selectedRandomMode == 1;

            Rect valueFieldRect = RectUtils.GetNextColumn(selectedRandomModeRect, 15f)
                .WithWidth(position.width - selectedRandomModeRect.width - 15f);


            // Fixed
            if (selectedRandomMode == 0)
            {
                if (itemPropertyValueType is ItemPropertyValueType.Float or ItemPropertyValueType.Double)
                {
                    float value = EditorGUI.FloatField(valueFieldRect, valueRangeProperty.vector2Value.x);
                    valueRangeProperty.vector2Value = new Vector2(value, value);
                }
                else
                {
                    float value = EditorGUI.IntField(valueFieldRect, Mathf.RoundToInt(valueRangeProperty.vector2Value.x));
                    valueRangeProperty.vector2Value = new Vector2(Mathf.Clamp(value, -10000000, 10000000), Mathf.Clamp(value, -10000000, 10000000));
                }
            }
            else // Random
            {
                float[] randomizationRange = new float[] { valueRangeProperty.vector2Value.x, valueRangeProperty.vector2Value.y };

                valueRangeProperty.vector2Value = itemPropertyValueType != ItemPropertyValueType.Integer
                    ? EditorGUI.Vector2Field(valueFieldRect, GUIContent.none, valueRangeProperty.vector2Value)
                    : EditorGUI.Vector2IntField(valueFieldRect, GUIContent.none, new Vector2Int(
                        Mathf.RoundToInt(valueRangeProperty.vector2Value.x),
                        Mathf.RoundToInt(valueRangeProperty.vector2Value.y)));
            }

        }

        private static void DrawBoolValueSelectoin(Rect position, SerializedProperty valueRangeProperty)
        {
            bool isTrue = Mathf.Approximately(valueRangeProperty.vector2Value.x, 0f) == false;

            EditorGUI.LabelField(position, "Bool");

            Rect booleanToggleRect = position.ShiftRight(80f).WithWidth(EditorGUIUtility.singleLineHeight);
            isTrue = EditorGUI.Toggle(booleanToggleRect, isTrue);

            valueRangeProperty.vector2Value = new Vector2(isTrue ? 1f : 0f, 0f);
        }

        private void DrawLinkedItemIDSelection(Rect position, SerializedProperty valueRangeProperty)
        {
            _itemReferenceDrawer ??= new DefinitionReferenceDrawer(new DefinitionReferenceAttribute()
            {
                DefinitionType = typeof(ItemDefinition),
                NullElement = "None"
            });

            EditorGUI.LabelField(position, "Linked Item");

            Rect popupRect = EditorGUI.IndentedRect(position).ShiftRight(80f);
            RectUtils.GetRightColumnRect(popupRect, ColumnSplitRatio);

            _itemReferenceDrawer.DrawProperty(popupRect, valueRangeProperty, GUIContent.none);
        }
    }
}