using Nexora.Options;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    [CustomPropertyDrawer(typeof(Option<>))]
    public class OptionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using(property.PropertyScope(position, label))
            {
                EditorGUI.PropertyField(position, property.FindPropertyRelative("_value"), label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_value"), label);
        }
    }
}