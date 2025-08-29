using Nexora.FPSDemo.Movement;
using Nexora.UI;
using UnityEngine;

namespace Nexora.FPSDemo.UI.CharacterBehaviours
{
    public interface ICharacterStaminaDisplay : ICharacterUIBehaviour
    {

    }

    public sealed class CharacterStamina : 
        CharacterUIBehaviour,
        ICharacterStaminaDisplay
    {
        [SerializeField]
        private FadingProgressBar _staminaBar;

        private IStaminaController _staminaController;

        protected override void OnCharacterAttached(ICharacter character)
        {
            _staminaController = character.GetCC<IStaminaController>();

            if(_staminaController == null)
            {
                Debug.LogWarning("CharacterStamina: Not found stamina controller.");
            }
        }

        private void FixedUpdate()
        {
            if (_staminaController == null)
            {
                return;
            }

            _staminaBar.UpdateProgressBar(_staminaController.CurrentStamina, _staminaController.MaxStamina);
        }
    }
}