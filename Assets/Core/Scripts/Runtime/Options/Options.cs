using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Nexora.Options
{
    /// <summary>
    /// Base class for options, provides implementations for saving/loading to a persistent file 
    /// and also restoring from options asset available in the Resources folder.
    /// </summary>
    /// <remarks>
    /// Each unique <see cref="Options"/> might have its own Save file and asset which is referred as the default asset.
    /// If asset for the type does not exist, then an empty one with the <see langword="class"/> defaults will be used.
    /// </remarks>
    public abstract class Options : ScriptableObject
    {
        protected const string CreateAssetMenuPath = "Nexora/Options/";

        protected virtual void Reset() { }

        protected void Save()
        {
            OptionsIOUtility.SaveOptionsFrom(this);
            Apply();
        }

        /// <summary>
        /// Override this method to use use option values, it is called after every Save/Load operation.
        /// </summary>
        protected virtual void Apply() { }

        /// <summary>
        /// Restores options using the current options file available, if not exists then creates a default instance.
        /// </summary>
        public void RestoreDefaultOptions()
        {
            Options options = OptionsIOUtility.LoadDefaultOptions(GetType());
            options.Reset();

            CopyOptions(options, this);
            Apply();

            return;

            void CopyOptions(Options source, Options target)
            {
                List<IOption> sourceOptions = ExtractOptions(source);
                List<IOption> targetOptions = ExtractOptions(target);

                for (int i = 0; i < targetOptions.Count; i++)
                {
                    targetOptions[i].Value = sourceOptions[i].Value;
                }
            }
        }

        /// <summary>
        /// Extracts the all <see cref="IOption"/> fields of the <paramref name="options"/>.
        /// </summary>
        private static List<IOption> ExtractOptions(Options options)
        {
            var optionFields = new List<IOption>();
            var allFields = options.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .OrderBy(field => field.MetadataToken);

            foreach(FieldInfo field in allFields)
            {
                if(typeof(IOption).IsAssignableFrom(field.FieldType))
                {
                    IOption option = field.GetValue(options) as IOption;
                    if(option != null)
                    {
                        optionFields.Add(option);
                    }
                }
            }

            return optionFields;
        }

        protected void Load()
        {
            OptionsIOUtility.LoadSavedOptionsInto(this);
            Apply();
        }
    }

    /// <summary>
    /// Singleton base for <see cref="Options"/> with the underlying value of type <typeparamref name="T"/>.
    /// Ensures the singleton is never null. Initializes the singleton using save file if exists, if not uses the options asset.
    /// <br></br>
    /// Create a new <see langword="class"/> with <see cref="Option{T}"/> fields deriving from this class to implement your own custom options
    /// like for in-game Graphics, Audio, Input etc. or for developer tools Debug, Profile etc..
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="Options" path="/remarks"/> 
    /// When retrieving singleton, new instance is always created and data is written onto it whether it is a save file or an asset.
    /// </remarks>
    public abstract class Options<T> : 
        Options
        where T : Options<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                EnsureSingletonNotNull();
                return _instance;
            }
        }

        protected static void EnsureSingletonNotNull()
        {
            if (_instance == null)
            {
                _instance = CreateNewInstance();
            }
        }

        /// <summary>
        /// Creates a new instance using saved options.
        /// Fallbacks to <see cref="Resources"/> asset, if save is not available.
        /// </summary>
        private static T CreateNewInstance()
        {
            bool saveExists = File.Exists(OptionsIOUtility.GetSavePath<T>());
            return saveExists
                ? CreateFromSaveFile()
                : CreateFromSavedAsset();
        }

        private static T CreateFromSaveFile()
        {
            T instance = ScriptableObject.CreateInstance<T>();
            instance.Load();
            return instance;
        }

        /// <summary>
        /// Creates a new instance using the asset in <see cref="Resources"/> folder.
        /// If asset does not exist, then creates a one and uses it.
        /// </summary>
        private static T CreateFromSavedAsset()
        {
            var instance = OptionsIOUtility.DuplicateFromDefaultOptions<T>();
            instance.Reset();
            instance.Save();
            return instance;
        }
    }
}