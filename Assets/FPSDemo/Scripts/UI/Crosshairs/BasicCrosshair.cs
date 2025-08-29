using UnityEngine;
using UnityEngine.UI;

namespace Nexora.FPSDemo.UI
{
    public sealed class BasicCrosshair : CrosshairDisplay
    {
        private Image[] _crosshairParts;

        private void Awake() => _crosshairParts = GetComponentsInChildren<Image>();

        public override void SetSize(float accuracy, float scale) { }

        public override void SetColor(Color color)
        {
            foreach(var image in _crosshairParts)
            {
                image.color = color;
            }
        }
    }
}