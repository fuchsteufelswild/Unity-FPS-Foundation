using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nexora.Options
{
    /// <summary>
    /// Utility class for the I/O operations regarding options. These I/O operations include 
    /// reading/writing a persistent file, getting asset from resources and providing file paths.
    /// </summary>
    /// <remarks>
    /// Each unique <see cref="Options"/> derived class has its own
    /// file saved in the root folder. Each <see cref="Options"/> save file path can be accessed through 
    /// <see cref="GetSavePath{T}()"/> or <see cref="GetSavePath(Type)"/>.
    /// </remarks>
    public static class OptionsIOUtility
    {
        public const string OptionsFolderPath = OptionsFolderName + "/";
        private const string OptionsFolderName = "Options";
        private const string OptionsFileExtension = "json";

        /// <summary>
        /// Gets the persistent save path for the given type.
        /// </summary>
        public static string GetSavePath<T>()
            where T : Options
        {
            return GetSavePath(typeof(T));
        }

        /// <summary>
        /// <inheritdoc cref="GetSavePath{T}()" path="/summary"/>
        /// </summary>
        public static string GetSavePath(Type optionsType)
        {
            AssertOptionsType(optionsType);
            return Path.Combine(GetRootSaveFolderPath(), $"{optionsType.Name}.{OptionsFileExtension}");
        }

        /// <summary>
        /// Ensures if the passed type <paramref name="optionsType"/> is derived from <see cref="Options"/>.
        /// </summary>
        private static void AssertOptionsType(Type optionsType)
        {
            Assert.IsTrue(typeof(Options).IsAssignableFrom(optionsType),
                string.Format("{0} must be a derived class {1}", nameof(optionsType), nameof(Options)));
        }

        /// <summary>
        /// All option files are saved under one root folder, returns that folder's path.
        /// </summary>
        private static string GetRootSaveFolderPath()
        {
            string folderPath = Path.Combine(Application.persistentDataPath, OptionsFolderName);

            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            return folderPath;
        }

        /// <summary>
        /// Loads the saved options file into <paramref name="options"/> if a save exists.
        /// </summary>
        public static void LoadSavedOptionsInto(Options options)
        {
            string savePath = GetSavePath(options.GetType());
            if (File.Exists(savePath) == false)
            {
                return;
            }

            LoadFromFile();

            return;

            void LoadFromFile()
            {
                string saveData = File.ReadAllText(savePath);
                JsonUtility.FromJsonOverwrite(saveData, options);
            }
        }

        /// <summary>
        /// Saves the passed <paramref name="options"/> into a file.
        /// </summary>
        public static void SaveOptionsFrom(Options options)
        {
            string saveData = JsonUtility.ToJson(options, true);
            File.WriteAllText(GetSavePath(options.GetType()), saveData);
        }

        /// <summary>
        /// Loads the options asset available with the given type. If asset does not exist, then creates an empty one.
        /// </summary>
        public static T LoadDefaultOptions<T>()
            where T : Options<T>
        {
            return LoadDefaultOptions(typeof(T)) as T;
        }

        /// <summary>
        /// <inheritdoc cref="LoadDefaultOptions{T}()" path="/summary"/>
        /// </summary>
        public static Options LoadDefaultOptions(Type optionsType)
        {
            Options asset = LoadDefaultOptionsAsset(optionsType);

            return asset != null 
                 ? asset 
                 : CreateEmptyOptionsAsset(optionsType);
        }

        private static Options LoadDefaultOptionsAsset(Type optionsType)
        {
            AssertOptionsType(optionsType);
            string optionsPath = Path.Combine(OptionsFolderName, optionsType.Name);
            return Resources.Load(optionsPath, optionsType) as Options;
        }

        private static Options CreateEmptyOptionsAsset(Type optionsType) 
            => ScriptableObject.CreateInstance(optionsType) as Options;

        /// <summary>
        /// If options asset already exists, duplicates it into a new asset. If not creates an empty asset.
        /// </summary>
        public static T DuplicateFromDefaultOptions<T>()
            where T : Options<T>
        {
            return DuplicateFromDefaultOptions(typeof(T)) as T;
        }

        /// <summary>
        /// <inheritdoc cref="DuplicateFromDefaultOptions{T}()" path="/summary"/>
        /// </summary>
        public static Options DuplicateFromDefaultOptions(Type optionsType)
        {
            Options save = LoadDefaultOptionsAsset(optionsType);

            return save != null 
                ? UnityEngine.Object.Instantiate(save) 
                : CreateEmptyOptionsAsset(optionsType);
        }
    }
}