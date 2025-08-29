using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nexora.Editor
{
    /// <summary>
    /// Drawing a box and apply paddings to the contents so that 
    /// they will render aligned within the box.
    /// </summary>
    /// <remarks>
    /// <b>If you want to draw normally after the box,
    /// then you need to retrieve old <see cref="Rect"/> using the field
    /// <see cref="InitialRect"/>.</b>
    /// </remarks>
    public class BoxContainerScope : IDisposable
    {
        private readonly RectOffset _padding;

        private readonly Rect _initialRect;

        private const float HorizontalTitlePaddingRatio = 0.3f;
        private const float VerticalTitlePaddingAmount = 4f;
        
        public Rect InitialRect => _initialRect;

        public BoxContainerScope(
            ref Rect position,
            float height,
            string boxTitle,
            RectOffset padding = null)
        {
            _initialRect = position; 
            _padding = padding ?? new RectOffset(6, 6, 4, 4);

            EditorGUIUtils.DrawHelpBoxBackground(position, height);

            Rect titleRect = new Rect(
                position.x + padding.left * HorizontalTitlePaddingRatio,
                position.y + VerticalTitlePaddingAmount,
                position.width,
                position.height);

            EditorGUI.LabelField(titleRect, boxTitle, EditorGUIStyles.LargeHeaderCentered);

            position = new Rect(
                position.x + padding.left,
                position.y + padding.top + EditorGUIUtility.singleLineHeight,
                position.width - padding.horizontal,
                position.height);
        }

        public void Dispose()
        {
            
        }
    }

    public class BoxContainerLayoutScope : IDisposable
    {
        private readonly RectOffset _padding;
        private readonly IndentedScope _indentScope;

        public BoxContainerLayoutScope(RectOffset padding = null)
        {
            _padding = padding ?? new RectOffset(6, 6, 4, 4);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            ApplyPadding(_padding);

            _indentScope = new IndentedScope(_padding.left, true, false);

            return;


            void ApplyPadding(RectOffset padding)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(padding.left);
                GUILayout.BeginVertical();
                GUILayout.Space(padding.top);
            }
        }

        public void Dispose()
        {
            _indentScope.Dispose();
            CleanupPadding();
            

            // End box
            GUILayout.EndVertical();

            void CleanupPadding()
            {
                GUILayout.Space(_padding.bottom);
                GUILayout.EndVertical();
                GUILayout.Space(_padding.right);
                GUILayout.EndHorizontal();
            }
        }
    }
}