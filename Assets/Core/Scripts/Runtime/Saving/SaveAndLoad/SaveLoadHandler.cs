using OdinSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nexora.SaveSystem
{
    /// <summary>
    /// A game save consists of a scene data and a metadata.
    /// </summary>
    public enum SaveParts
    {
        /// <summary>
        /// Actual save data of the game, contains objects.
        /// </summary>
        SceneData,

        /// <summary>
        /// Metadata of the save, contains general information to 
        /// show saves in the UI and information to load them.
        /// </summary>
        Metadata
    }

    /// <summary>
    /// Controls saving/loading game data. Working with exceptions just in case of failure
    /// of disk operations.
    /// </summary>
    public class SaveLoadHandler
    {
        private readonly SaveFileOperations _fileOperations;

        public int MaxSaveFiles 
            => _fileOperations.SaveIndexValidator?.MaxSaveFiles ?? DefaultSaveIndexValidator.DefaultMaxSaveFiles;

        public SaveLoadHandler(
            IFileSystem fileSystem = null,
            ISerializer serializer = null,
            ISavePathProvider savePathProvider = null,
            ISaveIndexValidator saveIndexValidator = null)
        {
            _fileOperations = new SaveFileOperations(fileSystem, serializer, savePathProvider, saveIndexValidator);
        }

        public bool SaveFileExists(int saveIndex) => _fileOperations.SaveFileExists(saveIndex);

        public SaveResult SaveGame(GameSaveData gameSaveData, int saveIndex)
        {
            if(gameSaveData == null)
            {
                return SaveResult.Failure("Game save data cannot be null");
            }

            try
            {
                _fileOperations.SaveData(gameSaveData.SceneSaveDataArray, saveIndex, SaveParts.SceneData);
                _fileOperations.SaveData(gameSaveData.SaveMetadata, saveIndex, SaveParts.Metadata);

                return SaveResult.Success();
            }
            catch (Exception ex)
            {
                return SaveResult.Failure($"Failed to save game: {ex.Message}");
            }
        }
            
        public LoadResult<GameSaveData> LoadGame(int saveIndex)
        {
            try
            {
                var metadata = _fileOperations.LoadData<GameSaveMetadata>(saveIndex, SaveParts.Metadata);
                var sceneData = _fileOperations.LoadData<SceneSaveData[]>(saveIndex, SaveParts.SceneData);

                if(metadata == null || sceneData == null)
                {
                    return LoadResult<GameSaveData>.Failure($"Save file {saveIndex} not found or corrupted");
                }

                var gameData = new GameSaveData(metadata, sceneData);
                return LoadResult<GameSaveData>.Success(gameData);
            }
            catch (Exception ex)
            {
                return LoadResult<GameSaveData>.Failure($"Failed to load game: {ex.Message}");
            }
        }

        public LoadResult<List<GameSaveMetadata>> LoadAllMetadata(int? maxCount = null)
        {
            int count = maxCount ?? MaxSaveFiles;
            var metadataList = new List<GameSaveMetadata>();
            var errors = new List<string>();

            for (int i = 0; i < count; i++)
            {
                LoadResult<GameSaveMetadata> metadataLoadResult = LoadMetadata(i);

                if (metadataLoadResult.Succesful)
                {
                    metadataList.Add(metadataLoadResult.Data);
                }
                else
                {
                    errors.Add($"Failed to load metadata at slot {i}: {metadataLoadResult.ErrorMessage}");
                }
            }

            if (errors.IsEmpty() == false)
            {
                Debug.LogWarning($"Some metadata failed to load {string.Join(", ", errors)}");
            }

            return LoadResult<List<GameSaveMetadata>>.Success(metadataList);
        }

        public LoadResult<GameSaveMetadata> LoadMetadata(int saveIndex)
        {
            try
            {
                var metadata = _fileOperations.LoadData<GameSaveMetadata>(saveIndex, SaveParts.Metadata);

                return metadata != null
                    ? LoadResult<GameSaveMetadata>.Success(metadata)
                    : LoadResult<GameSaveMetadata>.Failure($"Metadata for save {saveIndex} not found or corrupted");
            }
            catch(Exception ex)
            {
                return LoadResult<GameSaveMetadata>.Failure($"Failed to load metadata: {ex.Message}");
            }
        }

        public SaveResult DeleteSave(int saveIndex)
        {
            try
            {
                _fileOperations.DeleteSaveFile(saveIndex);
                return SaveResult.Success();
            }
            catch(Exception ex)
            {
                return SaveResult.Failure($"Failed to save file at index {saveIndex}: {ex.Message}");
            }
        }

        public SaveSlotInfo[] GetAllSaveSlotInfo()
        {
            var slots = new SaveSlotInfo[MaxSaveFiles];

            for(int i = 0; i < MaxSaveFiles; i++)
            {
                var exists = SaveFileExists(i);
                LoadResult<GameSaveMetadata> metadataLoadResult = exists ? LoadMetadata(i) : null;

                slots[i] = new SaveSlotInfo()
                {
                    Index = i,
                    Exists = exists,
                    Metadata = metadataLoadResult.Data
                };
            }

            return slots;
        }

        /// <summary>
        /// Class to handle all file operations that should be conducted for Save/Load operations.
        /// Operates to handle saves with regard to their <b>indices</b>.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="IFileSystem"/>, <see cref="ISerializer"/>, <see cref="ISavePathProvider"/>,
        /// <see cref="ISaveIndexValidator"/> classes for handling encapsulate functionalities. Uses
        /// default implementations of none provdided.
        /// </remarks>
        private sealed class SaveFileOperations
        {
            private readonly IFileSystem _fileSystem;
            private readonly ISerializer _serializer;
            private readonly ISavePathProvider _savePathProvider;
            private readonly ISaveIndexValidator _saveIndexValidator;

            public ISaveIndexValidator SaveIndexValidator => _saveIndexValidator;

            public SaveFileOperations(
                IFileSystem fileSystem = null,
                ISerializer serializer = null,
                ISavePathProvider savePathProvider = null,
                ISaveIndexValidator saveIndexValidator = null)
            {
                _fileSystem = fileSystem ?? new UnityFileSystem();
                _serializer = serializer ?? new OdinSerializer();
                _savePathProvider = savePathProvider ?? new DefaultSavePathProvider();
                _saveIndexValidator = saveIndexValidator ?? new DefaultSaveIndexValidator();

                EnsureSaveDirectoryExists();
            }

            private void EnsureSaveDirectoryExists()
            {
                if (_fileSystem.DirectoryExists(_savePathProvider.SaveDirectory) == false)
                {
                    _fileSystem.CreateDirectory(_savePathProvider.SaveDirectory);
                }
            }

            public bool SaveFileExists(int saveIndex)
            {
                if (_saveIndexValidator.IsValidIndex(saveIndex) == false)
                {
                    return false;
                }

                return _fileSystem.FileExists(_savePathProvider.GetSavePathByType(SaveParts.SceneData, saveIndex));
            }

            public void SaveData<T>(T data, int saveIndex, SaveParts fileType)
                where T : class
            {
                _saveIndexValidator.ValidateIndex(saveIndex);

                if (data == null)
                {
                    throw new ArgumentNullException(nameof(data));
                }

                try
                {
                    var filePath = _savePathProvider.GetSavePathByType(fileType, saveIndex);
                    using var stream = _fileSystem.OpenWrite(filePath);
                    _serializer.Serialize<T>(data, stream);
                }
                catch (Exception ex)
                {
                    throw new SaveLoadException($"Failed to save {fileType} data for index {saveIndex}" +
                        $" exception: {ex.Message}", ex);
                }
            }

            public T LoadData<T>(int saveIndex, SaveParts fileType)
                where T : class
            {
                _saveIndexValidator.ValidateIndex(saveIndex);

                var filePath = _savePathProvider.GetSavePathByType(fileType, saveIndex);

                if (_fileSystem.FileExists(filePath) == false)
                {
                    return null;
                }

                try
                {
                    var data = _fileSystem.ReadAllBytes(filePath);
                    return _serializer.Deserialize<T>(data);
                }
                catch (Exception ex)
                {
                    throw new SaveLoadException($"Failed to load {fileType} data for index {saveIndex}" +
                        $" exception: {ex.Message}", ex);
                }
            }

            public void DeleteSaveFile(int saveIndex)
            {
                _saveIndexValidator.ValidateIndex(saveIndex);

                try
                {
                    var sceneDataPath = _savePathProvider.GetSavePathByType(SaveParts.SceneData, saveIndex);
                    if (_fileSystem.FileExists(sceneDataPath))
                    {
                        _fileSystem.DeleteFile(sceneDataPath);
                    }

                    var metadataPath = _savePathProvider.GetSavePathByType(SaveParts.Metadata, saveIndex);
                    if (_fileSystem.FileExists(metadataPath))
                    {
                        _fileSystem.DeleteFile(metadataPath);
                    }
                }
                catch (Exception ex)
                {
                    throw new SaveLoadException($"Failed to delete save file {saveIndex} " +
                        $"exception: {ex.Message}", ex);
                }
            }

            public sealed class SaveLoadException : Exception
            {
                public SaveLoadException(string message) : base(message) { }
                public SaveLoadException(string message, Exception innerException)
                    : base(message, innerException) { }
            }
        }

        /// <summary>
        /// Represents a result of a save operation from <see cref="SaveLoadHandler"/>.
        /// </summary>
        public sealed class SaveResult
        {
            public bool Successful { get; }
            public string ErrorMessage { get; }

            private SaveResult(bool success, string errorMessage = null)
            {
                Successful = success;
                ErrorMessage = errorMessage;
            }

            public static SaveResult Success() => new(true);
            public static SaveResult Failure(string errorMessage) => new(false, errorMessage);
        }

        /// <summary>
        /// Represents a result of a load operation from <see cref="SaveLoadHandler"/>.
        /// </summary>
        /// <typeparam name="T">Type of the data to be loaded.</typeparam>
        public sealed class LoadResult<T>
        {
            public bool Succesful { get; }
            public T Data { get; }
            public string ErrorMessage { get; }

            public LoadResult(bool succesful, T data, string errorMessage = null)
            {
                Succesful = succesful;
                Data = data;
                ErrorMessage = errorMessage;
            }

            public static LoadResult<T> Success(T data) => new(true, data);
            public static LoadResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
        }


        public sealed class SaveSlotInfo
        {
            public int Index { get; set; }
            public bool Exists { get; set; }
            public GameSaveMetadata Metadata { get; set; }
        }
    }
}