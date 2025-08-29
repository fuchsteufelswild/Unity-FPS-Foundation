using System;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    using Object = UnityEngine.Object;

    public interface IPanel
    {
        void RenderPanel(Rect position = default);
    }

    /// <summary>
    /// A panel that renders a Unity object(<see cref="Object"/>) inspector within a UI area.
    /// </summary>
    public class ObjectInspectorPanel : Nexora.Editor.IPanel
    {
        private UnityEditorInspectorWrapper _inspector;
        private readonly string _panelDisplayTitle;

        public Object InspectedObject => _inspector.PrimaryInspectionTarget;
        public string PanelTitle => _panelDisplayTitle;


        public ObjectInspectorPanel(Object targetObject)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException("Cannot create inspector for null objects.");
            }

            _inspector = new UnityEditorInspectorWrapper(targetObject);
            _panelDisplayTitle = ObjectNames.NicifyVariableName(targetObject.GetType().Name);
        }

        /// <summary>
        /// Renders the inspector within the given area of <paramref name="position"/>
        /// </summary>
        /// <param name="position">Area where the inspector will be drawn in.</param>
        public void RenderPanel(Rect position = default)
        {
            GUILayout.Label(_panelDisplayTitle, EditorGUIStyles.HelpBoxTitle);
            _inspector.RenderInspectorUI(EditorStyles.helpBox);
        }
    }
}