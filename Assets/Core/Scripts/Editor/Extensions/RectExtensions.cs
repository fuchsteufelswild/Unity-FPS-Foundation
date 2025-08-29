using UnityEngine;
using UnityEngine.UIElements;

namespace Nexora
{
    public static class RectExtensions
    {
        // --- Padders ---
        public static Rect AddPadding(this Rect rect, float horizontalPadding, float verticalPadding)
        {
            return new Rect(
                rect.x + horizontalPadding,
                rect.y + verticalPadding,
                rect.width,
                rect.height);
        }

        public static Rect AddPadding(this Rect rect, float padding)
        {
            return new Rect(
                rect.x + padding,
                rect.y + padding,
                rect.width - 2 * padding,
                rect.height - 2 * padding);
        }

        public static Rect AddPadding(this Rect rect, float left, float right, float top, float bottom)
        {
            return new Rect(
                rect.x + left,
                rect.y + top,
                rect.width - left - top,
                rect.height - top - bottom);
        }

        public static Rect AddHorizontalPadding(this Rect rect, float padding)
        {
            return new Rect(
                rect.x + padding,
                rect.y,
                rect.width,
                rect.height);
        }

        public static Rect AddVerticalPadding(this Rect rect, float padding)
        {
            return new Rect(
                rect.x,
                rect.y + padding,
                rect.width,
                rect.height);
        }
        // 

        // --- Adders/Removers ---
        public static Rect AddWidth(this Rect rect, float amount)
            => rect.SetWidth(rect.width + amount);

        public static Rect RemoveWidth(this Rect rect, float amount)
            => rect.SetWidth(rect.width - amount);

        public static Rect AddHeight(this Rect rect, float amount)
            => rect.SetHeight(rect.height + amount);

        public static Rect RemoveHeight(this Rect rect, float amount)
            => rect.SetHeight(rect.height - amount);
        //

        // --- Shifters ---
        public static Rect ShiftRight(this Rect rect, float amount)
            => rect.SetX(rect.x + amount);

        public static Rect ShiftLeft(this Rect rect, float amount)
            => rect.SetX(rect.x - amount);

        public static Rect ShiftDown(this Rect rect, float amount)
            => rect.SetY(rect.y + amount);
        public static Rect ShiftUp(this Rect rect, float amount)
            => rect.SetY(rect.y - amount);
        //

        // --- Setters ---
        public static Rect WithX(this Rect rect, float x) => rect.SetX(x);
        public static Rect WithY(this Rect rect, float y) => rect.SetY(y);
        public static Rect WithHeight(this Rect rect, float height) => rect.SetHeight(height);
        public static Rect WithWidth(this Rect rect, float width) => rect.SetWidth(width);
        
        public static Rect SetX(this Rect rect, float value)
            => new Rect(value, rect.y, rect.width, rect.height);

        public static Rect SetY(this Rect rect, float value)
            => new Rect(rect.x, value, rect.width, rect.height);

        public static Rect SetWidth(this Rect rect, float value)
            => new Rect(rect.x, rect.y, value, rect.height);

        public static Rect SetHeight(this Rect rect, float value)
            => new Rect(rect.x, rect.y, rect.width, value);
        // 
    }
}
