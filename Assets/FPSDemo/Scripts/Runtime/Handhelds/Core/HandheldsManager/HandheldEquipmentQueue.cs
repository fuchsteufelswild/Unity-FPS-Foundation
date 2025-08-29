using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nexora.FPSDemo.Handhelds
{
    public readonly struct HandheldQueueEntry
    {
        public readonly int UniqueID;
        public readonly IHandheld Handheld;
        public readonly UnityAction EquipCallback;
        public readonly DateTime QueuedAt;

        public HandheldQueueEntry(IHandheld handheld, UnityAction equipCallback = null)
        {
            UniqueID = GenerateUniqueID();
            Handheld = handheld;
            EquipCallback = equipCallback;
            QueuedAt = DateTime.UtcNow;
        }

        private static int _nextID;

        private static int GenerateUniqueID()
        {
            return System.Threading.Interlocked.Increment(ref _nextID);
        }
    }

    /// <summary>
    /// Handles stack of <see cref="IHandheld"/> to equip.
    /// </summary>
    public sealed class HandheldEquipmentQueue
    {
        private readonly List<HandheldQueueEntry> _equipQueue = new();

        /// <summary>
        /// How many entries are available in the queue?
        /// </summary>
        public int Count => _equipQueue.Count;

        /// <summary>
        /// Is queue is currently empty?
        /// </summary>
        public bool IsEmpty => _equipQueue.IsEmpty();

        /// <summary>
        /// Last added entry to the queue.
        /// </summary>
        public HandheldQueueEntry? Current => IsEmpty ? null : _equipQueue.Last();
        
        /// <summary>
        /// Invoked when a new entry is added to the queue.
        /// </summary>
        public event UnityAction<HandheldQueueEntry> EntryAdded;

        /// <summary>
        /// Invoked when an entry is removed from the queue.
        /// </summary>
        public event UnityAction<HandheldQueueEntry> EntryRemoved;

        /// <summary>
        /// Invoked when queue is cleared.
        /// </summary>
        public event UnityAction QueueCleared;

        /// <summary>
        /// Tries to push <paramref name="handheld"/> into the equipment queue.
        /// </summary>
        /// <remarks>
        /// Moves <paramref name="handheld"/> to the top of the queue if it was earlier added.
        /// </remarks>
        /// <param name="handheld">Handheld to push into the queue.</param>
        /// <param name="equipCallback">Callback that is called upon equip of the handheld.</param>
        /// <returns>If push performed successfuly (if it wasn't already active handheld).</returns>
        public bool TryPush(IHandheld handheld, UnityAction equipCallback)
        {
            if(handheld == null)
            {
                return false;
            }

            // Remove existing entry if had one, but not the first as its the 'default handheld'
            int existingIndex = FindHandheldIndex(handheld, startIndex: 1);
            if(existingIndex != CollectionConstants.NotFound)
            {
                // Already at the top, no need for a change
                if(existingIndex == _equipQueue.Count - 1)
                {
                    return false;
                }

                _equipQueue.RemoveAt(existingIndex);
            }

            var entry = new HandheldQueueEntry(handheld, equipCallback);
            _equipQueue.Add(entry);
            EntryAdded?.Invoke(entry);

            return true;
        }

        private int FindHandheldIndex(IHandheld handheld, int startIndex = 0)
        {
            for(int i = startIndex; i < _equipQueue.Count; i++)
            {
                if (_equipQueue[i].Handheld == handheld)
                {
                    return i;
                }
            }

            return CollectionConstants.NotFound;
        }

        private void RemoveAt(int index)
        {
            HandheldQueueEntry entry = _equipQueue[index];
            _equipQueue.RemoveAt(index);
            EntryRemoved?.Invoke(entry);
        }

        /// <summary>
        /// Tries to pop <paramref name="handheld"/> from the queue.
        /// </summary>
        /// <param name="handheld">Handheld to remove from the equipment queue.</param>
        /// <returns>If <paramref name="handheld"/> was the active handheld.</returns>
        public bool TryPop(IHandheld handheld)
        {
            int index = FindHandheldIndex(handheld);

            // Entry not found or it is the default handheld
            if(index <= 0)
            {
                return false;
            }

            bool wasActiveHandheld = index == _equipQueue.Count - 1;
            RemoveAt(index);

            return wasActiveHandheld;
        }

        /// <summary>
        /// Clears the queue except for the first element.
        /// </summary>
        /// <remarks>
        /// Removes every element and fires events.
        /// </remarks>
        public void Clear()
        {
            if (_equipQueue.Count <= 1)
            {
                return;
            }

            var toRemove = _equipQueue.Skip(1).ToList();
            _equipQueue.RemoveRange(1, _equipQueue.Count - 1);

            foreach(var entry in toRemove)
            {
                EntryRemoved?.Invoke(entry);
            }

            QueueCleared?.Invoke();
        }

        /// <summary>
        /// Sets <paramref name="handheld"/> as the default handheld that is always in the queue.
        /// </summary>
        /// <param name="handheld">Handheld to set as a default.</param>
        public void SetDefaultHandheld(IHandheld handheld)
        {
            var entry = new HandheldQueueEntry(handheld);

            if(_equipQueue.IsEmpty())
            {
                _equipQueue.Add(entry);
            }
            else
            {
                _equipQueue[0] = entry;
            }
        }
    }
}