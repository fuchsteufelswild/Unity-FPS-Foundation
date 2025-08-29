using UnityEngine;

namespace Nexora.Editor
{
    /// <summary>
    /// Methods to create <see cref="GUIContent[]"/> for specific array of objects.
    /// </summary>
    public static partial class DropdownContentBuilder
    {
        /// <summary>
        /// Creates array of <see cref="GUIContent"/> to draw in a dropdown.
        /// </summary>
        /// <param name="nullElementText">Visible in the dropdown when item is null.</param>
        /// <param name="includeIcons">Should icons shown?</param>
        public static GUIContent[] CreateOptions(Definition[] definitions, string nullElementText, bool includeIcons)
        {
            if (definitions == null)
            {
                return new[] { new GUIContent(nullElementText ?? "None") };
            }

            var options = new GUIContent[definitions.Length + 1];
            options[0] = new GUIContent(nullElementText ?? "None");

            for (int i = 0; i < definitions.Length; i++)
            {
                Definition definition = definitions[i];
                string displayName = definition.DisplayName ?? definition.Name ?? $"Definition {i}";
                Texture2D icon = includeIcons ? definition.Icon?.texture : null;
                string tooltip = definition.Description;

                options[i + 1] = new GUIContent(displayName, icon, tooltip);
            }

            return options;
        }
    }
}