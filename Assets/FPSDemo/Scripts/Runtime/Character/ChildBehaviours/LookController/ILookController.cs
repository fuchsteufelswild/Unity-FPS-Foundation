using UnityEngine;

namespace Nexora.FPSDemo.CharacterBehaviours
{
    public interface ILookController : ICharacterBehaviour
    {
        /// <summary>
        /// Is look currently active? (Receiving any inputs and not manually deactivated)
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Current view angles of the character.
        /// </summary>
        Vector2 ViewAngles { get; }

        /// <summary>
        /// Resulting look input of the character, after smoothing and all inputs.
        /// </summary>
        Vector2 LookInput { get; }

        /// <summary>
        /// How much look changed in the last frame?
        /// </summary>
        Vector2 LookDelta { get; }

        /// <summary>
        /// Sets <paramref name="inputProvider"/> as primary input provider. This provider works every time and
        /// must be had for look controller to work.
        /// </summary>
        void SetPrimaryInputProvider(ILookInputProvider inputProvider);

        /// <summary>
        /// Sets <paramref name="inputProvider"/> as additional input provider. This is an optional provider that is 
        /// added after all primary look input is processed.
        /// </summary>
        void SetAdditiveInputProvider(ILookInputProvider inputProvider);

        /// <summary>
        /// Set active status of the look controller.
        /// </summary>
        void SetActive(bool active);
    }
}