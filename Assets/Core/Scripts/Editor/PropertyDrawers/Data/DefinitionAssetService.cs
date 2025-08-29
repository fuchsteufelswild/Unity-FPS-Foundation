using Codice.Client.BaseCommands;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    /// <summary>
    /// Class to handle functionalities of Creating/Deleting/Refreshing <see cref="Definition"/> assets.
    /// All different <see cref="Definition"/> of different generic types, has their own folder for their assets.
    /// And they are all placed under the <b>Resources</b> folder.
    /// <br></br>
    /// <see cref="Definition"/> assets are saved without the suffix <b>Definition</b>, just plain name, any
    /// suffix of <b>Definition</b> is stripped out.
    /// </summary>
    public static class DefinitionAssetService
    {
        private const string FallbackCreationPath = "Assets/TEMP/Resources/Definitions";
        private const string ResourcesFolderName = "Resources";
        private const string DefinitionsFolderName = "Definitions";
        private const string AssetExtension = ".asset";
        private const string DefinitionSuffix = "Definition";

        /// <summary>
        /// Creates a new <see cref="Definition{T}"/> from the template.
        /// If a template is not available, an empty instnace is created.
        /// </summary>
        /// <remarks>
        /// The definition is created, registered, validated, but not <b>SAVED to the disk</b>.
        /// </remarks>
        /// <typeparam name="T">Type of the definition.</typeparam>
        /// <param name="template">Template the definition will be created from.</param>
        /// <param name="nameSuffix">Suffix to be added after the name.</param>
        public static T CreateDefinitionInstance<T>(T template = null, string nameSuffix = "")
            where T : Definition<T>
        {
            T newDefinition = CreateNewDefinitionObject();
            ValidateDefinitionFields();
            DefinitionRegistry<T>.EditorUtility.AddDefinition(newDefinition);
            
            return newDefinition;

            T CreateNewDefinitionObject()
            {
                string objectName = (template != null 
                    ? template.name 
                    : typeof(T).Name) + nameSuffix;

                T definition = template != null
                    ? UnityEngine.Object.Instantiate(template)
                    : ScriptableObject.CreateInstance<T>();

                definition.name = objectName;
                return definition;
            }

            void ValidateDefinitionFields()
            {
                var validationArgs = new EditorValidationArgs(
                    validationTrigger: ValidationTrigger.Creation,
                    isInEditor: true);

                newDefinition.EditorValidate(in validationArgs);
            }
        }

        public static void SaveDefinitionToDisk<T>(T definition, string fileName)
            where T : Definition<T>
        {
            if(definition == null)
            {
                Debug.LogWarning("Cannot save null definition to the disk.");
                return;
            }

            string assetPath = GenerateUniqueAssetPath<T>(fileName);
            AssetDatabase.CreateAsset(definition, assetPath);

            Debug.Log($"Saved {typeof(T).Name} to: {assetPath}");
        }

        private static string GenerateUniqueAssetPath<T>(string fileName)
            where T : Definition<T>
        {
            string targetFolder = DetermineTargetFolder<T>();
            string proposedPath = Path.Combine(targetFolder, fileName + AssetExtension);

            return AssetDatabase.GenerateUniqueAssetPath(proposedPath);
        }

        /// <summary>
        /// Determines the target folder for the <see cref="Definition"/> of type <typeparamref name="T"/>.
        /// </summary>
        private static string DetermineTargetFolder<T>()
            where T : Definition<T>
        {
            string existingDefinitionFolder = FindExistingDefinitionFolder<T>();

            return string.IsNullOrEmpty(existingDefinitionFolder)
                ? CreateConventionalDefinitionFolder<T>()
                : existingDefinitionFolder;
        }

        /// <summary>
        /// It gets the existing definition folder if previously any <see cref="Definition"/>
        /// of type <typeparamref name="T"/> placed in any folder.
        /// <br></br>
        /// Returns empty string if no <see cref="Definition{T}"/> is found.
        /// </summary>
        /// <remarks>
        /// We assume that all <see cref="Definition"/> of the same type as <see cref="Definition{T}"/>
        /// will go into the same folder. E.g all items are in the same folder.
        /// <b>It will get the directory of the first <see cref="Definition{T}"/> found.</b>
        /// </remarks>
        private static string FindExistingDefinitionFolder<T>()
            where T : Definition<T>
        {
            var existingDefinitions = DefinitionRegistry<T>.AllDefinitions;
            if(existingDefinitions.IsEmpty())
            {
                return string.Empty;
            }

            string firstDefinitionPath = AssetDatabase.GetAssetPath(existingDefinitions[0]);

            return string.IsNullOrEmpty(firstDefinitionPath)
                ? string.Empty
                : Path.GetDirectoryName(firstDefinitionPath);
        }

        /// <summary>
        /// Creates a definition folder for the <see cref="Definition{T}"/>. If any
        /// <b>Resources</b> folder already available, then it puts it under it through
        /// the path <see cref="DefinitionsFolderName"/> followed by the name of the definition without suffix.
        /// <br></br>
        /// If <b>Resources</b> folder is not available, it places it under default folder <see cref="FallbackCreationPath"/>.
        /// </summary>
        /// <remarks>
        /// It recursively creates folders for the found path if any disconnection is present, 
        /// so to not have any broken folder structure. So the returned path is always valid.
        /// </remarks>
        private static string CreateConventionalDefinitionFolder<T>()
            where T : Definition<T>
        {
            string resourcesFolder = 
                AssetDatabaseUtils.FindFoldersContainingName(ResourcesFolderName).FirstOrDefault();

            string definitionTypeName = ExtractDefinitionTypeName();
            
            string targetPath = string.IsNullOrEmpty(resourcesFolder) == false
                ? Path.Combine(resourcesFolder, DefinitionsFolderName, definitionTypeName)
                : Path.Combine(FallbackCreationPath, definitionTypeName);

            AssetDatabaseUtils.EnsureFolderExists(targetPath);

            return targetPath;

            // Removes the last part of "Definition" from the name
            // FireSwordDefinition -> FireSword
            string ExtractDefinitionTypeName()
            {
                string typeName = typeof(T).Name;

                return typeName.EndsWith(DefinitionSuffix)
                    ? typeName.Substring(0, typeName.Length - DefinitionSuffix.Length)
                    : typeName;
            }
        }

        /// <summary>
        /// Removes <paramref name="definition"/> from <see cref="DefinitionRegistry{T}"/>
        /// and deletes it from the disk.
        /// </summary>
        private static void DeleteDefinitionCompletely<T>(T definition)
            where T : Definition<T>
        {
            if(definition == null)
            {
                Debug.LogWarning("Cannot delete null definition.");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(definition);
            DefinitionRegistry<T>.EditorUtility.RemoveDefinition(definition);
            UnityEngine.Object.DestroyImmediate(definition, allowDestroyingAssets: true);

            if(string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.MoveAssetToTrash(assetPath);
            }

            Debug.Log($"Deleted {definition.name} from: {assetPath}");
        }

        /// <summary>
        /// Refreshes all available <see cref="Definition{T}"/> in the project,
        /// that are under <b>Resources</b> folder. Calls <see cref="Definition.RunValidation"/>
        /// on each of them irrespective of the type.
        /// </summary>
        private static void RefreshAllDefinitionsInProject()
        {
            var allDefinitions = Resources.LoadAll<Definition>(string.Empty);

            foreach (var definition in allDefinitions)
            {
                definition.RunValidation();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Refreshed and validated all found {allDefinitions.Length} definitions.");
        }
    }
}
