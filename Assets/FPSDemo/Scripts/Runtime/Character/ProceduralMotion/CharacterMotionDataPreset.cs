using Nexora.FPSDemo.Movement;
using Nexora.Motion;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    [CreateAssetMenu(menuName = AssetMenuPath + nameof(CharacterMotionDataPreset), fileName = "Motion_")]
    public sealed class CharacterMotionDataPreset : MotionDataPreset<MovementStateType>
    {
        
    }
}