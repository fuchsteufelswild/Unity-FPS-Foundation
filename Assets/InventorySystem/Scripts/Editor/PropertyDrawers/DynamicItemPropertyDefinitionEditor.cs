using Nexora.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nexora.InventorySystem.Editor
{
    [CustomEditor(typeof(DynamicItemPropertyDefinition))]
    public sealed class DynamicItemPropertyDefinitionEditor : DefinitionEditor<DynamicItemPropertyDefinition>
    {
        private static bool _showFoldout = false;

        private List<ItemDefinition> _linkedItems;
        private Vector2 _scrollViewPosition;

        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            _showFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_showFoldout, "Property Used by Items");

            if(_showFoldout)
            {
                DrawItemReferences();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawItemReferences()
        {
            var dynamicPropertyDefinition = (DynamicItemPropertyDefinition)target;
            _linkedItems ??= ItemDefinitionUtility.GetAllItemsWithProperty(dynamicPropertyDefinition);

            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollViewPosition))
            {
                using(new EditorGUI.DisabledScope(true))
                {
                    if(_linkedItems == null)
                    {
                        GUILayout.Label("No item uses this dynamic property.");
                    }

                    foreach(ItemDefinition linkedItem in _linkedItems)
                    {
                        EditorGUILayout.ObjectField(linkedItem, typeof(ItemDefinition), false);
                    }
                }
                _scrollViewPosition = scroll.scrollPosition;
            }
        }
    }
}