using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Nexora.UI
{
    /// <summary>
    /// A <see cref="LayoutGroup"/> that dynamically controls row/column count of the layout.
    /// It places how many element it can onto a row before going to the next column, and 
    /// object sizes are not fixed as well.
    /// <br></br>
    /// It uses <see cref="AlignmentHelper"/>, <see cref="RowBuilder"/>, <see cref="ChildPositioner"/>
    /// inner helper classes.
    /// </summary>
    /// <remarks>
    /// It is like <see cref="GridLayoutGroup"/> but without a element size constraint, and
    /// not predefined row/column count.
    /// </remarks>
    public class DynamicLayoutGroup : LayoutGroup
    {
        [SerializeField]
        private float _spacing = 0f;

        [SerializeField]
        private bool _forceExpandChildWidth = false;

        [SerializeField]
        private bool _forceExpandChildHeight = false;

        /// <summary>
        /// Layouts total height by all rows combined.
        /// </summary>
        private float _calculatedLayoutHeight = 0f;

        private AlignmentHelper _alignmentHelper;
        private RowBuilder _rowBuilder;
        private ChildPositioner _childPositioner;

        protected override void Awake()
        {
            base.Awake();
            InitializeHelpers();
        }

        private void InitializeHelpers()
        {
            _alignmentHelper = new AlignmentHelper(this);
            _rowBuilder = new RowBuilder(this, _alignmentHelper);
            _childPositioner = new ChildPositioner(this, _alignmentHelper);
        }

        public float HorizontalPadding => m_Padding.left + m_Padding.right;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            float largestChildMinWidth = rectChildren.Max(child => LayoutUtility.GetMinWidth(child));
            float minWidth = largestChildMinWidth + HorizontalPadding;
            SetLayoutInputForAxis(minWidth, -1, -1, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            _calculatedLayoutHeight = CalculateTotalLayoutHeight();

            SetLayoutInputForAxis(_calculatedLayoutHeight, _calculatedLayoutHeight, -1, 1);
        }

        private float CalculateTotalLayoutHeight()
        {
            EnsureHelpers();

            float workingWidth = GetWorkingWidth();
            List<LayoutRow> rows = _rowBuilder.BuildRows(rectChildren, workingWidth);
            return CalculateTotalHeight(rows);
        }

        /// <summary>
        /// Width of the <see cref="RectTransform"/> where we can place elements.
        /// </summary>
        private float GetWorkingWidth() => rectTransform.rect.width - HorizontalPadding;

        private void EnsureHelpers()
        {
            if (_alignmentHelper == null)
            {
                InitializeHelpers();
            }
        }

        private float CalculateTotalHeight(List<LayoutRow> rows)
        {
            float totalHeight = padding.bottom + padding.top;

            for (int i = 0; i < rows.Count; i++)
            {
                if (i > 0)
                {
                    totalHeight += _spacing;
                }
                totalHeight += rows[i].Height;
            }

            return totalHeight;
        }

        public override void SetLayoutHorizontal() => PerformLayout(0);
        public override void SetLayoutVertical() => PerformLayout(1);

        private void PerformLayout(int axis)
        {
            EnsureHelpers();

            float workingWidth = GetWorkingWidth();
            List<LayoutRow> rows = _rowBuilder.BuildRows(rectChildren, workingWidth);
            _childPositioner.PositionChildrenIntoRows(rows, workingWidth, _calculatedLayoutHeight, axis);
        }


        /// <summary>
        /// Helper class to calculate offset and positions depending on the alignment of elements.
        /// </summary>
        private sealed class AlignmentHelper
        {
            private readonly DynamicLayoutGroup _layoutGroup;

            public bool IsCenterAligned => IsAlignment(TextAnchor.LowerCenter, TextAnchor.MiddleCenter, TextAnchor.UpperCenter);
            public bool IsRightAligned => IsAlignment(TextAnchor.LowerRight, TextAnchor.MiddleRight, TextAnchor.UpperRight);
            public bool IsMiddleAligned => IsAlignment(TextAnchor.MiddleLeft, TextAnchor.MiddleCenter, TextAnchor.MiddleRight);
            public bool IsLowerAligned => IsAlignment(TextAnchor.LowerLeft, TextAnchor.LowerCenter, TextAnchor.LowerRight);

            private bool IsAlignment(params TextAnchor[] anchors) => anchors.Contains(_layoutGroup.m_ChildAlignment);

            public AlignmentHelper(DynamicLayoutGroup layoutGroup) => _layoutGroup = layoutGroup;

            public float CalculateRowHorizontalOffset(float availableWidth, float usedWidth, bool forceExpand)
            {
                if (forceExpand)
                {
                    return 0f;
                }

                if (IsCenterAligned)
                {
                    return (availableWidth - usedWidth) * 0.5f;
                }

                if (IsRightAligned)
                {
                    return availableWidth - usedWidth;
                }

                return 0f;
            }

            public float CalculateRowVerticalOffset(
                float containerHeight, 
                float totalLayoutHeight, 
                float currentOffset, 
                float rowHeight)
            {
                if (IsLowerAligned)
                {
                    return containerHeight - currentOffset - rowHeight;
                }

                if (IsMiddleAligned)
                {
                    return (containerHeight - totalLayoutHeight) * 0.5f + currentOffset;
                }

                return currentOffset;
            }

            public float CalculateChildVerticalPosition(float rowTop, float rowHeight, float childHeight)
            {
                if(IsMiddleAligned)
                {
                    return rowTop + (rowHeight - childHeight) * 0.5f;
                }

                if(IsLowerAligned)
                {
                    return rowTop + (rowHeight - childHeight);
                }

                return rowTop;
            }
        }

        /// <summary>
        /// Helper class to build rows one after another in for the layout.
        /// </summary>
        private sealed class RowBuilder
        {
            private readonly DynamicLayoutGroup _layoutGroup;
            private readonly AlignmentHelper _alignmentHelper;

            public RowBuilder(DynamicLayoutGroup layoutGroup, AlignmentHelper alignmentHelper)
            {
                _layoutGroup = layoutGroup;
                _alignmentHelper = alignmentHelper;
            }

            /// <summary>
            /// Builds a list of <see cref="LayoutRow"/> and place the <paramref name="children"/> in them.
            /// </summary>
            /// <param name="children"></param>
            /// <param name="maximumRowWidth"></param>
            /// <returns>List of <see cref="LayoutRow"/> that are populated with <paramref name="children"/>.</returns>
            public List<LayoutRow> BuildRows(IList<RectTransform> children, float maximumRowWidth)
            {
                var rows = new List<LayoutRow>();
                var currentRow = new LayoutRow();

                for(int i = 0; i < children.Count; i++)
                {
                    RectTransform child = GetChildAtIndex(children, i);
                    float childWidth = Mathf.Min(LayoutUtility.GetPreferredSize(child, 0), maximumRowWidth);
                    float childHeight = LayoutUtility.GetPreferredSize(child, 1);

                    // We start a new row only if the current on is overflown
                    if(ShouldStartNewRow(currentRow, childWidth, maximumRowWidth))
                    {
                        rows.Add(currentRow);
                        currentRow = new LayoutRow();
                    }
                    
                    AddChildToRow(currentRow, child, childWidth, childHeight);
                }

                // Final row should be added as it is not added in the loop
                if(currentRow.Children.IsEmpty() == false)
                {
                    rows.Add(currentRow);
                }

                return rows;
            }

            /// <summary>
            /// Gets a child <paramref name="i"/> depending on the alignment of the row.
            /// </summary>
            private RectTransform GetChildAtIndex(IList<RectTransform> children, int i)
            {
                // Get from reverse or not
                int index = _alignmentHelper.IsLowerAligned ? children.Count - 1 - i : i;
                return children[index];
            }

            /// <summary>
            /// Checks if <paramref name="currentRow"/> which has <paramref name="maximumWidth"/> 
            /// would be overflown is we are to add a child with <paramref name="childWidth"/>.
            /// </summary>
            /// <remarks>
            /// <see cref="LayoutRow"/> already has a parameter for already filled space, so we don't need that.
            /// </remarks>
            /// <returns>Should a new row be started.</returns>
            private bool ShouldStartNewRow(LayoutRow currentRow, float childWidth, float maximumWidth)
            {
                if(currentRow.Children.IsEmpty())
                {
                    return false;
                }

                float totalWidth = currentRow.Width + _layoutGroup._spacing + childWidth;
                return totalWidth > maximumWidth;
            }

            /// <summary>
            /// Adds the <paramref name="child"/> to the <paramref name="row"/> and
            /// updates width and height of the <paramref name="row"/> accordingly.
            /// </summary>
            private void AddChildToRow(LayoutRow row, RectTransform child, float childWidth, float childHeight)
            {
                if(row.Children.IsEmpty() == false)
                {
                    row.Width += _layoutGroup._spacing;
                }

                row.Children.Add(child);
                row.Width += childWidth;
                row.Height = Mathf.Max(row.Height, childHeight);
            }
        }

        private sealed class ChildPositioner
        {
            private readonly DynamicLayoutGroup _layoutGroup;
            private readonly AlignmentHelper _alignmentHelper;

            public ChildPositioner(DynamicLayoutGroup layoutGroup, AlignmentHelper alignmentHelper)
            {
                _layoutGroup = layoutGroup;
                _alignmentHelper = alignmentHelper;
            }

            public void PositionChildrenIntoRows(List<LayoutRow> rows, float maximumRowWidth, float totalLayoutHeight, int axis)
            {
                float currentVerticalOffset = _alignmentHelper.IsLowerAligned 
                    ? _layoutGroup.padding.bottom 
                    : _layoutGroup.padding.top;

                foreach(LayoutRow row in rows)
                {
                    float rowVerticalPosition = _alignmentHelper.CalculateRowVerticalOffset(
                        _layoutGroup.rectTransform.rect.height,
                        totalLayoutHeight,
                        currentVerticalOffset,
                        row.Height);

                    PositionRowChildren(row, maximumRowWidth, rowVerticalPosition, axis);
                    currentVerticalOffset += row.Height + _layoutGroup._spacing;
                }
            }

            private void PositionRowChildren(LayoutRow row, float maximumRowWidth, float verticalPosition, int axis)
            {
                float horizontalOffset = _layoutGroup.padding.left;
                horizontalOffset += _alignmentHelper.CalculateRowHorizontalOffset(
                    maximumRowWidth, row.Width, _layoutGroup._forceExpandChildWidth);

                // Extra width for flexible elements
                float extraWidth = CalculateExtraWidth(row, maximumRowWidth);

                for(int i = 0; i < row.Children.Count; i++)
                {
                    RectTransform child = GetRowChildAtIndex(row, i);
                    ChildDimensions childDimensions = CalculateChildDimensions(
                        child, extraWidth, row.Height, maximumRowWidth);

                    float childVerticalPosition = _alignmentHelper.CalculateChildVerticalPosition(
                        verticalPosition, row.Height, childDimensions.Height);

                    if(axis == 0)
                    {
                        _layoutGroup.SetChildAlongAxis(child, 0, horizontalOffset, childDimensions.Width);
                    }
                    else
                    {
                        _layoutGroup.SetChildAlongAxis(child, 1, childVerticalPosition, childDimensions.Height);
                    }

                    horizontalOffset += childDimensions.Width + _layoutGroup._spacing;
                }
            }

            private float CalculateExtraWidth(LayoutRow row, float maximumRowWidth)
            {
                if(_layoutGroup._forceExpandChildWidth)
                {
                    return 0f;
                }

                int flexibleChildCount = 0;
                foreach(RectTransform child in row.Children)
                {
                    if(LayoutUtility.GetFlexibleWidth(child) > 0f)
                    {
                        flexibleChildCount++;
                    }
                }

                return flexibleChildCount > 0 ? (maximumRowWidth - row.Width) / flexibleChildCount : 0f;
            }

            private RectTransform GetRowChildAtIndex(LayoutRow row, int i)
            {
                int index = _alignmentHelper.IsLowerAligned ? row.Children.Count - 1 - i : i;
                return row.Children[index];
            }

            private ChildDimensions CalculateChildDimensions(RectTransform child, float extraWidth, float rowHeight, float maximumRowWidth)
            {
                float width = LayoutUtility.GetPreferredSize(child, 0);
                if(LayoutUtility.GetFlexibleWidth(child) > 0f)
                {
                    width += extraWidth;
                }
                width = Mathf.Min(width, maximumRowWidth);

                float height = _layoutGroup._forceExpandChildHeight 
                    ? rowHeight 
                    : LayoutUtility.GetPreferredSize(child, 1);

                return new ChildDimensions(width, height);
            }

            private readonly struct ChildDimensions
            {
                public readonly float Width;
                public readonly float Height;

                public ChildDimensions(float width, float height)
                {
                    Width = width;
                    Height = height;
                }
            }
        }

        /// <summary>
        /// Represents a single row in the <see cref="LayoutGroup"/>.
        /// </summary>
        private sealed class LayoutRow
        {
            public List<RectTransform> Children { get; } = new List<RectTransform>();

            public float Width { get; set; }
            public float Height { get; set; }
        }
    }
}