using UnityEngine;

namespace Nexora.FPSDemo.UI
{
    public abstract class CrosshairDisplay : MonoBehaviour
    {
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public abstract void SetSize(float accuracy, float scale);
        public abstract void SetColor(Color color);
    }
}