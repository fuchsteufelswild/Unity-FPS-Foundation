using UnityEngine;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Interface used for classes that might modify the speed of the movement. 
    /// <br></br>
    /// Like when equipped, with heavy weapons player move slower than pistols.
    /// </summary>
    public interface IMovementSpeedAdjuster
    {
        /// <summary>
        /// Modifier that can be used to modify the speed of the movement.
        /// </summary>
        CompositeValue SpeedModifier { get; }
    }
}