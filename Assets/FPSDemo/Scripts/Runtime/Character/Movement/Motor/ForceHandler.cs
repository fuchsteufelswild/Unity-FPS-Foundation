using System;
using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Handles and combines all forces applied to the body. Accumulates forces and then with a call
    /// returns all accumulated forces and reset its state.
    /// </summary>
    public sealed class ForceHandler : IForceHandler
    {
        private readonly CharacterMotorConfig _motorConfig;

        private Vector3 _accumulatedExternalForce;
        private bool _disableSnapToGround;

        public ForceHandler(CharacterMotorConfig characterMotorConfig) => _motorConfig = characterMotorConfig;

        public bool ShouldDisableSnapToGround() => _disableSnapToGround;

        public void AddForce(Vector3 force, ForceMode forceMode, bool snapToGround)
        {
            _accumulatedExternalForce += forceMode switch
            {
                ForceMode.Force => force * (1f / _motorConfig.Mass),
                ForceMode.Acceleration => force * Time.deltaTime,
                ForceMode.Impulse => force * (1f / _motorConfig.Mass),
                ForceMode.VelocityChange => force,
                _ => throw new ArgumentOutOfRangeException(nameof(forceMode), forceMode, null)
            };

            _disableSnapToGround = !snapToGround;
        }

        public Vector3 ConsumeExternalForce()
        {
            Vector3 force = _accumulatedExternalForce;
            Reset();
            return force;
        }

        public void Reset()
        {
            _accumulatedExternalForce = Vector3.zero;
            _disableSnapToGround = false;
        }
    }
}