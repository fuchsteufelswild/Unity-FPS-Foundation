using Nexora.Options;
using System.Diagnostics;
using UnityEngine;

namespace Nexora.FPSDemo.Options
{
    [CreateAssetMenu(menuName = CreateAssetMenuPath + "Input Options", fileName = nameof(InputOptions))]
    public sealed class InputOptions : Options<InputOptions>
    {
        public const float MinMouseSensitivity = 0.1f;
        public const float MaxMouseSensitity = 10f;
        public const float MaxMouseSmoothness = 1f;

        [SerializeField]
        private Option<bool> _runToggleMode = new(false);

        [SerializeField]
        private Option<bool> _crouchToggleMode = new(true);

        [SerializeField]
        private Option<bool> _proneToggleMode = new(true);

        [SerializeField]
        private Option<bool> _leanToggleMode = new(false);

        [SerializeField]
        private Option<bool> _aimToggleMode = new(false);

        [SerializeField]
        private Option<bool> _autoRunToggleMode = new(false); // [Revisit]

        [SerializeField]
        private Option<float> _mouseSensitivity = new(1f);

        [SerializeField]
        private Option<float> _mouseSmoothness = new(0.3f);

        [SerializeField]
        private Option<bool> _invertMouse = new(false);

        public Option<bool> RunToggleMode => _runToggleMode;
        public Option<bool> CrouchToggleMode => _crouchToggleMode;
        public Option<bool> ProneToggleMode => _proneToggleMode;
        public Option<bool> LeanToggleMode => _leanToggleMode;
        public Option<bool> AimToggleMode => _aimToggleMode;
        public Option<bool> AutoRunToggleMode => _autoRunToggleMode;

        public Option<float> MouseSensitivity => _mouseSensitivity;
        public Option<float> MouseSmoothness => _mouseSmoothness;

        public Option<bool> InvertMouse => _invertMouse;

        [Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            if(Application.isPlaying == false)
            {
                _mouseSensitivity.SetValue(Mathf.Clamp(_mouseSensitivity.Value, MinMouseSensitivity, MaxMouseSensitity));
                _mouseSmoothness.SetValue(Mathf.Clamp(_mouseSmoothness.Value, 0f, MaxMouseSmoothness));
            }
        }

    }
}