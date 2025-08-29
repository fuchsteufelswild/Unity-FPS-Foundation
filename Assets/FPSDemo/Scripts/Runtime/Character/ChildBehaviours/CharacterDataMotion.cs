using Nexora.Motion;
using UnityEngine;

namespace Nexora.FPSDemo
{
    public abstract class CharacterDataMotion<TMotionData> : 
        DataMotion<ICharacterBehaviour, ICharacter, TMotionData>
        where TMotionData : class, IMotionData
    {

    }
}