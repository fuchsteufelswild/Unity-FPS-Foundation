using Nexora.SaveSystem;
using System;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    /// <summary>
    /// Provide a view to inspect order of saving of child <see cref="ISaveableComponent"/>.
    /// </summary>
    [CustomEditor(typeof(SaveableGameObject), true)]
    public class SaveableGameObjectEditor : ToolboxEditor
    {
        private const float ScrollViewHeight = 150f;

        private readonly GUILayoutOption[] _showButtonLayoutOptions =
        {
            GUILayout.Width(50f),
            GUILayout.Height(20f)
        };

        private Vector2 _scrollPosition;
        
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            EditorGUILayout.Space();

            using (new BoxContainerLayoutScope(new RectOffset(0, 10, 0, 5)))
            {
                var allSaveables = GatherAllSaveableComponents();

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("Components to Save", EditorGUIStyles.LargeHeaderCentered);
                    ToolboxEditorGui.DrawLine();
                    DrawAllSaveables(allSaveables);
                }
            }
        }

        /// <summary>
        /// Gathers all <see cref="ISaveableComponent"/> on this object or in the children.
        /// </summary>
        /// <returns></returns>
        private ISaveableComponent[] GatherAllSaveableComponents()
        {
            Component component = target as Component;

            ISaveableComponent[] allSaveables = component?.GetComponentsInChildren<ISaveableComponent>()
                ?? Array.Empty<ISaveableComponent>();

            return allSaveables;
        }

        private void DrawAllSaveables(ISaveableComponent[] allSaveables)
        {
            if(allSaveables.IsEmpty())
            {
                EditorGUILayout.HelpBox("No saveable components available in children.", UnityEditor.MessageType.Warning);
                return;
            }

            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition, GUILayout.Height(ScrollViewHeight)))
            {
                foreach (var saveableComponent in allSaveables)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.TextField(ObjectNames.NicifyVariableName(saveableComponent.GetType().Name));
                        }

                        if (saveableComponent is Component component && GUILayout.Button("Show", _showButtonLayoutOptions))
                        {
                            EditorGUIUtility.PingObject(component);
                        }
                    }
                }

                _scrollPosition = scroll.scrollPosition;
            }
        }
    }
}
