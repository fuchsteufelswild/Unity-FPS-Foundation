using Nexora.FPSDemo.CharacterBehaviours;
using Nexora.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nexora.FPSDemo
{
    public interface IFirstPersonLookInputHandler : ICharacterBehaviour
    {

    }

    [RequireCharacterBehaviour(typeof(ILookController))]
    public sealed class FirstPersonLookInput :
        InputHandler<ICharacterBehaviour, ICharacter>,
        IFirstPersonLookInputHandler,
        ILookInputProvider
    {
        [SerializeField]
        private InputActionReference _lookInput;

        private ILookController _lookController;

        protected override void OnBehaviourStart(ICharacter parent) => _lookController = parent.GetCC<ILookController>();

        protected override void OnBehaviourEnable(ICharacter parent)
        {
            _lookInput.IncrementUsageCount();
            _lookController.SetPrimaryInputProvider(this);
        }

        protected override void OnBehaviourDisable(ICharacter parent)
        {
            _lookInput.DecrementUsageCount();
            _lookController.SetPrimaryInputProvider(null);
        }

        public Vector2 GetLookInput()
        {
#if UNITY_EDITOR
            if (Time.timeSinceLevelLoad < 0.2f)
            {
                return Vector2.zero;
            }
#endif

            Vector2 lookInput = _lookInput.action.ReadValue<Vector2>();
            (lookInput.x, lookInput.y) = (lookInput.y, lookInput.x);
            return lookInput;
        }
    }
}