using Nexora.Serialization;
using System;
using Toolbox.Editor;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace Nexora.Editor
{
    public class UnityGuidDrawer : ToolboxTargetTypeDrawer
    {
        public override Type GetTargetType() => typeof(UnityGuid);
        public override bool UseForChildren() => true;

        public override void OnGui(SerializedProperty property, GUIContent label)
        {
            using(new EditorGUI.DisabledScope(true))
            {
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
                if(property.isExpanded == false)
                {
                    return;
                }

                using(new GUILayout.HorizontalScope())
                {
                    DrawGuidValue(property);
                    DrawNewGuidButton(property);
                }
            }
        }

        private void DrawGuidValue(SerializedProperty property)
        {
            var guid = (UnityGuid)property.boxedValue;
            EditorGUILayout.TextField("GUID", guid.ToString());
        }

        private void DrawNewGuidButton(SerializedProperty property)
        {
            if(IsPropertyDisabled(property))
            {
                return;
            }

            var attribute = PropertyUtility.GetAttribute<UnityGuidUIOptionsAttribute>(property);
            if(attribute?.HasNewGuidButton != true)
            {
                return;
            }

            GUI.enabled = true;
            if(GUILayout.Button("Generate New Guid"))
            {
                GenerateNewGuid();
            }

            return;

            void GenerateNewGuid()
            {
                var targetObject = property.serializedObject.targetObject;

                if(targetObject is StableGuidComponent component)
                {
                    GuidRegistry.Add(component);
                }

                property.boxedValue = new UnityGuid(Guid.NewGuid());
                EditorUtility.SetDirty(targetObject);
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Returns if property should be drawed as disabled. It is drawn as disabled,
        /// if the containing asset/object is a prefab and in the case <see cref="UnityGuidUIOptionsAttribute"/>
        /// present, it should be have field true for prefabs.
        /// </summary>
        private bool IsPropertyDisabled(SerializedProperty property)
        {
            var attribute = PropertyUtility.GetAttribute<UnityGuidUIOptionsAttribute>(property);

            return (attribute?.DisableForPrefabs ?? false) &&
                IsInPrefab();


            bool IsInPrefab()
            {
                var targetObject = property.serializedObject.targetObject as Component;
                return UnityEditorUtils.IsAssetOnDisk(targetObject);
            }
        }
    }
}
