using System;
using Toolbox.Editor;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    public abstract class DefinitionEditor<T> :
        ToolboxEditor
        where T : Definition
    {
        private IToolboxEditorDrawer _drawer;

        protected T Definition { get; private set; }

        public sealed override IToolboxEditorDrawer Drawer => _drawer ??= new ToolboxEditorDrawer(GetDrawAction());

        /// <summary>
        /// We use this method so that we can control drawing more fine-grained.
        /// </summary>
        protected virtual Action<SerializedProperty> GetDrawAction()
            => ToolboxEditorGui.DrawToolboxProperty;

        private void OnEnable() => Definition = target as T;

        public override void DrawCustomInspector()
        {
            DrawAssetField();
            ProcessRefreshEvent();

            base.DrawCustomInspector();
        }

        protected void DrawAssetField()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Asset", target, target.GetType(), target);
            }
        }

        protected void ProcessRefreshEvent()
        {
            var evt = Event.current;
            var editorValidatable = Definition as IEditorValidatable;
            if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.F5 && editorValidatable != null)
            {
                editorValidatable.RunValidation();
            }
        }

        protected void DrawProperty(string propertyName)
        {
            var property = serializedObject.FindProperty(propertyName);
            ToolboxEditorGui.DrawToolboxProperty(property);
        }
    }

    [CustomEditor(typeof(Definition), editorForChildClasses: true, isFallback = true)]
    public class DefinitionEditor : DefinitionEditor<Definition>
    {
    }
}
