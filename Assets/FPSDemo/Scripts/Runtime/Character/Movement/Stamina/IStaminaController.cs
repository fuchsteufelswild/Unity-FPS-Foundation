using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Movement
{
    public interface IStaminaController : ICharacterBehaviour
    {
        /// <summary>
        /// Current stamina amount.
        /// </summary>
        float CurrentStamina { get; }

        /// <summary>
        /// Maximum stamina that can be had.
        /// </summary>
        float MaxStamina { get; }

        /// <summary>
        /// Modifies current stamina by <paramref name="amount"/>.
        /// </summary>
        /// <param name="amount">Change in the stamina (positive or negative).</param>
        void ModifyStamina(float amount);

        /// <summary>
        /// Sets maximum stamina to be <paramref name="maxStamina"/>.
        /// </summary>
        /// <param name="maxStamina">New max stamina.</param>
        void SetMaxStamina(float maxStamina);

        /// <summary>
        /// Restores current stamina to the max stamina.
        /// </summary>
        void RestoreToMax();

        /// <summary>
        /// Event invoked when there is a change in the current stamina.
        /// </summary>
        event UnityAction<float> StaminaChanged;
    }
}