using Toolbox.Editor;
using UnityEditor;

namespace Nexora.UI.Editor
{
    [CustomEditor(typeof(InteractiveUIElement))]
    public sealed class InteractiveUIElementEditor : ToolboxEditor
    {
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            InteractiveUIElement element = (InteractiveUIElement)target;
            if(element.TryGetComponentInParent<NavigationGroupBase>(out _) == false)
            {
                EditorGUILayout.HelpBox($"No component of type {nameof(NavigationGroupBase)} is found" +
                    $" in the parent object. Navigation will be disabled for this object", UnityEditor.MessageType.Info);
            }
        }
    }
}