using Nexora;
using System;
using Toolbox.Editor;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    [CustomPropertyDrawer(typeof(SpriteThumbnailAttribute))]
    public sealed class SpriteThumbnailDrawer : PropertyDrawerBase
    {
        /// <summary>
        /// Used if no height is defined in for <see cref="SpriteThumbnailAttribute"/>.
        /// </summary>
        private const float DefaultThumbnailSize = 64f;

        public SpriteThumbnailAttribute SpriteThumbnailAttribute => (SpriteThumbnailAttribute)attribute;

        private float ThumbnailSize
           => SpriteThumbnailAttribute.Height > 0 ? SpriteThumbnailAttribute.Height : DefaultThumbnailSize;

        protected override float GetPropertyHeightSafe(SerializedProperty property, GUIContent label)
        {
            if (IsValidSprite(property))
            {
                return EditorGUIUtils.DefaultUnitySelectorHeight +
                    ThumbnailSize + 
                    (EditorGUIUtils.ContainerBoxPadding * 2);
            }

            return EditorGUI.GetPropertyHeight(property, label);
        }

        private bool IsValidSprite(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ObjectReference &&
                   property.objectReferenceValue is Sprite;
        }

        protected override void OnGUISafe(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUIUtils.DrawDefaultUnitySelector<Sprite>(position, property);

            if (IsValidSprite(property) == false)
            {
                return;
            }

            // Draw sprite
            Sprite sprite = property.objectReferenceValue as Sprite;
            Rect boxStart = new Rect(
                position.x, position.y + EditorGUIUtils.DefaultUnitySelectorHeight, position.width, position.height);
            EditorGUIUtils.DrawTextureInBox(boxStart, sprite?.texture, ThumbnailSize, EditorGUIUtils.ContainerBoxPadding);

            EditorGUI.EndProperty();
        }
    }
}