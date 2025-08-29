using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    public interface IMovementStateBlocker
    {
        /// <summary>
        /// Adds the <paramref name="blocker"/> to the <paramref name="stateType"/>.
        /// </summary>
        /// <param name="blocker">Blocker object, not doing anything just blocks.</param>
        void AddStateBlocker(Object blocker, MovementStateType stateType);

        /// <summary>
        /// Removes the <paramref name="blocker"/> from the <paramref name="stateType"/>.
        /// </summary>
        /// <param name="blocker">Blocker object, not doing anything just blocks.</param>
        void RemoveStateBlocker(Object blocker, MovementStateType stateType);
    }
}