using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.Options;
using Nexora.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nexora.FPSDemo
{
    public interface IFirstPersonMovementInputHandler : ICharacterBehaviour
    {
        public Vector2 RawMovementDirection { get; }

        public Vector3 WorldMovementDirection { get; }

        public bool IsRunningHeld { get; }
        public bool IsCrouchingHeld { get; }
        public bool IsProningHeld { get; }
        public bool IsJumpPressed { get; }

        public void ConsumeRunInput();
        public void ConsumeCrouchInput();
        public void ConsumeProneInput();
        public void ConsumeJumpInput();
    }

    public sealed class FirstPersonMovementInput :
        InputHandler<ICharacterBehaviour, ICharacter>,
        IFirstPersonMovementInputHandler,
        IMovementInputController
    {
        private const float InputActivationThreshold = 0.1f;

        [Title("Input Actions")]
        [SerializeField]
        private InputActionReference _moveAction;

        [SerializeField]
        private InputActionReference _runAction;

        [SerializeField]
        private InputActionReference _crouchAction;

        [SerializeField]
        private InputActionReference _proneAction;

        [SerializeField]
        private InputActionReference _jumpAction;

        [Title("Configuration")]
        [SerializeField, Range(0f, 1f)]
        private float _jumpInputBuffer = 0.05f;

        private Transform _characterRoot;
        private Vector2 _rawInput;

        private bool _hasRunInput;
        private bool _hasCrouchInput;
        private bool _hasProneInput;
        private bool _hasJumpInput;

        private float _jumpInputExpireTime;

        public Vector2 RawMovementDirection => _rawInput;

        public Vector3 WorldMovementDirection
            => Vector3.ClampMagnitude(_characterRoot.TransformVector(new Vector3(_rawInput.x, 0f, _rawInput.y)), 1f);

        public bool IsRunningHeld => _hasRunInput;
        public bool IsCrouchingHeld => _hasCrouchInput;
        public bool IsProningHeld => _hasProneInput;
        public bool IsJumpPressed => _hasJumpInput;

        public void ConsumeRunInput() => _hasRunInput = false;
        public void ConsumeCrouchInput() => _hasCrouchInput = false;
        public void ConsumeProneInput() => _hasProneInput = false;
        public void ConsumeJumpInput() => _hasJumpInput = false;

        protected override void Awake()
        {
            base.Awake();
            _characterRoot = transform.root;
        }

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            ResetInputStates();
            EnableAllActions();
        }

        protected override void OnBehaviourDisable(ICharacter parent)
        {
            ResetInputStates();
            DisableAllActions();
        }

        private void ResetInputStates()
        {
            _rawInput = Vector2.zero;
            _hasRunInput = false;
            _hasJumpInput = false;
            //[Revisit] Should reset crouch and prone?
            _jumpInputExpireTime = 0;
        }

        private void EnableAllActions()
        {
            _moveAction.IncrementUsageCount();
            _runAction.IncrementUsageCount();
            _crouchAction.IncrementUsageCount();
            _proneAction.IncrementUsageCount();
            _jumpAction.IncrementUsageCount();
        }

        private void DisableAllActions()
        {
            _moveAction.DecrementUsageCount();
            _runAction.DecrementUsageCount();
            _crouchAction.DecrementUsageCount();
            _proneAction.DecrementUsageCount();
            _jumpAction.DecrementUsageCount();
        }

        private void Update()
        {
            UpdateMovementInput();
            UpdateRunInput();
            UpdateCrouchInput();
            UpdateProneInput();
            UpdateJumpInput();
        }

        private void UpdateMovementInput() => _rawInput = _moveAction.action.ReadValue<Vector2>();

        private void UpdateRunInput()
        {
            if(InputOptions.Instance.RunToggleMode)
            {
                _hasRunInput = _runAction.action.triggered ? !_hasRunInput : _hasRunInput;
            }
            else
            {
                _hasRunInput = _runAction.action.ReadValue<float>() > InputActivationThreshold;
            }
        }

        private void UpdateCrouchInput()
        {
            if(InputOptions.Instance.CrouchToggleMode)
            {
                _hasCrouchInput = _crouchAction.action.triggered ? !_hasCrouchInput : _hasCrouchInput;
            }
            else
            {
                _hasCrouchInput = _crouchAction.action.ReadValue<float>() > InputActivationThreshold;
            }
        }

        private void UpdateProneInput()
        {
            if(InputOptions.Instance.ProneToggleMode)
            {
                _hasProneInput = _proneAction.action.triggered ? !_hasProneInput : _hasProneInput;
            }
            else
            {
                _hasProneInput = _proneAction.action.ReadValue<float>() > InputActivationThreshold;
            }
        } 

        private void UpdateJumpInput()
        {
            if(Time.time > _jumpInputExpireTime || _hasJumpInput == false)
            {
                bool newJumpState = _jumpAction.action.triggered;
                _hasJumpInput = newJumpState;

                if(newJumpState)
                {
                    _jumpInputExpireTime = Time.time + _jumpInputBuffer;
                }
            }
        }
    }
}