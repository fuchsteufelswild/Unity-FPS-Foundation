using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nexora.UI
{
    [RequireComponent(typeof(Slider))]
    [AddComponentMenu(ComponentMenuPaths.BaseUI + nameof(ValueSliderDisplay))]
    public sealed class ValueSliderDisplay : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _valueText;

        [SerializeField]
        private string _valueSuffix;

        private Slider _slider;

        private void OnEnable()
        {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(UpdateValueText);
        }

        private void OnDisable() => _slider.onValueChanged.RemoveListener(UpdateValueText);

        private void UpdateValueText(float value)
            => _valueText.text = value.ToString(_slider.wholeNumbers ? "F0" : "F2") + _valueSuffix;
    }
}