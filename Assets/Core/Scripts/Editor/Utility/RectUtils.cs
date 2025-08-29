using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    public enum UIAnchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    /// <summary>
    /// Provides methods for commonlyl used <see cref="Rect"/> modifications in the Editor.
    /// </summary>
    public static class RectUtils
    {
        /// <summary>
        /// Calculates the <see cref="Rect"/> for a small icon like 
        /// <b>Error</b>, <b>Warning</b>, <b>Info</b> with anchored 
        /// to any of anchors of <see cref="UIAnchor"/>.
        /// </summary>
        public static Rect CalculateRectForIcon(
            Rect position, 
            float iconSize, 
            float iconPadding, 
            UIAnchor anchor = UIAnchor.MiddleRight)
        {
            float x, y;

            // Calculate X position based on horizontal alignment
            switch (anchor)
            {
                case UIAnchor.TopLeft:
                case UIAnchor.MiddleLeft:
                case UIAnchor.BottomLeft:
                    x = position.x + iconPadding;
                    break;

                case UIAnchor.TopCenter:
                case UIAnchor.MiddleCenter:
                case UIAnchor.BottomCenter:
                    x = position.x + (position.width - iconSize) * 0.5f;
                    break;

                case UIAnchor.TopRight:
                case UIAnchor.MiddleRight:
                case UIAnchor.BottomRight:
                default:
                    x = position.x + position.width - iconSize - iconPadding;
                    break;
            }

            // Calculate Y position based on vertical alignment
            switch (anchor)
            {
                case UIAnchor.TopLeft:
                case UIAnchor.TopCenter:
                case UIAnchor.TopRight:
                    y = position.y + iconPadding;
                    break;

                case UIAnchor.MiddleLeft:
                case UIAnchor.MiddleCenter:
                case UIAnchor.MiddleRight:
                default:
                    y = position.y + (position.height - iconSize) * 0.5f;
                    break;

                case UIAnchor.BottomLeft:
                case UIAnchor.BottomCenter:
                case UIAnchor.BottomRight:
                    y = position.y + position.height - iconSize - iconPadding;
                    break;
            }

            return new Rect(x, y, iconSize, iconSize);
        }

        /// <summary>
        /// Calculates rect that is placed in the center of <paramref name="container"/>,
        /// and has area of <paramref name="areaWidth"/>, <paramref name="areaHeight"/>.
        /// </summary>
        public static Rect CalculateCenteredRect(Rect container, float areaWidth, float areaHeight)
        {
            float areaAspect = areaWidth / areaHeight;
            float containerAspect = container.width / container.height;

            if (areaAspect > containerAspect)
            {
                // Width is the limiting factor, fit width to container
                float height = container.width / areaAspect;
                return new Rect(
                    container.x,
                    container.y + (container.height - height) * 0.5f,
                    container.width,
                    height);
            }
            else
            {
                // Height is the limiting factor, fit height to container
                float width = container.height * areaAspect;
                return new Rect(
                    container.x + (container.width - width) * 0.5f,
                    container.y,
                    width,
                    container.height);
            }
        }

        /// <summary>
        /// Gets <see cref="Rect"/> of the left column when the column is divided into
        /// two fields and left part takes (<paramref name="columnSplitRatio"/> * 100)%.
        /// </summary>
        public static Rect GetLeftColumnRect(Rect position, float columnSplitRatio)
        {
            return new Rect(
                position.x, 
                position.y, 
                position.width * columnSplitRatio, 
                position.height);
        }

        /// <summary>
        /// Gets <see cref="Rect"/> of the right column when the column is divided into
        /// two fields and left part takes (<paramref name="columnSplitRatio"/> * 100)%.
        /// </summary>
        public static Rect GetRightColumnRect(Rect position, float columnSplitRatio)
        {
            float leftColumnWidth = position.width * columnSplitRatio;
            return new Rect(
                position.x + leftColumnWidth, 
                position.y,
                position.width - leftColumnWidth, 
                position.height);
        }

        public static void MoveToNextLine(ref Rect position)
        {
            position.y += EditorGUIUtils.SingleLineHeight;
        }

        /// <summary>
        /// Might be used after the use of <see cref="UnityEditor.EditorGUI.PrefixLabel(Rect, int, GUIContent)"/>
        /// to realign field correctly with other components.
        /// </summary>
        public static Rect AlignFieldWithHeader(Rect position, float labelExpansion)
        {
            return new Rect(
                position.x - labelExpansion,
                position.y,
                position.width + labelExpansion,
                position.height);
        }

        public static Rect GetNextColumn(Rect position, float padding)
        {
            Rect newColumn = position;
            newColumn.x = position.xMax + padding;
            return newColumn;
        }

        public static Rect GetNextRow(Rect position, float padding)
        {
            Rect newRow = position;
            newRow.y = position.yMax + padding;
            return newRow;
        }
    }
}
