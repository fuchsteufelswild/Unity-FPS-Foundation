using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    public static class EditorGUIStyles
    {
        public const float BackgroundBoxVerticalPadding = 10f;
        public const float BackgroundBoxHorizontalPadding = 18f;


        public static readonly Color RedErrorColor = new(1f, 0.5f, 0.5f, 0.75f);
        public static readonly Color YellowWarningColor = new Color(1.0f, 0.85f, 0.35f, 1.0f);
        public static readonly Color BlueInfoColor = new Color(0.8f, 0.92f, 1.065f, 0.78f);
        public static readonly Color GreenValidColor = new(0.5f, 1f, 0.5f, 0.75f);

        public static readonly Color YellowColor = new(1f, 1f, 0.8f, 0.75f);

        public static readonly Color DefaultTextColor = EditorGUIUtility.isProSkin 
                ? new Color(1, 1, 1, 0.7f) 
                : new Color(0.1f, 0.1f, 0.1f, 0.85f);

        public static readonly Color TitleBackgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f)
                : new Color(0.76f, 0.76f, 0.76f);

        public static readonly Color DefaultBoxBackgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.15f, 0.15f, 0.15f)
                : new Color(0.85f, 0.85f, 0.85f);

        public static readonly Color DefaultBoxBackgroundBorderColor = EditorGUIUtility.isProSkin
                ? new Color(0, 0, 0, 0.6f)
                : new Color(0.5f, 0.5f, 0.5f, 0.4f);

        /// <summary>
        /// Box style show tooltip in.
        /// </summary>
        public static readonly GUIStyle TipBox = new GUIStyle(EditorStyles.helpBox)
        {
            fontSize = 10,
            padding = new RectOffset(4, 4, 4, 4),
            wordWrap = true
        };

        /// <summary>
        /// Red border for error.
        /// </summary>
        public static readonly GUIStyle RedBorder = new GUIStyle(EditorStyles.textField)
        {
            normal = { background = EditorGUIUtility.Load("icons/d_console.erroricon") as Texture2D },
            border = new RectOffset(3, 3, 3, 3)
        };

        public static readonly GUIStyle Box = new GUIStyle("box");

        public static readonly GUIStyle Background = "TextField";
        public static readonly GUIStyle TextField = "TextField";

        public static readonly GUIStyle MiniLabelSuffix =
            EditorStyles.miniLabel.Duplicate()
            .SetTextAlignment(TextAnchor.MiddleRight)
            .SetFontSize(EditorStyles.miniLabel.fontSize - 1);

        public static readonly GUIStyle LargeHeaderCentered =
            EditorStyles.boldLabel.Duplicate()
            .SetTextAlignment(TextAnchor.MiddleCenter)
            .SetFontSize(EditorStyles.largeLabel.fontSize);

        public static readonly GUIStyle HelpBoxTitle =
            EditorStyles.helpBox.Duplicate()
            .SetTextAlignment(TextAnchor.MiddleCenter)
            .SetFontSize(12);

        public static readonly GUIStyle GeneralButton =
             EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).button.Duplicate()
            .SetFontSize(12)
            .SetFontStyle(FontStyle.Normal)
            .SetTextAlignment(TextAnchor.MiddleCenter)
            .SetPadding(new RectOffset(0, 0, 3, 3))
            .SetTextColor(Color.white.WithAlpha(0.85f));

        public static readonly GUIStyle RadioButton =
            EditorStyles.radioButton.Duplicate()
            .SetPadding(new RectOffset(16, 0, -2, 0));
    }
}
