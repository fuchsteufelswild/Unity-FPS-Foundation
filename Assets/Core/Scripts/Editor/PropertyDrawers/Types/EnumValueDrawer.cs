using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    /// <summary>
    /// To prevent ugly foldout look.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumValue<>))]
    public class EnumValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (property.PropertyScope(position, label))
            {
                var enumProperty = property.FindPropertyRelative("_value");
                EditorGUI.PropertyField(position, enumProperty, label);
            }
        }
    }
}