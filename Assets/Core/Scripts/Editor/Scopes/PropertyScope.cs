using System;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    public struct PropertyScope : IDisposable
    {
        public PropertyScope(Rect position, GUIContent label, SerializedProperty property)
            => EditorGUI.BeginProperty(position, label, property);

        public void Dispose() => EditorGUI.EndProperty();
    }
}