using UnityEngine;

namespace Nexora.FPSDemo
{
    public interface ICharacterBehaviour : IChildBehaviour<ICharacterBehaviour, ICharacter>
    {

    }

    public abstract class CharacterBehaviour : ChildBehaviour<ICharacterBehaviour, ICharacter>
    {
    }
}