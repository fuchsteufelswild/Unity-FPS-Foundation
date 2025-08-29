using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.FPSDemo.Options;
using Nexora.FPSDemo.ProceduralMotion;
using Nexora.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nexora.FPSDemo
{
    public interface IFirstPersonLeanInputHandler : ICharacterBehaviour
    {

    }

    public sealed class FirstPersonLeanInput :
        InputHandler<ICharacterBehaviour, ICharacter>,
        IFirstPersonLeanInputHandler
    {
        [SerializeField]
        private InputActionReference _leanInput;

        private IBodyLeanController _bodyLeanController;

        protected override void OnBehaviourStart(ICharacter parent) => _bodyLeanController = parent.GetCC<IBodyLeanController>();

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            _bodyLeanController.SetLeanState(LeanState.Center);
            _leanInput.IncrementUsageCount();
        }

        protected override void OnBehaviourDisable(ICharacter parent)
        {
            _bodyLeanController.SetLeanState(LeanState.Center);
            _leanInput.DecrementUsageCount();
        }

        private void Update()
        {
            LeanState targetState = (LeanState)Mathf.CeilToInt(_leanInput.action.ReadValue<float>());

            if (InputOptions.Instance.LeanToggleMode)
            {
                HandleToggleLean(targetState);
            }
            else
            {
                _bodyLeanController.SetLeanState(targetState);
            }
        }

        private void HandleToggleLean(LeanState targetState)
        {
            if(_leanInput.action.WasPressedThisFrame() == false)
            {
                return;
            }

            LeanState currentState = _bodyLeanController.CurrentLeanState;

            if(currentState == targetState)
            {
                _bodyLeanController.SetLeanState(LeanState.Center);
            }
            else if(currentState != LeanState.Center)
            {
                // If at any sides already, any input gets it back to center
                _bodyLeanController.SetLeanState(LeanState.Center);
            }
            else
            {
                _bodyLeanController.SetLeanState(targetState);
            }
        }
    }
}