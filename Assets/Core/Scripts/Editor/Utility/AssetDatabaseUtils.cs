using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    /// <summary>
    /// Utility class to provide methods for workign with Unity <see cref="AssetDatabase"/>.
    /// </summary>
    public static class AssetDatabaseUtils
    {
        /// <summary>
        /// Finds all folders containing the string <paramref name="partialName"/> under the 
        /// <paramref name="parentDirectory"/>.
        /// </summary>
        /// <param name="partialName">The string to look for in the folder names.</param>
        /// <param name="parentDirectory">The directory under which it will be searched for the folders.</param>
        /// <returns></returns>
        public static List<string> FindFoldersContainingName(string partialName, string parentDirectory = "Assets")
        {
            var matchingFolders = new List<string>();

            string[] allFolders = AssetDatabase.GetSubFolders(parentDirectory);

            foreach(string folderPath in allFolders)
            {
                if(folderPath.Contains(partialName))
                {
                    matchingFolders.Add(folderPath);
                }
            }

            return matchingFolders;
        }

        /// <summary>
        /// Makes sure the folder with the given valid path <paramref name="folderPath"/> exists.
        /// Creates unavailable folders in the path recursively, starting from the last part to the
        /// first.
        /// </summary>
        /// <remarks>
        /// As Unity database only works with the paths starting with <b>Assets</b>, the
        /// <paramref name="folderPath"/> must start with <b>Assets</b> like Assets/Data/Audio.
        /// </remarks>
        /// <param name="folderPath">Relative path to the folder starting from the 'Assets', e.g "Assets/Data/Audio"</param>
        public static void EnsureFolderExists(string folderPath)
        {
            if(folderPath.StartsWith("Assets") == false)
            {
                Debug.LogError("Unity only works on the paths under the 'Assets' folder, " +
                    "make sure provided path starts with 'Asset'.");
                return;
            }

            // Should we need to create a new folder?
            if(AssetDatabase.IsValidFolder(folderPath) == false)
            {
                string parentFolder = Path.GetDirectoryName(folderPath);
                string folderName = Path.GetFileName(folderPath);

                // Is both parent and current folders are valid?
                if(parentFolder != null && string.IsNullOrEmpty(folderName) == false)
                {
                    EnsureFolderExists(parentFolder);
                    AssetDatabase.CreateFolder(parentFolder, folderName);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogError($"Given folder path {folderPath} is invalid.");
                }
            }
        }
    }
}
