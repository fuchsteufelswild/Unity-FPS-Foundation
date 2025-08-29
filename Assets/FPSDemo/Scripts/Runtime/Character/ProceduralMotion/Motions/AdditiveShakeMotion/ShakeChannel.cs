using System.Collections.Generic;
using UnityEngine;

namespace Nexora.FPSDemo.ProceduralMotion
{
    public class ShakeChannel
    {
        private readonly List<Shake> _activeShakes = new();
        private readonly Stack<Shake> _shakesPool;

        public ShakeChannel(int poolCapacity = 32)
        {
            _shakesPool = new Stack<Shake>(poolCapacity);
            for(int i = 0; i < poolCapacity; i++)
            {
                _shakesPool.Push(new Shake());
            }
        }

        /// <summary>
        /// Gets a free <see cref="Shake"/> from the pool and initializes it
        /// with given parameters.
        /// </summary>
        /// <param name="shakeDefinition">Shake to play.</param>
        /// <param name="duration">Duration of the shake.</param>
        /// <param name="intensity">Intensity of the shake.</param>
        public void AddShake(in ShakeDefinition shakeDefinition, float duration, float intensity)
        {
            if(duration < 0.01f || _shakesPool.TryPop(out Shake shake) == false)
            {
                return;
            }

            shake.Init(shakeDefinition, duration, intensity);
            _activeShakes.Add(shake);
        }

        /// <summary>
        /// Evaluates total shakes by all added shakes, and removes expired ones.
        /// </summary>
        /// <returns>Accumulated shake.</returns>
        public Vector3 Evaluate()
        {
            if(_activeShakes.IsEmpty())
            {
                return Vector3.zero;
            }

            Vector3 totalValue = Vector3.zero;

            for(int i = _activeShakes.Count - 1; i >= 0; i--)
            {
                Shake shake = _activeShakes[i];
                // Add back to pool
                if(shake.IsDone)
                {
                    _activeShakes.RemoveAt(i);
                    _shakesPool.Push(shake);
                }
                else
                {
                    totalValue += shake.Evaluate();
                }
            }

            return totalValue;
        }
    }
}