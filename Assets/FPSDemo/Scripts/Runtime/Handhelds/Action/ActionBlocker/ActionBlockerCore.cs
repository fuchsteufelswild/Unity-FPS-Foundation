using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    /// <summary>
    /// Keeps list of blockers that are active unless removed.
    /// </summary>
    public sealed class PersistentBlocker
    {
        private readonly HashSet<Object> _blockers = new();

        public bool IsBlocked => _blockers.IsEmpty() == false;

        /// <summary>
        /// Adds <paramref name="blocker"/> to the list of blockers.
        /// </summary>
        /// <param name="blocker">Blocker object.</param>
        /// <returns>Block status changes from <see langword="false"/> to <see langword="true"/>.</returns>
        public bool AddBlocker(Object blocker)
        {
            if (_blockers.Contains(blocker))
            {
                return false;
            }

            bool wasBlocked = IsBlocked;
            _blockers.Add(blocker);

            return wasBlocked == false;
        }
        public void RemoveBlocker(Object blocker) => _blockers.Remove(blocker);
    }

    /// <summary>
    /// A blocker that blocks actions for a specific duration of time.
    /// </summary>
    public sealed class CooldownBasedBlocker
    {
        private float _blockEndTime;

        public bool IsBlocked => _blockEndTime > Time.time;

        /// <summary>
        /// Adds cooldown (in seconds) to the blocking time.
        /// </summary>
        /// <param name="duration">Duration to block the action.</param>
        /// <returns>Block status changes from <see langword="false"/> to <see langword="true"/>.</returns>
        public bool AddCooldown(float duration)
        {
            bool resultsInBlock = false;

            float newBlockTime = Time.time + duration;
            if(newBlockTime > _blockEndTime)
            {
                resultsInBlock = _blockEndTime < Time.time;
                _blockEndTime = newBlockTime;
            }

            return resultsInBlock;
        }

        public void Clear() => _blockEndTime = 0f;
    }

    public sealed class ActionBlockerCore
    {
        private readonly PersistentBlocker _persistentBlocker = new();
        private readonly CooldownBasedBlocker _cooldownBasedBlocker = new();

        public bool IsBlocked => _persistentBlocker.IsBlocked || _cooldownBasedBlocker.IsBlocked;

        public event UnityAction Blocked;

        public void BlockFor(float duration)
        {
            if(_cooldownBasedBlocker.AddCooldown(duration))
            {
                Blocked?.Invoke();
            }
        }

        public void ClearDuration() => _cooldownBasedBlocker.Clear();

        public void AddBlocker(Object blocker)
        {
            if(_persistentBlocker.AddBlocker(blocker))
            {
                Blocked?.Invoke();
            }
        }

        public void RemoveBlocker(Object blocker) => _persistentBlocker.RemoveBlocker(blocker);
    }
}