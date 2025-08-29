using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    public static class GUIContentExtensions
    {
        /// <summary>
        /// Extension method to deconstruct GUIContent.
        /// </summary>
        public static void Deconstruct(this GUIContent content, out string text, out Texture image)
        {
            text = content.text;
            image = content.image;
        }

        /// <inheritdoc cref="Deconstruct(GUIContent, out string, out Texture)"/>
        public static void Deconstruct(this GUIContent content, out string text, out Texture image, out string tooltip)
        {
            text = content.text;
            image = content.image;
            tooltip = content.tooltip;
        }

        public static GUIContent WithTooltip(this GUIContent content, string tooltip)
        {
            return new GUIContent(content)
            {
                tooltip = tooltip
            };
        }

        public static GUIContent WithTooltip(this GUIContent content, SerializedProperty property)
        {
            return new GUIContent(content)
            {
                tooltip = property.tooltip
            };
        }

        public static GUIContent WithText(this GUIContent content, string text)
        {
            return new GUIContent(content)
            {
                text = text
            };
        }
    }

    public static class PropertyExtensions
    {
        public static PropertyScope PropertyScope(this SerializedProperty property, Rect position, GUIContent label)
            => new PropertyScope(position, label, property);
    }

    public static class GUIStyleExtensions
    {
        public static GUIStyle Duplicate(this GUIStyle style)
            => new GUIStyle(style);

        public static GUIStyle SetTextAlignment(this GUIStyle style, TextAnchor alignment)
        {
            style.alignment = alignment;
            return style;
        }

        public static GUIStyle SetFontSize(this GUIStyle style, int fontSize)
        {
            style.fontSize = fontSize;
            return style;
        }

        public static GUIStyle SetFontStyle(this GUIStyle style, FontStyle fontStyle)
        {
            style.fontStyle = fontStyle;
            return style;
        }

        public static GUIStyle SetPadding(this  GUIStyle style, RectOffset padding)
        {
            style.padding = padding;
            return style;
        }

        public static GUIStyle SetTextColor(this GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            return style;
        }
    }
}
