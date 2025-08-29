using System;
using UnityEngine;

namespace Nexora.Motion
{
    [CreateAssetMenu(menuName = "Nexora/Procedural Motion/Shake Profile", fileName = "Shake_")]
    public sealed class ShakeProfile : ScriptableObject
    {
        public ShakeDefinition PositionShake = ShakeDefinition.Default;

        public ShakeDefinition RotationShake = ShakeDefinition.Default;
    }
}
