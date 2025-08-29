using Nexora.FPSDemo.ProceduralMotion;
using UnityEngine;

namespace Nexora.FPSDemo
{
    public interface IFPSCharacter : ICharacter
    {
        CharacterMotionHandler HeadMotion { get; }
        CharacterMotionHandler HandsMotion { get; }
    }
}