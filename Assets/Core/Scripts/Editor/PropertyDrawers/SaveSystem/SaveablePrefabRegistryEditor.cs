using Nexora.SaveSystem;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    [CustomEditor(typeof(SaveablePrefabRegistry), true)]
    public class SaveablePrefabRegistryEditor : ToolboxEditor
    {
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            EditorGUILayout.Space();
            if(GUILayout.Button("Reload Registry"))
            {
                ((SaveablePrefabRegistry)target).SetRegisteredPrefabs(
                    SaveablePrefabRegistry.FindAllSaveableGameObjectPrefabs());
            }
        }
    }
}