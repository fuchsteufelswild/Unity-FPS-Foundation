using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    [CustomEditor(typeof(CategoryDefinition<,>))]
    public class CategoryDefinitionEditor : DefinitionEditor
    {
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            DrawProperty("_members");                
        }
    }
}