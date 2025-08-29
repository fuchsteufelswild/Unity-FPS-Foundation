using Nexora.InputSystem;
using UnityEngine;

namespace Nexora.FPSDemo
{
    public interface IPlayerInputHandler : ICharacterBehaviour
    {

    }

    public abstract class PlayerInputHandler : 
        InputHandler<ICharacterBehaviour, ICharacter>, 
        IPlayerInputHandler
    {
    }
}