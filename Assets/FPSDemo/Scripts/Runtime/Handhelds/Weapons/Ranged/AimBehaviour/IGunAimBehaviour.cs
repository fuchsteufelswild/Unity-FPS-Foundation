using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds.RangedWeapon
{
    public interface IGunAimBehaviour
    {
        /// <summary>
        /// Is currently aiming?
        /// </summary>
        bool IsAiming { get; }

        /// <summary>
        /// Multipleir for accuracy (e.g increases accuracy when aiming).
        /// </summary>
        float FireAccuracyModifier { get; }

        /// <summary>
        /// Called when aiming action is started.
        /// </summary>
        event UnityAction OnAimingStarted;

        /// <summary>
        /// Called when aiming action is stopped.
        /// </summary>
        event UnityAction OnAimingStopped;

        /// <summary>
        /// Starts aiming.
        /// </summary>
        /// <returns>Aim started successfuly.</returns>
        bool StartAiming();

        /// <summary>
        /// Stops aiming.
        /// </summary>
        /// <returns>Aim stopped successfuly.</returns>
        bool StopAiming();


        bool TryGetEffectorOfType<T>(out T effector) where T : AimEffector
        {
            effector = GetEffectorOfType<T>();
            return effector != null;
        }

        T GetEffectorOfType<T>() where T : AimEffector;
    }

    public sealed class NullAimBehaviour : IGunAimBehaviour
    {
        public static readonly NullAimBehaviour Instance = new();

        public bool IsAiming => false;
        public float FireAccuracyModifier => 1f;
        public event UnityAction OnAimingStarted { add { } remove { } }
        public event UnityAction OnAimingStopped { add { } remove { } }

        public T GetEffectorOfType<T>() where T : AimEffector
        {
            throw new NotImplementedException();
        }

        public bool StartAiming() => false;
        public bool StopAiming() => false;
    }
}