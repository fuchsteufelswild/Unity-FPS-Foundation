using System;
using System.IO;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// Interface to customize save paths for the actual save data and metadata of the game.
    /// </summary>
    public interface ISavePathProvider
    {
        string SaveDirectory { get; }

        void Initialize(string baseDirectory = null);

        string GetSavePathByType(SaveParts saveFileType, int saveIndex);
    }

    [Serializable]
    public class DefaultSavePathProvider : ISavePathProvider
    {
        private const string SaveMetadataFileExtension = ".meta";
        private const string SaveDataFileExtension = ".sav";
        private const string SaveFileName = "Save";

        public string SaveDirectory { get; private set; }

        public void Initialize(string baseDirectory = null)
        {
            SaveDirectory = baseDirectory ?? Path.Combine(Application.persistentDataPath, "Saves");
        }

        public string GetSavePathByType(SaveParts saveFileType, int saveIndex)
        {
            return saveFileType switch
            {
                SaveParts.SceneData => GetSceneDataPath(saveIndex),
                SaveParts.Metadata => GetMetadataPath(saveIndex),
                _ => throw new InvalidOperationException()
            };
        }

        private string GetSceneDataPath(int saveIndex)
        {
            return Path.Combine(SaveDirectory, $"{SaveFileName}{saveIndex}{SaveDataFileExtension}");
        }

        private string GetMetadataPath(int saveIndex)
        {
            return Path.Combine(SaveDirectory, $"{SaveFileName}{saveIndex}{SaveMetadataFileExtension}");
        }
    }
}