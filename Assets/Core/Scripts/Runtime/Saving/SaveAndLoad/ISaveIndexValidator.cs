using System;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Interface to check for save index of the game.
    /// Provides maximum number of available save slots and their validation.
    /// </summary>
    public interface ISaveIndexValidator
    {
        int MaxSaveFiles { get; }

        bool IsValidIndex(int saveIndex);
        void ValidateIndex(int saveIndex);
    }

    [Serializable]
    public class DefaultSaveIndexValidator : ISaveIndexValidator
    {
        [SerializeField]
        private int maxSaveFiles;

        public const int DefaultMaxSaveFiles = 16;

        public int MaxSaveFiles { get; } = DefaultMaxSaveFiles;
        public DefaultSaveIndexValidator(int maxSaveFiles = DefaultMaxSaveFiles) => MaxSaveFiles = maxSaveFiles;
        public bool IsValidIndex(int saveIndex) => saveIndex >= 0 && saveIndex < MaxSaveFiles;
        public void ValidateIndex(int saveIndex)
        {
            if (IsValidIndex(saveIndex) == false)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(saveIndex),
                    saveIndex,
                    $"Save index must be between 0 and {MaxSaveFiles - 1}");
            }
        }
    }
}