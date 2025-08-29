using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    [CustomPropertyDrawer(typeof(Nexora.Serialization.SerializedDateTime))]
    public sealed class SerializedDateTimeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // One line for datetime, one line for ticks
            return base.GetPropertyHeight(property, label) * 2f; 
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            using(property.PropertyScope(position, label))
            {
                var ticksProperty = property.FindPropertyRelative("_ticks");
                position = EditorGUI.PrefixLabel(position, label);
                
                DateTime dateTime = new(ticksProperty.longValue);

                EditorGUI.BeginChangeCheck();
                var dateTimeString = EditorGUI.DelayedTextField(position, dateTime.ToString(CultureInfo.InvariantCulture));
                if (EditorGUI.EndChangeCheck())
                {
                    if (DateTime.TryParse(dateTimeString, out dateTime))
                    {
                        UpdateTicks(ticksProperty, dateTime);
                    }
                }

                DrawTicks(position, ticksProperty);
            }
        }

        private void UpdateTicks(SerializedProperty ticksProperty, DateTime dateTime)
        {
            ticksProperty.serializedObject.Update();
            ticksProperty.longValue = dateTime.Ticks;
            ticksProperty.serializedObject.ApplyModifiedProperties();
        }

        private void DrawTicks(Rect position, SerializedProperty ticksProperty)
        {
            const float columnSplitRatio = 0.15f;

            RectUtils.MoveToNextLine(ref position);
            using (new EditorGUI.DisabledScope(true))
            {
                Rect labelPosition = RectUtils.GetLeftColumnRect(position, columnSplitRatio);
                EditorGUI.LabelField(labelPosition, "Ticks");
                Rect fieldPosition = RectUtils.GetRightColumnRect(position, columnSplitRatio); 
                EditorGUI.LongField(fieldPosition, ticksProperty.longValue);
            }
        }
        
    }
}