using UnityEngine;

namespace Nexora.FPSDemo.Movement
{
    public interface IStaminaRegenerator
    {
        /// <summary>
        /// Can stamina regenerate at the moment?
        /// </summary>
        bool CanRegenerate { get; }

        /// <summary>
        /// Pauses the abilitiy to regenerate for <paramref name="pauseDuration"/>.
        /// </summary>
        /// <param name="pauseDuration">The amount of time to block regeneration (in seconds).</param>
        void PauseRegeneration(float pauseDuration);

        /// <summary>
        /// Update for stamina regenerator, might contain any specific or advanced behaviour.
        /// </summary>
        void Update();
    }

    public class StaminaRegenerator : IStaminaRegenerator
    {
        private float _regenerationTimer;

        public bool CanRegenerate => Time.time > _regenerationTimer;
        
        public void PauseRegeneration(float pauseDuration) => _regenerationTimer = Time.time + pauseDuration;

        public virtual void Update()
        {
            // Might be used for any specific regeneration behaviour.
        }
    }
}