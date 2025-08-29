using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    /// <summary>
    /// Helper class to handle sliding velocity.
    /// </summary>
    public class SlopeSlider : ISlopeSlider
    {
        private readonly CharacterMotorConfig _motorConfig;
        private Vector3 _slideVelocity;

        public float SlideSpeed => _motorConfig.SlideSpeed;

        public SlopeSlider(CharacterMotorConfig characterMotorConfig) => _motorConfig = characterMotorConfig;
        public void Reset() => _slideVelocity = Vector3.zero;

        public Vector3 CalculateSlideVelocity(bool isOnSlope, Vector3 groundNormal, float deltaTime)
        {
            if(isOnSlope)
            {
                Vector3 slideDirection = groundNormal + Vector3.down;
                _slideVelocity += slideDirection * (_motorConfig.SlideSpeed * deltaTime);
            }
            else
            {
                _slideVelocity = Vector3.Lerp(_slideVelocity, Vector3.zero, deltaTime * 10f);
            }

            return _slideVelocity;
        }
    }
}