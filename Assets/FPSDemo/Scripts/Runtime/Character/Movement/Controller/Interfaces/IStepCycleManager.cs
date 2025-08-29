using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    public interface IStepCycleManager
    {
        /// <summary>
        /// Current step cycle, 0f or 1f to determine the step (left or right).
        /// </summary>
        float StepCycle { get; }

        /// <summary>
        /// Called when a single step fully completed, one foot stepped forward.
        /// </summary>
        event UnityAction StepCycleEnded;
    }
}