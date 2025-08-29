using Nexora.FPSDemo.Movement;
using Nexora.FPSDemo.SurfaceSystem;
using System.Runtime.CompilerServices;

namespace Nexora.FPSDemo.Footsteps
{
    public interface ISurfaceModifiers
    {
        /// <summary>
        /// Sets the current surface, which we will be using modifiers of.
        /// </summary>
        /// <param name="surface">New surface to use.</param>
        void SetSurface(SurfaceDefinition surface);

        /// <summary>
        /// Applies the modifiers of the surface to the controller.
        /// </summary>
        void ApplyModifiers();

        /// <summary>
        /// Removes the modifiers of the surface from the controller.
        /// </summary>
        void RemoveModifiers();
    }

    /// <summary>
    /// Class to manage the modifiers of the surface to apply/remove from <see cref="IMovementController"/>.
    /// </summary>
    public sealed class SurfaceModifiers : ISurfaceModifiers
    {
        private readonly IMovementController _movementController;
        private SurfaceDefinition _currentSurface;

        public SurfaceModifiers(IMovementController movementController) => _movementController = movementController;
        public void SetSurface(SurfaceDefinition surface) => _currentSurface = surface;

        public void ApplyModifiers()
        {
            _movementController.SpeedModifier.AddModifier(GetSpeedModifier);
            _movementController.AccelerationModifier.AddModifier(GetAccelerationModifier);
            _movementController.DecelerationModifier.AddModifier(GetDecelerationModifier);
        }

        public void RemoveModifiers()
        {
            _movementController.SpeedModifier.RemoveModifier(GetSpeedModifier);
            _movementController.AccelerationModifier.RemoveModifier(GetAccelerationModifier);
            _movementController.DecelerationModifier.RemoveModifier(GetDecelerationModifier);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetSpeedModifier() => _currentSurface?.SpeedMultiplier ?? 1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetAccelerationModifier() => _currentSurface?.Friction ?? 1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetDecelerationModifier() => _currentSurface?.Friction ?? 1f;
    }
}