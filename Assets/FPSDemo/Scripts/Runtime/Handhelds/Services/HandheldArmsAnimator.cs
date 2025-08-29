using Nexora.Motion;
using UnityEngine;
using UnityEngine.Animations;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// This class is out of use, as there are no assets could be found that are compatible for
    /// both weapons, and unarmed character.
    /// </summary>
    public sealed class HandheldArmsAnimator : 
        BaseHandheldAnimator
    {
        [SerializeField]
        private HandheldAnimator _handheldAnimator;

        private IHandheldArmsController _armsController;
        private IHandheld _handheld;
        private bool _wasGeometryVisible;

        public override bool IsObjectVisible
        {
            get => _armsController.IsVisible;
            set => _armsController.IsVisible = value;
        }

        protected override Animator GetTargetAnimator() => _armsController?.Animator;

        protected override void Awake()
        {
            _handheld = GetComponentInParent<IHandheld>();
            _armsController = _handheld.Character.GetCC<IHandheldArmsController>();

            if (_armsController == null)
            {
                enabled = false;
                return;
            }

            if (_handheldAnimator == null)
            {
                _handheldAnimator = GetComponent<HandheldAnimator>();
            }

            _holsterSpeed = GetHolsterSpeed();

            _wasGeometryVisible = _handheld.IsGeometryVisible;
            _armsController.IsVisible = _wasGeometryVisible;
        }

        protected override void OnEnable()
        {
            _armsController?.EnableArms();
            var defaultParameters = _handheldAnimator != null ? _handheldAnimator.OverrideProfile.DefaultParameters : _overrideProfile.DefaultParameters;
            TargetAnimator.runtimeAnimatorController = _overrideProfile.OverrideController;
            foreach (var parameter in _overrideProfile.DefaultParameters)
            {
                parameter.ApplyTo(TargetAnimator);
            }
        }

        private void OnDisable() => _armsController?.DisableArms();

        private void LateUpdate()
        {
            bool isGeometryVisible = _handheld.IsGeometryVisible;
            if (isGeometryVisible != _wasGeometryVisible)
            {
                _armsController.IsVisible = isGeometryVisible;
                _wasGeometryVisible = isGeometryVisible;
            }
        }
    }
}