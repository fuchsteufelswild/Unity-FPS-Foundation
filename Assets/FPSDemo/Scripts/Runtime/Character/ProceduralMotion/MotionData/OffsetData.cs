using Nexora.Motion;
using System;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    /// <summary>
    /// Represents a data for a static offset, and has impulses when entering/exiting state.
    /// </summary>
    [Serializable]
    public sealed class OffsetData : IMotionData
    {
        public SpringSettings PositionSpring = SpringSettings.Default;
        public SpringSettings RotationSpring = SpringSettings.Default;

        public SpringImpulseDefinition StateEnterForce = SpringImpulseDefinition.Default;
        public SpringImpulseDefinition StateExitForce = SpringImpulseDefinition.Default;

        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
    }
}