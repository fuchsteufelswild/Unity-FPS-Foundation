using Nexora.Editor;
using System;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Nexora.InventorySystem.Editor
{
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionEditor : DefinitionEditor<ItemDefinition>
    {
        protected override Action<SerializedProperty> GetDrawAction() => Draw;

        private void Draw(SerializedProperty property)
        {
            if(property.name == "_itemActions")
            {
                DrawItemActions(property);
            }
            else
            {
                ToolboxEditorGui.DrawToolboxProperty(property);
            }
        }

        private void DrawItemActions(SerializedProperty property)
        {
            ToolboxEditorGui.DrawToolboxProperty(property);

            if(Definition.BelongsToCategory == false)
            {
                return;
            }

            // Draw category actions
            using(new GUILayout.VerticalScope(EditorStyles.helpBox))
            using(new EditorGUI.DisabledScope(true))
            {
                GUILayout.Label("[Category Actions]");
                GUILayout.Space(4f);

                using(new GUILayout.HorizontalScope())
                {
                    foreach(var action in Definition.OwningCategory.BaseActions)
                    {
                        EditorGUILayout.ObjectField(action, typeof(ItemAction), false);
                    }
                }
            }

            EditorGUILayout.Space();
        }
    }
}