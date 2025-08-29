using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    [CustomPropertyDrawer(typeof(MaxAttribute))]
    public class MaxAttributeDrawer : PropertyDrawer
    {
        private MaxAttribute Attribute => (MaxAttribute)attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (property.PropertyScope(position, label))
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        property.intValue = Mathf.Min(property.intValue, (int)Attribute.MaxValue);
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = Mathf.Min(property.floatValue, Attribute.MaxValue);
                        break;
                }

                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}